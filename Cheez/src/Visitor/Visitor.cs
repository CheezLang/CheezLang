using Cheez.Ast;

namespace Cheez.Visitor
{
    public interface IVisitor<T, D>
    {
        T VisitFunctionDeclaration(FunctionDeclaration function, D data = default(D));
        T VisitStringLiteral(StringLiteral str, D data = default(D));
        T VisitPrintStatement(PrintStatement print, D data = default(D)); // @Temporary
    }

    public interface IVoidVisitor<D>
    {
        void VisitFunctionDeclaration(FunctionDeclaration function, D data = default(D));
        void VisitStringLiteral(StringLiteral str, D data = default(D));
        void VisitPrintStatement(PrintStatement print, D data = default(D)); // @Temporary
    }
}
