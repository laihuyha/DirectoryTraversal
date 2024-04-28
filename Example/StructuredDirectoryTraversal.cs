using System;
using System.IO;
using System.Runtime.InteropServices;
using TestDirTraversal.Models;

namespace TestDirTraversal
{
    public static class StructuredDirectoryTraversal
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern IntPtr FindFirstFileA(string lpFileName, out WIN32_FIND_DATAA lpFindFileData);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        public static extern bool FindNextFileA(IntPtr hFindFile, out WIN32_FIND_DATAA lpFindFileData);

        [DllImport("kernel32.dll")]
        public static extern bool FindClose(IntPtr hFindFile);

        const IntPtr INVALID_HANDLE_VALUE = -1;

        static bool FindNextFilePInvokeRecursive(string path, out Entry folder)
        {
            folder = new Entry
            {
                FullPath = path,
                Name = Path.GetFileName(path),
            };

            IntPtr findHandle = INVALID_HANDLE_VALUE;

            try
            {
                path = path.EndsWith(@"\") ? path : path + @"\";
                findHandle = FindFirstFileA(path + @"*", out WIN32_FIND_DATAA findData);

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

        static void ProcessFoundFilesAndFolders(IntPtr findHandle, WIN32_FIND_DATAA findData, string path, Entry folder)
        {
            do
            {
                if (ShouldSkip(findData))
                    continue;

                string fullPath = path + findData.cFileName;

                if (IsDirectory(findData))
                {
                    AddSubfolder(fullPath, folder);
                }
                else
                {
                    AddFile(fullPath, folder);
                }
            }
            while (FindNextFileA(findHandle, out findData));
        }

        static bool ShouldSkip(WIN32_FIND_DATAA findData)
        {
            return findData.cFileName == "." || findData.cFileName == "..";
        }

        static bool IsDirectory(WIN32_FIND_DATAA findData)
        {
            return findData.dwFileAttributes.HasFlag(FileAttributes.Directory) && !findData.dwFileAttributes.HasFlag(FileAttributes.ReparsePoint);
        }

        static void AddSubfolder(string fullPath, Entry folder)
        {
            if (FindNextFilePInvokeRecursive(fullPath, out Entry subfolder))
            {
                folder.Subfolders.Add(subfolder);
            }
        }

        static void AddFile(string fullPath, Entry folder)
        {
            folder.Files.Add(fullPath);
        }

        public static Entry GetFolderStructure(string rootPath)
        {
            FindNextFilePInvokeRecursive(rootPath, out Entry rootFolder);
            return rootFolder;
        }
    }
}