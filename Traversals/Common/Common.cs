using System;
using System.Diagnostics;
using TestDirTraversal.Traversals.Specifications;

namespace TestDirTraversal.Traversals.Common
{
    public class Common : Windows
    {
        public Common()
        {
        }

        public Entry GetDirectoryTreeStructure(string rootPath)
        {
            try
            {
                var watch = new Stopwatch();
                watch.Start();
                var folder = GetFolderStructure(rootPath);
                watch.Stop();
                Console.BackgroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"===> Common.GetDirectoryTreeStructure taken: {watch.ElapsedMilliseconds} ms");
                return folder;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
                return null;
            }
        }
    }
}