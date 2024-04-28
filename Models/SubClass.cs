using System;
using System.Collections.Generic;

namespace TestDirTraversal
{
    public class Folder
    {
        public string Name { get; set; }
        public DateTime LastWriteTime { get; set; }
        public List<Folder> Subfolders { get; set; }
        public List<string> Files { get; set; }
    }

    public class FileInformation
    {
        public string FullPath { get; set; }
        public DateTime LastWriteTime { get; set; }
    }

    public class DirectoryInformation
    {
        public string FullPath { get; set; }
        public DateTime LastWriteTime { get; set; }
    }
}