using System.IO;
using TestDirTraversal.Models;

namespace TestDirTraversal.Helper
{
    public static class TraversalHelper
    {
        public static readonly byte DT_UNKNOWN = 0;
        public static readonly byte DT_DIR = 4;
        public static readonly byte DT_REG = 8;

        public delegate void FindNextCallback(string fullPath, out Entry subfolder);

        public static bool ShouldSkip(WIN32_FIND_DATAA findData)
        {
            return findData.cFileName == "." || findData.cFileName == "..";
        }

        public static bool ShouldSkip(Dirent direntData)
        {
            return direntData.d_name == "." || direntData.d_name == "..";
        }


        public static bool IsDirectory(WIN32_FIND_DATAA findData)
        {
            return findData.dwFileAttributes.HasFlag(FileAttributes.Directory) && !findData.dwFileAttributes.HasFlag(FileAttributes.ReparsePoint);
        }

        public static bool IsDirectory(Dirent direntData)
        {
            return (direntData.d_type == DT_DIR) && !(direntData.d_type == DT_REG && direntData.d_type == DT_UNKNOWN);
        }

        public static void AddSubfolder(string fullPath, Entry folder, FindNextCallback callbackFunction)
        {
            callbackFunction(fullPath, out Entry subfolder);

            folder.Subfolders.Add(subfolder);
        }

        public static void AddFile(string fullPath, Entry folder)
        {
            folder.Files.Add(fullPath);
        }
    }
}