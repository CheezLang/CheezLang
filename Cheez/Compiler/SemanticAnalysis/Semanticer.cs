using Cheez.Compiler.Ast;
using Cheez.Compiler.ParseTree;
using Cheez.Compiler.Parsing;
using Cheez.Compiler.Visitor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

    public class CompileStatement
    {
        public SemanticerData Data { get; }
        public AstStatement Stmt { get; }

        public IEnumerator<object> Enumerator { get; }

        public CompileStatement(IEnumerable<object> en)
        {
            Enumerator = en.GetEnumerator();
        }

        //public CompileStatement(AstStatement s, SemanticerData dat)
        //{
        //    Stmt = s;
        //    Data = dat;
        //}
    }

    public class LambdaCondition : ICondition
    {
        private Func<bool> mCondition;
        private Action<IErrorHandler> mAction;

        public LambdaCondition(Func<bool> cond, Action<IErrorHandler> act)
        {
            this.mCondition = cond;
            this.mAction = act;
        }

        public bool Check()
        {
            return mCondition?.Invoke() ?? false;
        }

        public void Report(IErrorHandler handler)
        {
            mAction?.Invoke(handler);
        }
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

    public class GenericError : IError
    {
        private IText text;
        private ILocation loc;
        private string message;

        public GenericError(IText text, ILocation loc, string message)
        {
            this.text = text;
            this.loc = loc;
            this.message = message;
        }

        public void Report(IErrorHandler handler)
        {
            handler.ReportError(text, loc, message);
        }
    }

    public class WaitForType : ICondition
    {
        public Scope Scope { get; }
        public string TypeName { get; } = null;
        public PTTypeExpr TypeExpr { get; } = null;
        public IText Text { get; set; }
        public bool OnlyWaitForDefinition { get; set; }

        public WaitForType(IText text, Scope scope, string typeName, bool onlyWaitForDefinition = false)
        {
            this.Scope = scope;
            this.TypeName = typeName;
            this.Text = text;
        }

        public WaitForType(IText text, Scope scope, PTTypeExpr type, bool onlyWaitForDefinition = false)
        {
            this.Scope = scope;
            this.TypeExpr = type;
            this.Text = text;
        }

        public bool Check()
        {
            if (TypeExpr != null)
            {
                var t = Scope.GetCheezType(TypeExpr);
                if (t == null)
                    return false;
                if (OnlyWaitForDefinition)
                    return true;
                if (t is StructType s && !s.Analyzed)
                    return false;
                return true;
            }
            if (TypeName != null)
            {
                var t = Scope.GetCheezType(TypeName);
                if (t == null)
                    return false;
                if (OnlyWaitForDefinition)
                    return true;
                if (t is StructType s && !s.Analyzed)
                    return false;
                return true;
            }

            Debug.Assert(false, "UNREACHABLE");
            return false;
        }

        public void Report(IErrorHandler handler)
        {
            handler.ReportError(Text, TypeExpr, $"Unknown type '{TypeName ?? TypeExpr.ToString()}'");
        }
    }

    public class WaitForSymbol : ICondition
    {
        public Scope Scope { get; }
        public string SymbolName { get; }
        public ILocation Node { get; set; }
        public IText Text { get; set; }

        public WaitForSymbol(IText text, ILocation node, Scope scope, string varNem)
        {
            this.Scope = scope;
            this.SymbolName = varNem;
            this.Text = text;
            this.Node = node;
        }

        public bool Check()
        {
            return Scope.GetSymbol(SymbolName) != null;
        }

        public void Report(IErrorHandler handler)
        {
            handler.ReportError(Text, Node, $"Unknown symbol '{SymbolName}'");
        }
    }

    public class ReplaceAstExpr
    {
        public AstExpression NewExpression { get; set; }

        public ReplaceAstExpr(AstExpression newExpr)
        {
            NewExpression = newExpr;
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
        public CheezType ImplTarget { get; set; }

        [DebuggerStepThrough]
        public SemanticerData()
        {
        }

        [DebuggerStepThrough]
        public SemanticerData(Scope Scope = null, IText Text = null, AstFunctionDecl Function = null, CheezType Impl = null)
        {
            this.Scope = Scope;
            this.Text = Text;
            this.Function = Function;
            this.ImplTarget = Impl;
        }

        [DebuggerStepThrough]
        public SemanticerData Clone(Scope Scope = null, IText Text = null, AstFunctionDecl Function = null, CheezType Impl = null)
        {
            return new SemanticerData
            {
                Scope = Scope ?? this.Scope,
                Text = Text ?? this.Text,
                Function = Function ?? this.Function,
                ImplTarget = Impl ?? this.ImplTarget
            };
        }
    }

    public class Semanticer : VisitorBase<IEnumerable<object>, SemanticerData>
    {
        private Workspace workspace;

        public void DoWork(Workspace workspace, List<AstStatement> statements, IErrorHandler errorHandler)
        {
            this.workspace = workspace;
            List<IEnumerator<object>> enums = new List<IEnumerator<object>>();

            foreach (var s in statements)
            {
                var enumerator = s.Accept(this, new SemanticerData(workspace.GlobalScope, s.GenericParseTreeNode.SourceFile)).GetEnumerator();
                enums.Add(enumerator);
            }

            List<(IEnumerator<object> enumerator, ICondition condition)> waiting = new List<(IEnumerator<object>, ICondition)>();
            List<IError> errors = new List<IError>();

            var TrueCondition = new LambdaCondition(() => true, null);

            while (enums.Count > 0)
            {
                foreach (var e in enums)
                {
                    bool cont;
                    do
                    {
                        cont = false;
                        var hasNext = e.MoveNext();

                        if (hasNext && e.Current != null)
                        {
                            switch (e.Current)
                            {
                                case ICondition cond:
                                    if (cond.Check())
                                        cont = true;
                                    else
                                        waiting.Add((e, cond));
                                    break;

                                case IError err:
                                    errors.Add(err);
                                    break;

                                case CompileStatement cs:
                                    waiting.Add((cs.Enumerator, TrueCondition));
                                    cont = true;
                                    break;
                            }
                        }
                    } while (cont);
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

        #region Functions

        public override IEnumerable<object> VisitEnumDeclaration(AstEnumDecl en, SemanticerData data = null)
        {
            var scope = data.Scope;
            en.Scope = scope;


            // check if all names in this enum are unique
            var names = new HashSet<string>();
            foreach (var m in en.Members)
            {
                if (names.Contains(m.Name))
                {
                    yield return new GenericError(data.Text, m.ParseTreeNode.Name, $"A member with name '{m.Name}' already exists in enum '{en.Name}'");
                }

                names.Add(m.Name);
            }

            var t = scope.DefineType(en);
            if (t == null)
            {
                yield return new GenericError(data.Text, en.ParseTreeNode.Name, $"A type with name '{en.Name}' already exists in current scope");
            }

            scope.TypeDeclarations.Add(en);
        }

        public override IEnumerable<object> VisitTypeDeclaration(AstTypeDecl type, SemanticerData data = null)
        {
            var scope = data.Scope;
            type.Scope = scope;
            
            scope.TypeDeclarations.Add(type);
            if (!scope.DefineType(type))
            {
                yield return new DuplicateTypeError(data.Text, type.ParseTreeNode.Name, type.Name);
            }

            foreach (var mem in type.Members)
            {
                yield return new WaitForType(data.Text, scope, mem.ParseTreeNode.Type, true);
                mem.Type = scope.GetCheezType(mem.ParseTreeNode.Type);
            }

            var st = scope.GetCheezType(type.Name) as StructType;
            Debug.Assert(st != null && st.Declaration == type);
            st.Analyzed = true;
        }

        public void Using(Scope scope, StructType @struct, INamed name)
        {
            foreach (var member in @struct.Declaration.Members)
            {
                scope.DefineSymbol(new Using(member.Name, new AstDotExpr(null, new AstIdentifierExpr(null, name.Name), member.Name, false)));
            }
        }

        public void Using(Scope scope, StructType @struct, AstExpression sub)
        {
            foreach (var member in @struct.Declaration.Members)
            {
                scope.DefineSymbol(new Using(member.Name, new AstDotExpr(null, sub, member.Name, false)));
            }
        }

        public override IEnumerable<object> VisitFunctionDeclaration(AstFunctionDecl function, SemanticerData data = null)
        {
            if (function.Name == "Main" && data.Scope == workspace.GlobalScope)
                workspace.MainFunction = function;

            var scope = data.Scope;
            function.Scope = scope;
            function.SubScope = NewScope($"fn {function.Name}", scope);
            var subScope = function.SubScope;

            bool returns = false;

            if (data.ImplTarget != null)
            {
                var tar = data.ImplTarget;
                while (tar is PointerType p)
                    tar = p.TargetType;

                if (tar is StructType @struct)
                {
                    Using(subScope, @struct, function.Parameters[0]);
                }
            }

            // return type
            if (function.ParseTreeNode.ReturnType != null)
            {
                yield return new WaitForType(data.Text, scope, function.ParseTreeNode.ReturnType);
                function.ReturnType = scope.GetCheezType(function.ParseTreeNode.ReturnType);
            }
            else
            {
                function.ReturnType = CheezType.Void;
            }

            // parameters
            foreach (var p in function.Parameters)
            {
                p.Scope = function.SubScope;

                if (p.Type == null)
                {
                    yield return new WaitForType(data.Text, scope, p.ParseTreeNode.Type);
                    p.Type = scope.GetCheezType(p.ParseTreeNode.Type);
                }

                if (!function.SubScope.DefineSymbol(p))
                {
                    yield return new ArgumentAlreadyExists(data.Text, p);
                }
            }

            function.Type = FunctionType.GetFunctionType(function);
            scope.FunctionDeclarations.Add(function);
            if (!scope.DefineSymbol(function))
            {
                yield return new LambdaError(eh => eh.ReportError(data.Text, function.ParseTreeNode.Name, $"A function or variable with name '{function.Name}' already exists in current scope"));
            }

            if (function.HasImplementation)
            {
                var subData = data.Clone(Scope: subScope, Function: function);
                foreach (var s in function.Statements)
                {
                    foreach (var v in s.Accept(this, subData))
                        yield return v;

                    if (s.GetFlag(StmtFlags.Returns))
                    {
                        returns = true;
                    }
                }

                if (function.ReturnType != CheezType.Void && !returns)
                {
                    yield return new LambdaError(eh => eh.ReportError(data.Text, function.ParseTreeNode.Name, "Not all code paths return a value!"));
                }
            }

            yield break;
        }

        public override IEnumerable<object> VisitPrintStatement(AstPrintStmt print, SemanticerData data = null)
        {
            var scope = data.Scope;
            print.Scope = scope;

            if (print.Separator != null)
            {
                foreach (var v in print.Separator.Accept(this, data.Clone()))
                    if (v is ReplaceAstExpr r)
                        print.Separator = r.NewExpression;
                    else
                        yield return v;
            }

            for (int i = 0; i < print.Expressions.Count; i++)
            {
                foreach (var v in print.Expressions[i].Accept(this, data.Clone()))
                    if (v is ReplaceAstExpr r)
                        print.Expressions[i] = r.NewExpression;
                    else
                        yield return v;
            }

            yield break;
        }

        public override IEnumerable<object> VisitIfStatement(AstIfStmt ifs, SemanticerData data = null)
        {
            var scope = data.Scope;
            ifs.Scope = scope;

            bool returns = true;

            // check condition
            {
                foreach (var v in ifs.Condition.Accept(this, data.Clone()))
                    if (v is ReplaceAstExpr r)
                        ifs.Condition = r.NewExpression;
                    else
                        yield return v;

                if (ifs.Condition.Type != CheezType.Bool)
                {
                    yield return new LambdaError(eh =>
                        eh.ReportError(data.Text, ifs.ParseTreeNode.Condition, $"if-statement condition must be of type 'bool', got '{ifs.Condition.Type}'"));
                }
            }

            // if case
            {
                foreach (var v in ifs.IfCase.Accept(this, data.Clone(NewScope("if", scope))))
                    yield return v;

                if (!ifs.IfCase.GetFlag(StmtFlags.Returns))
                    returns = false;
            }

            // else case
            if (ifs.ElseCase != null)
            {
                foreach (var v in ifs.ElseCase.Accept(this, data.Clone(NewScope("else", scope))))
                    yield return v;

                if (!ifs.ElseCase.GetFlag(StmtFlags.Returns))
                    returns = false;
            }

            if (returns)
                ifs.SetFlag(StmtFlags.Returns);

            yield break;
        }

        public override IEnumerable<object> VisitBlockStatement(AstBlockStmt block, SemanticerData data = null)
        {
            var scope = data.Scope;
            block.Scope = scope;
            block.SubScope = NewScope("{}", scope);

            var subData = data.Clone(Scope: block.SubScope);
            foreach (var s in block.Statements)
            {
                foreach (var v in s.Accept(this, subData))
                    yield return v;

                if (s.GetFlag(StmtFlags.Returns))
                {
                    block.SetFlag(StmtFlags.Returns);
                }
            }

            yield break;
        }

        public override IEnumerable<object> VisitReturnStatement(AstReturnStmt ret, SemanticerData data = null)
        {
            var scope = data.Scope;
            ret.Scope = scope;

            ret.SetFlag(StmtFlags.Returns);

            if (ret.ReturnValue != null)
            {
                foreach (var v in ret.ReturnValue.Accept(this, data.Clone()))
                    if (v is ReplaceAstExpr r)
                        ret.ReturnValue = r.NewExpression;
                    else
                        yield return v;
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

        public override IEnumerable<object> VisitVariableDeclaration(AstVariableDecl variable, SemanticerData data = null)
        {
            var scope = data.Scope;
            scope.VariableDeclarations.Add(variable);
            variable.Scope = scope;
            variable.SubScope = NewScope($"var {variable.Name}", scope);

            if (variable.ParseTreeNode.Type != null)
            {
                yield return new WaitForType(data.Text, scope, variable.ParseTreeNode.Type);
                variable.Type = scope.GetCheezType(variable.ParseTreeNode.Type);
            }

            if (variable.Initializer != null)
            {
                foreach (var v in variable.Initializer.Accept(this, data.Clone()))
                    if (v is ReplaceAstExpr r)
                        variable.Initializer = r.NewExpression;
                    else
                        yield return v;

                if (variable.Type == null)
                {
                    if (variable.Initializer.Type == IntType.LiteralType)
                    {
                        variable.Initializer.Type = IntType.DefaultType;
                    }
                    variable.Type = variable.Initializer.Type;
                }
                else
                {
                    if (variable.Initializer.Type == IntType.LiteralType && (variable.Type is IntType || variable.Type is FloatType))
                    {
                        variable.Initializer.Type = variable.Type;
                    }
                    else if (variable.Initializer.Type == FloatType.LiteralType && variable.Type is FloatType)
                    {
                        variable.Initializer.Type = variable.Type;
                    }
                    else if (variable.Initializer.Type != variable.Type)
                    {
                        yield return new LambdaError(eh => eh.ReportError(data.Text, variable.ParseTreeNode.Initializer, $"Can't assign value of type '{variable.Initializer.Type}' to '{variable.Type}'"));
                    }
                }
            }

            if (!scope.DefineSymbol(variable))
            {
                // @Note: This should probably never happen, except for global variables, which are not implemented yet
                yield return new LambdaError(eh => eh.ReportError(data.Text, variable.ParseTreeNode.Name, $"A variable with name '{variable.Name}' already exists in current scope"));
            }

            data.Scope = variable.SubScope;
            yield break;
        }

        public override IEnumerable<object> VisitAssignment(AstAssignment ass, SemanticerData data = null)
        {
            var scope = data.Scope;
            ass.Scope = scope;

            // check target
            foreach (var v in ass.Target.Accept(this, data.Clone()))
                if (v is ReplaceAstExpr r)
                    ass.Target = r.NewExpression;
                else
                    yield return v;

            // check source
            foreach (var v in ass.Value.Accept(this, data.Clone()))
                if (v is ReplaceAstExpr r)
                    ass.Value = r.NewExpression;
                else
                    yield return v;

            if (!CastIfLiteral(ass.Value.Type, ass.Target.Type, out var type))
                yield return new LambdaError(eh => eh.ReportError(data.Text, ass.ParseTreeNode, $"Can't assign value of type {ass.Value.Type} to {ass.Target.Type}"));
            else if (!ass.Target.GetFlag(ExprFlags.IsLValue))
                yield return new LambdaError(eh => eh.ReportError(data.Text, ass.ParseTreeNode.Target, $"Left side of assignment has to be a lvalue"));

            yield break;
        }

        public override IEnumerable<object> VisitExpressionStatement(AstExprStmt stmt, SemanticerData data = null)
        {
            stmt.Scope = data.Scope;
            foreach (var v in stmt.Expr.Accept(this, data.Clone()))
                if (v is ReplaceAstExpr r)
                    stmt.Expr = r.NewExpression;
                else
                    yield return v;
            yield break;
        }

        public override IEnumerable<object> VisitImplBlock(AstImplBlock impl, SemanticerData data = null)
        {
            var scope = data.Scope;
            impl.Scope = scope;

            if (impl.ParseTreeNode.Trait != null)
            {
                yield return new WaitForSymbol(data.Text, impl.ParseTreeNode.Trait, scope, impl.ParseTreeNode.Trait.Name);
                impl.Trait = "TODO";
            }

            yield return new WaitForType(data.Text, impl.Scope, impl.ParseTreeNode.Target);
            impl.TargetType = scope.GetCheezType(impl.ParseTreeNode.Target);
            impl.SubScope = new Scope($"impl {impl.TargetType}", impl.Scope);


            foreach (var f in impl.Functions)
            {
                var selfType = impl.TargetType;
                if (f.RefSelf)
                    selfType = ReferenceType.GetRefType(selfType);
                f.Parameters.Insert(0, new AstFunctionParameter("self", selfType));
                //scope.DefineImplFunction(impl.TargetType, f);
            }

            foreach (var f in impl.Functions)
            {
                var cc = f.Accept(this, data.Clone(Scope: impl.SubScope, Impl: impl.TargetType))
                    .WithAction(() => scope.DefineImplFunction(impl.TargetType, f));
                yield return new CompileStatement(cc);
                //foreach (var v in f.Accept(this, data.Clone(Scope: impl.SubScope, Impl: impl.TargetType)))
                //    yield return v;
            }

            scope.ImplBlocks.Add(impl);
        }

        public override IEnumerable<object> VisitUsingStatement(AstUsingStmt use, SemanticerData data = null)
        {
            var scope = data.Scope;
            use.Scope = scope;

            foreach (var v in use.Value.Accept(this, data.Clone()))
                if (v is ReplaceAstExpr r)
                    use.Value = r.NewExpression;
                else
                    yield return v;

            if (use.Value.Type is StructType @struct)
            {
                Using(scope, @struct, use.Value);
            }
            else
            {
                yield return new LambdaError(eh => eh.ReportError(data.Text, use.GenericParseTreeNode, "Only struct types can be used in 'using' statement"));
            }


            yield break;
        }

        public override IEnumerable<object> VisitWhileStatement(AstWhileStmt ws, SemanticerData data = null)
        {
            var scope = data.Scope;
            ws.Scope = scope;

            foreach (var v in ws.Condition.Accept(this, data.Clone()))
                if (v is ReplaceAstExpr r)
                    ws.Condition = r.NewExpression;
                else
                    yield return v;

            foreach (var v in ws.Body.Accept(this, data.Clone()))
                yield return v;

            yield break;
        }

        #endregion

        #region Expressions

        public override IEnumerable<object> VisitStructValueExpression(AstStructValueExpr str, SemanticerData data = null)
        {
            var scope = data.Scope;
            str.Scope = scope;

            yield return new WaitForType(data.Text, scope, str.Name);

            str.Type = scope.GetCheezType(str.Name);

            if (str.Type is StructType s)
            {
                if (str.MemberInitializers.Length > s.Declaration.Members.Count)
                {
                    yield return new GenericError(data.Text, str.ParseTreeNode?.Name, $"Struct initialization has to many values");
                }
                else
                {
                    int namesProvided = 0;
                    foreach (var m in str.MemberInitializers)
                    {
                        if (m.Name != null)
                        {
                            if (!s.Declaration.Members.Any(m2 => m2.Name == m.Name))
                            {
                                yield return new GenericError(data.Text, m.GenericParseTreeNode.Name, $"'{m.Name}' is not a member of struct {s.Declaration.Name}");
                            }
                            namesProvided++;
                        }
                    }

                    if (namesProvided == 0)
                    {
                        for (int i = 0; i < str.MemberInitializers.Length; i++)
                        {
                            foreach (var v in str.MemberInitializers[i].Value.Accept(this, data))
                            {
                                if (v is ReplaceAstExpr r)
                                    str.MemberInitializers[i].Value = r.NewExpression;
                                else
                                    yield return v;
                            }

                            var mem = s.Declaration.Members[i];
                            var mi = str.MemberInitializers[i];
                            mi.Name = mem.Name;

                            if (CastIfLiteral(mi.Value.Type, mem.Type, out var miType))
                            {
                                mi.Value.Type = miType;
                            }
                            else
                            {
                                yield return new GenericError(data.Text, mi.GenericParseTreeNode.Value, $"Value of type '{mi.Value.Type}' cannot be assigned to struct member '{mem.Name}' with type '{mem.Type}'");
                            }
                        }
                    }
                    else if (namesProvided == str.MemberInitializers.Length)
                    {

                    }
                    else
                    {
                        yield return new GenericError(data.Text, str.ParseTreeNode?.Name, $"Either all or no values must have a name");
                    }
                }
            }
            else
            {
                yield return new GenericError(data.Text, str.ParseTreeNode?.Name, $"'{str.Name}' is not a struct type");
            }

            yield break;
        }

        public override IEnumerable<object> VisitArrayAccessExpression(AstArrayAccessExpr arr, SemanticerData data = null)
        {
            foreach (var v in arr.SubExpression.Accept(this, data))
                if (v is ReplaceAstExpr r)
                    arr.SubExpression = r.NewExpression;
                else yield return v;

            foreach (var v in arr.Indexer.Accept(this, data))
                if (v is ReplaceAstExpr r)
                    arr.Indexer = r.NewExpression;
                else yield return v;

            if (arr.SubExpression.Type is PointerType p)
            {
                arr.Type = p.TargetType;
            }
            else if (arr.SubExpression.Type is ArrayType a)
            {
                arr.Type = a.TargetType;
            }
            else
            {
                arr.Type = CheezType.Error;
                yield return new LambdaError(eh => eh.ReportError(data.Text, arr.SubExpression.GenericParseTreeNode, $"[] operator can only be used with array and pointer types, got '{arr.SubExpression.Type}'"));
            }

            if (arr.Indexer.Type is IntType i)
            {

            }
            else
            {
                arr.Type = CheezType.Error;
                yield return new LambdaError(eh => eh.ReportError(data.Text, arr.SubExpression.GenericParseTreeNode, $"Indexer of [] operator has to be an integer, got '{arr.Indexer.Type}"));
            }

            arr.SetFlag(ExprFlags.IsLValue);
        }

        public override IEnumerable<object> VisitBinaryExpression(AstBinaryExpr bin, SemanticerData data = null)
        {
            var scope = data.Scope;
            bin.Scope = scope;

            foreach (var v in bin.Left.Accept(this, data))
                if (v is ReplaceAstExpr r)
                    bin.Left = r.NewExpression;
                else
                    yield return v;

            foreach (var v in bin.Right.Accept(this, data))
                if (v is ReplaceAstExpr r)
                    bin.Right = r.NewExpression;
                else
                    yield return v;


            bool leftIsLiteral = IsNumberLiteralType(bin.Left.Type);
            bool rightIsLiteral = IsNumberLiteralType(bin.Right.Type);
            if (leftIsLiteral && rightIsLiteral)
            {
                if (bin.Left.Type == FloatType.LiteralType || bin.Right.Type == FloatType.LiteralType)
                {
                    bin.Left.Type = FloatType.DefaultType;
                    bin.Right.Type = FloatType.DefaultType;
                }
                else
                {
                    bin.Left.Type = IntType.DefaultType;
                    bin.Right.Type = IntType.DefaultType;
                }
            }
            else if (leftIsLiteral)
            {
                if (IsNumberType(bin.Right.Type))
                    bin.Left.Type = bin.Right.Type;
                else
                    bin.Left.Type = IntType.DefaultType;
            }
            else if (rightIsLiteral)
            {
                if (IsNumberType(bin.Left.Type))
                    bin.Right.Type = bin.Left.Type;
                else
                    bin.Right.Type = IntType.DefaultType;
            }

            var ops = scope.GetOperators(bin.Operator, bin.Left.Type, bin.Right.Type);

            if (ops.Count > 1)
            {
                yield return new LambdaError(eh => eh.ReportError(data.Text, bin.GenericParseTreeNode, $"Multiple operators match the types '{bin.Left.Type}' and '{bin.Right.Type}'"));
            }
            else if (ops.Count == 0)
            {
                yield return new LambdaError(eh => eh.ReportError(data.Text, bin.GenericParseTreeNode, $"No operator matches the types '{bin.Left.Type}' and '{bin.Right.Type}'"));
            }
            else
            {
                var op = ops[0];
                bin.Type = op.ResultType;
            }

            yield break;
        }

        public override IEnumerable<object> VisitCallExpression(AstCallExpr call, SemanticerData data = null)
        {
            var scope = data.Scope;
            call.Scope = scope;

            foreach (var v in call.Function.Accept(this, data))
                if (v is ReplaceAstExpr r)
                    call.Function = r.NewExpression;
                else
                    yield return v;

            if (call.Function is AstDotExpr d && d.IsDoubleColon)
            {
                call.Arguments.Insert(0, d.Left);
            }

            if (call.Function.Type is FunctionType f)
            {
                if (f.ParameterTypes.Length != call.Arguments.Count)
                {
                    yield return new LambdaError(eh => eh.ReportError(data.Text, call.GenericParseTreeNode, $"Wrong number of arguments in function call. Expected {f.ParameterTypes.Length}, got {call.Arguments.Count}"));
                }

                call.Type = f.ReturnType;

                for (int i = 0; i < call.Arguments.Count; i++)
                {
                    foreach (var v in call.Arguments[i].Accept(this, data))
                        if (v is ReplaceAstExpr r)
                            call.Arguments[i] = r.NewExpression;
                        else
                            yield return v;

                    var expectedType = f.ParameterTypes[i];

                    if (!CastIfLiteral(call.Arguments[i].Type, expectedType, out var t))
                    {
                        yield return new LambdaError(eh => eh.ReportError(data.Text, call.Arguments[i].GenericParseTreeNode, $"Argument type does not match parameter type. Expected {expectedType}"));
                    }

                    call.Arguments[i].Type = t;
                }
            }
            else
            {
                yield return new LambdaError(eh => eh.ReportError(data.Text, call.Function.GenericParseTreeNode, $"Type .{call.Function.Type}' is not a callable type"));
            }

            yield break;
        }

        public override IEnumerable<object> VisitIdentifierExpression(AstIdentifierExpr ident, SemanticerData data = null)
        {
            var scope = data.Scope;
            ident.Scope = scope;

            yield return new WaitForSymbol(data.Text, ident.GenericParseTreeNode, scope, ident.Name);
            var v = scope.GetSymbol(ident.Name);

            if (v is Using u)
            {
                var e = u.Expr.Clone();
                e.GenericParseTreeNode = ident.GenericParseTreeNode;
                foreach (var vv in ReplaceAstExpr(e, data))
                    yield return vv;
                yield break;
            }

            ident.Type = v.Type;
            ident.SetFlag(ExprFlags.IsLValue);

            yield break;
        }

        public override IEnumerable<object> VisitNumberExpression(AstNumberExpr num, SemanticerData data = null)
        {
            num.Type = IntType.LiteralType;
            yield break;
        }

        public override IEnumerable<object> VisitStringLiteral(AstStringLiteral str, SemanticerData data = null)
        {
            str.Type = CheezType.String;
            yield break;
        }

        public override IEnumerable<object> VisitBoolExpression(AstBoolExpr bo, SemanticerData data = null)
        {
            bo.Type = CheezType.Bool;
            yield break;
        }

        public override IEnumerable<object> VisitAddressOfExpression(AstAddressOfExpr add, SemanticerData data = null)
        {
            add.Scope = data.Scope;

            foreach (var v in add.SubExpression.Accept(this, data))
                if (v is ReplaceAstExpr r)
                    add.SubExpression = r.NewExpression;
                else
                    yield return v;

            add.Type = PointerType.GetPointerType(add.SubExpression.Type);
            if (!add.SubExpression.GetFlag(ExprFlags.IsLValue))
                yield return new LambdaError(eh => eh.ReportError(data.Text, add.SubExpression.GenericParseTreeNode, $"Sub expression of & is not a lvalue"));

            yield break;
        }

        public override IEnumerable<object> VisitDereferenceExpression(AstDereferenceExpr deref, SemanticerData data = null)
        {
            deref.Scope = data.Scope;

            foreach (var v in deref.SubExpression.Accept(this, data))
                if (v is ReplaceAstExpr r)
                    deref.SubExpression = r.NewExpression;
                else
                    yield return v;

            if (deref.SubExpression.Type is PointerType p)
            {
                deref.Type = p.TargetType;
            }
            else
            {
                yield return new LambdaError(eh => eh.ReportError(data.Text, deref.SubExpression.GenericParseTreeNode, $"Sub expression of & is not a pointer"));
            }
            yield break;
        }

        public override IEnumerable<object> VisitCastExpression(AstCastExpr cast, SemanticerData data = null)
        {
            cast.Scope = data.Scope;


            yield return new WaitForType(data.Text, data.Scope, cast.ParseTreeNode.TargetType);
            cast.Type = data.Scope.GetCheezType(cast.ParseTreeNode.TargetType);

            // check subExpression
            foreach (var v in cast.SubExpression.Accept(this, data))
                if (v is ReplaceAstExpr r)
                    cast.SubExpression = r.NewExpression;
                else
                    yield return v;


            if (!CastIfLiteral(cast.SubExpression.Type, cast.Type, out var type)) ;
            //{
            //    yield return new LambdaError(eh => eh.ReportError(data.Text, cast.ParseTreeNode, $"Can't cast a value of to '{cast.SubExpression.Type}' to '{cast.Type}'"));
            //}
            cast.SubExpression.Type = type;

            yield break;
        }

        public override IEnumerable<object> VisitDotExpression(AstDotExpr dot, SemanticerData data = null)
        {
            dot.Scope = data.Scope;

            foreach (var v in dot.Left.Accept(this, data))
                if (v is ReplaceAstExpr r)
                    dot.Left = r.NewExpression;
                else
                    yield return v;

            while (dot.Left.Type is ReferenceType r)
                dot.Left.Type = r.TargetType;

            if (dot.Left.Type == IntType.LiteralType)
                dot.Left.Type = IntType.DefaultType;
            else if (dot.Left.Type == FloatType.LiteralType)
                dot.Left.Type = FloatType.DefaultType;

            if (dot.IsDoubleColon)
            {
                yield return new LambdaCondition(() => dot.Scope.GetImplFunction(dot.Left.Type, dot.Right) != null,
                    eh => eh.ReportError(data.Text, dot.GenericParseTreeNode, $"No impl function  '{dot.Right}' exists for type '{dot.Left.Type}'"));
                var func = dot.Scope.GetImplFunction(dot.Left.Type, dot.Right);
                dot.Type = func.Type;
            }
            else
            {
                while (dot.Left.Type is PointerType p)
                {
                    dot.Left = new AstDereferenceExpr(dot.Left.GenericParseTreeNode, dot.Left);
                    dot.Left.Type = p.TargetType;
                }

                if (dot.Left.Type is StructType s)
                {
                    var member = s.Declaration.Members.FirstOrDefault(m => m.Name == dot.Right);
                    if (member == null)
                    {
                        yield return new LambdaCondition(() => dot.Scope.GetImplFunction(dot.Left.Type, dot.Right) != null,
                            eh => eh.ReportError(data.Text, dot.GenericParseTreeNode, $"No impl function  '{dot.Right}' exists for type '{dot.Left.Type}'"));
                        var func = dot.Scope.GetImplFunction(dot.Left.Type, dot.Right);
                        dot.Type = func.Type;
                        dot.IsDoubleColon = true;
                        //yield return new LambdaError(eh => eh.ReportError(data.Text, dot.ParseTreeNode.Right, $"'{dot.Right}' is not a member of struct '{dot.Left.Type}'"));
                    }
                    else
                    {
                        dot.Type = member.Type;
                    }
                }
                else
                {
                    yield return new LambdaCondition(() => dot.Scope.GetImplFunction(dot.Left.Type, dot.Right) != null,
                        eh => eh.ReportError(data.Text, dot.GenericParseTreeNode, $"No impl function  '{dot.Right}' exists for type '{dot.Left.Type}'"));
                    var func = dot.Scope.GetImplFunction(dot.Left.Type, dot.Right);
                    dot.Type = func.Type;
                    dot.IsDoubleColon = true;
                    //yield return new LambdaError(eh => eh.ReportError(data.Text, dot.ParseTreeNode.Left, $"Left side of '.' has to a struct type, got '{dot.Left.Type}'"));
                }

                dot.SetFlag(ExprFlags.IsLValue);
            }

            yield break;
        }

        #endregion

        private bool CastIfLiteral(CheezType sourceType, CheezType targetType, out CheezType outSource)
        {
            outSource = sourceType;

            while (targetType is ReferenceType r)
                targetType = r.TargetType;

            while (sourceType is ReferenceType r)
                sourceType = r.TargetType;

            if (sourceType == IntType.LiteralType && (targetType is IntType || targetType is FloatType))
            {
                outSource = targetType;
            }
            else if (sourceType == FloatType.LiteralType && targetType is FloatType)
            {
                outSource = targetType;
            }
            else if (sourceType != targetType)
            {
                return false;
            }

            return true;
        }

        private bool IsNumberLiteralType(CheezType type)
        {
            return type == IntType.LiteralType || type == FloatType.LiteralType;
        }

        private bool IsNumberType(CheezType type)
        {
            return type is IntType || type is FloatType;
        }

        private IEnumerable<object> ReplaceAstExpr(AstExpression expr, SemanticerData data)
        {
            foreach (var v in expr.Accept(this, data))
            {
                if (v is ReplaceAstExpr r)
                {
                    expr = r.NewExpression;
                    break;
                }
                else
                    yield return v;
            }

            yield return new ReplaceAstExpr(expr);
        }
    }
}
