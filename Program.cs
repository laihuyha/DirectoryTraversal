using System;
using TestDirTraversal;
using TestDirTraversal.Traversals.Common;

public class Program
{
    public static void Main(string[] args)
    {
        Console.BackgroundColor = Config.CONSOLE_COLOUR;
        
        string directoryPath = @"E:\";
        Common common = new();
        var tree = common.GetDirectoryTreeStructure(directoryPath);
    }
}
