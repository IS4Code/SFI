using IS4.MultiArchiver.Tools;
using System;
using System.IO;

namespace IS4.MultiArchiver.Media
{
    class LzExtractor
    {
        readonly BinaryReader ifile;

        protected BinaryReader Reader => ifile;

        readonly byte[] ihead_buffer = new byte[0x10 * sizeof(ushort)], ohead_buffer = new byte[0x10 * sizeof(ushort)], inf_buffer = new byte[8 * sizeof(ushort)];
        readonly byte[] sigbuf = new byte[sig90.Length];

        Span<ushort> ihead => ihead_buffer.AsSpan().MemoryCast<ushort>();
        Span<ushort> ohead => ohead_buffer.AsSpan().MemoryCast<ushort>();
        Span<ushort> inf => inf_buffer.AsSpan().MemoryCast<ushort>();
        long loadsize;

        readonly int? ver;

        public bool Valid => ver != null;

        public LzExtractor(BinaryReader reader)
        {
            ifile = reader;
            if(!rdhead(out var ver))
            {
                return;
            }
            this.ver = ver;
        }

        // Ported from https://github.com/mywave82/unlzexe/blob/master/unlzexe.c
        // Code is orignally written by Mitugu(Kou) Kurizono
        // Updated by Stian Sebastian Skjelstad (mywave82)
        // LZEXE was written by Fabrice Bellard, fabrice.bellard at free.fr.

        public MemoryStream Decompress()
        {
            var stream = new MemoryStream();
            var owriter = new BinaryWriter(stream);
            if(!mkreltbl(owriter, ver.GetValueOrDefault()))
            {
                return null;
            }
            if(!unpack(owriter))
            {
                return null;
            }
            wrhead(owriter);
            return stream;
        }

        /* EXE header test (is it LZEXE file?) */
        bool rdhead(out int ver)
        {
            ifile.BaseStream.Position = 0;
            long entry;
            ver = 0;
            if(ifile.Read(ihead_buffer, 0, ihead_buffer.Length) != ihead_buffer.Length)
                return false;
            Array.Copy(ihead_buffer, ohead_buffer, ohead_buffer.Length);
            if(ihead[0x0d] != 0 || ihead[0x0c] != 0x1c)
                return false;
            entry = ((long)(ihead[4] + ihead[0x0b]) << 4) + ihead[0x0a];
            ifile.BaseStream.Position = entry;
            if(ifile.Read(sigbuf, 0, sigbuf.Length) != sigbuf.Length)
                return false;
            var sigspan = sigbuf.AsSpan();
            if(sigspan.SequenceEqual(sig90))
            {
                ver = 90;
                return true;
            }
            if(sigspan.SequenceEqual(sig91))
            {
                ver = 91;
                return true;
            }
            return false;
        }

        /* make relocation table */
        bool mkreltbl(BinaryWriter ofile, int ver)
        {
            long fpos;
            int i;

            fpos = (long)(ihead[0x0b] + ihead[4]) << 4;     /* goto CS:0000 */
            ifile.BaseStream.Position = fpos;
            ifile.Read(inf_buffer, 0, inf_buffer.Length);
            ohead[0x0a] = inf[0];   /* IP */
            ohead[0x0b] = inf[1];   /* CS */
            ohead[0x08] = inf[2];   /* SP */
            ohead[0x07] = inf[3];   /* SS */
                                    /* inf[4]:size of compressed load module (PARAGRAPH)*/
                                    /* inf[5]:increase of load module size (PARAGRAPH)*/
                                    /* inf[6]:size of decompressor with  compressed relocation table (BYTE) */
                                    /* inf[7]:check sum of decompresser with compressd relocation table(Ver.0.90) */
            ohead[0x0c] = 0x1c;     /* start position of relocation table */
            ofile.BaseStream.Position = 0x1cL;
            switch(ver)
            {
                case 90:
                    if(!reloc90(ofile, fpos)) return false;
                    break;
                case 91:
                    if(!reloc91(ofile, fpos)) return false;
                    break;
                default:
                    return false;
            }
            fpos = ofile.BaseStream.Position;
            i = (0x200 - (int)fpos) & 0x1ff;
            ohead[4] = unchecked((ushort)(int)((fpos + i) >> 4));

            for(; i > 0; i--)
                ofile.Write((byte)0);
            return true;
        }

        /* for LZEXE ver 0.90 */
        bool reloc90(BinaryWriter ofile, long fpos)
        {
            uint c;
            ushort rel_count = 0;
            ushort rel_seg, rel_off;

            ifile.BaseStream.Position = fpos + 0x19d;
            /* 0x19d=compressed relocation table address */
            rel_seg = 0;
            do
            {
                if(ifile.BaseStream.Position >= ifile.BaseStream.Length) return false;
                c = ifile.ReadUInt16();
                for(; c > 0; c--)
                {
                    rel_off = ifile.ReadUInt16();
                    ofile.Write(rel_off);
                    ofile.Write(rel_seg);
                    rel_count++;
                }
                rel_seg += 0x1000;
            } while(rel_seg != 0);
            ohead[3] = rel_count;
            return true;
        }

        /* for LZEXE ver 0.91*/
        bool reloc91(BinaryWriter ofile, long fpos)
        {
            ushort span;
            ushort rel_count = 0;
            ushort rel_seg, rel_off;

            ifile.BaseStream.Position = fpos + 0x158;
            /* 0x158=compressed relocation table address */
            rel_off = 0; rel_seg = 0;
            for(; ; )
            {
                if(ifile.BaseStream.Position >= ifile.BaseStream.Length) return false;
                if((span = ifile.ReadByte()) == 0)
                {
                    span = ifile.ReadUInt16();
                    if(span == 0)
                    {
                        rel_seg += 0x0fff;
                        continue;
                    } else if(span == 1)
                    {
                        break;
                    }
                }
                rel_off += span;
                rel_seg += unchecked((ushort)((rel_off & ~0x0f) >> 4));
                rel_off &= 0x0f;
                ofile.Write(rel_off);
                ofile.Write(rel_seg);
                rel_count++;
            }
            ohead[3] = rel_count;
            return true;
        }

        /*---------------------*/
        struct bitstream
        {
            BinaryReader fp;
            ushort buf;
            byte count;

            public bitstream(BinaryReader filep)
            {
                fp = filep;
                count = 0x10;
                buf = filep.ReadUInt16();
            }

            public int getbit()
            {
                int b;
                b = buf & 1;
                if(--count == 0)
                {
                    buf = fp.ReadUInt16();
                    count = 0x10;
                } else
                    buf >>= 1;

                return b;
            }
        }

        static byte[] data = new byte[0x4500];

        /*---------------------*/
        /* decompressor routine */
        bool unpack(BinaryWriter ofile)
        {
            int len;
            ushort span;
            long fpos;
            int p = 0;

            fpos = ((long)ihead[0x0b] - (long)inf[4] + (long)ihead[4]) << 4;
            ifile.BaseStream.Position = fpos;
            fpos = (long)ohead[4] << 4;
            ofile.BaseStream.Position = fpos;
            var bits = new bitstream(ifile);
            for(; ; )
            {
                if(p > 0x4000)
                {
                    ofile.Write(data, 0, 0x2000);
                    p -= 0x2000;
                    Array.Copy(data, 0x2000, data, 0, p);
                }
                if(bits.getbit() != 0)
                {
                    data[p++] = ifile.ReadByte();
                    continue;
                }
                if(bits.getbit() == 0)
                {
                    len = bits.getbit() << 1;
                    len |= bits.getbit();
                    len += 2;
                    span = unchecked((ushort)(ifile.ReadByte() | 0xff00));
                } else
                {
                    span = ifile.ReadByte();
                    len = ifile.ReadByte();
                    span = unchecked((ushort)(span | ((len & ~0x07) << 5) | 0xe000));
                    len = (len & 0x07) + 2;
                    if(len == 2)
                    {
                        len = ifile.ReadByte();

                        if(len == 0)
                            break;    /* end mark of compreesed load module */

                        if(len == 1)
                            continue; /* segment change */
                        else
                            len++;
                    }
                }
                for(; len > 0; len--, p++)
                {
                    data[p] = data[p + unchecked((short)span)];
                }
            }
            if(p != 0)
                ofile.Write(data, 0, p);
            loadsize = ofile.BaseStream.Position - fpos;
            return true;
        }

        /* write EXE header*/
        void wrhead(BinaryWriter ofile)
        {
            if(ihead[6] != 0)
            {
                ohead[5] -= unchecked((ushort)(inf[5] + ((inf[6] + 16 - 1) >> 4) + 9));
                if(ihead[6] != 0xffff)
                    ohead[6] -= unchecked((ushort)(ihead[5] - ohead[5]));
            }
            ohead[1] = unchecked((ushort)(((ushort)loadsize + (ohead[4] << 4)) & 0x1ff));
            ohead[2] = (ushort)((loadsize + ((long)ohead[4] << 4) + 0x1ff) >> 9);
            ofile.BaseStream.Position = 0;
            ofile.Write(ohead_buffer, 0, 0x0e * sizeof(ushort));
        }


        /*-------------------------------------------*/

        static byte[] sig90 = {
            0x06, 0x0E, 0x1F, 0x8B, 0x0E, 0x0C, 0x00, 0x8B,
            0xF1, 0x4E, 0x89, 0xF7, 0x8C, 0xDB, 0x03, 0x1E,
            0x0A, 0x00, 0x8E, 0xC3, 0xB4, 0x00, 0x31, 0xED,
            0xFD, 0xAC, 0x01, 0xC5, 0xAA, 0xE2, 0xFA, 0x8B,
            0x16, 0x0E, 0x00, 0x8A, 0xC2, 0x29, 0xC5, 0x8A,
            0xC6, 0x29, 0xC5, 0x39, 0xD5, 0x74, 0x0C, 0xBA,
            0x91, 0x01, 0xB4, 0x09, 0xCD, 0x21, 0xB8, 0xFF,
            0x4C, 0xCD, 0x21, 0x53, 0xB8, 0x53, 0x00, 0x50,
            0xCB, 0x2E, 0x8B, 0x2E, 0x08, 0x00, 0x8C, 0xDA,
            0x89, 0xE8, 0x3D, 0x00, 0x10, 0x76, 0x03, 0xB8,
            0x00, 0x10, 0x29, 0xC5, 0x29, 0xC2, 0x29, 0xC3,
            0x8E, 0xDA, 0x8E, 0xC3, 0xB1, 0x03, 0xD3, 0xE0,
            0x89, 0xC1, 0xD1, 0xE0, 0x48, 0x48, 0x8B, 0xF0,
            0x8B, 0xF8, 0xF3, 0xA5, 0x09, 0xED, 0x75, 0xD8,
            0xFC, 0x8E, 0xC2, 0x8E, 0xDB, 0x31, 0xF6, 0x31,
            0xFF, 0xBA, 0x10, 0x00, 0xAD, 0x89, 0xC5, 0xD1,
            0xED, 0x4A, 0x75, 0x05, 0xAD, 0x89, 0xC5, 0xB2,
            0x10, 0x73, 0x03, 0xA4, 0xEB, 0xF1, 0x31, 0xC9,
            0xD1, 0xED, 0x4A, 0x75, 0x05, 0xAD, 0x89, 0xC5,
            0xB2, 0x10, 0x72, 0x22, 0xD1, 0xED, 0x4A, 0x75,
            0x05, 0xAD, 0x89, 0xC5, 0xB2, 0x10, 0xD1, 0xD1,
            0xD1, 0xED, 0x4A, 0x75, 0x05, 0xAD, 0x89, 0xC5,
            0xB2, 0x10, 0xD1, 0xD1, 0x41, 0x41, 0xAC, 0xB7,
            0xFF, 0x8A, 0xD8, 0xE9, 0x13, 0x00, 0xAD, 0x8B,
            0xD8, 0xB1, 0x03, 0xD2, 0xEF, 0x80, 0xCF, 0xE0,
            0x80, 0xE4, 0x07, 0x74, 0x0C, 0x88, 0xE1, 0x41,
            0x41, 0x26, 0x8A, 0x01, 0xAA, 0xE2, 0xFA, 0xEB,
            0xA6, 0xAC, 0x08, 0xC0, 0x74, 0x40, 0x3C, 0x01,
            0x74, 0x05, 0x88, 0xC1, 0x41, 0xEB, 0xEA, 0x89
        };

        static byte[] sig91 = {
            0x06, 0x0E, 0x1F, 0x8B, 0x0E, 0x0C, 0x00, 0x8B,
            0xF1, 0x4E, 0x89, 0xF7, 0x8C, 0xDB, 0x03, 0x1E,
            0x0A, 0x00, 0x8E, 0xC3, 0xFD, 0xF3, 0xA4, 0x53,
            0xB8, 0x2B, 0x00, 0x50, 0xCB, 0x2E, 0x8B, 0x2E,
            0x08, 0x00, 0x8C, 0xDA, 0x89, 0xE8, 0x3D, 0x00,
            0x10, 0x76, 0x03, 0xB8, 0x00, 0x10, 0x29, 0xC5,
            0x29, 0xC2, 0x29, 0xC3, 0x8E, 0xDA, 0x8E, 0xC3,
            0xB1, 0x03, 0xD3, 0xE0, 0x89, 0xC1, 0xD1, 0xE0,
            0x48, 0x48, 0x8B, 0xF0, 0x8B, 0xF8, 0xF3, 0xA5,
            0x09, 0xED, 0x75, 0xD8, 0xFC, 0x8E, 0xC2, 0x8E,
            0xDB, 0x31, 0xF6, 0x31, 0xFF, 0xBA, 0x10, 0x00,
            0xAD, 0x89, 0xC5, 0xD1, 0xED, 0x4A, 0x75, 0x05,
            0xAD, 0x89, 0xC5, 0xB2, 0x10, 0x73, 0x03, 0xA4,
            0xEB, 0xF1, 0x31, 0xC9, 0xD1, 0xED, 0x4A, 0x75,
            0x05, 0xAD, 0x89, 0xC5, 0xB2, 0x10, 0x72, 0x22,
            0xD1, 0xED, 0x4A, 0x75, 0x05, 0xAD, 0x89, 0xC5,
            0xB2, 0x10, 0xD1, 0xD1, 0xD1, 0xED, 0x4A, 0x75,
            0x05, 0xAD, 0x89, 0xC5, 0xB2, 0x10, 0xD1, 0xD1,
            0x41, 0x41, 0xAC, 0xB7, 0xFF, 0x8A, 0xD8, 0xE9,
            0x13, 0x00, 0xAD, 0x8B, 0xD8, 0xB1, 0x03, 0xD2,
            0xEF, 0x80, 0xCF, 0xE0, 0x80, 0xE4, 0x07, 0x74,
            0x0C, 0x88, 0xE1, 0x41, 0x41, 0x26, 0x8A, 0x01,
            0xAA, 0xE2, 0xFA, 0xEB, 0xA6, 0xAC, 0x08, 0xC0,
            0x74, 0x34, 0x3C, 0x01, 0x74, 0x05, 0x88, 0xC1,
            0x41, 0xEB, 0xEA, 0x89, 0xFB, 0x83, 0xE7, 0x0F,
            0x81, 0xC7, 0x00, 0x20, 0xB1, 0x04, 0xD3, 0xEB,
            0x8C, 0xC0, 0x01, 0xD8, 0x2D, 0x00, 0x02, 0x8E,
            0xC0, 0x89, 0xF3, 0x83, 0xE6, 0x0F, 0xD3, 0xEB,
            0x8C, 0xD8, 0x01, 0xD8, 0x8E, 0xD8, 0xE9, 0x72
        };
    }
}
