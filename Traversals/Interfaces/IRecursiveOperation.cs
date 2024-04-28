namespace TestDirTraversal.Traversals.Interfaces
{
    public interface IRecursiveOperation
    {
        public void PInvokeRecursive(string path, out Folder folder);
    }
}