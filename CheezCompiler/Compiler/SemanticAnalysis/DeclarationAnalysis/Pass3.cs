using Cheez.Compiler.Ast;
using System.Collections.Generic;
using System.Linq;

namespace Cheez.Compiler.SemanticAnalysis.DeclarationAnalysis
{
    /// <summary>
    /// This pass resolves the types of struct members
    /// </summary>
    public partial class DeclarationAnalyzer
    {
        /// <summary>
        /// pass 3: resolve the types of struct members
        /// </summary>
        public void Pass3()
        {
            var newInstances = new List<AstStructDecl>();

            newInstances.AddRange(mStructs);

            foreach (var @struct in mPolyStructs)
            {
                newInstances.AddRange(@struct.PolymorphicInstances);
            }

            mWorkspace.ResolveStructs(newInstances);
        }
    }
}
