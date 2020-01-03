using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Types;
using Cheez.Types.Abstract;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Cheez
{
    public partial class Workspace
    {

        private AstExpression InferTypeStructTypeExpr(AstStructTypeExpr expr)
        {
            if (expr.IsPolyInstance)
            {

            }
            else
            {
                expr.SubScope = new Scope("struct", expr.Scope);
                if (expr.Parent is AstConstantDeclaration c)
                    expr.Name = c.Name.Name;
            }

            bool isCopy = expr.HasDirective("copy");

            if (expr.IsPolymorphic)
            {
                // @todo
                foreach (var p in expr.Parameters)
                {
                    p.Scope = expr.Scope;
                    p.TypeExpr.Scope = expr.Scope;
                    p.TypeExpr = ResolveTypeNow(p.TypeExpr, out var t);
                    p.Type = t;

                    ValidatePolymorphicParameterType(p, p.Type);

                    expr.SubScope.DefineTypeSymbol(p.Name.Name, new PolyType(p.Name.Name, true));
                }

                expr.Type = CheezType.Type;
                expr.Value = new GenericStructType(expr, isCopy, expr.Name);
                return expr;
            }

            // not polymorphic
            expr.Extendable = expr.HasDirective("extendable");

            foreach (var decl in expr.Declarations)
            {
                decl.Scope = expr.SubScope;

                if (decl is AstConstantDeclaration con)
                {
                    AnalyseConstantDeclaration(con);
                }
            }

            expr.Type = CheezType.Type;
            expr.Value = new StructType(expr, isCopy, expr.Name);
            return expr;
        }

        private void SetupStructMembers(AstStructTypeExpr expr)
        {
            if (expr.Members != null)
                return;
            expr.Members = new List<AstStructMemberNew>();

            if (expr.TryGetDirective("extend", out var dir))
            {
                if (dir.Arguments.Count != 1)
                    ReportError(dir, $"Must have one type argument");
                else
                {
                    var arg = dir.Arguments[0];
                    arg.AttachTo(expr);
                    arg = ResolveTypeNow(arg, out var type);

                    if (type is StructType str)
                    {
                        if (str.Declaration.Extendable)
                            expr.Extends = str;
                        else
                            ReportError(arg, $"Type '{str}' is not extendable");
                    }
                    else if (!type.IsErrorType)
                        ReportError(arg, $"Argument must be a struct type");
                }
            }
            if (expr.Extendable || expr.Extends != null)
            {
                var mem = mCompiler.ParseStatement(
                    $"__type_ptr__ : &TypeInfo = @type_info(§self)",
                    new Dictionary<string, AstExpression>
                    {
                            { "self", new AstTypeRef(expr.StructType, expr) }
                    }) as AstVariableDecl;
                mem.Parent = expr;
                mem.Scope = expr.SubScope;
                expr.Members.Add(new AstStructMemberNew(mem, false, true, expr.Members.Count));
            }

            if (expr.Extends != null)
            {
                // skip type_info of parent
                ComputeTypeMembers(expr.Extends);
                foreach (var m in expr.Extends.Declaration.Members.Skip(1))
                {
                    var clone = m.Decl.Clone() as AstVariableDecl;
                    expr.Members.Add(new AstStructMemberNew(clone, true, false, expr.Members.Count));
                }
            }

            foreach (var decl in expr.Declarations)
            {
                if (decl is AstVariableDecl mem)
                {
                    expr.Members.Add(new AstStructMemberNew(mem, true, false, expr.Members.Count));
                }
            }
        }

        private void ComputeStructMemberSizes(AstStructTypeExpr expr)
        {
            if (expr.TypesComputed)
                return;
            SetupStructMembers(expr);

            foreach (var m in expr.Members)
            {
                ComputeStructMember(m, false);
                if (expr.StructType.IsCopy && !m.Decl.Type.IsCopy)
                    ReportError(m.Decl, "Member is not copyable");
            }

            expr.TypesComputed = true;
        }

        private void ComputeStructMembers(AstStructTypeExpr expr)
        {
            if (expr.Name == "STARTUPINFOA")
            { }
            if (expr.TypesComputed && expr.InitializersComputed)
                return;
            SetupStructMembers(expr);

            foreach (var m in expr.Members)
            {
                ComputeStructMember(m, true);
                if (expr.StructType.IsCopy && !m.Decl.Type.IsCopy)
                    ReportError(m.Decl, "Member is not copyable");
            }

            expr.TypesComputed = true;
            expr.InitializersComputed = true;
        }

        void ComputeStructMember(AstStructMemberNew mem, bool computeInitializer)
        {
            var decl = mem.Decl;

            if (!(decl.Pattern is AstIdExpr memName))
            {
                ReportError(decl.Pattern, $"Only single names allowed");
                return;
            }

            if (decl.Directives != null)
                foreach (var dir in decl.Directives)
                    InferTypeAttributeDirective(dir, decl, decl.Scope);

            if (decl.TypeExpr != null && decl.Type == null)
            {
                decl.TypeExpr.AttachTo(decl);
                decl.TypeExpr = ResolveTypeNow(decl.TypeExpr, out var t);
                decl.Type = t;
            }

            if (decl.Initializer != null && decl.Initializer.Type == null && (computeInitializer || decl.TypeExpr == null))
            {
                decl.Initializer.AttachTo(decl);
                decl.Initializer = InferType(decl.Initializer, decl.Type);
                ConvertLiteralTypeToDefaultType(decl.Initializer, decl.Type);

                if (decl.Type == null)
                    decl.Type = decl.Initializer.Type;
                else
                    decl.Initializer = CheckType(decl.Initializer, decl.Type);
            }

            switch (decl.Type)
            {
                case IntType _:
                case FloatType _:
                case BoolType _:
                case CharType _:
                case SliceType _:
                case StringType _:
                case ArrayType _:
                case StructType _:
                case EnumType _:
                case TraitType _:
                case PointerType _:
                case ReferenceType _:
                case FunctionType _:
                case AnyType _:
                case TupleType _:
                    break;

                default:
                    ReportError(decl, $"A struct member can't have type '{decl.Type}'");
                    break;
            }

            ComputeTypeMembers(decl.Type);
        }

        private AstStructTypeExpr InstantiatePolyStruct(AstStructTypeExpr decl, List<(CheezType type, object value)> args, ILocation location = null)
        {
            if (args.Count != decl.Parameters.Count)
            {
                if (location != null)
                    ReportError(location, "Polymorphic struct instantiation has wrong number of arguments.", ("Declaration here:", decl));
                else
                    ReportError("Polymorphic struct instantiation has wrong number of arguments.", ("Declaration here:", decl));
                return null;
            }

            AstStructTypeExpr instance = null;

            // check if instance already exists
            foreach (var pi in decl.PolymorphicInstances)
            {
                Debug.Assert(pi.Parameters.Count == args.Count);

                bool eq = true;
                for (int i = 0; i < pi.Parameters.Count; i++)
                {
                    var param = pi.Parameters[i];
                    var arg = args[i];
                    if (param.Value != arg.value)
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
                instance = decl.Clone() as AstStructTypeExpr;
                instance.SubScope = new Scope($"struct.poly", instance.Scope);
                instance.IsPolyInstance = true;
                instance.IsGeneric = false;
                instance.Template = decl;
                instance.Name = decl.Name;

                // @todo
                //instance.SetFlag(StmtFlags.IsCopy, decl.GetFlag(StmtFlags.IsCopy));
                decl.PolymorphicInstances.Add(instance);

                Debug.Assert(instance.Parameters.Count == args.Count);

                for (int i = 0; i < instance.Parameters.Count; i++)
                {
                    var param = instance.Parameters[i];
                    var arg = args[i];
                    param.Type = arg.type;
                    param.Value = arg.value;

                    // TODO: what if arg.value is not a type?
                    instance.SubScope.DefineTypeSymbol(param.Name.Name, param.Value as CheezType);
                }

                instance = InferType(instance, null) as AstStructTypeExpr;

                //instance.Type = new StructType(instance);

                //if (instances != null)
                //    instances.Add(instance);
                //else
                //{
                //    ResolveTypeDeclaration(instance);
                //}
            }

            return instance;
        }

    }
}
