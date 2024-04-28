using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using TestDirTraversal.Helper;
using TestDirTraversal.Traversals.Interfaces;

namespace TestDirTraversal.Traversals.Specifications
{
    public class Windows : IDfsOperation, IRecursiveOperation
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr FindFirstFileW(string lpFileName, out WIN32_FIND_DATAW lpFindFileData);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATAW lpFindFileData);

        [DllImport("kernel32.dll")]
        public static extern bool FindClose(IntPtr hFindFile);

        private const IntPtr INVALID_HANDLE_VALUE = -1;

        public void PInvokeRecursive(string path, out Folder folder)
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

        public Folder PInvokeDfsRecursiveAsync(string rootPath, Folder folder, HashSet<string> traversedPaths)
        {
            try
            {
                var stack = new Stack<KeyValuePair<string, Folder>>();

                stack.Push(new KeyValuePair<string, Folder>(rootPath, folder));

                while (stack.Count > 0)
                {
                    var currentNode = stack.Pop();

                    if (traversedPaths.Contains(currentNode.Key)) continue;

                    GetDirectoryContent(currentNode.Key, out folder);

                    traversedPaths.Add(currentNode.Key);

                    folder.Subfolders.ForEach(x =>
                        stack.Push(new KeyValuePair<string, Folder>(rootPath + @"\" + x.Name, x))
                    );

                    stack.AsParallel().ForAll(pair => PInvokeDfsRecursiveAsync(pair.Key, pair.Value, traversedPaths));
                }
                return folder;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        #region Recursive
        public void ProcessFoundFilesAndFolders(IntPtr findHandle, WIN32_FIND_DATAW findData, string path, Folder folder)
        {
            string fullPath = path + @"\" + findData.cFileName;
            do
            {
                if (TraversalHelper.ShouldSkip(findData))
                    continue;


                if (TraversalHelper.IsWindowsDirectory(findData))
                {
                    TraversalHelper.AddSubfolder(fullPath, folder, PInvokeRecursive);
                }
                else
                {
                    TraversalHelper.AddFile(fullPath, folder);
                }
            }
            while (FindNextFile(findHandle, out findData));
        }
        #endregion Recursive

        #region Dfs
        public static void GetDirectoryContent(string path, out Folder folder)
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
                    return;

                string fullPath = path + @"\" + findData.cFileName;
                do
                {
                    if (TraversalHelper.ShouldSkip(findData))
                        continue;


                    if (TraversalHelper.IsWindowsDirectory(findData))
                    {
                        folder.Subfolders.Add(new Folder { Name = findData.cFileName, Files = [], Subfolders = [] });
                    }
                    else
                    {
                        TraversalHelper.AddFile(fullPath, folder);
                    }
                }
                while (FindNextFile(findHandle, out findData));
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
        }
        #endregion Dfs

        public Folder GetFolderStructure(string rootPath)
        {
            PInvokeRecursive(rootPath, out Folder rootFolder);

            // Folder rootFolder = new() { Files = [], Subfolders = [], Name = Path.GetFileName(rootPath) };
            // HashSet<string> traversedPaths = [];
            // PInvokeDfsRecursiveAsync(rootPath, rootFolder, traversedPaths);
            
            return rootFolder;
        }
    }
}