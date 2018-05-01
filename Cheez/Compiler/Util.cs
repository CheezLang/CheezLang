using System;
using System.IO;

namespace Cheez.Compiler
{
    public static class Util
    {
        private static int mId = 0;

        public static int NewId => mId++;

        public static string PathNormalize(this string path)
        {
            return Path.GetFullPath(new Uri(path).LocalPath)
                       .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        
        public static bool PathEqual(this string path1, string path2)
        {
            return path1.PathNormalize() == path2.PathNormalize();
        }
    }
}
