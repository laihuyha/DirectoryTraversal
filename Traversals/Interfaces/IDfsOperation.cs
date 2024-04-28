using System.Collections.Generic;

namespace TestDirTraversal.Traversals.Interfaces
{
    public interface IDfsOperation
    {
        public void PInvokeDfsRecursiveAsync(string rootPath, HashSet<string> traversedPaths, out Entry entry);
    }
}