using System.Collections.Generic;

namespace Cheez.CodeGeneration
{
    public interface ICodeGenerator
    {
        bool GenerateCode(Workspace workspace, string intDir, string outDir, string targetFile, bool optimize, bool outputIntermediateFile);
        bool CompileCode(IEnumerable<string> libraryIncludeDirectories, IEnumerable<string> libraries, string subsystem, IErrorHandler errorHandler);
    }
}
