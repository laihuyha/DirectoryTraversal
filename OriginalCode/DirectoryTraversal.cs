using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace TestDirTraversal
{
    // Code from : https://stackoverflow.com/questions/26321366/fastest-way-to-get-directory-data-in-net
    public static class DirectoryTraversal
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr FindFirstFileW(string lpFileName, out WIN32_FIND_DATAW lpFindFileData);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATAW lpFindFileData);

        [DllImport("kernel32.dll")]
        public static extern bool FindClose(IntPtr hFindFile);

        static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        static bool FindNextFilePInvokeRecursive(string path, out List<FileInformation> files, out List<DirectoryInformation> directories)
        {
            List<FileInformation> fileList = new List<FileInformation>();
            List<DirectoryInformation> directoryList = new List<DirectoryInformation>();
            WIN32_FIND_DATAW findData;
            IntPtr findHandle = INVALID_HANDLE_VALUE;
            try
            {
                findHandle = FindFirstFileW(path + @"\*", out findData);
                if (findHandle != INVALID_HANDLE_VALUE)
                {
                    do
                    {
                        // Skip current directory and parent directory symbols that are returned.
                        if (findData.cFileName != "." && findData.cFileName != "..")
                        {
                            string fullPath = path + @"\" + findData.cFileName;
                            // Check if this is a directory and not a symbolic link since symbolic links could lead to repeated files and folders as well as infinite loops.
                            if (findData.dwFileAttributes.HasFlag(FileAttributes.Directory) && !findData.dwFileAttributes.HasFlag(FileAttributes.ReparsePoint))
                            {
                                directoryList.Add(new DirectoryInformation { FullPath = fullPath, LastWriteTime = findData.ftLastWriteTime.ToDateTime() });
                                List<FileInformation> subDirectoryFileList = new List<FileInformation>();
                                List<DirectoryInformation> subDirectoryDirectoryList = new List<DirectoryInformation>();
                                if (FindNextFilePInvokeRecursive(fullPath, out subDirectoryFileList, out subDirectoryDirectoryList))
                                {
                                    fileList.AddRange(subDirectoryFileList);
                                    directoryList.AddRange(subDirectoryDirectoryList);
                                }
                            }
                            else if (!findData.dwFileAttributes.HasFlag(FileAttributes.Directory))
                            {
                                fileList.Add(new FileInformation { FullPath = fullPath, LastWriteTime = findData.ftLastWriteTime.ToDateTime() });
                            }
                        }
                    }
                    while (FindNextFile(findHandle, out findData));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Caught exception while trying to enumerate a directory. {0}", exception.ToString());
                if (findHandle != INVALID_HANDLE_VALUE) FindClose(findHandle);
                files = null;
                directories = null;
                return false;
            }
            if (findHandle != INVALID_HANDLE_VALUE) FindClose(findHandle);
            files = fileList;
            directories = directoryList;
            return true;
        }

        public static bool FindNextFilePInvokeRecursiveParalleled(string path, out List<FileInformation> files, out List<DirectoryInformation> directories)
        {
            List<FileInformation> fileList = new List<FileInformation>();
            object fileListLock = new object();
            List<DirectoryInformation> directoryList = new List<DirectoryInformation>();
            object directoryListLock = new object();
            WIN32_FIND_DATAW findData;
            IntPtr findHandle = INVALID_HANDLE_VALUE;
            List<Tuple<string, DateTime>> info = new List<Tuple<string, DateTime>>();
            try
            {
                path = path.EndsWith(@"\") ? path : path + @"\";
                findHandle = FindFirstFileW(path + @"*", out findData);
                if (findHandle != INVALID_HANDLE_VALUE)
                {
                    do
                    {
                        // Skip current directory and parent directory symbols that are returned.
                        if (findData.cFileName != "." && findData.cFileName != "..")
                        {
                            string fullPath = path + findData.cFileName;
                            // Check if this is a directory and not a symbolic link since symbolic links could lead to repeated files and folders as well as infinite loops.
                            if (findData.dwFileAttributes.HasFlag(FileAttributes.Directory) && !findData.dwFileAttributes.HasFlag(FileAttributes.ReparsePoint))
                            {
                                directoryList.Add(new DirectoryInformation { FullPath = fullPath, LastWriteTime = findData.ftLastWriteTime.ToDateTime() });
                            }
                            else if (!findData.dwFileAttributes.HasFlag(FileAttributes.Directory))
                            {
                                fileList.Add(new FileInformation { FullPath = fullPath, LastWriteTime = findData.ftLastWriteTime.ToDateTime() });
                            }
                        }
                    }
                    while (FindNextFile(findHandle, out findData));
                    directoryList.AsParallel().ForAll(x =>
                    {
                        List<FileInformation> subDirectoryFileList = new List<FileInformation>();
                        List<DirectoryInformation> subDirectoryDirectoryList = new List<DirectoryInformation>();
                        if (FindNextFilePInvokeRecursive(x.FullPath, out subDirectoryFileList, out subDirectoryDirectoryList))
                        {
                            lock (fileListLock)
                            {
                                fileList.AddRange(subDirectoryFileList);
                            }
                            lock (directoryListLock)
                            {
                                directoryList.AddRange(subDirectoryDirectoryList);
                            }
                        }
                    });
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Caught exception while trying to enumerate a directory. {0}", exception.ToString());
                if (findHandle != INVALID_HANDLE_VALUE) FindClose(findHandle);
                files = null;
                directories = null;
                return false;
            }
            if (findHandle != INVALID_HANDLE_VALUE) FindClose(findHandle);
            files = fileList;
            directories = directoryList;
            return true;
        }
    }
}