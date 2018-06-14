using System.Collections.Generic;

namespace Cheez.Compiler.CodeGeneration
{
    public interface ICodeGenerator
    {
        bool GenerateCode(Workspace workspace, string targetFile);
        bool CompileCode(IEnumerable<string> libraryIncludeDirectories, IEnumerable<string> libraries, IErrorHandler errorHandler);
    }
}
