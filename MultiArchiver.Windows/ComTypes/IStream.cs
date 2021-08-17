using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace IS4.MultiArchiver.Windows.ComTypes
{
    [ComImport, Guid("0000000c-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    unsafe interface IStream
    {
        void Read([Out] byte* pv, int cb, [Out] int* pcbRead);
        void Write([In] byte* pv, int cb, [Out] int* pcbWritten);
        void Seek(long dlibMove, int dwOrigin, [Out] long* plibNewPosition);
        void SetSize(long libNewSize);
        void CopyTo(IStream pstm, long cb, [Out] long* pcbRead, [Out] long* pcbWritten);
        void Commit(int grfCommitFlags);
        void Revert();
        void LockRegion(long libOffset, long cb, int dwLockType);
        void UnlockRegion(long libOffset, long cb, int dwLockType);
        void Stat(out STATSTG pstatstg, int grfStatFlag);
        void Clone(out IStream ppstm);
    }
}
