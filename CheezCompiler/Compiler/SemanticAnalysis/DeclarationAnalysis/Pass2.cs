using Cheez.Compiler.Ast;

namespace Cheez.Compiler.SemanticAnalysis.DeclarationAnalysis
{
    public partial class DeclarationAnalyzer
    {
        /// <summary>
        /// Pass 2: poly struct parameter types
        /// </summary>
        private void Pass2()
        {
            foreach (var polyStruct in mPolyStructs)
            {
                Pass2PolyStruct(polyStruct);
            }
        }

        private void Pass2PolyStruct(AstStructDecl @struct)
        {
            foreach (var param in @struct.Parameters)
            {
                param.TypeExpr.Scope = @struct.Scope;
                param.Type = mWorkspace.ResolveType(param.TypeExpr);

                switch (param.Type)
                {
                    case IntType _:
                    case FloatType _:
                    case CheezTypeType _:
                    case BoolType _:
                    case CharType _:
                        break;

                    case ErrorType _:
                        break;

                    default:
                        mWorkspace.ReportError(param.TypeExpr, $"The type '{param.Type}' is not allowed here.");
                        break;
                }
            }
        }
    }
}
