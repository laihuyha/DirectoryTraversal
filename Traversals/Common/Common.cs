using System;
using TestDirTraversal.Traversals.Specifications;

namespace TestDirTraversal.Traversals.Common
{
    public class Common
    {
        public Common()
        {
        }

        public Entry GetDirectoryTreeStructure(string rootPath)
        {
            try
            {
                Entry folderTree;
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    Console.WriteLine("====> Windows Base System");
                    var winBaseHelper = new Windows();
                    winBaseHelper.PInvokeRecursive(rootPath, out folderTree);
                    return folderTree;
                }
                else if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    Console.WriteLine("====> Unix Base System");
                    var unixBaseHelper = new Unix();
                    unixBaseHelper.PInvokeRecursive(rootPath, out folderTree);
                    return folderTree;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
                return null;
            }
        }
    }
}