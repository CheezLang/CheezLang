using Cheez.Ast.Statements;
using Cheez.Types;
using Cheez.Types.Complex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompilerLibrary.Extras
{
    public class EnumValue
    {
        public EnumType Type { get; }
        public long Tag { get; }
        public AstEnumMemberNew[] Members { get; }

        public EnumValue(EnumType type, params AstEnumMemberNew[] members)
        {
            if (members.Length == 0)
                throw new ArgumentException($"'{nameof(members)}' must contain at least one value");

            Type = type;
            Tag = 0;
            Members = members;

            foreach (var mem in members)
                Tag |= mem.Value.ToLong();
        }

        public override string ToString()
        {
            if (Type.Declaration.IsFlags)
            {
                var mems = string.Join("|", Members.Select(m => m.Name));
                return $"{Type}.({mems})";
            }
            else
            {
                return $"{Type}.{Members[0].Name}";
            }
        }
    }
}
