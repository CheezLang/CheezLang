using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cheez.Compiler.IR
{
    public class IRFunction
    {
        private List<IRBasicBlock> basicBlocks = new List<IRBasicBlock>();

        private Dictionary<IRValue, string> valueNames = new Dictionary<IRValue, string>();

        public IRFunction()
        {

        }

        public bool RegisterValue(string name, IRValue value)
        {
            if (valueNames.ContainsValue(name))
                return false;
            valueNames[value] = name;
            return true;
        }
    }

    public class IRBasicBlock
    {
        private string name;

        private List<IRInstruction> instructions = new List<IRInstruction>();

        public IRBasicBlock(string name)
        {
            this.name = name;
        }

        public void AddInstruction(IRInstruction inst)
        {
            instructions.Add(inst);
        }

        public bool Validate()
        {
            if (instructions.Count == 0)
                return false;

            for (int i = 0; i < instructions.Count - 1; i++)
            {
                if (instructions[i] is IRInstBranch)
                    return false;
            }
            return instructions.Last() is IRInstBranch;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(name).AppendLine(":");
            foreach (var i in instructions)
            {
                sb.Append("  ").AppendLine(i.ToString());
            }
            return sb.ToString();
        }
    }
}
