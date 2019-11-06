using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Extras;
using Cheez.Types;
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
        private void CalculateSizeOfDecl(CheezType decl, ILocation location, HashSet<CheezType> done, HashSet<CheezType> path)
        {
            if (done.Contains(decl))
                return;

            if (path.Contains(decl))
            {
                ReportError(location, $"Failed to calculate the size of {decl}");
                path.Remove(decl);
                done.Add(decl);
                return;
            }

            path.Add(decl);

            if (decl is StructType @struct)
            {
                ComputeStructMembers(@struct.Declaration);
                foreach (var em in @struct.Declaration.Members)
                {
                    CalculateSizeOfDecl(em.Type, em.Location, done, path);
                }

                @struct.CalculateSize();
            }
            else if (decl is EnumType @enum)
            {
                foreach (var em in @enum.Declaration.Members)
                {
                    if (em.AssociatedTypeExpr != null)
                    {
                        CalculateSizeOfDecl(em.AssociatedType, location, done, path);
                    }
                }

                @enum.CalculateSize();
            }

            path.Remove(decl);
            done.Add(decl);
        }

        private void CalculateEnumAndStructSizes(List<AstDecl> declarations)
        {
            var done = new HashSet<CheezType>();
            foreach (var decl in declarations)
            {
                CalculateSizeOfDecl(decl.Type, decl, done, new HashSet<CheezType>());
            }
        }
    }
}
