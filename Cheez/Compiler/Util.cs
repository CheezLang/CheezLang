using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Cheez.Compiler
{
    public static class Util
    {
        private static int mId = 0;

        public static int NewId => mId++;

        public static string Replace(this string str, params (string from, string to)[] reps)
        {
            foreach (var (f, t) in reps)
            {
                str = str.Replace(f, t);
            }
            return str;
        }

        public static string PathNormalize(this string path)
        {
            return Path.GetFullPath(new Uri(path).LocalPath)
                       .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        
        public static bool PathEqual(this string path1, string path2)
        {
            return path1.PathNormalize() == path2.PathNormalize();
        }

        public static IEnumerable<object> WithAction(this IEnumerable<object> en, Action a)
        {
            foreach (var v in en)
                yield return v;
            a();
            yield break;
        }
    }
}
