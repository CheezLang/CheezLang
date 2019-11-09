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
        ReturnType VisitForStmt(AstForStmt stmt, DataType data = default);
        ReturnType VisitDirectiveStmt(AstDirectiveStatement stmt, DataType data = default);
        ReturnType VisitDeferStmt(AstDeferStmt stmt, DataType data = default);
        ReturnType VisitReturnStmt(AstReturnStmt stmt, DataType data = default);
        ReturnType VisitUsingStmt(AstUsingStmt stmt, DataType data = default);

        ReturnType VisitEmptyStmt(AstEmptyStatement stmt, DataType data = default);

        // declarations
        ReturnType VisitConstantDeclaration(AstConstantDeclaration decl, DataType data = default);
        ReturnType VisitVariableDecl(AstVariableDecl decl, DataType data = default);
        ReturnType VisitImplDecl(AstImplBlock decl, DataType data = default);
        ReturnType VisitTraitDecl(AstTraitDeclaration decl, DataType data = default);

        // expressions
        ReturnType VisitRangeExpr(AstRangeExpr expr, DataType data = default);
        ReturnType VisitBreakExpr(AstBreakExpr expr, DataType data = default);
        ReturnType VisitContinueExpr(AstContinueExpr expr, DataType data = default);
        ReturnType VisitLambdaExpr(AstLambdaExpr expr, DataType data = default);
        ReturnType VisitBlockExpr(AstBlockExpr expr, DataType data = default);
        ReturnType VisitIfExpr(AstIfExpr expr, DataType data = default);
        ReturnType VisitIdExpr(AstIdExpr expr, DataType data = default);
        ReturnType VisitStringLiteralExpr(AstStringLiteral expr, DataType data = default);
        ReturnType VisitCharLiteralExpr(AstCharLiteral expr, DataType data = default);
        ReturnType VisitNumberExpr(AstNumberExpr expr, DataType data = default);
        ReturnType VisitDotExpr(AstDotExpr expr, DataType data = default);
        ReturnType VisitCallExpr(AstCallExpr expr, DataType data = default);
        ReturnType VisitCompCallExpr(AstCompCallExpr expr, DataType data = default);
        ReturnType VisitNaryOpExpr(AstNaryOpExpr expr, DataType data = default);
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
        ReturnType VisitDefaultExpr(AstDefaultExpr expr, DataType data = default);
        ReturnType VisitMatchExpr(AstMatchExpr expr, DataType data = default);

        ReturnType VisitEmptyExpression(AstEmptyExpr expr, DataType data = default);

        // type expressions
        ReturnType VisitImplTraitTypeExpr(AstImplTraitTypeExpr type, DataType data = default);
        ReturnType VisitReferenceTypeExpr(AstReferenceTypeExpr type, DataType data = default);
        ReturnType VisitSliceTypeExpr(AstSliceTypeExpr type, DataType data = default);
        ReturnType VisitArrayTypeExpr(AstArrayTypeExpr type, DataType data = default);
        ReturnType VisitFunctionTypeExpr(AstFunctionTypeExpr type, DataType data = default);
        ReturnType VisitStructTypeExpr(AstStructTypeExpr expr, DataType data = default);
        ReturnType VisitEnumTypeExpr(AstEnumTypeExpr expr, DataType data = default);
        ReturnType VisitTraitTypeExpr(AstTraitTypeExpr expr, DataType data = default);
        ReturnType VisitFuncExpr(AstFuncExpr expr, DataType data = default);

        // special expressions
        ReturnType VisitVariableRef(AstVariableRef expr, DataType data = default);
        ReturnType VisitTypeExpr(AstTypeRef expr, DataType data = default);
        ReturnType VisitTempVarExpr(AstTempVarExpr expr, DataType data = default);
        ReturnType VisitSymbolExpr(AstSymbolExpr expr, DataType data = default);
        ReturnType VisitUfcFuncExpr(AstUfcFuncExpr expr, DataType data = default);
        ReturnType VisitEnumValueExpr(AstEnumValueExpr expr, DataType data = default);


        // other
        ReturnType VisitParameter(AstParameter param, DataType data = default);
    }


    public abstract class VisitorBase<ReturnType, DataType> : IVisitor<ReturnType, DataType>
    {
        // statements
        public virtual ReturnType VisitDirectiveStmt(AstDirectiveStatement stmt, DataType data = default) => default;
        public virtual ReturnType VisitAssignmentStmt(AstAssignment stmt, DataType data = default) => default;
        public virtual ReturnType VisitExpressionStmt(AstExprStmt stmt, DataType data = default) => default;
        public virtual ReturnType VisitDeferStmt(AstDeferStmt stmt, DataType data = default) => default;
        public virtual ReturnType VisitReturnStmt(AstReturnStmt stmt, DataType data = default) => default;
        public virtual ReturnType VisitUsingStmt(AstUsingStmt stmt, DataType data = default) => default;
        public virtual ReturnType VisitWhileStmt(AstWhileStmt stmt, DataType data = default) => default;
        public virtual ReturnType VisitForStmt(AstForStmt stmt, DataType data = default) => default;

        public virtual ReturnType VisitEmptyStmt(AstEmptyStatement stmt, DataType data = default) => default;

        // declarations
        public virtual ReturnType VisitConstantDeclaration(AstConstantDeclaration decl, DataType data = default) => default;
        public virtual ReturnType VisitTraitDecl(AstTraitDeclaration decl, DataType data = default) => default;
        public virtual ReturnType VisitVariableDecl(AstVariableDecl decl, DataType data = default) => default;
        public virtual ReturnType VisitImplDecl(AstImplBlock decl, DataType data = default) => default;

        // expressions
        public virtual ReturnType VisitRangeExpr(AstRangeExpr expr, DataType data = default) => default;
        public virtual ReturnType VisitBreakExpr(AstBreakExpr stmt, DataType data = default) => default;
        public virtual ReturnType VisitContinueExpr(AstContinueExpr stmt, DataType data = default) => default;
        public virtual ReturnType VisitLambdaExpr(AstLambdaExpr expr, DataType data = default) => default;
        public virtual ReturnType VisitBlockExpr(AstBlockExpr expr, DataType data = default) => default;
        public virtual ReturnType VisitIfExpr(AstIfExpr expr, DataType data = default) => default;
        public virtual ReturnType VisitStringLiteralExpr(AstStringLiteral expr, DataType data = default) => default;
        public virtual ReturnType VisitCharLiteralExpr(AstCharLiteral expr, DataType data = default) => default;
        public virtual ReturnType VisitStructValueExpr(AstStructValueExpr expr, DataType data = default) => default;
        public virtual ReturnType VisitNaryOpExpr(AstNaryOpExpr expr, DataType data = default) => default;
        public virtual ReturnType VisitBinaryExpr(AstBinaryExpr expr, DataType data = default) => default;
        public virtual ReturnType VisitUnaryExpr(AstUnaryExpr expr, DataType data = default) => default;
        public virtual ReturnType VisitArrayExpr(AstArrayExpr expr, DataType data = default) => default;
        public virtual ReturnType VisitAddressOfExpr(AstAddressOfExpr expr, DataType data = default) => default;
        public virtual ReturnType VisitArrayAccessExpr(AstArrayAccessExpr expr, DataType data = default) => default;
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
        public virtual ReturnType VisitDefaultExpr(AstDefaultExpr expr, DataType data = default) => default;
        public virtual ReturnType VisitMatchExpr(AstMatchExpr expr, DataType data = default) => default;

        public virtual ReturnType VisitEmptyExpression(AstEmptyExpr expr, DataType data = default) => default;

        // type expressions
        public virtual ReturnType VisitImplTraitTypeExpr(AstImplTraitTypeExpr type, DataType data = default) => default;
        public virtual ReturnType VisitReferenceTypeExpr(AstReferenceTypeExpr type, DataType data = default) => default;
        public virtual ReturnType VisitSliceTypeExpr(AstSliceTypeExpr type, DataType data = default) => default;
        public virtual ReturnType VisitArrayTypeExpr(AstArrayTypeExpr type, DataType data = default) => default;
        public virtual ReturnType VisitFunctionTypeExpr(AstFunctionTypeExpr type, DataType data = default) => default;
        public virtual ReturnType VisitStructTypeExpr(AstStructTypeExpr expr, DataType data = default) => default;
        public virtual ReturnType VisitEnumTypeExpr(AstEnumTypeExpr expr, DataType data = default) => default;
        public virtual ReturnType VisitTraitTypeExpr(AstTraitTypeExpr expr, DataType data = default) => default;
        public virtual ReturnType VisitFuncExpr(AstFuncExpr expr, DataType data = default) => default;

        // special
        public virtual ReturnType VisitArgumentExpr(AstArgument expr, DataType data = default) => default;
        public virtual ReturnType VisitTypeExpr(AstTypeRef expr, DataType data = default) => default;
        public virtual ReturnType VisitVariableRef(AstVariableRef expr, DataType data = default) => default;
        public virtual ReturnType VisitTempVarExpr(AstTempVarExpr expr, DataType data = default) => default;
        public virtual ReturnType VisitSymbolExpr(AstSymbolExpr expr, DataType data = default) => default;
        public virtual ReturnType VisitUfcFuncExpr(AstUfcFuncExpr expr, DataType data = default) => default;
        public virtual ReturnType VisitEnumValueExpr(AstEnumValueExpr expr, DataType data = default) => default;

        // other
        public virtual ReturnType VisitParameter(AstParameter param, DataType data = default) => default;
    }
}
