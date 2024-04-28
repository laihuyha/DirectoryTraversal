using TestDirTraversal.Traversals.Common;

public class Win32DirectoryTraversal
{
    public static void Main(string[] args)
    {
        string directoryPath = @"E:\";
        Common common = new();
        var tree = common.GetDirectoryTreeStructure(directoryPath);
    }
}
