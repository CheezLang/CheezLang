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

            foreach (var @struct in mStructs)
            {
                mWorkspace.ResolveStruct(@struct, newInstances);
            }

            foreach (var @struct in mPolyStructs)
            {
                // this kind of loop because @struct.PolymorphicInstances can change while iterating
                var count = @struct.PolymorphicInstances.Count;
                for (int i = 0; i < count; i++)
                {
                    mWorkspace.ResolveStruct(@struct.PolymorphicInstances[i], newInstances);
                }
            }

            {
                var nextInstances = new List<AstStructDecl>();

                int i = 0;
                const int max_count = 100;
                while (i < max_count && newInstances.Count != 0)
                {
                    foreach (var instance in newInstances)
                    {
                        mWorkspace.ResolveStruct(instance, nextInstances);
                    }
                    newInstances.Clear();

                    var t = newInstances;
                    newInstances = nextInstances;
                    nextInstances = t;

                    i++;
                }

                if (i == max_count)
                {
                    var details = newInstances.Select(str => ("Here:", str.Location)).ToList();
                    mWorkspace.ReportError($"Detected a potential infinite loop in polymorphic struct declarations after {max_count} steps", details);
                }
            }
        }
    }
}
