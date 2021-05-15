using System;
using System.Runtime.InteropServices;

namespace IS4.MultiArchiver.Windows
{
    [ComImport, Guid("00000109-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IPersistStream
    {
        Guid GetClassID();
        [PreserveSig]
        int IsDirty();
        [PreserveSig]
        int Load(IStream pStm);
        void Save(IStream pStm, bool fClearDirty);
        uint GetSizeMax();
    }
}
