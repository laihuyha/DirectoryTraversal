using System.Collections.Generic;

namespace TestDirTraversal.Traversals.Interfaces
{
    public interface IDfsOperation
    {
        public Folder PInvokeDfsRecursiveAsync(string rootPath, Folder folder, HashSet<string> traversedPaths);
    }
}