using Cheez.Compiler.Ast;

namespace Cheez.Compiler.Visitor
{
    public interface IVisitorAcceptor
    {
        T Accept<T, D>(IVisitor<T, D> visitor, D data = default);
    }

    public interface IVisitor<ReturnType, DataType>
    {
        ReturnType VisitFunctionDeclaration(AstFunctionDecl function, DataType data = default);
        ReturnType VisitVariableDeclaration(AstVariableDecl variable, DataType data = default);
        //ReturnType VisitConstantDeclaration(ConstantDeclaration constant, DataType data = default);
        ReturnType VisitStructDeclaration(AstStructDecl type, DataType data = default);
        ReturnType VisitEnumDeclaration(AstEnumDecl en, DataType data = default);
        ReturnType VisitAssignment(AstAssignment ass, DataType data = default);
        ReturnType VisitExpressionStatement(AstExprStmt stmt, DataType data = default);
        ReturnType VisitIfStatement(AstIfStmt ifs, DataType data = default);
        ReturnType VisitWhileStatement(AstWhileStmt ws, DataType data = default);
        ReturnType VisitBlockStatement(AstBlockStmt block, DataType data = default);
        ReturnType VisitImplBlock(AstImplBlock impl, DataType data = default);
        ReturnType VisitReturnStatement(AstReturnStmt ret, DataType data = default);
        ReturnType VisitUsingStatement(AstUsingStmt use, DataType data = default);
        ReturnType VisitTypeAlias(AstTypeAliasDecl al, DataType data = default);
        ReturnType VisitDeferStatement(AstDeferStmt def, DataType data = default);
        ReturnType VisitMatchStatement(AstMatchStmt match, DataType data = default);
        ReturnType VisitBreakStatement(AstBreakStmt br, DataType data = default);
        ReturnType VisitContinueStatement(AstContinueStmt cont, DataType data = default);

        ReturnType VisitEmptyStatement(AstEmptyStatement em, DataType data = default);

        ReturnType VisitEmptyExpression(AstEmptyExpr em, DataType data = default);

        ReturnType VisitIdentifierExpression(AstIdentifierExpr ident, DataType data = default);
        ReturnType VisitStringLiteral(AstStringLiteral str, DataType data = default);
        ReturnType VisitNumberExpression(AstNumberExpr num, DataType data = default);
        ReturnType VisitDotExpression(AstDotExpr dot, DataType data = default);
        ReturnType VisitCallExpression(AstCallExpr call, DataType data = default);
        ReturnType VisitCompCallExpression(AstCompCallExpr call, DataType data = default);
        ReturnType VisitBinaryExpression(AstBinaryExpr bin, DataType data = default);
        ReturnType VisitUnaryExpression(AstUnaryExpr bin, DataType data = default);
        ReturnType VisitBoolExpression(AstBoolExpr bo, DataType data = default);
        ReturnType VisitAddressOfExpression(AstAddressOfExpr add, DataType data = default);
        ReturnType VisitDereferenceExpression(AstDereferenceExpr deref, DataType data = default);
        ReturnType VisitCastExpression(AstCastExpr cast, DataType data = default);
        ReturnType VisitArrayAccessExpression(AstArrayAccessExpr arr, DataType data = default);
        ReturnType VisitStructValueExpression(AstStructValueExpr str, DataType data = default);
        ReturnType VisitPointerTypeExpr(AstPointerTypeExpr astPointerTypeExpr, DataType data = default);
        ReturnType VisitArrayTypeExpr(AstArrayTypeExpr astArrayTypeExpr, DataType data = default);
        ReturnType VisitTypeExpr(AstTypeExpr astArrayTypeExpr, DataType data = default);
        ReturnType VisitArrayExpression(AstArrayExpression arr, DataType data = default);
        ReturnType VisitNullExpression(AstNullExpr nul, DataType data = default);
    }


    public abstract class VisitorBase<ReturnType, DataType> : IVisitor<ReturnType, DataType>
    {
        public virtual ReturnType VisitAddressOfExpression(AstAddressOfExpr add, DataType data = default) => default;

        public virtual ReturnType VisitArrayAccessExpression(AstArrayAccessExpr arr, DataType data = default) => default;

        public virtual ReturnType VisitArrayTypeExpr(AstArrayTypeExpr astArrayTypeExpr, DataType data = default) => default;

        public virtual ReturnType VisitAssignment(AstAssignment ass, DataType data = default) => default;

        public virtual ReturnType VisitBinaryExpression(AstBinaryExpr bin, DataType data = default) => default;

        public virtual ReturnType VisitBlockStatement(AstBlockStmt block, DataType data = default) => default;

        public virtual ReturnType VisitBoolExpression(AstBoolExpr bo, DataType data = default) => default;

        public virtual ReturnType VisitCallExpression(AstCallExpr call, DataType data = default) => default;

        public virtual ReturnType VisitCastExpression(AstCastExpr cast, DataType data = default) => default;

        public virtual ReturnType VisitCompCallExpression(AstCompCallExpr call, DataType data = default) => default;

        public virtual ReturnType VisitDereferenceExpression(AstDereferenceExpr deref, DataType data = default) => default;
        
        public virtual ReturnType VisitDotExpression(AstDotExpr dot, DataType data = default) => default;

        public virtual ReturnType VisitEmptyExpression(AstEmptyExpr em, DataType data = default) => default;

        public virtual ReturnType VisitEnumDeclaration(AstEnumDecl en, DataType data = default) => default;

        public virtual ReturnType VisitExpressionStatement(AstExprStmt stmt, DataType data = default) => default;

        public virtual ReturnType VisitFunctionDeclaration(AstFunctionDecl function, DataType data = default) => default;

        public virtual ReturnType VisitIdentifierExpression(AstIdentifierExpr ident, DataType data = default) => default;

        public virtual ReturnType VisitIfStatement(AstIfStmt ifs, DataType data = default) => default;

        public virtual ReturnType VisitImplBlock(AstImplBlock impl, DataType data = default) => default;

        public virtual ReturnType VisitNumberExpression(AstNumberExpr num, DataType data = default) => default;

        public virtual ReturnType VisitPointerTypeExpr(AstPointerTypeExpr astPointerTypeExpr, DataType data = default) => default;

        public virtual ReturnType VisitReturnStatement(AstReturnStmt ret, DataType data = default) => default;

        public virtual ReturnType VisitStringLiteral(AstStringLiteral str, DataType data = default) => default;

        public virtual ReturnType VisitStructValueExpression(AstStructValueExpr str, DataType data = default) => default;

        public virtual ReturnType VisitTypeAlias(AstTypeAliasDecl al, DataType data = default) => default;

        public virtual ReturnType VisitStructDeclaration(AstStructDecl type, DataType data = default) => default;

        public virtual ReturnType VisitUnaryExpression(AstUnaryExpr bin, DataType data = default) => default;

        public virtual ReturnType VisitUsingStatement(AstUsingStmt use, DataType data = default) => default;

        public virtual ReturnType VisitVariableDeclaration(AstVariableDecl variable, DataType data = default) => default;

        public virtual ReturnType VisitWhileStatement(AstWhileStmt ws, DataType data = default) => default;

        public virtual ReturnType VisitEmptyStatement(AstEmptyStatement em, DataType data = default) => default;

        public virtual ReturnType VisitTypeExpr(AstTypeExpr astArrayTypeExpr, DataType data = default) => default;

        public virtual ReturnType VisitDeferStatement(AstDeferStmt def, DataType data = default) => default;

        public virtual ReturnType VisitArrayExpression(AstArrayExpression arr, DataType data = default) => default;

        public virtual ReturnType VisitMatchStatement(AstMatchStmt match, DataType data = default) => default;

        public virtual ReturnType VisitNullExpression(AstNullExpr nul, DataType data = default) => default;

        public virtual ReturnType VisitBreakStatement(AstBreakStmt br, DataType data = default) => default;

        public virtual ReturnType VisitContinueStatement(AstContinueStmt cont, DataType data = default) => default;
    }
}
