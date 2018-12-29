using System;
using Cheez.Compiler.Ast;

namespace Cheez.Compiler
{
    public partial class Workspace
    {

        /// <summary>
        /// pass 1:
        /// collect types (structs, enums, traits)
        /// </summary>
        private void Pass1()
        {
            foreach (var s in Statements)
            {
                switch (s)
                {
                    case AstStructDecl @struct:
                        {
                            @struct.Scope = GlobalScope;
                            Pass1StructDeclaration(@struct);

                            if (@struct.Parameters.Count > 0)
                                mPolyStructs.Add(@struct);
                            else
                                mStructs.Add(@struct);
                            break;
                        }

                    case AstTraitDeclaration @trait:
                        {
                            trait.Scope = GlobalScope;
                            Pass1TraitDeclaration(@trait);
                            mTraits.Add(@trait);
                            break;
                        }

                    case AstEnumDecl @enum:
                        {
                            @enum.Scope = GlobalScope;
                            Pass1EnumDeclaration(@enum);
                            mEnums.Add(@enum);
                            break;
                        }

                    case AstVariableDecl @var:
                        {
                            @var.Scope = GlobalScope;
                            mVariables.Add(@var);
                            Pass1VariableDeclaration(@var);
                            break;
                        }

                    case AstFunctionDecl func:
                        {
                            func.Scope = GlobalScope;
                            mFunctions.Add(func);
                            break;
                        }

                    case AstImplBlock impl:
                        {
                            impl.Scope = GlobalScope;
                            impl.SubScope = new Scope($"impl", impl.Scope);
                            if (impl.TraitExpr != null) mTraitImpls.Add(impl);
                            else mAllImpls.Add(impl);
                            break;
                        }

                    case AstTypeAliasDecl type:
                        {
                            type.Scope = GlobalScope;
                            mTypeDefs.Add(type);
                            break;
                        }
                }
            }

            // typedefs
            foreach (var t in mTypeDefs)
            {
                Pass1TypeAlias(t);
            }
        }

        private void Pass1VariableDeclaration(AstVariableDecl var)
        {
            var.Type = new VarDeclType(var);
            var res = var.Scope.DefineSymbol(var);
            if (!res.ok)
            {
                (string, ILocation)? detail = null;
                if (res.other != null) detail = ("Other declaration here:", res.other);
                ReportError(var.Name, $"A symbol with name '{var.Name.Name}' already exists in current scope", detail);
            }
            else
            {
                var.Scope.VariableDeclarations.Add(var);
            }
        }

        private void Pass1TraitDeclaration(AstTraitDeclaration trait)
        {
            trait.Scope.TypeDeclarations.Add(trait);
            trait.Type = new TraitType(trait);

            var res = trait.Scope.DefineDeclaration(trait);
            if (!res.ok)
            {
                (string, ILocation)? detail = null;
                if (res.other != null) detail = ("Other declaration here:", res.other);
                ReportError(trait.Name, $"A symbol with name '{trait.Name.Name}' already exists in current scope", detail);
            }
        }

        private void Pass1EnumDeclaration(AstEnumDecl @enum)
        {
            @enum.Scope.TypeDeclarations.Add(@enum);
            @enum.Type = new EnumType(@enum);


            var res = @enum.Scope.DefineDeclaration(@enum);
            if (!res.ok)
            {
                (string, ILocation)? detail = null;
                if (res.other != null) detail = ("Other declaration here:", res.other);
                ReportError(@enum.Name, $"A symbol with name '{@enum.Name.Name}' already exists in current scope", detail);
            }
        }

        private void Pass1StructDeclaration(AstStructDecl @struct)
        {
            if (@struct.Parameters.Count > 0)
            {
                @struct.IsPolymorphic = true;
                @struct.Type = new GenericStructType(@struct);
                @struct.SubScope = new Scope($"struct {@struct.Name.Name}", @struct.Scope);
            }
            else
            {
                @struct.Scope.TypeDeclarations.Add(@struct);
                @struct.Type = new StructType(@struct);
                @struct.SubScope = new Scope($"struct {@struct.Name.Name}", @struct.Scope);
            }

            var res = @struct.Scope.DefineDeclaration(@struct);
            if (!res.ok)
            {
                (string, ILocation)? detail = null;
                if (res.other != null) detail = ("Other declaration here:", res.other);
                ReportError(@struct.Name, $"A symbol with name '{@struct.Name.Name}' already exists in current scope", detail);
            }
        }

        private void Pass1TypeAlias(AstTypeAliasDecl alias)
        {
            alias.TypeExpr.Scope = alias.Scope;
            alias.Type = new AliasType(alias);

            var res = alias.Scope.DefineDeclaration(alias);
            if (!res.ok)
            {
                (string, ILocation)? detail = null;
                if (res.other != null) detail = ("Other declaration here:", res.other);
                ReportError(alias.Name, $"A symbol with name '{alias.Name.Name}' already exists in current scope", detail);
            }
        }
    }
}
