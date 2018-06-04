using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Cheez.Compiler
{
    public class Reference<T>
    {
        public T Value { get; set; }

        public Reference()
        {
            Value = default;
        }

        public Reference(T v)
        {
            Value = v;
        }
    }

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

        public static string Indent(this string s, int level)
        {
            if (s == null)
                return "";
            if (level == 0)
                return s;
            return string.Join("\n", s.Split('\n').Select(line => $"{new string(' ', level)}{line}"));
        }

        public static string Indent(int level)
        {
            if (level == 0)
                return "";
            return new string(' ', level);
        }

        public static IEnumerable<string> Scan(this string value, string pattern)
        {
            var regex = new Regex(pattern);
            var matches = regex.Match(value);

            foreach (Group c in matches.Groups)
            {
                yield return c.Value;
            }
        }

        public static IEnumerable<string> Scan1(this string value, string pattern)
        {
            return value.Scan(pattern).Skip(1);
        }

        public static Process StartProcess(string exe, List<string> argList = null, string workingDirectory = null, DataReceivedEventHandler stdout = null, DataReceivedEventHandler stderr = null)
        {
            argList = argList ?? new List<string>();
            var args = string.Join(" ", argList.Select(a =>
            {
                if (a.Contains(" "))
                    return $"\"{a}\"";
                return a;
            }));
            return StartProcess(exe, args, workingDirectory, stdout, stderr);
        }

        public static Process StartProcess(string exe, string args = null, string workingDirectory = null, DataReceivedEventHandler stdout = null, DataReceivedEventHandler stderr = null)
        {
            //Console.WriteLine($"{exe} {args}");

            var process = new Process();
            process.StartInfo.FileName = exe;
            if (workingDirectory != null)
                process.StartInfo.WorkingDirectory = workingDirectory;
            if (args != null)
                process.StartInfo.Arguments = args;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            if (stdout != null)
            {
                process.StartInfo.RedirectStandardOutput = true;
                process.OutputDataReceived += stdout;
            }

            if (stderr != null)
            {
                process.StartInfo.RedirectStandardError = true;
                process.ErrorDataReceived += stderr;
            }

            process.Start();

            if (stdout != null)
                process.BeginOutputReadLine();
            if (stderr != null)
                process.BeginErrorReadLine();

            return process;
        }
    }
}
