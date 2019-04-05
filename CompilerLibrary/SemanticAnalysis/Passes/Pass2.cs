using Cheez.Ast;
using Cheez.Ast.Statements;
using Cheez.Types;
using Cheez.Types.Abstract;
using Cheez.Types.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cheez
{
    public partial class Workspace
    {
        private HashSet<AstDecl> Pass2TypeAlias(AstTypeAliasDecl alias)
        {
            var deps = new HashSet<AstDecl>();
            
            alias.TypeExpr.Scope = alias.Scope;
            alias.TypeExpr = ResolveTypeNow(alias.TypeExpr, out var newType, dependencies: deps, forceInfer: true);
            if (newType != alias.Type && !(newType is AliasType))
                alias.Type = newType;

            alias.Type = newType;
            return deps;
        }

        private HashSet<AstDecl> Pass2PolyEnum(AstEnumDecl @enum)
        {
            var deps = new HashSet<AstDecl>();

            foreach (var param in @enum.Parameters)
            {
                param.TypeExpr.Scope = @enum.Scope;
                param.TypeExpr = ResolveTypeNow(param.TypeExpr, out var newType, dependencies: deps, forceInfer: true);

                if (newType is AbstractType)
                {
                    continue;
                }

                param.Type = newType;

                switch (param.Type)
                {
                    case CheezTypeType _:
                        param.Value = new PolyType(param.Name.Name, true);
                        break;

                    case IntType _:
                    case FloatType _:
                    case BoolType _:
                    case CharType _:
                        break;

                    case ErrorType _:
                        break;

                    default:
                        ReportError(param.TypeExpr, $"The type '{param.Type}' is not allowed here.");
                        break;
                }
            }

            return deps;
        }

        private HashSet<AstDecl> Pass2PolyStruct(AstStructDecl @struct)
        {
            var deps = new HashSet<AstDecl>();

            foreach (var param in @struct.Parameters)
            {
                param.TypeExpr.Scope = @struct.Scope;
                param.TypeExpr = ResolveTypeNow(param.TypeExpr, out var newType, dependencies: deps, forceInfer: true);

                if (newType is AbstractType)
                {
                    continue;
                }

                param.Type = newType;

                switch (param.Type)
                {
                    case CheezTypeType _:
                        param.Value = new PolyType(param.Name.Name, true);
                        break;

                    case IntType _:
                    case FloatType _:
                    case BoolType _:
                    case CharType _:
                        break;

                    case ErrorType _:
                        break;

                    default:
                        ReportError(param.TypeExpr, $"The type '{param.Type}' is not allowed here.");
                        break;
                }
            }

            return deps;
        }
    }
}
