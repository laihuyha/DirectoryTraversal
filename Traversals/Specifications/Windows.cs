using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using TestDirTraversal.Helper;
using TestDirTraversal.Models;
using TestDirTraversal.Traversals.Interfaces;

namespace TestDirTraversal.Traversals.Specifications
{
    public class Windows : IRecursiveOperation
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr FindFirstFileA(string lpFileName, out WIN32_FIND_DATAA lpFindFileData);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        private static extern bool FindNextFileA(IntPtr hFindFile, out WIN32_FIND_DATAA lpFindFileData);

        [DllImport("kernel32.dll")]
        private static extern bool FindClose(IntPtr hFindFile);

        private const IntPtr INVALID_HANDLE_VALUE = -1;

        public void PInvokeRecursive(string path, out Entry folder)
        {
            folder = new Entry
            {
                Name = Path.GetFileName(path),
                FullPath = Path.GetFullPath(path),
                Subfolders = [],
                Files = [],
            };

            IntPtr findHandle = INVALID_HANDLE_VALUE;

            try
            {
                path = path.EndsWith(@"\") ? path : path + @"\";
                findHandle = FindFirstFileA(path + @"*", out WIN32_FIND_DATAA findData);

                if (findHandle == INVALID_HANDLE_VALUE)
                    return;

                ProcessFoundFilesAndFolders(findHandle, findData, path, folder);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Caught exception while trying to enumerate a directory. {0}", exception.ToString());
                if (findHandle != INVALID_HANDLE_VALUE)
                    FindClose(findHandle);
                return;
            }
            finally
            {
                if (findHandle != INVALID_HANDLE_VALUE)
                    FindClose(findHandle);
            }

            return;
        }

        public void PInvokeRecursiveParalleled(string path, out Entry rootEntry)
        {
            rootEntry = new Entry
            {
                Name = Path.GetFileName(path),
                FullPath = Path.GetFullPath(path),
                Subfolders = [],
                Files = [],
            };

            IntPtr findHandle = INVALID_HANDLE_VALUE;

            try
            {
                path = path.EndsWith(@"\") ? path : path + @"\";
                findHandle = FindFirstFileA(path + @"*", out WIN32_FIND_DATAA findData);

                if (findHandle == INVALID_HANDLE_VALUE)
                    return;

                GetFolderEntries(findHandle, findData, path, rootEntry);

                rootEntry.Subfolders.AsParallel().ForAll((folder) =>
                {
                    object lockObject = new(); // Lock object for synchronization
                    PInvokeRecursive(folder.FullPath, out Entry subfolder);
                    lock (lockObject)
                    {
                        folder.Subfolders.AddRange(subfolder.Subfolders);
                        folder.Files.AddRange(subfolder.Files);
                    }
                });
            }
            catch (Exception exception)
            {
                Console.WriteLine("Caught exception while trying to enumerate a directory. {0}", exception.ToString());
                if (findHandle != INVALID_HANDLE_VALUE)
                    FindClose(findHandle);
                return;
            }
            finally
            {
                if (findHandle != INVALID_HANDLE_VALUE)
                    FindClose(findHandle);
            }
            return;
        }


        protected Entry GetFolderStructure(string rootPath)
        {
            // PInvokeRecursiveParalleled(rootPath, out Entry rootFolder);
            
            PInvokeRecursive(rootPath, out Entry rootFolder);
            return rootFolder;
        }

        #region Recursive
        protected void ProcessFoundFilesAndFolders(IntPtr findHandle, WIN32_FIND_DATAA findData, string path, Entry folder)
        {
            do
            {
                if (TraversalHelper.ShouldSkip(findData))
                    continue;

                string fullPath = path + findData.cFileName;

                if (TraversalHelper.IsWindowsDirectory(findData))
                {
                    TraversalHelper.AddSubfolder(fullPath, folder, PInvokeRecursive);
                }
                else
                {
                    TraversalHelper.AddFile(fullPath, folder);
                }
            }
            while (FindNextFileA(findHandle, out findData));
        }

        protected void GetFolderEntries(IntPtr findHandle, WIN32_FIND_DATAA findData, string path, Entry folder)
        {
            do
            {
                if (TraversalHelper.ShouldSkip(findData))
                    continue;

                string fullPath = path + findData.cFileName;

                if (TraversalHelper.IsWindowsDirectory(findData))
                {
                    folder.Subfolders.Add(new Entry { Name = Path.GetFileName(fullPath), FullPath = fullPath });
                }
                else
                {
                    TraversalHelper.AddFile(fullPath, folder);
                }
            }
            while (FindNextFileA(findHandle, out findData));
        }
        #endregion Recursive

    }
}