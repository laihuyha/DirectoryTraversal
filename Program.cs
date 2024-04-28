
using System;
using System.Diagnostics;
using TestDirTraversal;

public class Win32DirectoryTraversal
{
    public static void Main(string[] args)
    {
        string directoryPath = @"E:\";
        var watch = new Stopwatch();
        /// Structured Directory Traversal Usage
        watch.Start();
        Folder rootFolder = StructuredDirectoryTraversal.GetFolderStructure(directoryPath);
        watch.Stop();
        Console.WriteLine($"==> StructuredDirectoryTraversal.GetFolderStructure Elapsed in {watch.ElapsedMilliseconds} ms");
        watch.Reset();
    }
}
