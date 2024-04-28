using System.IO;
using TestDirTraversal.Models;

namespace TestDirTraversal.Helper
{
    public static class TraversalHelper
    {
        public delegate void FindNextFileCallback(string fullPath, out Entry subfolder);

        public static bool ShouldSkip(WIN32_FIND_DATAA findData)
        {
            return findData.cFileName == "." || findData.cFileName == "..";
        }

        public static bool IsWindowsDirectory(WIN32_FIND_DATAA findData)
        {
            return findData.dwFileAttributes.HasFlag(FileAttributes.Directory) && !findData.dwFileAttributes.HasFlag(FileAttributes.ReparsePoint);
        }

        public static void AddSubfolder(string fullPath, Entry folder, FindNextFileCallback callbackFunction)
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