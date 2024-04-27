using System.IO;
using System.Runtime.InteropServices;

namespace TestDirTraversal
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct WIN32_FIND_DATAW
    {
        public FileAttributes dwFileAttributes;
        internal System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
        internal System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
        internal System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
        public int nFileSizeHigh;
        public int nFileSizeLow;
        public int dwReserved0;
        public int dwReserved1;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string cFileName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
        public string cAlternateFileName;
    }
}