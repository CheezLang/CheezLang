using Cheez.Compiler.Ast;
using Cheez.Compiler.Parsing;
using Cheez.Compiler.Visitor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cheez.Compiler.ParseTree;
using System.Runtime.CompilerServices;
using System.Numerics;

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

    public class WaitForSymbol : ICondition
    {
        public Scope Scope { get; }
        public string SymbolName { get; }
        public ILocation Node { get; set; }
        public IText Text { get; set; }

        public string File { get; }
        public string Function { get; }
        public int Line { get; }

        public WaitForSymbol(IText text, ILocation node, Scope scope, string varNem, [CallerFilePath] string callingFunctionFile = "", [CallerMemberName] string callingFunctionName = "", [CallerLineNumber] int callLineNumber = 0)
        {
            this.Scope = scope;
            this.SymbolName = varNem;
            this.Text = text;
            this.Node = node;
            File = callingFunctionFile;
            Function = callingFunctionName;
            Line = callLineNumber;
        }

        public bool Check()
        {
            return Scope.GetSymbol(SymbolName) != null;
        }

        public void Report(IErrorHandler handler)
        {
            handler.ReportError(Text, Node, $"Unknown symbol '{SymbolName}'", null, File, Function, Line);
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

    public class ReplaceAstStmt
    {
        public AstStatement NewStatement { get; set; }

        public ReplaceAstStmt(AstStatement newExpr)
        {
            NewStatement = newExpr;
        }
    }

    #endregion

    public class SemanticerData : IErrorHandler
    {
        public Scope Scope { get; set; }
        public IText Text { get; set; }

        public AstImplBlock ImplBlock { get; set; }
        public AstFunctionDecl Function { get; set; }
        //public CheezType ImplTarget { get; set; }

        public CheezType ExpectedType { get; set; }

        public IErrorHandler ErrorHandler { get; set; }

        public bool HasErrors => ErrorHandler.HasErrors;

        [DebuggerStepThrough]
        public SemanticerData()
        {
        }

        [DebuggerStepThrough]
        public SemanticerData(Scope Scope = null, IText Text = null, AstFunctionDecl Function = null/*, CheezType Impl = null*/, AstImplBlock ImplBlock = null, CheezType ExpectedType = null, IErrorHandler ErrorHandler = null)
        {
            this.Scope = Scope;
            this.Text = Text;
            this.Function = Function;
            //this.ImplTarget = Impl;
            this.ImplBlock = ImplBlock;
            this.ExpectedType = ExpectedType;
            this.ErrorHandler = ErrorHandler;
        }

        [DebuggerStepThrough]
        public SemanticerData Clone(Scope Scope = null, IText Text = null, AstFunctionDecl Function = null/*, CheezType Impl = null*/, AstImplBlock ImplBlock = null, CheezType ExpectedType = null, IErrorHandler ErrorHandler = null)
        {
            return new SemanticerData
            {
                Scope = Scope ?? this.Scope,
                Text = Text ?? this.Text,
                Function = Function ?? this.Function,
                //ImplTarget = Impl ?? this.ImplTarget,
                ImplBlock = ImplBlock ?? this.ImplBlock,
                ExpectedType = ExpectedType,
                ErrorHandler = ErrorHandler ?? this.ErrorHandler
            };
        }

        public void ReportError(string message, [CallerFilePath] string callingFunctionFile = "", [CallerMemberName] string callingFunctionName = "", [CallerLineNumber] int callLineNumber = 0)
        {
            ErrorHandler.ReportError(message, callingFunctionFile, callingFunctionName, callLineNumber);
        }

        public void ReportError(IText text, ILocation location, string message, List<Error> subErrors = null, [CallerFilePath] string callingFunctionFile = "", [CallerMemberName] string callingFunctionName = "", [CallerLineNumber] int callLineNumber = 0)
        {
            ErrorHandler.ReportError(text, location, message, subErrors, callingFunctionFile, callingFunctionName, callLineNumber);
        }

        public void ReportError(ILocation location, string message, List<Error> subErrors = null, [CallerFilePath] string callingFunctionFile = "", [CallerMemberName] string callingFunctionName = "", [CallerLineNumber] int callLineNumber = 0)
        {
            ErrorHandler.ReportError(Text, location, message, subErrors, callingFunctionFile, callingFunctionName, callLineNumber);
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
                var enumerator = s.Accept(this, new SemanticerData(workspace.GlobalScope, s.GenericParseTreeNode.SourceFile, ErrorHandler: errorHandler)).GetEnumerator();
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

        [DebuggerStepThrough]
        private Scope NewScope(string name, Scope parent)
        {
            var s = new Scope(name, parent);
            //AllScopes.Add(s);
            return s;
        }

        #endregion

        #region Statements

        public override IEnumerable<object> VisitTypeAlias(AstTypeAliasDecl al, SemanticerData data = null)
        {
            al.Scope = data.Scope;
            foreach (var v in al.TypeExpr.Accept(this, data.Clone()))
                yield return v;
            if (al.TypeExpr.Value is CheezType t)
                al.Type = t;
            else
            {
                data.ReportError(al.TypeExpr.GenericParseTreeNode, $"Expected type, got {al.TypeExpr.Type}");
            }
        }

        public override IEnumerable<object> VisitEmptyExpression(AstEmptyExpr em, SemanticerData data = null)
        {
            em.Scope = data.Scope;
            em.Type = CheezType.Error;
            yield break;
        }

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
                    data.ReportError(m.ParseTreeNode.Name, $"A member with name '{m.Name}' already exists in enum '{en.Name}'");
                    continue;
                }

                names.Add(m.Name);
            }

            var enumType = new EnumType(en);
            if (!scope.DefineTypeSymbol(en.Name.Name, enumType))
            {
                data.ReportError(en.ParseTreeNode.Name, $"A type with name '{en.Name}' already exists in current scope");
            }

            scope.TypeDeclarations.Add(en);

            yield break;
        }

        public IEnumerable<object> VisitParameter(AstParameter param, SemanticerData context)
        {
            foreach (var v in param.TypeExpr.Accept(this, context))
                yield return v;

            if (param.TypeExpr.Value is CheezType t)
                param.Type = t;
            else
                context.ReportError(param.TypeExpr.GenericParseTreeNode, $"Expected type, got {param.TypeExpr.Type}");
        }

        private IEnumerable<object> CallPolyStruct(AstCallExpr call, GenericStructType @struct, SemanticerData context)
        {
            var types = new Dictionary<string, CheezType>();

            // args

            if (call.Arguments.Count != @struct.Declaration.Parameters.Count)
            {
                context.ReportError(call.GenericParseTreeNode, "Wrong number of arguments in struct type instantiation");
                yield break;
            }

            int argCount = call.Arguments.Count;
            for (int i = 0; i < argCount; i++)
            {
                var param = @struct.Declaration.Parameters[i];
                var arg = call.Arguments[i];

                foreach (var v in arg.Accept(this, context))
                    yield return v;

                if (arg.Type == CheezType.Type)
                {
                    types[param.Name.Name] = arg.Value as CheezType;
                }
                else
                {
                    context.ReportError(arg.GenericParseTreeNode, $"Argument has to be a type, got {arg.Type}");
                }
            }

            // check if instance already exists
            AstStructDecl instance = null;
            foreach (var pi in @struct.Declaration.PolymorphicInstances)
            {
                bool eq = true;
                foreach (var param in pi.Parameters)
                {
                    var existingType = param.Value;
                    var newType = types[param.Name.Name];
                    if (existingType != newType)
                    {
                        eq = false;
                        break;
                    }
                }

                if (eq)
                {
                    instance = pi;
                    break;
                }
            }


            // instatiate type
            if (instance == null)
            {
                instance = @struct.Declaration.Clone() as AstStructDecl;
                instance.SubScope = NewScope($"struct {@struct.Declaration.Name.Name}", @struct.Declaration.Scope);
                instance.IsPolyInstance = true;
                instance.IsPolymorphic = false;
                @struct.Declaration.PolymorphicInstances.Add(instance);

                foreach (var p in instance.Parameters)
                {
                    p.Value = types[p.Name.Name];
                }

                foreach (var kv in types)
                {
                    instance.SubScope.DefineTypeSymbol(kv.Key, kv.Value);
                }

                foreach (var v in instance.Accept(this, new SemanticerData(instance.Scope, context.Text, ErrorHandler: context.ErrorHandler)))
                    yield return v;
            }

            call.Function = new AstStructExpression(call.Function.GenericParseTreeNode, instance, call.Function);
            call.Type = CheezType.Type;
            call.Value = instance.Type;

            yield break;
        }

        public override IEnumerable<object> VisitStructDeclaration(AstStructDecl str, SemanticerData data = null)
        {
            var scope = data.Scope;
            str.Scope = scope;

            if (str.Parameters != null && !str.IsPolyInstance)
            {
                str.IsPolymorphic = true;

                // check that all parameters are types
                bool ok = true;
                foreach (var p in str.Parameters)
                {
                    foreach (var v in VisitParameter(p, data.Clone()))
                        yield return v;

                    if (p.Type != CheezType.Type)
                    {
                        ok = false;
                        data.ReportError(p.ParseTreeNode, $"Struct parameters can only be types, found {p.Type}");
                    }
                }

                if (!ok)
                    yield break;

                str.Type = new GenericStructType(str);
                if (!scope.DefineSymbol(str))
                {
                    data.ReportError(str.ParseTreeNode.Name, $"A symbol with name '{str.Name.Name}' already exists in current scope");
                }
                yield break;
            }

            scope.TypeDeclarations.Add(str);
            var structType = new StructType(str);
            if (!str.IsPolyInstance)
            {
                if (!scope.DefineTypeSymbol(str.Name.Name, structType))
                {
                    data.ReportError(str.ParseTreeNode.Name, $"A symbol with name '{str.Name.Name}' already exists in current scope");
                }
            }

            foreach (var mem in str.Members)
            {
                mem.TypeExpr.Scope = str.SubScope;
                foreach (var v in mem.TypeExpr.Accept(this, data.Clone(Scope: str.SubScope)))
                    yield return v;
                if (mem.TypeExpr.Value is CheezType t)
                    mem.Type = t;
                else
                    data.ReportError(mem.TypeExpr.GenericParseTreeNode, $"Expected type, got {mem.TypeExpr.Type}");
            }

            Debug.Assert(structType != null && structType.Declaration == str);
            structType.Analyzed = true;
            str.Type = structType;
        }

        public void Using(Scope scope, StructType @struct, INamed name)
        {
            foreach (var member in @struct.Declaration.Members)
            {
                scope.DefineSymbol(new Using(member.Name, new AstDotExpr(null, name.Name.Clone(), member.Name.Name, false)));
            }
        }

        public void Using(Scope scope, StructType @struct, AstExpression sub)
        {
            foreach (var member in @struct.Declaration.Members)
            {
                scope.DefineSymbol(new Using(member.Name, new AstDotExpr(null, sub, member.Name.Name, false)));
            }
        }

        private IEnumerable<object> VisitFunctionHeader(AstFunctionDecl function, SemanticerData context)
        {
            function.ImplBlock = context.ImplBlock;

            if (!function.IsPolyInstance && ((function.ReturnTypeExpr?.IsPolymorphic ?? false) || function.Parameters.Any(p => p.TypeExpr.IsPolymorphic)))
            {
                function.IsGeneric = true;
                function.Type = new GenericFunctionType(function);

                // collect polymorphic types
                CollectPolymorphicTypes(function.Parameters.Select(p => p.TypeExpr), out var types);
                CollectPolymorphicTypes(function.ReturnTypeExpr, ref types);
                function.PolymorphicTypeExprs = types;

                if (function.ImplBlock != null)
                {
                    if (!function.Scope.DefineImplFunction(function))
                    {
                        context.ReportError(function.Name.GenericParseTreeNode, $"Duplicate name: {function.Name}");
                    }
                }
                else if (!function.Scope.DefineSymbol(function))
                {
                    context.ReportError(function.Name.GenericParseTreeNode, $"Duplicate name: {function.Name}");
                }
                yield break;
            }

            // @Todo: make entry point configurable
            if (function.Name.Name == "Main" && function.Scope == workspace.GlobalScope)
                workspace.MainFunction = function;

            // return type
            if (function.ReturnTypeExpr != null)
            {
                foreach (var v in function.ReturnTypeExpr.Accept(this, new SemanticerData(Scope: function.HeaderScope, Text: context.Text, Function: function, ImplBlock: context.ImplBlock)))
                    yield return v;
                if (function.ReturnTypeExpr.Value is CheezType t)
                    function.ReturnType = t;
                else
                    context.ReportError(function.ReturnTypeExpr.GenericParseTreeNode, $"Expected type, got {function.ReturnTypeExpr.Type}");
            }
            else
            {
                function.ReturnType = CheezType.Void;
            }

            // parameters
            foreach (var p in function.Parameters)
            {
                p.Scope = function.HeaderScope;

                // infer type
                foreach (var v in p.TypeExpr.Accept(this, new SemanticerData(Scope: function.HeaderScope, Text: context.Text, Function: function, ImplBlock: context.ImplBlock, ErrorHandler: context.ErrorHandler)))
                    yield return v;
                if (p.TypeExpr.Value is CheezType t)
                    p.Type = t;
                else
                    context.ReportError(p.TypeExpr.GenericParseTreeNode, $"Expected type, got {p.TypeExpr.Type}");

                if (!function.HeaderScope.DefineSymbol(p))
                {
                    context.ReportError(p.ParseTreeNode, $"A parameter with name '{p.Name}' already exists in this function");
                }
            }

            function.Type = FunctionType.GetFunctionType(function);
            function.Scope.FunctionDeclarations.Add(function);

            if (!function.IsPolyInstance)
            {
                if (function.ImplBlock != null)
                {
                    if (!function.Scope.DefineImplFunction(function))
                    {
                        context.ReportError(function.Name.GenericParseTreeNode, $"Duplicate name: {function.Name}");
                    }
                }
                else if (!function.Scope.DefineSymbol(function))
                {
                    context.ReportError(function.Name.GenericParseTreeNode, $"A function or variable with name '{function.Name}' already exists in current scope");
                }
            }
        }

        private IEnumerable<object> VisitFunctionBody(AstFunctionDecl function, SemanticerData context)
        {
            if (function.ImplBlock != null)
            {
                var tar = function.Parameters[0].Type;
                while (tar is PointerType p)
                    tar = p.TargetType;

                if (tar is ReferenceType r)
                    tar = r.TargetType;

                if (tar is StructType @struct)
                {
                    Using(function.SubScope, @struct, function.Parameters[0]);
                }
            }

            var subData = context.Clone(Scope: function.SubScope, Function: function);
            foreach (var v in function.Body.Accept(this, subData))
                yield return v;
            bool returns = function.Body.GetFlag(StmtFlags.Returns);

            if (function.ReturnType != CheezType.Void && !returns)
            {
                context.ReportError(function.Name.GenericParseTreeNode, "Not all code paths return a value!");
            }
        }

        public override IEnumerable<object> VisitFunctionDeclaration(AstFunctionDecl function, SemanticerData context = null)
        {
            var scope = context.Scope;
            function.Scope = scope;
            function.HeaderScope = NewScope($"fn {function.Name}()", function.Scope);
            function.SubScope = NewScope("{}", function.HeaderScope);
            function.Text = context.Text;

            foreach (var v in VisitFunctionHeader(function, context))
                yield return v;

            if (!function.IsGeneric && function.Body != null)
            {
                foreach (var v in VisitFunctionBody(function, context))
                    yield return v;
            }
        }

        public override IEnumerable<object> VisitEmptyStatement(AstEmptyStatement em, SemanticerData data = null)
        {
            yield break;
        }

        public override IEnumerable<object> VisitIfStatement(AstIfStmt ifs, SemanticerData context = null)
        {
            var scope = context.Scope;
            ifs.Scope = scope;

            bool returns = true;

            // check condition
            {
                foreach (var v in ifs.Condition.Accept(this, context.Clone()))
                    if (v is ReplaceAstExpr r)
                        ifs.Condition = r.NewExpression;
                    else
                        yield return v;

                if (ifs.Condition.Type != CheezType.Bool)
                {
                    context.ReportError(ifs.ParseTreeNode.Condition, $"if-statement condition must be of type 'bool', got '{ifs.Condition.Type}'");
                }
            }

            if (ifs.Condition.Type == CheezType.Bool && ifs.Condition.IsCompTimeValue)
            {
                var b = (bool)ifs.Condition.Value;

                if (b)
                {
                    foreach (var v in ReplaceAstStmt(ifs.IfCase, context.Clone(NewScope("if", scope))))
                        yield return v;
                    yield break;
                }
                else if (ifs.ElseCase != null)
                {
                    foreach (var v in ReplaceAstStmt(ifs.ElseCase, context.Clone(NewScope("else", scope))))
                        yield return v;
                    yield break;
                }
                else
                {
                    foreach (var v in ReplaceAstStmt(new AstEmptyStatement(ifs.GenericParseTreeNode), context.Clone(NewScope("else", scope))))
                        yield return v;
                    yield break;
                }
            }

            // if case
            {
                foreach (var v in ifs.IfCase.Accept(this, context.Clone(NewScope("if", scope))))
                    yield return v;

                if (!ifs.IfCase.GetFlag(StmtFlags.Returns))
                    returns = false;
            }

            // else case
            if (ifs.ElseCase != null)
            {
                foreach (var v in ifs.ElseCase.Accept(this, context.Clone(NewScope("else", scope))))
                    yield return v;

                if (!ifs.ElseCase.GetFlag(StmtFlags.Returns))
                    returns = false;
            }

            if (returns)
                ifs.SetFlag(StmtFlags.Returns);

            yield break;
        }

        public override IEnumerable<object> VisitBlockStatement(AstBlockStmt block, SemanticerData context = null)
        {
            var scope = context.Scope;
            block.Scope = scope;
            block.SubScope = NewScope("{}", scope);

            var subData = context.Clone(Scope: block.SubScope);

            for (int i = 0; i < block.Statements.Count; i++)
            {
                foreach (var v in block.Statements[i].Accept(this, subData))
                    if (v is ReplaceAstStmt r)
                        block.Statements[i] = r.NewStatement;
                    else
                        yield return v;

                if (block.Statements[i].GetFlag(StmtFlags.Returns))
                {
                    block.SetFlag(StmtFlags.Returns);
                }
            }

            block.Statements.LastOrDefault()?.SetFlag(StmtFlags.IsLastStatementInBlock);

            yield break;
        }

        public override IEnumerable<object> VisitReturnStatement(AstReturnStmt ret, SemanticerData context = null)
        {
            var scope = context.Scope;
            ret.Scope = scope;

            ret.SetFlag(StmtFlags.Returns);

            if (ret.ReturnValue != null)
            {
                foreach (var v in ret.ReturnValue.Accept(this, context.Clone(ExpectedType: context.Function.ReturnType)))
                    if (v is ReplaceAstExpr r)
                        ret.ReturnValue = r.NewExpression;
                    else
                        yield return v;
            }

            Debug.Assert(context.Function != null, "return statement is only allowed in functions");
            if (context.Function.ReturnType != CheezType.Void && ret.ReturnValue == null) // !void, return
            {
                context.ReportError(ret.ParseTreeNode, $"Missing return value in non-void function {context.Function.Name}");
            }
            else if (context.Function.ReturnType == CheezType.Void && ret.ReturnValue != null) // void, return some
            {
                context.ReportError(ret.ParseTreeNode, $"Can't return value of type '{ ret.ReturnValue.Type }' in void function");
            }
            else if (context.Function.ReturnType != CheezType.Void && ret.ReturnValue != null) // !void, return some
            {
                // compare types
                if (ret.ReturnValue.Type == IntType.LiteralType && (context.Function.ReturnType is IntType || context.Function.ReturnType is FloatType))
                {
                    ret.ReturnValue.Type = context.Function.ReturnType;
                }
                else if (ret.ReturnValue.Type == FloatType.LiteralType && context.Function.ReturnType is FloatType)
                {
                    ret.ReturnValue.Type = context.Function.ReturnType;
                }
                else if (ret.ReturnValue.Type != context.Function.ReturnType)
                {
                    context.ReportError(ret.ParseTreeNode.ReturnValue, $"Can't return value of type '{ret.ReturnValue.Type}' in function with return type '{context.Function.ReturnType}'");
                }
            }

            yield break;
        }

        public override IEnumerable<object> VisitVariableDeclaration(AstVariableDecl variable, SemanticerData context = null)
        {
            var scope = context.Scope;
            variable.Scope = scope;

            if (context.Function != null)
            {
                variable.SubScope = NewScope($"var {variable.Name}", scope);
            }
            else
            {
                variable.SetFlag(StmtFlags.GlobalScope);
                variable.SubScope = variable.Scope;
            }

            if (variable.TypeExpr == null && variable.Initializer == null)
            {
                context.ReportError(variable.GenericParseTreeNode, "Either type or initializer has to be specified in variable declaration");
            }

            if (variable.TypeExpr != null)
            {
                variable.TypeExpr.Scope = variable.Scope;
                foreach (var v in variable.TypeExpr.Accept(this, context.Clone()))
                {
                    if (v is ReplaceAstExpr r)
                        variable.TypeExpr = r.NewExpression;
                    else
                        yield return v;
                }
                if (variable.TypeExpr.Value is CheezType t)
                    variable.Type = t;
                else
                    context.ReportError(variable.TypeExpr.GenericParseTreeNode, $"Expected type, got {variable.TypeExpr.Type}");
            }

            if (variable.Initializer != null)
            {
                foreach (var v in variable.Initializer.Accept(this, context.Clone(ExpectedType: variable.Type)))
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
                    if (!CastIfLiteral(variable.Initializer.Type, variable.Type, out var t))
                        context.ReportError(variable.Initializer.GenericParseTreeNode, $"Can't assign value of type '{variable.Initializer.Type}' to '{variable.Type}'");

                    variable.Initializer.Type = t;
                }
            }


            if (variable.Type == CheezType.Type)
            {
                variable.IsConstant = true;
                variable.SubScope.DefineTypeSymbol(variable.Name.Name, variable.Initializer.Value as CheezType);
            }
            else if (variable.SubScope.DefineSymbol(variable))
            {
                if (context.Function != null)
                    context.Function.LocalVariables.Add(variable);
                scope.VariableDeclarations.Add(variable);
            }
            else
            {
                // @Note: This should probably never happen, except for global variables, which are not implemented yet
                context.ReportError(variable.Name.GenericParseTreeNode, $"A variable with name '{variable.Name}' already exists in current scope");
            }

            context.Scope = variable.SubScope;
            yield break;
        }

        public override IEnumerable<object> VisitAssignment(AstAssignment ass, SemanticerData context = null)
        {
            var scope = context.Scope;
            ass.Scope = scope;

            // check target
            foreach (var v in ass.Target.Accept(this, context.Clone()))
                if (v is ReplaceAstExpr r)
                    ass.Target = r.NewExpression;
                else
                    yield return v;

            // check source
            foreach (var v in ass.Value.Accept(this, context.Clone(ExpectedType: ass.Target.Type)))
                if (v is ReplaceAstExpr r)
                    ass.Value = r.NewExpression;
                else
                    yield return v;

            if (!CastIfLiteral(ass.Value.Type, ass.Target.Type, out var type))
                context.ReportError(ass.ParseTreeNode, $"Can't assign value of type {ass.Value.Type} to {ass.Target.Type}");

            ass.Value.Type = type;
            if (!ass.Target.GetFlag(ExprFlags.IsLValue))
                context.ReportError(ass.ParseTreeNode.Target, $"Left side of assignment has to be a lvalue");

            if (ass.Value.Type == IntType.LiteralType)
                ass.Value.Type = IntType.DefaultType;

            if (ass.Value.Type == FloatType.LiteralType)
                ass.Value.Type = FloatType.DefaultType;

            yield break;
        }

        public override IEnumerable<object> VisitExpressionStatement(AstExprStmt stmt, SemanticerData context = null)
        {
            stmt.Scope = context.Scope;
            foreach (var v in stmt.Expr.Accept(this, context.Clone()))
                if (v is ReplaceAstExpr r)
                    stmt.Expr = r.NewExpression;
                else
                    yield return v;
            yield break;
        }

        private IEnumerable<object> CreateType(Scope scope, AstExpression e, IText text, IErrorHandler error)
        {
            switch (e)
            {
                case AstIdentifierExpr i:
                    {
                        if (i.IsPolymorphic)
                        {
                            yield return new PolyType(i.Name);
                        }
                        else
                        {
                            var v = scope.GetSymbol(i.Name);
                            if (v == null)
                            {
                                yield return new WaitForSymbol(text, e.GenericParseTreeNode, scope, i.Name);
                                v = scope.GetSymbol(i.Name);
                            }

                            if (v is CompTimeVariable comp)
                                yield return comp.Value as CheezType;
                            else
                                yield return v.Type;
                        }
                        yield break;
                    }

                case AstCallExpr c:
                    {
                        CheezType fType = null;
                        foreach (var v in CreateType(scope, c.Function, text, error))
                        {
                            if (v is CheezType t)
                                fType = t;
                            else
                                yield return v;
                        }

                        var aTypes = new CheezType[c.Arguments.Count];
                        for (int i = 0; i < aTypes.Length; i++)
                        {
                            foreach (var v in CreateType(scope, c.Arguments[i], text, error))
                            {
                                if (v is CheezType t)
                                    aTypes[i] = t;
                                else
                                    yield return v;
                            }
                        }

                        if (fType is GenericStructType s)
                        {
                            if (s.Declaration.Parameters.Count != aTypes.Length)
                            {
                                error.ReportError(text, c.Function.GenericParseTreeNode, $"Wrong number of arguments for struct '{s.Declaration.Name.Name}'");
                                yield return CheezType.Error;
                                yield break;
                            }

                            yield return new StructType(s.Declaration, aTypes);
                            yield break;
                        }
                        else
                        {
                            error.ReportError(text, c.Function.GenericParseTreeNode, $"Expected struct type, got '{c.Function}'");
                            yield return CheezType.Error;
                            yield break;
                        }
                    }
            }
        }

        public override IEnumerable<object> VisitImplBlock(AstImplBlock impl, SemanticerData context = null)
        {
            var scope = context.Scope;
            impl.Scope = scope;

            if (impl.ParseTreeNode.Trait != null)
            {
                yield return new WaitForSymbol(context.Text, impl.ParseTreeNode.Trait, scope, impl.ParseTreeNode.Trait.Name);
                impl.Trait = "TODO";
            }


            // types
            impl.TargetType = CheezType.Error;
            foreach (var v in CreateType(context.Scope, impl.TargetTypeExpr, context.Text, context.ErrorHandler))
            {
                if (v is CheezType type)
                    impl.TargetType = type;
                else
                    yield return v;
            }

            //impl.SubScope = new Scope($"impl {impl.TargetTypeExpr}", impl.Scope);
            impl.SubScope = impl.Scope;


            // add self parameter
            foreach (var f in impl.Functions)
            {
                AstExpression selfType = impl.TargetTypeExpr.Clone(); // new AstTypeExpr(impl.TargetTypeExpr.GenericParseTreeNode, impl.TargetType); // impl.TargetTypeExpr.Clone();
                if (f.RefSelf)
                    selfType = new AstPointerTypeExpr(selfType.GenericParseTreeNode, selfType)
                    {
                        IsReference = true
                    };
                f.Parameters.Insert(0, new AstFunctionParameter(new AstIdentifierExpr(null, "self", false), selfType));
            }

            foreach (var f in impl.Functions)
            {
                var cc = f.Accept(this, context.Clone(Scope: impl.SubScope, ImplBlock: impl, ErrorHandler: context.ErrorHandler));
                yield return new CompileStatement(cc);
            }

            scope.ImplBlocks.Add(impl);
        }

        public override IEnumerable<object> VisitUsingStatement(AstUsingStmt use, SemanticerData context = null)
        {
            var scope = context.Scope;
            use.Scope = scope;

            foreach (var v in use.Value.Accept(this, context.Clone()))
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
                context.ReportError(use.GenericParseTreeNode, "Only struct types can be used in 'using' statement");
            }


            yield break;
        }

        public override IEnumerable<object> VisitWhileStatement(AstWhileStmt ws, SemanticerData context = null)
        {
            var scope = context.Scope;
            ws.Scope = scope;

            foreach (var v in ws.Condition.Accept(this, context.Clone()))
                if (v is ReplaceAstExpr r)
                    ws.Condition = r.NewExpression;
                else
                    yield return v;

            if (ws.Condition.Type != CheezType.Bool)
            {
                context.ReportError(ws.Condition.GenericParseTreeNode, $"Condition of while statement has to be of type bool, but is of type {ws.Condition.Type}");
            }

            foreach (var v in ws.Body.Accept(this, context.Clone()))
                yield return v;

            yield break;
        }

        #endregion

        #region Expressions

        public override IEnumerable<object> VisitCompCallExpression(AstCompCallExpr call, SemanticerData context = null)
        {
            call.Scope = context.Scope;

            var name = call.Name.Name;

            foreach (var arg in call.Arguments)
            {
                arg.Scope = call.Scope;
                foreach (var v in arg.Accept(this, context))
                    yield return v;
            }

            switch (name)
            {
                case "sizeof":
                    {
                        if (call.Arguments.Count < 1)
                        {
                            context.ReportError(call.Name.GenericParseTreeNode, "Comptime function '@sizeof' requires one argument");
                            foreach (var v in ReplaceAstExpr(ConstInt(call.GenericParseTreeNode, new BigInteger(0)), context))
                                yield return v;
                        }
                        if (call.Arguments.Count > 1)
                            context.ReportError(call.Name.GenericParseTreeNode, "Comptime function '@sizeof' requires only one argument");

                        var arg = call.Arguments[0];

                        CheezType type = CheezType.Error;
                        if (arg.Type == CheezType.Type)
                        {
                            type = arg.Value as CheezType;
                        }
                        else
                        {
                            type = arg.Type;
                        }

                        foreach (var v in ReplaceAstExpr(ConstInt(call.GenericParseTreeNode, new BigInteger(type.Size)), context))
                            yield return v;
                        break;
                    }

                case "typeof":
                    {
                        if (call.Arguments.Count < 1)
                        {
                            context.ReportError(call.Name.GenericParseTreeNode, $"Comptime function '@{name}' requires one argument");
                            foreach (var v in ReplaceAstExpr(ConstInt(call.GenericParseTreeNode, new BigInteger(0)), context))
                                yield return v;
                        }
                        if (call.Arguments.Count > 1)
                            context.ReportError(call.Name.GenericParseTreeNode, $"Comptime function '@{name}' requires only one argument");

                        var arg = call.Arguments[0];

                        CheezType type = CheezType.Error;
                        if (arg.Type == CheezType.Type)
                        {
                            type = arg.Value as CheezType;
                        }
                        else
                        {
                            type = arg.Type;
                        }

                        foreach (var v in ReplaceAstExpr(new AstTypeExpr(call.GenericParseTreeNode, type), context))
                            yield return v;
                        break;
                    }

                case "isint":
                    {
                        if (call.Arguments.Count != 1)
                        {
                            context.ReportError(call.Name.GenericParseTreeNode, $"Comptime function '@{name}' requires one argument");
                            foreach (var v in ReplaceAstExpr(new AstBoolExpr(call.GenericParseTreeNode, false), context))
                                yield return v;
                        }

                        var arg = call.Arguments[0];
                        var type = (arg.Type == CheezType.Type) ? (arg.Value as CheezType) : arg.Type;

                        foreach (var v in ReplaceAstExpr(new AstBoolExpr(call.GenericParseTreeNode, type is IntType), context))
                            yield return v;
                        break;
                    }

                case "isbool":
                    {
                        if (call.Arguments.Count != 1)
                        {
                            context.ReportError(call.Name.GenericParseTreeNode, $"Comptime function '@{name}' requires one argument");
                            foreach (var v in ReplaceAstExpr(new AstBoolExpr(call.GenericParseTreeNode, false), context))
                                yield return v;
                        }

                        var arg = call.Arguments[0];
                        var type = (arg.Type == CheezType.Type) ? (arg.Value as CheezType) : arg.Type;

                        foreach (var v in ReplaceAstExpr(new AstBoolExpr(call.GenericParseTreeNode, type == CheezType.Bool), context))
                            yield return v;
                        break;
                    }

                case "isfloat":
                    {
                        if (call.Arguments.Count != 1)
                        {
                            context.ReportError(call.Name.GenericParseTreeNode, $"Comptime function '@{name}' requires one argument");
                            foreach (var v in ReplaceAstExpr(new AstBoolExpr(call.GenericParseTreeNode, false), context))
                                yield return v;
                        }

                        var arg = call.Arguments[0];
                        var type = (arg.Type == CheezType.Type) ? (arg.Value as CheezType) : arg.Type;

                        foreach (var v in ReplaceAstExpr(new AstBoolExpr(call.GenericParseTreeNode, type is FloatType), context))
                            yield return v;
                        break;
                    }

                case "ispointer":
                    {
                        if (call.Arguments.Count != 1)
                        {
                            context.ReportError(call.Name.GenericParseTreeNode, $"Comptime function '@{name}' requires one argument");
                            foreach (var v in ReplaceAstExpr(new AstBoolExpr(call.GenericParseTreeNode, false), context))
                                yield return v;
                        }

                        var arg = call.Arguments[0];
                        var type = (arg.Type == CheezType.Type) ? (arg.Value as CheezType) : arg.Type;

                        foreach (var v in ReplaceAstExpr(new AstBoolExpr(call.GenericParseTreeNode, type is PointerType), context))
                            yield return v;
                        break;
                    }

                case "isarray":
                    {
                        if (call.Arguments.Count != 1)
                        {
                            context.ReportError(call.Name.GenericParseTreeNode, $"Comptime function '@{name}' requires one argument");
                            foreach (var v in ReplaceAstExpr(new AstBoolExpr(call.GenericParseTreeNode, false), context))
                                yield return v;
                        }

                        var arg = call.Arguments[0];
                        var type = (arg.Type == CheezType.Type) ? (arg.Value as CheezType) : arg.Type;

                        foreach (var v in ReplaceAstExpr(new AstBoolExpr(call.GenericParseTreeNode, type is ArrayType), context))
                            yield return v;
                        break;
                    }

                case "isstring":
                    {
                        if (call.Arguments.Count != 1)
                        {
                            context.ReportError(call.Name.GenericParseTreeNode, $"Comptime function '@{name}' requires one argument");
                            foreach (var v in ReplaceAstExpr(new AstBoolExpr(call.GenericParseTreeNode, false), context))
                                yield return v;
                        }

                        var arg = call.Arguments[0];
                        var type = (arg.Type == CheezType.Type) ? (arg.Value as CheezType) : arg.Type;

                        foreach (var v in ReplaceAstExpr(new AstBoolExpr(call.GenericParseTreeNode, type is StringType), context))
                            yield return v;
                        break;
                    }

                case "isstruct":
                    {
                        if (call.Arguments.Count != 1)
                        {
                            context.ReportError(call.Name.GenericParseTreeNode, $"Comptime function '@{name}' requires one argument");
                            foreach (var v in ReplaceAstExpr(new AstBoolExpr(call.GenericParseTreeNode, false), context))
                                yield return v;
                        }

                        var arg = call.Arguments[0];
                        var type = (arg.Type == CheezType.Type) ? (arg.Value as CheezType) : arg.Type;

                        foreach (var v in ReplaceAstExpr(new AstBoolExpr(call.GenericParseTreeNode, type is StructType), context))
                            yield return v;
                        break;
                    }

                case "isenum":
                    {
                        if (call.Arguments.Count != 1)
                        {
                            context.ReportError(call.Name.GenericParseTreeNode, $"Comptime function '@{name}' requires one argument");
                            foreach (var v in ReplaceAstExpr(new AstBoolExpr(call.GenericParseTreeNode, false), context))
                                yield return v;
                        }

                        var arg = call.Arguments[0];
                        var type = (arg.Type == CheezType.Type) ? (arg.Value as CheezType) : arg.Type;

                        foreach (var v in ReplaceAstExpr(new AstBoolExpr(call.GenericParseTreeNode, type is EnumType), context))
                            yield return v;
                        break;
                    }

                case "isfunction":
                    {
                        if (call.Arguments.Count != 1)
                        {
                            context.ReportError(call.Name.GenericParseTreeNode, $"Comptime function '@{name}' requires one argument");
                            foreach (var v in ReplaceAstExpr(new AstBoolExpr(call.GenericParseTreeNode, false), context))
                                yield return v;
                        }

                        var arg = call.Arguments[0];
                        var type = (arg.Type == CheezType.Type) ? (arg.Value as CheezType) : arg.Type;

                        foreach (var v in ReplaceAstExpr(new AstBoolExpr(call.GenericParseTreeNode, type is FunctionType), context))
                            yield return v;
                        break;
                    }

                case "typeseq":
                    {
                        if (call.Arguments.Count != 2)
                        {
                            context.ReportError(call.Name.GenericParseTreeNode, $"Comptime function '@{name}' requires two arguments");
                            foreach (var v in ReplaceAstExpr(new AstBoolExpr(call.GenericParseTreeNode, false), context))
                                yield return v;
                        }

                        var type1 = (call.Arguments[0].Type == CheezType.Type) ? (call.Arguments[0].Value as CheezType) : call.Arguments[0].Type;
                        var type2 = (call.Arguments[1].Type == CheezType.Type) ? (call.Arguments[1].Value as CheezType) : call.Arguments[1].Type;

                        var eq = type1 == type2;

                        foreach (var v in ReplaceAstExpr(new AstBoolExpr(call.GenericParseTreeNode, eq), context))
                            yield return v;
                        break;
                    }

                case "error":
                    {
                        if (call.Arguments.Count != 1)
                        {
                            context.ReportError(call.Name.GenericParseTreeNode, $"Comptime function '@{name}' requires one argument");
                            foreach (var v in ReplaceAstExpr(new AstBoolExpr(call.GenericParseTreeNode, false), context))
                                yield return v;
                        }

                        var arg = call.Arguments[0];
                        if (arg.Type != CheezType.String || !arg.IsCompTimeValue)
                        {
                            context.ReportError(arg.GenericParseTreeNode, $"Argument of '@{name}' must be a string literal");
                        }
                        else
                        {
                            string value = arg.Value as string;
                            if (value != null)
                                context.ReportError(call.GenericParseTreeNode, value);
                        }

                        foreach (var v in ReplaceAstExpr(new AstEmptyExpr(call.GenericParseTreeNode), context))
                            yield return v;
                        break;
                    }

                default:
                    context.ReportError(call.Name.GenericParseTreeNode, $"Unknown comptime function '@{name}'");
                    call.Type = CheezType.Error;
                    break;
            }

            yield break;
        }

        public override IEnumerable<object> VisitStructValueExpression(AstStructValueExpr str, SemanticerData context = null)
        {
            var scope = context.Scope;
            str.Scope = scope;

            // types
            foreach (var v in str.TypeExpr.Accept(this, context.Clone(ExpectedType: null)))
                yield return v;
            if (str.TypeExpr.Value is CheezType t)
                str.Type = t;
            else
                context.ReportError(str.TypeExpr.GenericParseTreeNode, $"Expected type, got {str.TypeExpr.Type}");

            if (str.Type is StructType s)
            {
                if (str.MemberInitializers.Length > s.Declaration.Members.Count)
                {
                    context.ReportError(str.ParseTreeNode?.Type, $"Struct initialization has to many values");
                }
                else
                {
                    int namesProvided = 0;
                    foreach (var m in str.MemberInitializers)
                    {
                        if (m.Name != null)
                        {
                            if (!s.Declaration.Members.Any(m2 => m2.Name.Name == m.Name))
                            {
                                context.ReportError(m.GenericParseTreeNode.Name, $"'{m.Name}' is not a member of struct {s.Declaration.Name}");
                            }
                            namesProvided++;
                        }
                    }

                    if (namesProvided == 0)
                    {
                        for (int i = 0; i < str.MemberInitializers.Length; i++)
                        {
                            foreach (var v in str.MemberInitializers[i].Value.Accept(this, context.Clone(ExpectedType: null)))
                            {
                                if (v is ReplaceAstExpr r)
                                    str.MemberInitializers[i].Value = r.NewExpression;
                                else
                                    yield return v;
                            }

                            var mem = s.Declaration.Members[i];
                            var mi = str.MemberInitializers[i];
                            mi.Name = mem.Name.Name;

                            if (CastIfLiteral(mi.Value.Type, mem.Type, out var miType))
                            {
                                mi.Value.Type = miType;
                            }
                            else
                            {
                                context.ReportError(mi.GenericParseTreeNode.Value, $"Value of type '{mi.Value.Type}' cannot be assigned to struct member '{mem.Name}' with type '{mem.Type}'");
                            }
                        }
                    }
                    else if (namesProvided == str.MemberInitializers.Length)
                    {

                    }
                    else
                    {
                        context.ReportError(str.ParseTreeNode?.Type, $"Either all or no values must have a name");
                    }
                }
            }
            else
            {
                context.ReportError(str.ParseTreeNode?.Type, $"'{str.TypeExpr}' is not a struct type");
            }

            yield break;
        }

        public override IEnumerable<object> VisitArrayAccessExpression(AstArrayAccessExpr arr, SemanticerData context = null)
        {
            foreach (var v in arr.SubExpression.Accept(this, context.Clone(ExpectedType: null)))
                if (v is ReplaceAstExpr r)
                    arr.SubExpression = r.NewExpression;
                else yield return v;

            foreach (var v in arr.Indexer.Accept(this, context.Clone(ExpectedType: null)))
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
            else if (arr.SubExpression.Type is StringType)
            {
                arr.Type = IntType.GetIntType(1, true);
            }
            else
            {
                arr.Type = CheezType.Error;
                context.ReportError(arr.SubExpression.GenericParseTreeNode, $"[] operator can only be used with array and pointer types, got '{arr.SubExpression.Type}'");
            }

            if (arr.Indexer.Type is IntType i)
            {
                if (i == IntType.LiteralType)
                    arr.Indexer.Type = IntType.DefaultType;
            }
            else
            {
                arr.Type = CheezType.Error;
                context.ReportError(arr.SubExpression.GenericParseTreeNode, $"Indexer of [] operator has to be an integer, got '{arr.Indexer.Type}");
            }

            arr.SetFlag(ExprFlags.IsLValue);
        }

        public override IEnumerable<object> VisitUnaryExpression(AstUnaryExpr bin, SemanticerData context = null)
        {
            var scope = context.Scope;
            bin.Scope = scope;

            foreach (var v in bin.SubExpr.Accept(this, context.Clone(ExpectedType: null)))
                if (v is ReplaceAstExpr r)
                    bin.SubExpr = r.NewExpression;
                else
                    yield return v;

            if (bin.SubExpr is AstNumberExpr n)
            {
                if (n.Data.Type == NumberData.NumberType.Int)
                {
                    switch (bin.Operator)
                    {
                        case "-":
                            foreach (var vv in ReplaceAstExpr(new AstNumberExpr(bin.GenericParseTreeNode, n.Data.Negate()), context.Clone(ExpectedType: null)))
                                yield return vv;
                            yield break;

                        default:
                            throw new NotImplementedException("Compile time evaluation of int literals in unary operator other than '-'");
                    }
                }
                else
                {
                    throw new NotImplementedException("Compile time evaluation of float literals in unary operator");
                }
            }

            bool subIsLit = IsNumberLiteralType(bin.SubExpr.Type);
            if (subIsLit)
            {
                if (bin.SubExpr.Type == IntType.LiteralType)
                {
                    bin.SubExpr.Type = IntType.DefaultType;
                }
                else
                    bin.SubExpr.Type = FloatType.DefaultType;
            }

            var ops = scope.GetOperators(bin.Operator, bin.SubExpr.Type);

            if (ops.Count > 1)
            {
                context.ReportError(bin.GenericParseTreeNode, $"Multiple operators match the type '{bin.SubExpr.Type}");
                yield break;
            }
            else if (ops.Count == 0)
            {
                context.ReportError(bin.GenericParseTreeNode, $"No operator matches the type '{bin.SubExpr.Type}'");
                yield break;
            }

            var op = ops[0];
            bin.Type = op.ResultType;


            if (bin.SubExpr.IsCompTimeValue)
            {
                var result = op.Execute(bin.SubExpr.Value);

                if (result != null)
                {
                    if (result is bool b)
                    {
                        bin.Type = CheezType.Bool;
                        bin.Value = b;
                        bin.IsCompTimeValue = true;
                    }
                }
            }

            yield break;
        }

        public override IEnumerable<object> VisitBinaryExpression(AstBinaryExpr bin, SemanticerData context = null)
        {
            var scope = context.Scope;
            bin.Scope = scope;

            bin.Left.Scope = bin.Scope;
            bin.Right.Scope = bin.Scope;
            foreach (var v in bin.Left.Accept(this, context.Clone(ExpectedType: null)))
                if (v is ReplaceAstExpr r)
                    bin.Left = r.NewExpression;
                else
                    yield return v;

            foreach (var v in bin.Right.Accept(this, context.Clone(ExpectedType: null)))
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
                context.ReportError(bin.GenericParseTreeNode, $"Multiple operators match the types '{bin.Left.Type}' and '{bin.Right.Type}'");
                yield break;
            }
            else if (ops.Count == 0)
            {
                context.ReportError(bin.GenericParseTreeNode, $"No operator matches the types '{bin.Left.Type}' and '{bin.Right.Type}'");
                yield break;
            }

            var op = ops[0];
            bin.Type = op.ResultType;

            if (bin.Operator == "and" || bin.Operator == "or")
            {
                context.Function.LocalVariables.Add(bin);
            }


            if (bin.Left.IsCompTimeValue && bin.Right.IsCompTimeValue)
            {
                var result = op.Execute(bin.Left.Value, bin.Right.Value);

                if (result != null)
                {
                    if (result is bool b)
                    {
                        bin.Type = CheezType.Bool;
                        bin.Value = b;
                        bin.IsCompTimeValue = true;
                    }
                }
            }


            yield break;
        }

        private bool InferGenericParameterType(Dictionary<string, CheezType> result, AstExpression param, ref CheezType arg)
        {
            switch (param)
            {
                case AstIdentifierExpr i when i.IsPolymorphic && arg is PolyType:
                    return false;

                case AstIdentifierExpr i when i.IsPolymorphic:
                    if (result != null && !result.TryGetValue(i.Name, out var _))
                    {
                        if (arg == IntType.LiteralType)
                            arg = IntType.DefaultType;
                        if (arg == FloatType.LiteralType)
                            arg = FloatType.DefaultType;
                        result[i.Name] = arg;
                        return true;
                    }
                    return false;

                case AstArrayTypeExpr pArr:
                    if (arg is ArrayType aArr)
                    {
                        var t = aArr.TargetType;
                        var changes = InferGenericParameterType(result, pArr.Target, ref t);
                        aArr.TargetType = t;
                        return changes;
                    }
                    return false;

                case AstPointerTypeExpr pPtr:
                    if (pPtr.IsReference)
                    {
                        return InferGenericParameterType(result, pPtr.Target, ref arg);
                    }
                    if (arg is PointerType aPtr)
                    {
                        var t = aPtr.TargetType;
                        var changes = InferGenericParameterType(result, pPtr.Target, ref t);
                        aPtr.TargetType = t;
                        return changes;
                    }
                    return false;

                case AstCallExpr call:
                    if (arg is StructType s && s.Declaration.IsPolyInstance && s.Declaration.Parameters.Count == call.Arguments.Count)
                    {
                        bool changes = false;
                        var @struct = s.Declaration;
                        for (int i = 0; i < @struct.Parameters.Count; i++)
                        {
                            var p = call.Arguments[i];
                            var a = @struct.Parameters[i].Value as CheezType;

                            if (InferGenericParameterType(result, p, ref a))
                                changes = true;
                            @struct.Parameters[i].Value = a;
                        }

                        return changes;
                    }

                    return false;

                default:
                    if (arg is PolyType)
                    {
                        arg = param.Type;
                        return true;
                    }
                    return false;
            }
        }

        private IEnumerable<object> CallPolyFunction(AstCallExpr call, GenericFunctionType g, SemanticerData context)
        {
            if (g.Declaration.Parameters.Count != call.Arguments.Count)
            {
                context.ReportError(call.GenericParseTreeNode, $"Wrong number of arguments in function call");
            }

            var types = new Dictionary<string, CheezType>();
            int argCount = call.Arguments.Count;

            var parameterTypeExprs = g.Declaration.Parameters.Select(p => p.TypeExpr.Clone()).ToArray();
            var returnTypeExpr = g.Declaration.ReturnTypeExpr?.Clone();

            //foreach (var v in returnTypeExpr.Accept(this, context))
            //    yield return v;

            // try to infer type from expected type before checking arguments
            if (context.ExpectedType != null && returnTypeExpr != null)
            {
                var t = context.ExpectedType;
                if (InferGenericParameterType(types, returnTypeExpr, ref t))
                {
                    context.ExpectedType = t;

                    var scope = new Scope("temp", g.Declaration.Scope);
                    foreach (var kv in types)
                    {
                        scope.DefineTypeSymbol(kv.Key, kv.Value);
                    }

                    foreach (var p in parameterTypeExprs)
                    {
                        foreach (var v in p.Accept(this, new SemanticerData(Scope: scope, Text: context.Text, Function: g.Declaration)))
                            yield return v;
                    }
                }
            }

            { // check arguments
                for (int i = 0; i < call.Arguments.Count; i++)
                {
                    foreach (var v in call.Arguments[i].Accept(this, context.Clone(ExpectedType: parameterTypeExprs[i].Value as CheezType)))
                        if (v is ReplaceAstExpr r)
                            call.Arguments[i] = r.NewExpression;
                        else
                            yield return v;
                }
            }

            // find out types of generic parameters
            while (true)
            {
                bool changes = false;

                for (int i = 0; i < argCount; i++)
                {
                    var t = call.Arguments[i].Type;
                    changes |= InferGenericParameterType(types, parameterTypeExprs[i], ref t);
                    call.Arguments[i].Type = t;
                }

                if (context.ExpectedType != null && returnTypeExpr != null)
                {
                    var t = context.ExpectedType;
                    changes |= InferGenericParameterType(types, returnTypeExpr, ref t);
                    context.ExpectedType = t;
                }

                if (!changes)
                    break;
            }

            // check if all types are present
            {
                bool ok = true;
                foreach (var pt in g.Declaration.PolymorphicTypeExprs)
                {
                    if (!types.ContainsKey(pt.Key))
                    {
                        ok = false;
                        context.ReportError(context.Text, call.GenericParseTreeNode, $"Couldn't infer type for polymorphic parameter ${pt.Key}");
                    }
                }

                if (!ok)
                {
                    call.Function.Type = CheezType.Error;
                    yield break;
                }
            }

            AstFunctionDecl instance = null;
            foreach (var pi in g.Declaration.PolymorphicInstances)
            {
                bool eq = true;
                foreach (var kv in pi.PolymorphicTypes)
                {
                    var existingType = kv.Value;
                    var newType = types[kv.Key];
                    if (existingType != newType)
                    {
                        eq = false;
                        break;
                    }
                }

                if (eq)
                {
                    instance = pi;
                    break;
                }
            }
            if (instance == null)
            {
                // instantiate function
                instance = g.Declaration.Clone() as AstFunctionDecl;
                g.Declaration.PolymorphicInstances.Add(instance);
                instance.IsGeneric = false;
                instance.IsPolyInstance = true;
                instance.PolymorphicTypes = types;
                //g.Declaration.AddPolyInstance(types, instance);

                instance.HeaderScope = NewScope($"fn {instance.Name}()", instance.Scope);
                instance.SubScope = NewScope("{}", instance.HeaderScope);

                foreach (var kv in types)
                {
                    instance.HeaderScope.DefineTypeSymbol(kv.Key, kv.Value); // @Todo
                }

                var errorHandler = new SilentErrorHandler();
                var subContext = new SemanticerData(g.Declaration.Scope, g.Declaration.Text, null, g.Declaration.ImplBlock, null, errorHandler);

                foreach (var v in VisitFunctionHeader(instance, subContext)) // @Todo: implTarget, text
                    yield return v;
                foreach (var v in VisitFunctionBody(instance, subContext)) // @Todo: text
                    yield return v;

                if (errorHandler.HasErrors)
                {
                    context.ReportError(call.GenericParseTreeNode, "Failed to invoke polymorphic function", errorHandler.Errors);
                }
            }

            call.Function = new AstFunctionExpression(call.Function.GenericParseTreeNode, instance, call.Function);
        }

        public override IEnumerable<object> VisitCallExpression(AstCallExpr call, SemanticerData context = null)
        {
            var scope = context.Scope;
            call.Scope = scope;

            foreach (var v in call.Function.Accept(this, context.Clone(ExpectedType: null)))
                if (v is ReplaceAstExpr r)
                    call.Function = r.NewExpression;
                else
                    yield return v;

            if (call.Function is AstDotExpr d && d.IsDoubleColon)
            {
                call.Arguments.Insert(0, d.Left);
            }

            if (call.Function.Type is GenericFunctionType g)
            {
                foreach (var v in CallPolyFunction(call, g, context))
                    yield return v;
            }
            else if (call.Function.Type is GenericStructType genStruct)
            {
                foreach (var v in CallPolyStruct(call, genStruct, context))
                    yield return v;
                yield break;
            }
            else
            {
                for (int i = 0; i < call.Arguments.Count; i++)
                {
                    foreach (var v in call.Arguments[i].Accept(this, context.Clone(ExpectedType: CheezType.Type)))
                        if (v is ReplaceAstExpr r)
                            call.Arguments[i] = r.NewExpression;
                        else
                            yield return v;
                }
            }

            if (call.Function.Type is FunctionType f)
            {
                if (f.ParameterTypes.Length != call.Arguments.Count)
                {
                    context.ReportError(call.GenericParseTreeNode, $"Wrong number of arguments in function call. Expected {f.ParameterTypes.Length}, got {call.Arguments.Count}");
                }

                call.Type = f.ReturnType;

                for (int i = 0; i < call.Arguments.Count && i < f.ParameterTypes.Length; i++)
                {
                    var arg = call.Arguments[i];
                    var expectedType = f.ParameterTypes[i];

                    if (!CastIfLiteral(arg.Type, expectedType, out var t))
                    {
                        context.ReportError(context.Text, arg.GenericParseTreeNode, $"Argument type does not match parameter type. Expected {expectedType}, got {arg.Type}");
                    }

                    if (expectedType is ReferenceType && !arg.GetFlag(ExprFlags.IsLValue))
                        context.ReportError(arg.GenericParseTreeNode, "Can't convert an rvalue to a reference");

                    arg.Type = t;
                }
            }
            else if (call.Function.Type != CheezType.Error)
            {
                context.ReportError(call.Function.GenericParseTreeNode, $"Type '{call.Function.Type}' is not a callable type");
            }

            yield break;
        }

        public override IEnumerable<object> VisitIdentifierExpression(AstIdentifierExpr ident, SemanticerData context = null)
        {
            if (ident.IsPolymorphic)
            {
                if (context.ImplBlock != null && context.Function == null)
                {
                    ident.Scope = null;
                    ident.Value = new PolyType(ident.Name);
                    ident.Type = CheezType.Type;
                    yield break;
                }
                else if (context.Function != null)
                {
                    if (context.Function.IsPolyInstance)
                    {
                        ident.SetIsPolymorphic(false);
                    }
                    else
                    {
                        ident.Scope = context.Scope;
                        ident.Value = new PolyType(ident.Name);
                        ident.Type = CheezType.Type;
                        yield break;
                    }
                }
                //else
                //{
                //    yield return new GenericError(data.Text, ident.GenericParseTreeNode, "Polymorphic type not allowed here");
                //}
            }

            var scope = context.Scope;
            ident.Scope = scope;

            var v = scope.GetSymbol(ident.Name);
            if (v == null)
            {
                yield return new WaitForSymbol(context.Text, ident.GenericParseTreeNode, scope, ident.Name);
                v = scope.GetSymbol(ident.Name);
            }
            ident.Symbol = v;
            ident.Value = v;

            if (v is Using u)
            {
                var e = u.Expr.Clone();
                e.GenericParseTreeNode = ident.GenericParseTreeNode;
                foreach (var vv in ReplaceAstExpr(e, context.Clone(ExpectedType: null)))
                    yield return vv;
                yield break;
            }
            else if (v is CompTimeVariable comp)
            {
                ident.Type = comp.Type;
                ident.Value = comp.Value;
            }
            else
            {
                ident.Type = v.Type;
            }


            if (!v.IsConstant)
                ident.SetFlag(ExprFlags.IsLValue);

            yield break;
        }

        public override IEnumerable<object> VisitNumberExpression(AstNumberExpr num, SemanticerData context = null)
        {
            num.Type = IntType.LiteralType;
            yield break;
        }

        public override IEnumerable<object> VisitStringLiteral(AstStringLiteral str, SemanticerData context = null)
        {
            str.Type = CheezType.String;
            yield break;
        }

        public override IEnumerable<object> VisitBoolExpression(AstBoolExpr bo, SemanticerData context = null)
        {
            bo.Type = CheezType.Bool;
            yield break;
        }

        public override IEnumerable<object> VisitAddressOfExpression(AstAddressOfExpr add, SemanticerData context = null)
        {
            add.Scope = context.Scope;

            foreach (var v in add.SubExpression.Accept(this, context.Clone(ExpectedType: null)))
                if (v is ReplaceAstExpr r)
                    add.SubExpression = r.NewExpression;
                else
                    yield return v;

            var subType = add.SubExpression.Type;
            if (add.SubExpression.Type is ReferenceType t)
            {
                subType = t.TargetType;
            }

            add.Type = PointerType.GetPointerType(subType);
            if (!add.SubExpression.GetFlag(ExprFlags.IsLValue))
                context.ReportError(add.SubExpression.GenericParseTreeNode, $"Sub expression of & is not a lvalue");

            yield break;
        }

        public override IEnumerable<object> VisitDereferenceExpression(AstDereferenceExpr deref, SemanticerData context = null)
        {
            deref.Scope = context.Scope;

            foreach (var v in deref.SubExpression.Accept(this, context.Clone(ExpectedType: null)))
                if (v is ReplaceAstExpr r)
                    deref.SubExpression = r.NewExpression;
                else
                    yield return v;

            if (deref.SubExpression.Type is PointerType p)
            {
                deref.Type = p.TargetType;
            }
            else if (deref.SubExpression.Type is StringType s)
            {
                deref.Type = IntType.GetIntType(1, true);
            }
            else
            {
                context.ReportError(deref.SubExpression.GenericParseTreeNode, $"Sub expression of & is not a pointer");
            }

            deref.SetFlag(ExprFlags.IsLValue);
        }

        public override IEnumerable<object> VisitCastExpression(AstCastExpr cast, SemanticerData context = null)
        {
            cast.Scope = context.Scope;

            // type
            foreach (var v in cast.TypeExpr.Accept(this, context.Clone(ExpectedType: null)))
                yield return v;
            if (cast.TypeExpr.Value is CheezType t)
                cast.Type = t;
            else
                context.ReportError(cast.TypeExpr.GenericParseTreeNode, $"Expected type, got {cast.TypeExpr.Type}");

            // check subExpression
            foreach (var v in cast.SubExpression.Accept(this, context.Clone(ExpectedType: cast.Type)))
                if (v is ReplaceAstExpr r)
                    cast.SubExpression = r.NewExpression;
                else
                    yield return v;


            if (!CastIfLiteral(cast.SubExpression.Type, cast.Type, out var type))
            { }
            //{
            //    yield return new LambdaError(eh => eh.ReportError(data.Text, cast.ParseTreeNode, $"Can't cast a value of to '{cast.SubExpression.Type}' to '{cast.Type}'"));
            //}
            cast.SubExpression.Type = type;

            yield break;
        }

        public override IEnumerable<object> VisitDotExpression(AstDotExpr dot, SemanticerData context = null)
        {
            dot.Scope = context.Scope;

            foreach (var v in dot.Left.Accept(this, context.Clone(ExpectedType: null, ErrorHandler: context.ErrorHandler)))
                if (v is ReplaceAstExpr r)
                    dot.Left = r.NewExpression;
                else
                    yield return v;

            //while (dot.Left.Type is ReferenceType r)
            //    dot.Left.Type = r.TargetType;

            if (dot.Left.Type == IntType.LiteralType)
                dot.Left.Type = IntType.DefaultType;
            else if (dot.Left.Type == FloatType.LiteralType)
                dot.Left.Type = FloatType.DefaultType;

            if (dot.IsDoubleColon)
            {
                yield return new LambdaCondition(() => dot.Scope.GetImplFunction(dot.Left.Type, dot.Right) != null,
                    eh => eh.ReportError(context.Text, dot.GenericParseTreeNode, $"No impl function  '{dot.Right}' exists for type '{dot.Left.Type}'"));
                var func = dot.Scope.GetImplFunction(dot.Left.Type, dot.Right);
                dot.Type = func.Type;
                dot.Value = func;
            }
            else
            {
                while (dot.Left.Type is PointerType p)
                {
                    dot.Left = new AstDereferenceExpr(dot.Left.GenericParseTreeNode, dot.Left);
                    dot.Left.Type = p.TargetType;
                }

                var leftType = dot.Left.Type;
                if (leftType is ReferenceType r)
                    leftType = r.TargetType;

                if (leftType is StructType s)
                {
                    var member = s.Declaration.Members.FirstOrDefault(m => m.Name.Name == dot.Right);
                    if (member == null)
                    {
                        yield return new LambdaCondition(() => dot.Scope.GetImplFunction(leftType, dot.Right) != null,
                            eh => eh.ReportError(context.Text, dot.GenericParseTreeNode, $"No impl function  '{dot.Right}' exists for type '{leftType}'"));
                        var func = dot.Scope.GetImplFunction(leftType, dot.Right);
                        dot.Type = func.Type;
                        dot.IsDoubleColon = true;
                        dot.Value = func;
                    }
                    else
                    {
                        dot.Type = member.Type;
                    }
                }
                else
                {
                    yield return new LambdaCondition(() => dot.Scope.GetImplFunction(leftType, dot.Right) != null,
                        eh => eh.ReportError(context.Text, dot.GenericParseTreeNode, $"No impl function  '{dot.Right}' exists for type '{leftType}'"));
                    var func = dot.Scope.GetImplFunction(leftType, dot.Right);
                    dot.Type = func.Type;
                    dot.IsDoubleColon = true;
                    dot.Value = func;
                }

                dot.SetFlag(ExprFlags.IsLValue);
            }

            yield break;
        }

        public override IEnumerable<object> VisitTypeExpr(AstTypeExpr astArrayTypeExpr, SemanticerData data = null)
        {
            astArrayTypeExpr.Value = astArrayTypeExpr.Type;
            yield break;
        }

        public override IEnumerable<object> VisitArrayTypeExpr(AstArrayTypeExpr arr, SemanticerData context = default)
        {
            arr.Scope = context.Scope;
            foreach (var v in arr.Target.Accept(this, context.Clone(ExpectedType: null)))
                yield return v;

            arr.Type = CheezType.Type;
            if (arr.Target.Value is CheezType t)
            {
                arr.Value = ArrayType.GetArrayType(t);
            }
            else
            {
                context.ReportError(context.Text, arr.Target.GenericParseTreeNode, $"Expected type, got {arr.Target.Type}");
            }
        }

        public override IEnumerable<object> VisitPointerTypeExpr(AstPointerTypeExpr ptr, SemanticerData context = default)
        {
            ptr.Scope = context.Scope;
            foreach (var v in ptr.Target.Accept(this, context.Clone(ExpectedType: null)))
                yield return v;

            ptr.Type = CheezType.Type;
            if (ptr.Target.Value is CheezType t)
            {
                if (ptr.IsReference)
                    ptr.Value = ReferenceType.GetRefType(t);
                else
                    ptr.Value = PointerType.GetPointerType(t);
            }
            else
            {
                context.ReportError(ptr.Target.GenericParseTreeNode, $"Expected type, got {ptr.Target.Type}");
            }
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
            else if (sourceType == FloatType.LiteralType && (targetType is FloatType))
            {
                outSource = targetType;
            }
            else if (targetType == CheezType.Any)
            {
                if (sourceType == IntType.LiteralType)
                    outSource = IntType.DefaultType;
                else if (sourceType == FloatType.LiteralType)
                    outSource = FloatType.DefaultType;
                return true;
            }
            else if (sourceType != targetType)
            {
                if (sourceType == IntType.LiteralType)
                    outSource = IntType.DefaultType;
                else if (sourceType == FloatType.LiteralType)
                    outSource = FloatType.DefaultType;
                return false;
            }

            return true;
        }

        private void CastIfAny(CheezType source, CheezType target)
        {
            if (source == CheezType.Any && target == CheezType.Any)
                return;

            if (source == CheezType.Any)
            {

            }
            else if (source == CheezType.Any)
            {

            }
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

        private IEnumerable<object> ReplaceAstStmt(AstStatement stmt, SemanticerData data)
        {
            foreach (var v in stmt.Accept(this, data))
            {
                if (v is ReplaceAstStmt r)
                {
                    stmt = r.NewStatement;
                    break;
                }
                else
                    yield return v;
            }

            yield return new ReplaceAstStmt(stmt);
        }

        private void CollectPolymorphicTypes(IEnumerable<AstExpression> expr, out Dictionary<string, AstExpression> types)
        {
            types = new Dictionary<string, AstExpression>();

            foreach (var e in expr)
            {
                CollectPolymorphicTypes(e, ref types);
            }
        }

        private void CollectPolymorphicTypes(AstExpression expr, ref Dictionary<string, AstExpression> types)
        {
            switch (expr)
            {
                case AstIdentifierExpr i when i.IsPolymorphic:
                    types[i.Name] = expr;
                    break;

                case AstPointerTypeExpr p:
                    CollectPolymorphicTypes(p.Target, ref types);
                    break;

                case AstArrayTypeExpr p:
                    CollectPolymorphicTypes(p.Target, ref types);
                    break;

                case AstCallExpr c:
                    CollectPolymorphicTypes(c.Function, ref types);
                    foreach (var a in c.Arguments)
                        CollectPolymorphicTypes(a, ref types);
                    break;
            }
        }

        private AstNumberExpr ConstInt(PTExpr node, BigInteger value)
        {
            return new AstNumberExpr(node, new NumberData(value));
        }
    }
}
