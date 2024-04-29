using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using TestDirTraversal.Helper;
using TestDirTraversal.Models;
using TestDirTraversal.Traversals.Interfaces;

namespace TestDirTraversal.Traversals.Specifications
{
    public class Unix : IRecursiveOperation
    {
        [DllImport("libc", EntryPoint = "opendir", CharSet = CharSet.Auto)]
        private static extern nint Opendir(string name);

        [DllImport("libc", EntryPoint = "readdir", CharSet = CharSet.Auto)]
        private static extern nint Readdir(nint dirp);

        [DllImport("libc", EntryPoint = "closedir")]
        private static extern int Closedir(nint dirp);

        private readonly nint INVALID_HANDLE_VALUE = IntPtr.Zero;

        public void PInvokeRecursive(string path, out Entry folder)
        {
            folder = new Entry
            {
                Name = Path.GetFileName(path),
                FullPath = Path.GetFullPath(path),
                Subfolders = [],
                Files = [],
            };

            var dir = Opendir(path);

            if (dir == INVALID_HANDLE_VALUE)
            {
                return;
                throw new ArgumentNullException(nameof(path), "Failed to open directory with this path!");
            }

            IntPtr entryPtr = INVALID_HANDLE_VALUE;

            try
            {
                path = path.EndsWith(@"\") ? path : path + @"\";

                entryPtr = Readdir(dir);

                if (entryPtr == INVALID_HANDLE_VALUE) return;

                ProcessFoundFilesAndFolders(entryPtr, path, folder);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Caught exception while trying to enumerate a directory. {0}", exception.ToString());
                _ = Closedir(entryPtr);
                return;
            }
            finally
            {
                _ = Closedir(entryPtr);
            }

            return;
        }

        public void PInvokeRecursiveParalleled(string path, out Entry rootEntry)
        {
            object lockObject = new(); // Lock object for synchronization
            rootEntry = new Entry
            {
                Name = Path.GetFileName(path),
                FullPath = Path.GetFullPath(path),
                Subfolders = [],
                Files = [],
            };

            var dir = Opendir(path);
            if (dir == IntPtr.Zero)
            {
                return;
                throw new ArgumentNullException(nameof(path), "Failed to open directory with this path!");
            }

            IntPtr entryPtr = INVALID_HANDLE_VALUE;

            try
            {
                path = path.EndsWith(@"\") ? path : path + @"\";

                entryPtr = Readdir(dir);
                if (entryPtr == IntPtr.Zero) return;

                GetFolderEntries(entryPtr, path, rootEntry);

                rootEntry.Subfolders.AsParallel().ForAll((folder) =>
                {
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
                _ = Closedir(dir);
                return;
            }
            finally
            {
                _ = Closedir(dir);
            }
            return;
        }

        #region Sub method
        private void GetFolderEntries(nint entryPtr, string path, Entry rootEntry)
        {
            while (entryPtr != INVALID_HANDLE_VALUE)
            {
                var entry = Marshal.PtrToStructure<Dirent>(entryPtr);
                if (TraversalHelper.ShouldSkip(entry))
                    continue;

                string fullPath = path + entry.d_name;

                if (TraversalHelper.IsDirectory(entry))
                {
                    rootEntry.Subfolders.Add(new Entry { Name = Path.GetFileName(fullPath), FullPath = fullPath });
                }
                else
                {
                    TraversalHelper.AddFile(fullPath, rootEntry);
                }
            }
        }

        private void ProcessFoundFilesAndFolders(nint entryPtr, string path, Entry folder)
        {
            while (entryPtr != INVALID_HANDLE_VALUE)
            {
                var entry = Marshal.PtrToStructure<Dirent>(entryPtr);

                if (TraversalHelper.ShouldSkip(entry))
                {
                    continue; // Skip current and parent directories
                }

                var entryFullPath = Path.Combine(path, entry.d_name);

                if (TraversalHelper.IsDirectory(entry)) // Directory
                {
                    TraversalHelper.AddSubfolder(entryFullPath, folder, PInvokeRecursive);
                }
                else
                {
                    TraversalHelper.AddFile(entryFullPath, folder);
                }
            }
        }
        #endregion Sub method
    }
}