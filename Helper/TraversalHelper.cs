using System.IO;

namespace TestDirTraversal.Helper
{
    public static class TraversalHelper
    {
        private const byte DT_DIR = 4;

        public delegate void FindNextFileCallback(string fullPath, out Folder subfolder);

        public static bool ShouldSkip(WIN32_FIND_DATAW findData)
        {
            return findData.cFileName == "." || findData.cFileName == "..";
        }

        public static bool IsWindowsDirectory(WIN32_FIND_DATAW findData)
        {
            return findData.dwFileAttributes.HasFlag(FileAttributes.Directory) && !findData.dwFileAttributes.HasFlag(FileAttributes.ReparsePoint);
        }

        public static void AddSubfolder(string fullPath, Folder folder, FindNextFileCallback callbackFunction)
        {
            callbackFunction(fullPath, out Folder subfolder);

            folder.Subfolders.Add(subfolder);
        }

        public static void AddFile(string fullPath, Folder folder)
        {
            folder.Files.Add(fullPath);
        }
    }
}