using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TestDirTraversal
{
    public static class StructuredDirectoryTraversal
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr FindFirstFileW(string lpFileName, out WIN32_FIND_DATAW lpFindFileData);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATAW lpFindFileData);

        [DllImport("kernel32.dll")]
        public static extern bool FindClose(IntPtr hFindFile);

        static readonly IntPtr INVALID_HANDLE_VALUE = new(-1);

        static bool FindNextFilePInvokeRecursive(string path, out Folder folder)
        {
            folder = new Folder
            {
                Name = Path.GetFileName(path),
                Subfolders = [],
                Files = []
            };

            IntPtr findHandle = INVALID_HANDLE_VALUE;

            try
            {
                findHandle = FindFirstFileW(path + @"\*", out WIN32_FIND_DATAW findData);

                if (findHandle == INVALID_HANDLE_VALUE)
                    return false;

                ProcessFoundFilesAndFolders(findHandle, findData, path, folder);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Caught exception while trying to enumerate a directory. {0}", exception.ToString());
                if (findHandle != INVALID_HANDLE_VALUE)
                    FindClose(findHandle);
                return false;
            }
            finally
            {
                if (findHandle != INVALID_HANDLE_VALUE)
                    FindClose(findHandle);
            }

            return true;
        }

        static void ProcessFoundFilesAndFolders(IntPtr findHandle, WIN32_FIND_DATAW findData, string path, Folder folder)
        {
            string fullPath = path + @"\" + findData.cFileName;
            do
            {
                if (ShouldSkip(findData))
                    continue;


                if (IsDirectory(findData))
                {
                    AddSubfolder(fullPath, folder);
                }
                else
                {
                    AddFile(fullPath, folder);
                }
            }
            while (FindNextFile(findHandle, out findData));
        }

        static bool ShouldSkip(WIN32_FIND_DATAW findData)
        {
            return findData.cFileName == "." || findData.cFileName == "..";
        }

        static bool IsDirectory(WIN32_FIND_DATAW findData)
        {
            return findData.dwFileAttributes.HasFlag(FileAttributes.Directory) && !findData.dwFileAttributes.HasFlag(FileAttributes.ReparsePoint);
        }

        static void AddSubfolder(string fullPath, Folder folder)
        {
            if (FindNextFilePInvokeRecursive(fullPath, out Folder subfolder))
            {
                folder.Subfolders.Add(subfolder);
            }
        }

        static void AddFile(string fullPath, Folder folder)
        {
            folder.Files.Add(fullPath);
        }

        public static Folder GetFolderStructure(string rootPath)
        {
            FindNextFilePInvokeRecursive(rootPath, out Folder rootFolder);
            return rootFolder;
        }
    }
}