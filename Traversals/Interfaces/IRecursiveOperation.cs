namespace TestDirTraversal.Traversals.Interfaces
{
    public interface IRecursiveOperation
    {
        public void PInvokeRecursive(string path, out Entry folder);
        public void PInvokeRecursiveParalleled(string path, out Entry folder);
    }
}