using Cheez.Compiler.Ast;
using Cheez.Compiler.ParseTree;
using Cheez.Compiler.Parsing;
using Cheez.Compiler.Visitor;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cheez.Compiler.SemanticAnalysis
{
    #region Types

    public interface IError
    {
        void Report(IErrorHandler handler);
    }

    public interface ICondition : IError
    {
        bool Check();
    }

    public class LambdaError : IError
    {
        private Action<IErrorHandler> mAction;

        public LambdaError(Action<IErrorHandler> a)
        {
            this.mAction = a;
        }

        public void Report(IErrorHandler handler)
        {
            mAction?.Invoke(handler);
        }
    }

    public class WaitForType : ICondition
    {
        public Scope Scope { get; }
        public string TypeName { get; } = null;
        public PTTypeExpr TypeExpr { get; } = null;
        public IText Text { get; set; }

        public WaitForType(IText text, Scope scope, string typeName)
        {
            this.Scope = scope;
            this.TypeName = typeName;
            this.Text = text;
        }

        public WaitForType(IText text, Scope scope, PTTypeExpr type)
        {
            this.Scope = scope;
            this.TypeExpr = type;
            this.Text = text;
        }

        public bool Check()
        {
            if (TypeExpr != null)
            {
                return Scope.GetCheezType(TypeExpr) != null;
            }
            if (TypeName != null)
            {
                return Scope.GetCheezType(TypeName) != null;
            }

            Debug.Assert(false, "UNREACHABLE");
            return false;
        }

        public void Report(IErrorHandler handler)
        {
            handler.ReportError(Text, TypeExpr, $"Unknown type '{TypeName ?? TypeExpr.ToString()}'");
        }
    }

    public class WaitForFunction : ICondition
    {
        public Scope Scope { get; }
        public string FunctionName { get; }
        public ILocation Node { get; set; }
        public IText Text { get; set; }

        public WaitForFunction(IText text, ILocation node, Scope scope, string funcName)
        {
            this.Scope = scope;
            this.FunctionName = funcName;
            this.Text = text;
            this.Node = node;
        }

        public bool Check()
        {
            return Scope.GetFunction(FunctionName) != null;
        }

        public void Report(IErrorHandler handler)
        {
            handler.ReportError(Text, Node, $"Unknown function '{FunctionName}'");
        }
    }

    public class WaitForVariable : ICondition
    {
        public Scope Scope { get; }
        public string VarName { get; }
        public ILocation Node { get; set; }
        public IText Text { get; set; }

        public WaitForVariable(IText text, ILocation node, Scope scope, string varNem)
        {
            this.Scope = scope;
            this.VarName = varNem;
            this.Text = text;
            this.Node = node;
        }

        public bool Check()
        {
            return Scope.GetFunction(VarName) != null;
        }

        public void Report(IErrorHandler handler)
        {
            handler.ReportError(Text, Node, $"Unknown variable '{VarName}'");
        }
    }

    public class DuplicateTypeError : IError
    {
        public ILocation Node { get; set; }
        public string TypeName { get; set; }
        public IText Text { get; set; }

        public DuplicateTypeError(IText text, ILocation node, string typeName)
        {
            this.Node = node;
            this.TypeName = typeName;
            this.Text = text;
        }

        public void Report(IErrorHandler handler)
        {
            handler.ReportError(Text, Node, $"Type '{TypeName}' is already defined");
        }
    }

    public class ArgumentAlreadyExists : IError
    {
        public AstFunctionParameter Argument { get; }
        public IText Text { get; }

        public ArgumentAlreadyExists(IText text, AstFunctionParameter arg)
        {
            this.Text = text;
            this.Argument = arg;
        }

        public void Report(IErrorHandler handler)
        {
            handler.ReportError(Text, Argument.ParseTreeNode, $"Argument with name '{Argument.Name}' already exists in current function signature");
        }
    }

    #endregion

    public class SemanticerData
    {
        public Scope Scope { get; set; }
        public IText Text { get; set; }

        public AstFunctionDecl Function { get; set; }

        public SemanticerData()
        {
        }

        public SemanticerData(Scope Scope = null, IText Text = null, AstFunctionDecl Function = null)
        {
            this.Scope = Scope;
            this.Text = Text;
            this.Function = Function;
        }

        public SemanticerData Clone(Scope Scope = null, IText Text = null, AstFunctionDecl Function = null)
        {
            return new SemanticerData
            {
                Scope = Scope ?? this.Scope,
                Text = Text ?? this.Text,
                Function = Function ?? this.Function
            };
        }
    }

    public class Semanticer : VisitorBase<IEnumerator<object>, SemanticerData>
    {
        public void DoWork(Scope globalScope, List<AstStatement> statements, IErrorHandler errorHandler)
        {
            List<IEnumerator<object>> enums = new List<IEnumerator<object>>();

            foreach (var s in statements)
            {
                var enumerator = s.Accept(this, new SemanticerData(globalScope, s.GenericParseTreeNode.SourceFile));
                enums.Add(enumerator);
            }

            List<(IEnumerator<object> enumerator, ICondition condition)> waiting = new List<(IEnumerator<object>, ICondition)>();
            List<IError> errors = new List<IError>();

            while (enums.Count > 0)
            {
                foreach (var e in enums)
                {
                    var hasNext = e.MoveNext();

                    if (hasNext && e.Current != null)
                    {
                        switch (e.Current)
                        {
                            case ICondition cond:
                                waiting.Add((e, cond));
                                break;

                            case IError err:
                                errors.Add(err);
                                break;
                        }
                    }
                }

                enums.Clear();

                waiting.RemoveAll(x =>
                {
                    if (x.condition.Check())
                    {
                        enums.Add(x.enumerator);
                        return true;
                    }
                    return false;
                });
            }

            // print errors
            foreach (var err in errors)
            {
                err.Report(errorHandler);
            }

            foreach (var (e, cond) in waiting)
            {
                cond.Report(errorHandler);
            }
        }

        #region Helper Functions

        private Scope NewScope(string name, Scope parent)
        {
            var s = new Scope(name, parent);
            //AllScopes.Add(s);
            return s;
        }

        #endregion

        #region Visitor Functions

        public override IEnumerator<object> VisitTypeDeclaration(AstTypeDecl type, SemanticerData data = null)
        {
            var scope = data.Scope;
            type.Scope = scope;

            foreach (var mem in type.Members)
            {
                mem.Type = scope.GetCheezType(mem.ParseTreeNode.Type);
                if (mem.Type == null)
                {
                    yield return new WaitForType(data.Text, scope, mem.ParseTreeNode.Type);
                    mem.Type = scope.GetCheezType(mem.ParseTreeNode.Type);
                }
            }

            scope.TypeDeclarations.Add(type);
            if (!scope.DefineType(type))
            {
                yield return new DuplicateTypeError(data.Text, type.ParseTreeNode.Name, type.Name);
            }
            else
            {
                yield break;
            }
        }

        public override IEnumerator<object> VisitFunctionDeclaration(AstFunctionDecl function, SemanticerData data = null)
        {
            var scope = data.Scope;
            scope.DefineFunction(function);
            function.Scope = scope;
            function.SubScope = NewScope($"fn {function.Name}", scope);
            var subScope = function.SubScope;

            bool returns = false;

            // return type
            if (function.ParseTreeNode.ReturnType != null)
            {
                function.ReturnType = scope.GetCheezType(function.ParseTreeNode.ReturnType);
                if (function.ReturnType == null)
                {
                    yield return new WaitForType(data.Text, scope, function.ParseTreeNode.ReturnType);
                    function.ReturnType = scope.GetCheezType(function.ParseTreeNode.ReturnType);
                }
            }
            else
            {
                function.ReturnType = CheezType.Void;
            }

            // parameters
            foreach (var p in function.Parameters)
            {
                p.Scope = function.SubScope;

                p.VarType = scope.GetCheezType(p.ParseTreeNode.Type);
                if (p.VarType == null)
                {
                    yield return new WaitForType(data.Text, scope, p.ParseTreeNode.Type);
                    p.VarType = scope.GetCheezType(p.ParseTreeNode.Type);
                }

                if (!function.SubScope.DefineVariable(p))
                {
                    yield return new ArgumentAlreadyExists(data.Text, p);
                }
            }

            if (function.HasImplementation)
            {
                var subData = data.Clone(Scope: subScope, Function: function);
                foreach (var s in function.Statements)
                {
                    var enumerator = s.Accept(this, subData);

                    while (enumerator.MoveNext())
                        yield return enumerator.Current;

                    if (s.GetFlag(StmtFlags.Returns))
                    {
                        returns = true;
                    }
                }
            }

            if (function.ReturnType != CheezType.Void && !returns)
            {
                yield return new LambdaError(eh => eh.ReportError(data.Text, function.ParseTreeNode.Name, "Not all code paths return a value!"));
            }

            scope.FunctionDeclarations.Add(function);
            yield break;
        }

        public override IEnumerator<object> VisitPrintStatement(AstPrintStmt print, SemanticerData data = null)
        {
            var scope = data.Scope;
            print.Scope = scope;

            foreach (var expr in print.Expressions)
            {
                var enumerator = expr.Accept(this, data.Clone());
                while (enumerator.MoveNext())
                    yield return enumerator.Current;
            }

            yield break;
        }

        public override IEnumerator<object> VisitIfStatement(AstIfStmt ifs, SemanticerData data = null)
        {
            var scope = data.Scope;
            ifs.Scope = scope;

            bool returns = true;

            // check condition
            {
                var enu = ifs.Condition.Accept(this, data.Clone());
                while (enu.MoveNext())
                    yield return enu.Current;

                if (ifs.Condition.Type != CheezType.Bool)
                {
                    yield return new LambdaError(eh => 
                        eh.ReportError(data.Text, ifs.ParseTreeNode.Condition, $"if-statement condition must be of type 'bool', got '{ifs.Condition.Type}'"));
                }
            }

            // if case
            {
                var enu = ifs.IfCase.Accept(this, data.Clone(NewScope("if", scope)));
                while (enu.MoveNext())
                    yield return enu.Current;

                if (!ifs.IfCase.GetFlag(StmtFlags.Returns))
                    returns = false;
            }

            // else case
            if (ifs.ElseCase != null) {
                var enu = ifs.ElseCase.Accept(this, data.Clone(NewScope("else", scope)));
                while (enu.MoveNext())
                    yield return enu.Current;

                if (!ifs.ElseCase.GetFlag(StmtFlags.Returns))
                    returns = false;
            }

            if (returns)
                ifs.SetFlag(StmtFlags.Returns);

            yield break;
        }

        public override IEnumerator<object> VisitBlockStatement(AstBlockStmt block, SemanticerData data = null)
        {
            var scope = data.Scope;
            block.Scope = scope;
            block.SubScope = NewScope("{}", scope);
            
            var subData = data.Clone(Scope: block.SubScope);
            foreach (var s in block.Statements)
            {
                var enumerator = s.Accept(this, subData);

                while (enumerator.MoveNext())
                    yield return enumerator.Current;

                if (s.GetFlag(StmtFlags.Returns))
                {
                    block.SetFlag(StmtFlags.Returns);
                }
            }

            yield break;
        }

        public override IEnumerator<object> VisitReturnStatement(AstReturnStmt ret, SemanticerData data = null)
        {
            var scope = data.Scope;
            ret.Scope = scope;

            ret.SetFlag(StmtFlags.Returns);

            if (ret.ReturnValue != null)
            {
                var enu = ret.ReturnValue.Accept(this, data.Clone());
                while (enu.MoveNext())
                    yield return enu.Current;
            }

            Debug.Assert(data.Function != null, "return statement is only allowed in functions");
            if (data.Function.ReturnType != CheezType.Void && ret.ReturnValue == null) // !void, return
            {
                yield return new LambdaError(eh => eh.ReportError(data.Text, ret.ParseTreeNode, $"Missing return value in non-void function {data.Function.Name}"));
            }
            else if (data.Function.ReturnType == CheezType.Void && ret.ReturnValue != null) // void, return some
            {
                yield return new LambdaError(eh => eh.ReportError(data.Text, ret.ParseTreeNode, $"Can't return value of type '{ ret.ReturnValue.Type }' in void function"));
            }
            else if (data.Function.ReturnType != CheezType.Void && ret.ReturnValue != null) // !void, return some
            {
                // compare types
                if (ret.ReturnValue.Type == IntType.LiteralType && (data.Function.ReturnType is IntType || data.Function.ReturnType is FloatType))
                {
                    ret.ReturnValue.Type = data.Function.ReturnType;
                }
                else if (ret.ReturnValue.Type == FloatType.LiteralType && data.Function.ReturnType is FloatType)
                {
                    ret.ReturnValue.Type = data.Function.ReturnType;
                }
                else if (ret.ReturnValue.Type != data.Function.ReturnType)
                {
                    yield return new LambdaError(eh => eh.ReportError(data.Text, ret.ParseTreeNode.ReturnValue, $"Can't return value of type '{ret.ReturnValue.Type}' in function with return type '{data.Function.ReturnType}'"));
                }
            }

            yield break;
        }

        public override IEnumerator<object> VisitVariableDeclaration(AstVariableDecl variable, SemanticerData data = null)
        {
            var scope = data.Scope;
            scope.VariableDeclarations.Add(variable);
            variable.Scope = scope;
            variable.SubScope = NewScope($"var {variable.Name}", scope);

            if (variable.ParseTreeNode.Type != null)
            {
                variable.VarType = scope.GetCheezType(variable.ParseTreeNode.Type);
                if (variable.VarType == null)
                {
                    yield return new WaitForType(data.Text, scope, variable.ParseTreeNode.Type);
                    variable.VarType = scope.GetCheezType(variable.ParseTreeNode.Type);
                }
            }

            if (variable.Initializer != null)
            {
                var enu = variable.Initializer.Accept(this, data.Clone());
                while (enu.MoveNext())
                    yield return enu.Current;

                if (variable.VarType == null)
                {
                    if (variable.Initializer.Type == IntType.LiteralType)
                    {
                        variable.Initializer.Type = IntType.DefaultType;
                    }
                    variable.VarType = variable.Initializer.Type;
                }
                else
                {
                    if (variable.Initializer.Type == IntType.LiteralType && (variable.VarType is IntType || variable.VarType is FloatType))
                    {
                        variable.Initializer.Type = variable.VarType;
                    }
                    else if (variable.Initializer.Type == FloatType.LiteralType && variable.VarType is FloatType)
                    {
                        variable.Initializer.Type = variable.VarType;
                    }
                    else if (variable.Initializer.Type != variable.VarType)
                    {
                        yield return new LambdaError(eh => eh.ReportError(data.Text, variable.ParseTreeNode.Initializer, $"Can't assign value of type '{variable.Initializer.Type}' to '{variable.VarType}'"));
                    }
                }
            }

            if (!scope.DefineVariable(variable))
            {
                // @Note: This should probably never happen, except for global variables, which are not implemented yet
                yield return new LambdaError(eh => eh.ReportError(data.Text, variable.ParseTreeNode.Name, $"A variable with name '{variable.Name}' already exists in current scope"));
            }

            data.Scope = variable.SubScope;
            yield break;
        }

        public override IEnumerator<object> VisitExpressionStatement(AstExprStmt stmt, SemanticerData data = null)
        {
            stmt.Scope = data.Scope;
            var enu = stmt.Expr.Accept(this, data.Clone());
            while (enu.MoveNext())
                yield return enu.Current;
            yield break;
        }

        #endregion

        #region Expressions

        public override IEnumerator<object> VisitCallExpression(AstCallExpr call, SemanticerData data = null)
        {
            var scope = data.Scope;
            call.Scope = scope;

            if (call.Function is AstIdentifierExpr f)
            {
                var func = scope.GetFunction(f.Name);
                if (func == null)
                {
                    yield return new WaitForFunction(data.Text, call.ParseTreeNode, scope, f.Name);
                    func = scope.GetFunction(f.Name);
                }

                call.Type = func.ReturnType;
            }
            else
            {
                var enu = call.Function.Accept(this, data);
                while (enu.MoveNext())
                    yield return enu.Current;

                
            }

            yield break;
        }

        public override IEnumerator<object> VisitIdentifierExpression(AstIdentifierExpr ident, SemanticerData data = null)
        {
            var scope = data.Scope;
            ident.Scope = scope;

            var v = scope.GetVariable(ident.Name);
            if (v == null)
            {
                yield return new WaitForVariable(data.Text, ident.ParseTreeNode, scope, ident.Name);
                v = scope.GetVariable(ident.Name);
            }

            ident.Type = v.VarType;

            yield break;
        }

        public override IEnumerator<object> VisitNumberExpression(AstNumberExpr num, SemanticerData data = null)
        {
            num.Type = IntType.LiteralType;
            yield break;
        }

        public override IEnumerator<object> VisitStringLiteral(AstStringLiteral str, SemanticerData data = null)
        {
            str.Type = CheezType.String;
            yield break;
        }

        public override IEnumerator<object> VisitBoolExpression(AstBoolExpr bo, SemanticerData data = null)
        {
            bo.Type = CheezType.Bool;
            yield break;
        }

        #endregion
    }
}
