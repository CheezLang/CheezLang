using Cheez.Ast.Expressions;
using Cheez.Ast.Expressions.Types;
using Cheez.Ast.Statements;

namespace Cheez.Visitors
{
    public interface IVisitorAcceptor
    {
        T Accept<T, D>(IVisitor<T, D> visitor, D data = default);
    }

    public interface IVisitor<ReturnType, DataType>
    {
        // statements
        ReturnType VisitAssignmentStmt(AstAssignment stmt, DataType data = default);
        ReturnType VisitExpressionStmt(AstExprStmt stmt, DataType data = default);
        ReturnType VisitWhileStmt(AstWhileStmt stmt, DataType data = default);
        ReturnType VisitDirectiveStmt(AstDirectiveStatement stmt, DataType data = default);
        ReturnType VisitDeferStmt(AstDeferStmt stmt, DataType data = default);
        ReturnType VisitMatchStmt(AstMatchStmt stmt, DataType data = default);
        ReturnType VisitBreakStmt(AstBreakStmt stmt, DataType data = default);
        ReturnType VisitContinueStmt(AstContinueStmt stmt, DataType data = default);
        ReturnType VisitReturnStmt(AstReturnStmt stmt, DataType data = default);
        ReturnType VisitUsingStmt(AstUsingStmt stmt, DataType data = default);

        ReturnType VisitEmptyStmt(AstEmptyStatement stmt, DataType data = default);

        // declarations
        ReturnType VisitFunctionDecl(AstFunctionDecl decl, DataType data = default);
        ReturnType VisitVariableDecl(AstVariableDecl decl, DataType data = default);
        ReturnType VisitStructDecl(AstStructDecl decl, DataType data = default);
        ReturnType VisitEnumDecl(AstEnumDecl decl, DataType data = default);
        ReturnType VisitImplDecl(AstImplBlock decl, DataType data = default);
        ReturnType VisitTypeAliasDecl(AstTypeAliasDecl decl, DataType data = default);
        ReturnType VisitTraitDecl(AstTraitDeclaration decl, DataType data = default);

        // expressions
        ReturnType VisitBlockExpr(AstBlockExpr stmt, DataType data = default);
        ReturnType VisitIfExpr(AstIfExpr stmt, DataType data = default);
        ReturnType VisitIdExpr(AstIdExpr expr, DataType data = default);
        ReturnType VisitStringLiteralExpr(AstStringLiteral expr, DataType data = default);
        ReturnType VisitCharLiteralExpr(AstCharLiteral expr, DataType data = default);
        ReturnType VisitNumberExpr(AstNumberExpr expr, DataType data = default);
        ReturnType VisitDotExpr(AstDotExpr expr, DataType data = default);
        ReturnType VisitCallExpr(AstCallExpr expr, DataType data = default);
        ReturnType VisitCompCallExpr(AstCompCallExpr expr, DataType data = default);
        ReturnType VisitBinaryExpr(AstBinaryExpr expr, DataType data = default);
        ReturnType VisitUnaryExpr(AstUnaryExpr expr, DataType data = default);
        ReturnType VisitBoolExpr(AstBoolExpr expr, DataType data = default);
        ReturnType VisitAddressOfExpr(AstAddressOfExpr expr, DataType data = default);
        ReturnType VisitDerefExpr(AstDereferenceExpr expr, DataType data = default);
        ReturnType VisitCastExpr(AstCastExpr expr, DataType data = default);
        ReturnType VisitArrayAccessExpr(AstArrayAccessExpr expr, DataType data = default);
        ReturnType VisitStructValueExpr(AstStructValueExpr expr, DataType data = default);
        ReturnType VisitArrayExpr(AstArrayExpr expr, DataType data = default);
        ReturnType VisitNullExpr(AstNullExpr expr, DataType data = default);
        ReturnType VisitTupleExpr(AstTupleExpr expr, DataType data = default);
        ReturnType VisitArgumentExpr(AstArgument expr, DataType data = default);

        ReturnType VisitEmptyExpression(AstEmptyExpr expr, DataType data = default);

        // type expressions
        ReturnType VisitReferenceTypeExpr(AstReferenceTypeExpr type, DataType data = default);
        ReturnType VisitSliceTypeExpr(AstSliceTypeExpr type, DataType data = default);
        ReturnType VisitArrayTypeExpr(AstArrayTypeExpr type, DataType data = default);
        ReturnType VisitFunctionTypeExpr(AstFunctionTypeExpr type, DataType data = default);
        ReturnType VisitErrorTypeExpression(AstErrorTypeExpr type, DataType data = default);

        // special expressions
        ReturnType VisitVariableRef(AstVariableRef re, DataType data = default);
        ReturnType VisitTypeExpr(AstTypeRef re, DataType data = default);
        ReturnType VisitTempVarExpr(AstTempVarExpr te, DataType data = default);
        ReturnType VisitSymbolExpr(AstSymbolExpr te, DataType data = default);
        ReturnType VisitUfcFuncExpr(AstUfcFuncExpr expr, DataType data = default);
        

        // other
        ReturnType VisitParameter(AstParameter param, DataType data = default);
    }


    public abstract class VisitorBase<ReturnType, DataType> : IVisitor<ReturnType, DataType>
    {
        // statements
        public virtual ReturnType VisitDirectiveStmt(AstDirectiveStatement stmt, DataType data = default) => default;
        public virtual ReturnType VisitAssignmentStmt(AstAssignment stmt, DataType data = default) => default;
        public virtual ReturnType VisitBreakStmt(AstBreakStmt stmt, DataType data = default) => default;
        public virtual ReturnType VisitContinueStmt(AstContinueStmt stmt, DataType data = default) => default;
        public virtual ReturnType VisitExpressionStmt(AstExprStmt stmt, DataType data = default) => default;
        public virtual ReturnType VisitMatchStmt(AstMatchStmt stmt, DataType data = default) => default;
        public virtual ReturnType VisitDeferStmt(AstDeferStmt stmt, DataType data = default) => default;
        public virtual ReturnType VisitReturnStmt(AstReturnStmt stmt, DataType data = default) => default;
        public virtual ReturnType VisitUsingStmt(AstUsingStmt stmt, DataType data = default) => default;
        public virtual ReturnType VisitWhileStmt(AstWhileStmt stmt, DataType data = default) => default;

        public virtual ReturnType VisitEmptyStmt(AstEmptyStatement stmt, DataType data = default) => default;

        // declarations
        public virtual ReturnType VisitTypeAliasDecl(AstTypeAliasDecl decl, DataType data = default) => default;
        public virtual ReturnType VisitEnumDecl(AstEnumDecl decl, DataType data = default) => default;
        public virtual ReturnType VisitTraitDecl(AstTraitDeclaration decl, DataType data = default) => default;
        public virtual ReturnType VisitFunctionDecl(AstFunctionDecl decl, DataType data = default) => default;
        public virtual ReturnType VisitStructDecl(AstStructDecl decl, DataType data = default) => default;
        public virtual ReturnType VisitVariableDecl(AstVariableDecl decl, DataType data = default) => default;
        public virtual ReturnType VisitImplDecl(AstImplBlock decl, DataType data = default) => default;

        // expressions
        public virtual ReturnType VisitBlockExpr(AstBlockExpr stmt, DataType data = default) => default;
        public virtual ReturnType VisitIfExpr(AstIfExpr stmt, DataType data = default) => default;
        public virtual ReturnType VisitStringLiteralExpr(AstStringLiteral expr, DataType data = default) => default;
        public virtual ReturnType VisitCharLiteralExpr(AstCharLiteral expr, DataType data = default) => default;
        public virtual ReturnType VisitStructValueExpr(AstStructValueExpr expr, DataType data = default) => default;
        public virtual ReturnType VisitUnaryExpr(AstUnaryExpr expr, DataType data = default) => default;
        public virtual ReturnType VisitArrayExpr(AstArrayExpr expr, DataType data = default) => default;
        public virtual ReturnType VisitAddressOfExpr(AstAddressOfExpr expr, DataType data = default) => default;
        public virtual ReturnType VisitArrayAccessExpr(AstArrayAccessExpr expr, DataType data = default) => default;
        public virtual ReturnType VisitBinaryExpr(AstBinaryExpr bexprin, DataType data = default) => default;
        public virtual ReturnType VisitBoolExpr(AstBoolExpr expr, DataType data = default) => default;
        public virtual ReturnType VisitCallExpr(AstCallExpr expr, DataType data = default) => default;
        public virtual ReturnType VisitCastExpr(AstCastExpr expr, DataType data = default) => default;
        public virtual ReturnType VisitCompCallExpr(AstCompCallExpr expr, DataType data = default) => default;
        public virtual ReturnType VisitDerefExpr(AstDereferenceExpr expr, DataType data = default) => default;
        public virtual ReturnType VisitDotExpr(AstDotExpr expr, DataType data = default) => default;
        public virtual ReturnType VisitIdExpr(AstIdExpr expr, DataType data = default) => default;
        public virtual ReturnType VisitNullExpr(AstNullExpr expr, DataType data = default) => default;
        public virtual ReturnType VisitNumberExpr(AstNumberExpr expr, DataType data = default) => default;
        public virtual ReturnType VisitTupleExpr(AstTupleExpr expr, DataType data = default) => default;

        public virtual ReturnType VisitEmptyExpression(AstEmptyExpr expr, DataType data = default) => default;

        // type expressions
        public virtual ReturnType VisitReferenceTypeExpr(AstReferenceTypeExpr type, DataType data = default) => default;
        public virtual ReturnType VisitSliceTypeExpr(AstSliceTypeExpr type, DataType data = default) => default;
        public virtual ReturnType VisitArrayTypeExpr(AstArrayTypeExpr type, DataType data = default) => default;
        public virtual ReturnType VisitFunctionTypeExpr(AstFunctionTypeExpr type, DataType data = default) => default;
        public virtual ReturnType VisitErrorTypeExpression(AstErrorTypeExpr type, DataType data = default) => default;
        public virtual ReturnType VisitArgumentExpr(AstArgument expr, DataType data = default) => default;

        // special
        public virtual ReturnType VisitTypeExpr(AstTypeRef re, DataType data = default) => default;
        public virtual ReturnType VisitVariableRef(AstVariableRef re, DataType data = default) => default;
        public virtual ReturnType VisitTempVarExpr(AstTempVarExpr te, DataType data = default) => default;
        public virtual ReturnType VisitSymbolExpr(AstSymbolExpr te, DataType data = default) => default;
        public virtual ReturnType VisitUfcFuncExpr(AstUfcFuncExpr expr, DataType data = default) => default;

        // other
        public virtual ReturnType VisitParameter(AstParameter param, DataType data = default) => default;
    }
}
