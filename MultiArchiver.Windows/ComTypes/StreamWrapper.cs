using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace IS4.MultiArchiver.Windows.ComTypes
{
    unsafe class StreamWrapper : IStream
	{
		private readonly Stream baseStream;
		
		public StreamWrapper(Stream baseStream)
		{
			this.baseStream = baseStream;
		}
		
	    public void Read([Out] byte* pv, int cb, [Out] int* pcbRead)
	    {
            var buffer = ArrayPool<byte>.Shared.Rent(cb);
            try{
	    	    int read = baseStream.Read(buffer, 0, cb);
                if(pcbRead != null) *pcbRead = read;
                Marshal.Copy(buffer, 0, (IntPtr)pv, cb);
            }finally{
                ArrayPool<byte>.Shared.Return(buffer);
            }
	    }
	    
	    public void Write([In] byte* pv, int cb, [Out] int* pcbWritten)
	    {
            if(cb <= sizeof(int))
            {
                for(int i = 0; i < cb; i++)
                {
                    baseStream.WriteByte(pv[i]);
                }
            }else{
                var buffer = ArrayPool<byte>.Shared.Rent(cb);
                try{
                    Marshal.Copy((IntPtr)pv, buffer, 0, cb);
                    baseStream.Write(buffer, 0, cb);
                }finally{
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
            if(pcbWritten != null) *pcbWritten = cb;
        }
	    
	    public void Seek(long dlibMove, int dwOrigin, [Out] long* plibNewPosition)
	    {
	    	long pos = baseStream.Seek(dlibMove, (SeekOrigin)dwOrigin);
            if(plibNewPosition != null) *plibNewPosition = pos;
        }
	    
	    public void SetSize(long libNewSize)
	    {
	    	baseStream.SetLength(libNewSize);
	    }
	    
	    public void CopyTo(IStream pstm, long cb, [Out] long* pcbRead, [Out] long* pcbWritten)
	    {
            byte[] buffer = new byte[81920];
            fixed(byte* ptr = buffer)
            {
                int read;
                int written = 0;
                int* ptr_written = pcbWritten != null ? &written : null;
                while((read = baseStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    pstm.Write(ptr, read, ptr_written);
                    if(pcbRead != null) *pcbRead += read;
                    if(pcbWritten != null) *pcbWritten += written;
                }
            }
        }
	    
	    public void Commit(int grfCommitFlags)
	    {
	    	baseStream.Flush();
	    }
	    
	    public void Revert()
	    {
	    	throw new NotSupportedException();
	    }
	    
	    public void LockRegion(long libOffset, long cb, int dwLockType)
	    {
	    	if(baseStream is FileStream fileStream)
	    	{
	    		fileStream.Lock(libOffset, cb);
	    	}
	    	throw new NotSupportedException();
	    }
	    
	    public void UnlockRegion(long libOffset, long cb, int dwLockType)
        {
            if(baseStream is FileStream fileStream)
            {
	    		fileStream.Unlock(libOffset, cb);
	    	}
	    	throw new NotSupportedException();
	    }
	    
	    public void Stat(out STATSTG pstatstg, int grfStatFlag)
	    {
            pstatstg = default;
            pstatstg.type = 1;
            pstatstg.cbSize = baseStream.Length;
            pstatstg.grfMode = baseStream.CanRead ? baseStream.CanWrite ? 2 : 0 : baseStream.CanWrite ? 1 : 0;
            pstatstg.grfLocksSupported = baseStream is FileStream ? 1 : 0;
	    }
	    
	    public void Clone(out IStream ppstm)
	    {
	    	if(baseStream is ICloneable cloneable)
	    	{
	    		ppstm = new StreamWrapper((Stream)cloneable.Clone());
	    	}
	    	throw new NotSupportedException();
	    }
	}
}
