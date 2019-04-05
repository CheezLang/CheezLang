using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Extras;
using Cheez.Types.Abstract;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cheez
{
    /// <summary>
    /// This pass resolves the types of struct members
    /// </summary>
    public partial class Workspace
    {
        private void CalculateSizeOfDecl(AstDecl decl, HashSet<AstDecl> done, HashSet<AstDecl> path)
        {
            if (done.Contains(decl))
                return;

            if (path.Contains(decl))
            {
                ReportError(decl.Name, $"Failed to calculate the size of {decl.Name}");
                path.Remove(decl);
                done.Add(decl);
                return;
            }

            path.Add(decl);

            if (decl is AstEnumDecl @enum)
            {
                foreach (var em in @enum.Members)
                {
                    if (em.AssociatedTypeExpr != null)
                    {
                        if (em.AssociatedTypeExpr.Value is StructType s)
                            CalculateSizeOfDecl(s.Declaration, done, path);
                        else if (em.AssociatedTypeExpr.Value is EnumType e)
                            CalculateSizeOfDecl(e.Declaration, done, path);
                    }
                }

                if (@enum.Type is EnumType str)
                    str.CalculateSize();
            }
            else if (decl is AstStructDecl @struct)
            {
                foreach (var em in @struct.Members)
                {
                    if (em.Type is StructType s)
                        CalculateSizeOfDecl(s.Declaration, done, path);
                    else if (em.Type is EnumType e)
                        CalculateSizeOfDecl(e.Declaration, done, path);
                }

                if (@struct.Type is StructType str)
                    str.CalculateSize();
            }

            path.Remove(decl);
            done.Add(decl);
        }

        private void CalculateEnumAndStructSizes(List<AstDecl> declarations)
        {
            var done = new HashSet<AstDecl>();
            foreach (var decl in declarations)
            {
                CalculateSizeOfDecl(decl, done, new HashSet<AstDecl>());
            }
        }
    }
}
