using Cheez.Compiler.Ast;
using Cheez.Compiler.Visitor;
using System.Collections.Generic;
using System.Linq;

namespace Cheez.Compiler.CCE
{
    public class TWIValue
    {
        public CheezType Type;
        object Value;

        public TWIValue(CheezType type, object value)
        {
            this.Type = type;
            this.Value = value;
        }
    }

    public class TWIContext
    {
        public TWIContext Parent { get; }
        public TWIValue[] FunctionArgs { get; }

        private Dictionary<string, TWIValue> symbolTable = new Dictionary<string, TWIValue>();

        public TWIContext(TWIContext parent, TWIValue[] funcArgs = null)
        {
            this.Parent = parent;
            this.FunctionArgs = funcArgs;
        }

        public TWIValue GetValue(string name)
        {
            if (symbolTable.TryGetValue(name, out var val))
                return val;

            return null;
        }

        public void SetValue(string name, TWIValue val)
        {
            symbolTable[name] = val;
        }
    }

    public class TreeWalkInterpreter : VisitorBase<TWIValue, TWIContext>
    {
        private Workspace workspace;
        private Compiler compiler;

        public TreeWalkInterpreter(Compiler comp, Workspace ws)
        {
            compiler = comp;
            workspace = ws;
        }

        public TWIValue Execute(AstFunctionDecl func, params TWIValue[] args)
        {
            func.Accept(this);
            return null;
        }

        public override TWIValue VisitFunctionDeclaration(AstFunctionDecl function, TWIContext context)
        {
            var funcContext = new TWIContext(context);

            //foreach (var arg in funcContext.FunctionArgs.Zip(context.FunctionArgs, ))

            return null;
        }
    }
}
