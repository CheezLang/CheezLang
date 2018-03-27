using Cheez.Ast;

namespace Cheez.Visitor
{
    public interface IVisitor<ReturnType, DataType>
    {
        ReturnType VisitFunctionDeclaration(FunctionDeclaration function, DataType data = default);
        ReturnType VisitVariableDeclaration(VariableDeclaration variable, DataType data = default);
        ReturnType VisitConstantDeclaration(ConstantDeclaration constant, DataType data = default);
        ReturnType VisitTypeDeclaration(TypeDeclaration type, DataType data = default);
        ReturnType VisitAssignment(Assignment ass, DataType data = default);
        ReturnType VisitExpressionStatement(ExpressionStatement stmt, DataType data = default);
        ReturnType VisitIfStatement(IfStatement ifs, DataType data = default);
        ReturnType VisitBlockStatement(BlockStatement block, DataType data = default);
        ReturnType VisitImplBlock(ImplBlock impl, DataType data = default);

        ReturnType VisitIdentifierExpression(IdentifierExpression ident, DataType data = default);
        ReturnType VisitStringLiteral(StringLiteral str, DataType data = default);
        ReturnType VisitNumberExpression(NumberExpression num, DataType data = default);
        ReturnType VisitDotExpression(DotExpression dot, DataType data = default);

        ReturnType VisitPrintStatement(PrintStatement print, DataType data = default); // @Temporary
    }

    public interface IVoidVisitor<DataType>
    {
        void VisitFunctionDeclaration(FunctionDeclaration function, DataType data = default);
        void VisitVariableDeclaration(VariableDeclaration variable, DataType data = default);
        void VisitConstantDeclaration(ConstantDeclaration constant, DataType data = default);
        void VisitTypeDeclaration(TypeDeclaration type, DataType data = default);
        void VisitAssignment(Assignment ass, DataType data = default);
        void VisitExpressionStatement(ExpressionStatement stmt, DataType data = default);
        void VisitIfStatement(IfStatement ifs, DataType data = default);
        void VisitBlockStatement(BlockStatement block, DataType data = default);
        void VisitImplBlock(ImplBlock impl, DataType data = default);

        void VisitIdentifierExpression(IdentifierExpression ident, DataType data = default);
        void VisitStringLiteral(StringLiteral str, DataType data = default);
        void VisitNumberExpression(NumberExpression num, DataType data = default);
        void VisitDotExpression(DotExpression dot, DataType data = default);

        void VisitPrintStatement(PrintStatement print, DataType data = default); // @Temporary
    }

    public abstract class VisitorBase<ReturnType, DataType> : IVisitor<ReturnType, DataType>
    {
        public virtual ReturnType VisitAssignment(Assignment ass, DataType data = default) => default; 

        public virtual ReturnType VisitBlockStatement(BlockStatement block, DataType data = default) => default; 

        public virtual ReturnType VisitConstantDeclaration(ConstantDeclaration constant, DataType data = default) => default;

        public virtual ReturnType VisitDotExpression(DotExpression dot, DataType data = default) => default;

        public virtual ReturnType VisitExpressionStatement(ExpressionStatement stmt, DataType data = default) => default; 

        public virtual ReturnType VisitFunctionDeclaration(FunctionDeclaration function, DataType data = default) => default; 

        public virtual ReturnType VisitIdentifierExpression(IdentifierExpression ident, DataType data = default) => default; 

        public virtual ReturnType VisitIfStatement(IfStatement ifs, DataType data = default) => default;

        public virtual ReturnType VisitImplBlock(ImplBlock impl, DataType data = default) => default;

        public virtual ReturnType VisitNumberExpression(NumberExpression num, DataType data = default) => default; 

        public virtual ReturnType VisitPrintStatement(PrintStatement print, DataType data = default) => default; 

        public virtual ReturnType VisitStringLiteral(StringLiteral str, DataType data = default) => default; 

        public virtual ReturnType VisitTypeDeclaration(TypeDeclaration type, DataType data = default) => default; 

        public virtual ReturnType VisitVariableDeclaration(VariableDeclaration variable, DataType data = default) => default; 
    }

    public abstract class VisitorBase<DataType> : IVoidVisitor<DataType>
    {
        public virtual void VisitAssignment(Assignment ass, DataType data = default) {}

        public virtual void VisitBlockStatement(BlockStatement block, DataType data = default) {}

        public virtual void VisitConstantDeclaration(ConstantDeclaration constant, DataType data = default) {}

        public virtual void VisitExpressionStatement(ExpressionStatement stmt, DataType data = default) {}

        public virtual void VisitFunctionDeclaration(FunctionDeclaration function, DataType data = default) {}

        public virtual void VisitIdentifierExpression(IdentifierExpression ident, DataType data = default) {}

        public virtual void VisitIfStatement(IfStatement ifs, DataType data = default) {}

        public virtual void VisitNumberExpression(NumberExpression num, DataType data = default) {}

        public virtual void VisitPrintStatement(PrintStatement print, DataType data = default) {}

        public virtual void VisitStringLiteral(StringLiteral str, DataType data = default) {}

        public virtual void VisitTypeDeclaration(TypeDeclaration type, DataType data = default) {}

        public virtual void VisitVariableDeclaration(VariableDeclaration variable, DataType data = default) {}

        public virtual void VisitImplBlock(ImplBlock impl, DataType data = default) { }

        public virtual void VisitDotExpression(DotExpression dot, DataType data = default) { }
    }
}
