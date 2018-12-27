//using Cheez.Compiler.Ast;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace Cheez.Compiler.SemanticAnalysis
//{
//    class DeclarationAnalyzer
//    {
//        private Stack<IErrorHandler> mErrorHandler = new Stack<IErrorHandler>();
//        private Workspace mWorkspace;

//        private List<AstStructDecl> mPolyStructs = new List<AstStructDecl>();
//        private List<AstStructDecl> mPolyStructInstances = new List<AstStructDecl>();
//        private List<AstStructDecl> mStructs = new List<AstStructDecl>();

//        private List<AstTraitDeclaration> mTraits = new List<AstTraitDeclaration>();
//        private List<AstEnumDecl> mEnums = new List<AstEnumDecl>();
//        private List<AstVariableDecl> mVariables = new List<AstVariableDecl>();
//        private List<AstTypeAliasDecl> mTypeDefs = new List<AstTypeAliasDecl>();
//        private List<AstImplBlock> mImpls = new List<AstImplBlock>();

//        private List<AstFunctionDecl> mFunctions = new List<AstFunctionDecl>();
//        private List<AstFunctionDecl> mPolyFunctions = new List<AstFunctionDecl>();
//        private List<AstFunctionDecl> mFunctionInstances = new List<AstFunctionDecl>();

//        [SkipInStackFrame]
//        private void ReportError(ILocation lc, string message)
//        {
//            var (callingFunctionFile, callingFunctionName, callLineNumber) = Util.GetCallingFunction().GetValueOrDefault(("", "", -1));
//            var file = mWorkspace.GetFile(lc.Beginning.file);
//            mErrorHandler.Peek().ReportError(file, lc, message, null, callingFunctionFile, callingFunctionName, callLineNumber);
//        }

//        [SkipInStackFrame]
//        private void ReportError(ILocation lc, string message, params string[] details)
//        {
//            var (callingFunctionFile, callingFunctionName, callLineNumber) = Util.GetCallingFunction().GetValueOrDefault(("", "", -1));
//            var file = mWorkspace.GetFile(lc.Beginning.file);
//            mErrorHandler.Peek().ReportError(new Error
//            {
//                Text = file,
//                Location = lc,
//                Message = message,
//                Details = details.ToList()
//            }, callingFunctionFile, callingFunctionName, callLineNumber);
//        }

//        public void CollectDeclarations(Workspace ws, IErrorHandler errorHandler)
//        {
//            mWorkspace = ws;
//            mErrorHandler.Push(errorHandler);

//            var globalScope = ws.GlobalScope;

//            // pass 1:
//            // collect types (structs, enums, traits)
//            foreach (var s in ws.Statements)
//            {
//                switch (s)
//                {
//                    case AstStructDecl @struct:
//                        {
//                            @struct.Scope = globalScope;
//                            Pass1StructDeclaration(@struct);
//                            mStructs.Add(@struct);
//                            break;
//                        }

//                    case AstTraitDeclaration @trait:
//                        {
//                            trait.Scope = globalScope;
//                            Pass1TraitDeclaration(@trait);
//                            mTraits.Add(@trait);
//                            break;
//                        }

//                    case AstEnumDecl @enum:
//                        {
//                            @enum.Scope = globalScope;
//                            Pass1EnumDeclaration(@enum);
//                            mEnums.Add(@enum);
//                            break;
//                        }

//                    case AstVariableDecl @var:
//                        {
//                            @var.Scope = globalScope;
//                            mVariables.Add(@var);
//                            break;
//                        }

//                    case AstFunctionDecl func:
//                        {
//                            func.Scope = globalScope;
//                            mFunctions.Add(func);
//                            break;
//                        }

//                    case AstImplBlock impl:
//                        {
//                            impl.Scope = globalScope;
//                            mImpls.Add(impl);
//                            break;
//                        }

//                    case AstTypeAliasDecl type:
//                        {
//                            type.Scope = globalScope;
//                            mTypeDefs.Add(type);
//                            break;
//                        }
//                }
//            }

//            // pass 2:
//            // typedefs
//            foreach (var t in mTypeDefs)
//            {
//                Pass2TypeAlias(t);
//            }

//            // pass 3:
//            // functions, trait functions
//            foreach (var f in mFunctions)
//            {
//                Pass3FunctionDeclaration(f);
//            }
//            foreach (var t in mTraits)
//            {
//                Pass3TraitDeclaration(t);
//            }

//            // pass 4:
//            // impl blocks
//            foreach (var t in mImpls)
//            {
//                Pass4ImplDeclaration(t);
//            }

//            // pass 5:
//            // global variables
//            foreach (var t in mVariables)
//            {
//                Pass5GlobalVariableDeclaration(t);
//            }


//            // pass 6:
//            // struct member types
//            foreach (var str in mStructs)
//            {
//                if (str.IsPolymorphic)
//                    continue;
//                Pass6StructMembers(str);
//            }
//            foreach (var str in mPolyStructInstances)
//                Pass6StructMembers(str);

//            //
//            Console.WriteLine("--------------------------");
//            Console.WriteLine("regular functions");
//            foreach (var f in mFunctionInstances)
//            {
//                Console.WriteLine(f);
//            }

//            Console.WriteLine("--------------------------");
//            Console.WriteLine("poly functions");
//            foreach (var f in mPolyFunctions)
//            {
//                Console.WriteLine(f);
//            }
//            Console.WriteLine("--------------------------");
//        }

//        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//        #region Pass 1

//        private void Pass1TraitDeclaration(AstTraitDeclaration trait)
//        {
//            trait.Scope.TypeDeclarations.Add(trait);
//            trait.Type = new TraitType(trait);
//            if (!trait.Scope.DefineTypeSymbol(trait.Name.Name, trait.Type))
//            {
//                ReportError(trait.Name, $"A symbol with name '{trait.Name.Name}' already exists in current scope");
//            }
//        }

//        private void Pass1EnumDeclaration(AstEnumDecl @enum)
//        {
//            //int currentValue = 0;

//            //var nameSet = new HashSet<string>();
//            //foreach (var member in @enum.Members)
//            //{
//            //    if (nameSet.Contains(member.Name))
//            //        ReportError(member.ParseTreeNode, $"An enum member with name '{member.Name}' already exists");
//            //    nameSet.Add(member.Name);

//            //    member.Value = currentValue;

//            //    currentValue++;
//            //}

//            @enum.Scope.TypeDeclarations.Add(@enum);
//            @enum.Type = new EnumType(@enum);

//            if (!@enum.Scope.DefineTypeSymbol(@enum.Name.Name, @enum.Type))
//            {
//                ReportError(@enum.Name, $"A symbol with name '{@enum.Name.Name}' already exists in current scope");
//            }
//        }

//        private void Pass1StructDeclaration(AstStructDecl @struct)
//        {
//            CheezType type;

//            if (@struct.Parameters.Count > 0)
//            {
//                @struct.IsPolymorphic = true;
//                mPolyStructs.Add(@struct);

//                var nameSet = new HashSet<string>();

//                // check parameter types
//                foreach (var p in @struct.Parameters)
//                {
//                    if (nameSet.Contains(p.Name.Name))
//                        ReportError(p.Name, "Duplicate parameter names are not allowed");
//                    nameSet.Add(p.Name.Name);

//                    switch (p.TypeExpr)
//                    {
//                        case AstIdentifierExpr i:
//                            {
//                                var sym = @struct.Scope.GetSymbol(i.Name, false);
//                                if (!i.IsPolymorphic && sym is CompTimeVariable c && c.Type == CheezType.Type)
//                                {
//                                    switch (c.Value)
//                                    {
//                                        case CheezTypeType _:
//                                        case IntType _:
//                                        case FloatType _:
//                                        case BoolType _:
//                                        case CharType _:
//                                        case StringLiteralType _:
//                                            p.Type = c.Value as CheezType;
//                                            continue;
//                                    }
//                                }
//                                break;
//                            }
//                    }

//                    ReportError(p, $"Type '{p.TypeExpr}' is not allowed here", "The following types are allowed: type, bool, f32, f64, string, char and all integer types");
//                }

//                type = new PolyStructType(@struct);
//            }
//            else
//            {
//                @struct.Scope.TypeDeclarations.Add(@struct);
//                type = new StructType(@struct);

//                @struct.SubScope = new Scope($"struct {@struct.Name.Name}", @struct.Scope);
//            }

//            if (!@struct.Scope.DefineTypeSymbol(@struct.Name.Name, type))
//            {
//                ReportError(@struct.Name, $"A symbol with name '{@struct.Name.Name}' already exists in current scope");
//            }
//        }

//        #endregion

//        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//        #region Pass 2

//        private void Pass2TypeAlias(AstTypeAliasDecl t)
//        {
//            t.Initializer.Scope = t.Scope;
//            t.Type = ResolveType(t.Initializer);

//            if (!t.Scope.DefineTypeSymbol(t.Name.Name, t.Type))
//            {
//                ReportError(t.Name, $"A type with name '{t.Name.Name}' already exists in this scope.");
//            }
//        }

//        //private void Pass2StructDeclaration(AstStructDecl @struct)
//        //{
//        //    @struct.SubScope = new Scope("struct", @struct.Scope);

//        //    var nameSet = new HashSet<string>();
//        //    foreach (var m in @struct.Members)
//        //    {
//        //        if (nameSet.Contains(m.Name.Name))
//        //            ReportError(m.Name.GenericParseTreeNode, $"A member with name '{m.Name.Name}' already exists in struct '{@struct.Name}'");
//        //        nameSet.Add(m.Name.Name);

//        //        m.TypeExpr.Scope = @struct.SubScope;
//        //        m.Type = ResolveType(m.TypeExpr);
//        //    }
//        //}

//        #endregion

//        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//        #region Pass 3

//        /// <summary>
//        /// check parameter and return types, parameter name uniqueness, polymorphism
//        /// </summary>
//        /// <param name="func"></param>
//        private void Pass3FunctionDeclaration(AstFunctionDecl func)
//        {
//            var eh = new SilentErrorHandler();
//            mErrorHandler.Push(eh);

//            // check parameters
//            var nameSet = new HashSet<string>();
//            foreach (var p in func.Parameters)
//            {
//                if (nameSet.Contains(p.Name.Name))
//                    ReportError(p.Name, $"A parameter with name '{p.Name.Name}' already exists parameter list");
//                nameSet.Add(p.Name.Name);

//                p.TypeExpr.Scope = func.Scope;
//                p.Type = ResolveType(p.TypeExpr);

//                if (p.Type.IsPolyType)
//                {
//                    func.IsPorymorphic = true;
//                }
//            }

//            // check return type
//            if (func.ReturnTypeExpr != null)
//            {
//                func.ReturnTypeExpr.Scope = func.Scope;
//                func.ReturnType = ResolveType(func.ReturnTypeExpr);

//                if (func.ReturnType.IsPolyType)
//                {
//                    func.IsPorymorphic = true;
//                }
//            }
//            else
//            {
//                func.ReturnType = CheezType.Void;
//            }

//            mErrorHandler.Pop();

//            if (func.IsPorymorphic)
//            {
//                mPolyFunctions.Add(func);
//            }
//            else
//            {
//                mFunctionInstances.Add(func);
//                eh.ForwardErrors(mErrorHandler.Peek());
//            }

//            //
//            func.Scope.FunctionDeclarations.Add(func);
//            func.Type = FunctionType.GetFunctionType(func);

//            if (!func.Scope.DefineFunction(func))
//            {
//                ReportError(func.Name, $"A symbol with name '{func.Name.Name}' already exists in current scope");
//            }

//            if (func.GetDirective("main", out var dir))
//            {
//                if (dir.Arguments.Count > 0)
//                {
//                    ReportError(dir.ParseTreeNode, "Directive '#main' must have zero arguments.");
//                    return;
//                }

//                if (func.IsPorymorphic)
//                {
//                    ReportError(dir.ParseTreeNode, "Main function can not be polymorphic.");
//                    return;
//                }

//                if (func.ReturnType != CheezType.Void && func.ReturnType != IntType.GetIntType(32, true))
//                {
//                    ReportError(func.ReturnTypeExpr, "Main function must have either void or int as return type.");
//                    return;
//                }

//                if (func.Parameters.Count > 0)
//                {
//                    ReportError(new Location(func.Parameters), "Main function can not have parameters.");
//                    return;
//                }

//                if (mWorkspace.MainFunction != null)
//                {
//                    ReportError(dir.ParseTreeNode, $"Main function was already specified here: {mWorkspace.MainFunction.Beginning}");
//                    return;
//                }

//                mWorkspace.MainFunction = func;
//            }
//        }

//        private void Pass3TraitDeclaration(AstTraitDeclaration trait)
//        {
//            foreach (var f in trait.Functions)
//            {

//            }
//        }

//        #endregion

//        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//        #region Pass 4

//        private void Pass4ImplDeclaration(AstImplBlock t)
//        {
//            //throw new NotImplementedException();
//        }

//        #endregion

//        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//        #region Pass 5

//        /// <summary>
//        /// check global variable types
//        /// </summary>
//        /// <param name="t"></param>
//        private void Pass5GlobalVariableDeclaration(AstVariableDecl t)
//        {
//            if (t.TypeExpr == null)
//            {
//                ReportError(t.Name, "Global variables must have a type specified.");
//                return;
//            }

//            t.TypeExpr.Scope = t.Scope;
//            t.Type = ResolveType(t.TypeExpr);
//        }

//        #endregion

//        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//        #region Pass 6

//        private void Pass6StructMembers(AstStructDecl str)
//        {
//            foreach (var member in str.Members)
//            {
//                member.TypeExpr.Scope = str.SubScope;
//                member.Type = ResolveType(member.TypeExpr);
//            }
//        }

//        #endregion

//        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//        #region Helper Methods

//        private CheezValue ResolveConstantExpression(AstExpression expr)
//        {
//            switch (expr)
//            {
//                case AstIdentifierExpr i:
//                    {
//                        if (i.IsPolymorphic)
//                            return new PolyType(i.Name);

//                        var sym = expr.Scope.GetSymbol(i.Name, false);

//                        if (sym == null)
//                        {
//                            ReportError(expr, $"Unknown symbol");
//                            return null;
//                        }

//                        if (sym is CompTimeVariable c)
//                        {
//                            if (c.Type == CheezType.Type)
//                                return c.Value as CheezType;
//                            return new CheezValue(c.Type, c.Value);
//                        }
//                        else
//                        {
//                            ReportError(expr, $"'{expr}' is not a constant value");
//                        }

//                        break;
//                    }

//                case AstNumberExpr n:
//                    {
//                        if (n.Data.Type == Parsing.NumberData.NumberType.Float)
//                            return new CheezValue(FloatType.LiteralType, n.Data.ToDouble());
//                        else
//                            return new CheezValue(IntType.LiteralType, n.Data.ToLong());
//                    }

//                case AstBoolExpr b:
//                    return new CheezValue(CheezType.Bool, b.Value);

//                case AstStringLiteral s:
//                    if (s.IsChar)
//                        return new CheezValue(CheezType.Char, s.CharValue);
//                    else
//                        return new CheezValue(CheezType.String, s.StringValue);

//                case AstNullExpr n:
//                    return new CheezValue(PointerType.GetPointerType(CheezType.Any), null);
//            }

//            return ResolveType(expr);
//        }

//        private CheezType ResolveType(AstExpression typeExpr)
//        {
//            switch (typeExpr)
//            {
//                case AstIdentifierExpr i:
//                    {
//                        if (i.IsPolymorphic)
//                            return new PolyType(i.Name);

//                        var sym = typeExpr.Scope.GetSymbol(i.Name, false);

//                        if (sym == null)
//                        {
//                            ReportError(typeExpr, $"Unknown symbol");
//                            break;
//                        }

//                        if (sym is CompTimeVariable c && c.Type == CheezType.Type)
//                        {
//                            var t = c.Value as CheezType;
//                            return t;
//                        }
//                        else
//                        {
//                            ReportError(typeExpr, $"'{typeExpr}' is not a valid type");
//                        }

//                        break;
//                    }

//                case AstPointerTypeExpr p:
//                    {
//                        p.Target.Scope = typeExpr.Scope;
//                        var subType = ResolveType(p.Target);
//                        return PointerType.GetPointerType(subType);
//                    }

//                case AstArrayTypeExpr a:
//                    {
//                        a.Target.Scope = typeExpr.Scope;
//                        var subType = ResolveType(a.Target);
//                        return SliceType.GetSliceType(subType);
//                    }

//                case AstArrayAccessExpr arr:
//                    {
//                        arr.SubExpression.Scope = typeExpr.Scope;
//                        arr.Indexer.Scope = typeExpr.Scope;
//                        var subType = ResolveType(arr.SubExpression);
//                        var index = ResolveConstantExpression(arr.Indexer);

//                        if (index.type is IntType)
//                        {
//                            long v = (long)index.value;
//                            return ArrayType.GetArrayType(subType, (int)v);
//                        }
//                        ReportError(arr.Indexer, "Index must be a constant int");
//                        return CheezType.Error;
//                    }

//                case AstCallExpr call:
//                    {
//                        call.Function.Scope = call.Scope;
//                        var subType = ResolveType(call.Function);
//                        if (subType is PolyStructType @struct)
//                        {
//                            return ResolvePolyStructType(call, @struct);
//                        }
//                        return CheezType.Error;
//                    }
//            }

//            ReportError(typeExpr, $"Expected type");
//            return CheezType.Error;
//        }

//        private CheezType ResolvePolyStructType(AstCallExpr call, PolyStructType @struct)
//        {
//            var decl = @struct.Declaration;
//            var parameters = decl.Parameters;

//            // check amount of arguments
//            if (call.Arguments.Count != parameters.Count)
//            {
//                ReportError(call, $"Wrong number of arguments in struct type instantiation, expected {parameters.Count}");
//                return CheezType.Error;
//            }

//            // check arguments
//            var arguments = new Dictionary<string, CheezValue>();
//            int argCount = call.Arguments.Count;
//            for (int i = 0; i < argCount; i++)
//            {
//                var param = parameters[i];
//                var arg = call.Arguments[i];
//                arg.Scope = call.Scope;

//                var v = ResolveConstantExpression(arg);

//                if (v.type == CheezType.Type && v.value == CheezType.Error)
//                    return CheezType.Error;

//                arguments[param.Name.Name] = v;
//            }

//            var instance = InstantiatePolyStruct(decl, arguments);

//            mPolyStructInstances.Add(instance);

//            return instance.Type;
//        }

//        private AstStructDecl InstantiatePolyStruct(AstStructDecl template, Dictionary<string, CheezValue> arguments)
//        {
//            // check if instance already exists
//            AstStructDecl instance = null;
//            foreach (var pi in template.PolymorphicInstances)
//            {
//                bool eq = true;
//                foreach (var param in pi.Parameters)
//                {
//                    var existingType = param.Value;
//                    var newType = arguments[param.Name.Name];
//                    if (existingType != newType)
//                    {
//                        eq = false;
//                        break;
//                    }
//                }

//                if (eq)
//                {
//                    instance = pi;
//                    break;
//                }
//            }

//            if (instance == null)
//            {
//                // instantiate new struct
//                instance = template.Clone() as AstStructDecl;
//                instance.SubScope = new Scope($"struct {template.Name.Name}", template.Scope);
//                instance.IsPolyInstance = true;
//                instance.IsPolymorphic = false;
//                instance.Type = new StructType(instance);
//                template.PolymorphicInstances.Add(instance);

//                foreach (var p in instance.Parameters)
//                {
//                    p.Value = arguments[p.Name.Name];
//                }

//                foreach (var kv in arguments)
//                {
//                    var name = kv.Key;
//                    var v = kv.Value;
//                    if (v.type == CheezType.Any)
//                        instance.SubScope.DefineTypeSymbol(name, v.value as CheezType);
//                    else
//                        instance.SubScope.DefineSymbol(new CompTimeVariable(kv.Key, v));
//                }

//                mPolyStructInstances.Add(instance);
//            }

//            return instance;
//        }

//        #endregion
//    }
//}
