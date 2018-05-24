namespace Cheez.Compiler.CodeGeneration
{
    public interface ICodeGenerator
    {
        bool GenerateCode(Workspace workspace, string targetFile);
    }
}
