using System;
using System.Collections.Generic;

namespace TestDirTraversal
{
    public class Entry
    {
        public string FullPath { get; set; }
        public string Name { get; set; }
        public List<Entry> Subfolders { get; set; } = [];
        public List<string> Files { get; set; } = [];
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