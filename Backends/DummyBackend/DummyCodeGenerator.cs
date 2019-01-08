using System.Collections.Generic;
using System.IO;
using Cheez.CodeGeneration;
using Cheez.Visitors;
using Cheez;

namespace DummyBackend
{
    public class DummyCodeGenerator : ICodeGenerator
    {
        public bool CompileCode(IEnumerable<string> libraryIncludeDirectories, IEnumerable<string> libraries, string subsystem, IErrorHandler errorHandler)
        {
            // nothing to do
            return true;
        }

        public bool GenerateCode(Workspace workspace, string intDir, string outDir, string targetFile, bool optimize, bool outputIntermediateFile)
        {
            if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);
            var outPath = Path.Combine(outDir, targetFile + ".che");

            var printer = new AnalyzedAstPrinter();
            using (var file = File.Open(outPath, FileMode.Create))
            using (var writer = new StreamWriter(file))
            {
                printer.PrintWorkspace(workspace, writer);
            }
            return true;
        }
    }
}
