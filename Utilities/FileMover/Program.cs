using CommandLine;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace FileMover
{
    class Program
    {
        class Options
        {
            //[Option(HelpText = "Target Directory")]
            [Value(0)]
            public string TargetDirectory { get; set; }

            //[Option(HelpText = "Files and Folders to be moved", Required = true)]
            [Value(1, Min = 1)]
            public IEnumerable<string> InputFiles { get; set; }


            [Option('e', "exclude", Default = "", HelpText = "Regex which specifies which files and folders to exclude")]
            public string ExcludeRegex { get; set; }
        }

        static void OnSuccess(Options o)
        {
            string workingDir = Directory.GetCurrentDirectory();
            var exclude = new Regex(o.ExcludeRegex);

            // create target dir
            if (!Directory.Exists(o.TargetDirectory))
                Directory.CreateDirectory(o.TargetDirectory);

            foreach (var input in o.InputFiles)
            {
                string path = input;

                if (!Path.IsPathRooted(input))
                {
                    path = Path.Combine(workingDir, input);
                }

                var match = exclude.Match(path);
                if (match.Success && match.Index == 0 && match.Length == path.Length)
                    continue;

                if (File.Exists(path))
                {
                    string filename = Path.GetFileName(path);
                    string targetPath = Path.Combine(o.TargetDirectory, filename);
                    File.Copy(path, targetPath);
                }
                
                if (Directory.Exists(path))
                {
                    string foldername = GetDirectoryName(path);
                    string targetPath = Path.Combine(o.TargetDirectory, foldername);

                    // copy all sub files and dirs
                    CopyDirectory(targetPath, path, exclude);
                }
            }
        }

        static string GetDirectoryName(string path)
        {
            if (path.EndsWith("."))
                path = Path.GetDirectoryName(path);
            var index = path.LastIndexOf(Path.DirectorySeparatorChar);
            if (index >= 0 && index < path.Length -1)
            {
                return path.Substring(index + 1);
            }

            index = path.LastIndexOf(Path.AltDirectorySeparatorChar);
            if (index >= 0 && index < path.Length - 1)
            {
                return path.Substring(index + 1);
            }

            return path;
        }

        static void CopyDirectory(string target, string source, Regex exclude)
        {
            if (!Directory.Exists(target))
            {
                Directory.CreateDirectory(target);
            }

            foreach (var filepath in Directory.EnumerateFiles(source))
            {
                string filename = Path.GetFileName(filepath);
                string targetpath = Path.Combine(target, filename);

                var match = exclude.Match(filepath);
                if (match.Success && match.Index == 0 && match.Length == filepath.Length)
                    continue;

                File.Copy(filepath, targetpath, true);
            }

            foreach (var sourcepath in Directory.EnumerateDirectories(source))
            {
                var sourcename = GetDirectoryName(sourcepath);
                var targetpath = Path.Combine(target, sourcename);

                var match = exclude.Match(sourcepath);
                if (match.Success && match.Index == 0 && match.Length == sourcepath.Length)
                    continue;

                CopyDirectory(targetpath, sourcepath, exclude);
            }
        }

        static void OnError(IEnumerable<Error> errors)
        {
            foreach (var e in errors)
            {
                System.Console.WriteLine($"[ERROR] {e.Tag}");
            }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(OnSuccess).WithNotParsed(OnError);
        }
    }
}
