using Cheez.Compiler.Ast;

namespace Cheez.Compiler.SemanticAnalysis.DeclarationAnalysis
{
    public partial class DeclarationAnalyzer
    {

        /// <summary>
        /// pass 1:
        /// collect types (structs, enums, traits)
        /// </summary>
        private void Pass1()
        {
            var globalScope = mWorkspace.GlobalScope;

            foreach (var s in mWorkspace.Statements)
            {
                switch (s)
                {
                    case AstStructDecl @struct:
                        {
                            @struct.Scope = globalScope;
                            Pass1StructDeclaration(@struct);

                            if (@struct.Parameters.Count > 0)
                                mPolyStructs.Add(@struct);
                            else
                                mStructs.Add(@struct);
                            break;
                        }

                    case AstTraitDeclaration @trait:
                        {
                            trait.Scope = globalScope;
                            Pass1TraitDeclaration(@trait);
                            mTraits.Add(@trait);
                            break;
                        }

                    case AstEnumDecl @enum:
                        {
                            @enum.Scope = globalScope;
                            Pass1EnumDeclaration(@enum);
                            mEnums.Add(@enum);
                            break;
                        }

                    case AstVariableDecl @var:
                        {
                            @var.Scope = globalScope;
                            mVariables.Add(@var);
                            break;
                        }

                    case AstFunctionDecl func:
                        {
                            func.Scope = globalScope;
                            mFunctions.Add(func);
                            break;
                        }

                    case AstImplBlock impl:
                        {
                            impl.Scope = globalScope;
                            mImpls.Add(impl);
                            break;
                        }

                    case AstTypeAliasDecl type:
                        {
                            type.Scope = globalScope;
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

        private void Pass1TraitDeclaration(AstTraitDeclaration trait)
        {
            trait.Scope.TypeDeclarations.Add(trait);
            trait.Type = new TraitType(trait);

            var res = trait.Scope.DefineDeclaration(trait);
            if (!res.ok)
            {
                (string, ILocation)? detail = null;
                if (res.other != null) detail = ("Other declaration here:", res.other);
                mWorkspace.ReportError(trait.Name, $"A symbol with name '{trait.Name.Name}' already exists in current scope", detail);
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
                mWorkspace.ReportError(@enum.Name, $"A symbol with name '{@enum.Name.Name}' already exists in current scope", detail);
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
                mWorkspace.ReportError(@struct.Name, $"A symbol with name '{@struct.Name.Name}' already exists in current scope", detail);
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
                mWorkspace.ReportError(alias.Name, $"A symbol with name '{alias.Name.Name}' already exists in current scope", detail);
            }
        }
    }
}
