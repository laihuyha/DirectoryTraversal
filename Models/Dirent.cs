using System;
using System.Runtime.InteropServices;

namespace TestDirTraversal.Models
{
    /// <summary>
    /// This dirent struct using for UNIX base system
    /// <see cref="dirent">https://man7.org/linux/man-pages/man3/readdir.3.html</see>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct Dirent
    {
        public IntPtr d_ino;
        public IntPtr d_off;
        public ushort d_reclen;
        public byte d_type;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string d_name;
    }
}