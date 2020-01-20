using System;
using System.IO;
using System.Threading.Tasks;

namespace generator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 4) {
                System.Console.WriteLine($"Expected 4 arguments");
                System.Console.WriteLine($"Usage: generator.exe <file count> <functions per file> <calls per function> <use puts>");
                return;
            }


            int fileCount = int.Parse(args[0]);
            int functionCount = int.Parse(args[1]);
            int callCount = int.Parse(args[2]);
            bool usePuts = bool.Parse(args[3]);

            if (Directory.Exists("./out")) {
                Directory.Delete("./out", true);
            }
            Directory.CreateDirectory("./out");

            var (call, import) = usePuts switch {
                true => ("C.puts", "C :: import std.c"),
                false => ("io.println", "io :: import std.io")
            };
            System.Console.WriteLine($"Generating {fileCount} files with {functionCount} functions calling {call} {callCount} times");

            int linesOfCode = fileCount * (functionCount * (2 + callCount) + 1) + fileCount + 1;
            System.Console.WriteLine($"Resulting in {fileCount * functionCount} functions and roughly {linesOfCode} lines of code");

            var tasks = new Task[fileCount];

            using (var mainFile = File.OpenWrite("./out/main.che"))
            using (var main = new StreamWriter(mainFile))
            {
                for (int i = 0; i < fileCount; i++) {
                    main.WriteLine($"import file_{i}");

                    int index = i;
                    tasks[i] = Task.Run(() => GenerateFile(index, functionCount, callCount, call, import));
                }

                main.WriteLine("Main :: () {}");
                main.Flush();
            }

            Task.WaitAll(tasks);
        }

        static void GenerateFile(int index, int functionCount, int callCount, string call, string import) {
            using (var sourceFile = File.OpenWrite($"./out/file_{index}.che"))
            using (var file = new StreamWriter(sourceFile))
            {
                file.WriteLine(import);
                for (int i = 0; i < functionCount; i++) {
                    file.WriteLine($"function_{index}_{i} :: () {{");
                    for (int k = 0; k < callCount; k++)
                        file.WriteLine($"    {call}(\"Hello World!\")");
                    file.WriteLine("}");
                }
                file.Flush();
            }
        }
    }
}
