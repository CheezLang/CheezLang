using Cheez.Ast.Statements;
using Cheez.Extras;
using Cheez.Types;
using Cheez.Types.Abstract;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;
using CompilerLibrary.Extras;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cheez
{
    public interface INaryOperator
    {
        CheezType[] ArgTypes { get; }
        CheezType ResultType { get; }
        string Name { get; }

        int Accepts(params CheezType[] types);

        object Execute(params object[] args);
    }

    public interface IBinaryOperator
    {
        CheezType LhsType { get; }
        CheezType RhsType { get; }
        CheezType ResultType { get; }
        string Name { get; }

        int Accepts(CheezType lhs, CheezType rhs);

        object Execute(object left, object right);
    }

    public interface IUnaryOperator
    {
        CheezType SubExprType { get; }
        CheezType ResultType { get; }
        string Name { get; }

        int Accepts(CheezType sub);

        object Execute(object value);
    }

    public class BuiltInPointerOperator : IBinaryOperator
    {
        public CheezType LhsType => PointerType.GetPointerType(CheezType.Void, true);
        public CheezType RhsType => PointerType.GetPointerType(CheezType.Void, true);
        public CheezType ResultType { get; private set; }

        public string Name { get; private set; }

        public BuiltInPointerOperator(string name)
        {
            this.Name = name;
            switch (name)
            {
                case "==": ResultType = CheezType.Bool; break;
                case "!=": ResultType = CheezType.Bool; break;

                default: ResultType = PointerType.GetPointerType(CheezType.Void, true); break;
            }
        }

        public int Accepts(CheezType lhs, CheezType rhs)
        {
            if (lhs is PointerType lt && rhs is PointerType rt)
                return 0;
            return -1;
        }

        public object Execute(object left, object right)
        {
            throw new NotImplementedException();
        }
    }

    public class BuiltInTraitNullOperator : IBinaryOperator
    {
        public CheezType LhsType => null;
        public CheezType RhsType => PointerType.NullLiteralType;
        public CheezType ResultType => CheezType.Bool;

        public string Name { get; private set; }

        public BuiltInTraitNullOperator(string name)
        {
            this.Name = name;
        }

        public int Accepts(CheezType lhs, CheezType rhs)
        {
            if (lhs is TraitType && rhs == PointerType.NullLiteralType)
                return 0;
            return -1;
        }

        public object Execute(object left, object right)
        {
            throw new NotImplementedException();
        }
    }

    public class BuiltInEnumCompareOperator : IBinaryOperator
    {
        public CheezType LhsType => null;
        public CheezType RhsType => null;
        public CheezType ResultType => CheezType.Bool;

        public string Name { get; private set; }

        public BuiltInEnumCompareOperator(string name)
        {
            this.Name = name;
        }

        public int Accepts(CheezType lhs, CheezType rhs)
        {
            if (lhs is EnumType f1 && rhs is EnumType f2 && f1 == f2)
                return 0;
            return -1;
        }

        public object Execute(object left, object right)
        {
            throw new NotImplementedException();
        }
    }

    public class EnumFlagsCombineOperator : IBinaryOperator
    {
        public EnumType EnumType { get; }
        public CheezType LhsType => EnumType;
        public CheezType RhsType => EnumType;
        public CheezType ResultType => EnumType;

        public string Name => "or";

        public EnumFlagsCombineOperator(EnumType type)
        {
            EnumType = type;
        }

        public int Accepts(CheezType lhs, CheezType rhs)
        {
            if (lhs == rhs && lhs == EnumType)
                return 0;
            return -1;
        }

        public object Execute(object left, object right)
        {
            var l = left as EnumValue;
            var r = right as EnumValue;
            if (l == null || r == null)
                throw new ArgumentException($"'{nameof(left)}' and '{nameof(right)}' must be enum values, but are '{left}' and '{right}'");
            if (l.Type != r.Type)
                throw new ArgumentException($"'{nameof(left)}' and '{nameof(right)}' must have the same type, but have {l.Type} and {r.Type}");
            
            var members = new HashSet<AstEnumMemberNew>();
            members.UnionWith(l.Members);
            members.UnionWith(r.Members);
            return new EnumValue(l.Type, members.ToArray());
        }
    }

    public class EnumFlagsAndOperator : IBinaryOperator
    {
        public EnumType EnumType { get; }
        public CheezType LhsType => EnumType;
        public CheezType RhsType => EnumType;
        public CheezType ResultType => EnumType;

        public string Name => "and";

        public EnumFlagsAndOperator(EnumType type)
        {
            EnumType = type;
        }

        public int Accepts(CheezType lhs, CheezType rhs)
        {
            if (lhs == rhs && lhs == EnumType)
                return 0;
            return -1;
        }

        public object Execute(object left, object right)
        {
            var l = left as EnumValue;
            var r = right as EnumValue;
            if (l == null || r == null)
                throw new ArgumentException($"'{nameof(left)}' and '{nameof(right)}' must be enum values, but are '{left}' and '{right}'");
            if (l.Type != r.Type)
                throw new ArgumentException($"'{nameof(left)}' and '{nameof(right)}' must have the same type, but have {l.Type} and {r.Type}");

            return new EnumValue(l.Type, l.Members.Intersect(r.Members).ToArray());
        }
    }

    public class EnumFlagsTestOperator : IBinaryOperator
    {
        public EnumType EnumType { get; }
        public CheezType LhsType => EnumType;
        public CheezType RhsType => EnumType;
        public CheezType ResultType => CheezType.Bool;

        public string Name => "is in";

        public EnumFlagsTestOperator(EnumType type)
        {
            EnumType = type;
        }

        public int Accepts(CheezType lhs, CheezType rhs)
        {
            if (lhs == rhs && lhs == EnumType)
                return 0;
            return -1;
        }

        public object Execute(object left, object right)
        {
            var l = left as EnumValue;
            var r = right as EnumValue;
            if (l == null || r == null)
                throw new ArgumentException($"'{nameof(left)}' and '{nameof(right)}' must be enum values, but are '{left}' and '{right}'");
            if (l.Type != r.Type)
                throw new ArgumentException($"'{nameof(left)}' and '{nameof(right)}' must have the same type, but have {l.Type} and {r.Type}");

            var contains = l.Members.Intersect(r.Members).Count() > 0;
            return contains;
        }
    }

    public class EnumFlagsNotOperator : IUnaryOperator
    {
        public EnumType EnumType { get; }
        public CheezType SubExprType => EnumType;
        public CheezType ResultType => EnumType;

        public string Name => "!";

        public EnumFlagsNotOperator(EnumType type)
        {
            EnumType = type;
        }

        public int Accepts(CheezType sub)
        {
            if (SubExprType == sub)
                return 0;
            return -1;
        }

        public object Execute(object sub)
        {
            var l = sub as EnumValue;
            if (l == null)
                throw new ArgumentException($"'{nameof(sub)}' must be an enum value, but is '{sub}'");

            return new EnumValue(EnumType, EnumType.Declaration.Members.Except(l.Members).ToArray());
        }
    }


    public class BuiltInFunctionOperator : IBinaryOperator
    {
        public CheezType LhsType => null;
        public CheezType RhsType => null;
        public CheezType ResultType => CheezType.Bool;

        public string Name { get; private set; }

        public BuiltInFunctionOperator(string name)
        {
            this.Name = name;
        }

        public int Accepts(CheezType lhs, CheezType rhs)
        {
            if (lhs is FunctionType f1 && rhs is FunctionType f2 && f1 == f2)
                return 0;
            return -1;
        }

        public object Execute(object left, object right)
        {
            throw new NotImplementedException();
        }
    }

    public class BuiltInBinaryOperator : IBinaryOperator
    {
        public CheezType LhsType { get; private set; }
        public CheezType RhsType { get; private set; }
        public CheezType ResultType { get; private set; }

        public string Name { get; private set; }

        public delegate object ComptimeExecution(object left, object right);
        public ComptimeExecution Execution { get; }

        public BuiltInBinaryOperator(string name, CheezType resType, CheezType lhs, CheezType rhs, ComptimeExecution exe = null)
        {
            Name = name;
            ResultType = resType;
            LhsType = lhs;
            RhsType = rhs;
            Execution = exe;
        }

        public int Accepts(CheezType lhs, CheezType rhs)
        {
            var ml = LhsType.Match(lhs, null);
            var mr = RhsType.Match(rhs, null);
            if (ml == -1 || mr == -1)
                return -1;

            return ml + mr;
        }

        public override string ToString()
        {
            return $"({ResultType}) {LhsType} {Name} {RhsType}";
        }

        public object Execute(object left, object right)
        {
            return Execution?.Invoke(left, right);
        }
    }

    public class BuiltInPolyValueBinaryOperator : IBinaryOperator
    {
        public CheezType LhsType => CheezType.PolyValue;
        public CheezType RhsType => CheezType.PolyValue;
        public CheezType ResultType { get; private set; }

        public string Name { get; private set; }

        public delegate object ComptimeExecution(object left, object right);

        public BuiltInPolyValueBinaryOperator(string name, CheezType resType)
        {
            Name = name;
            ResultType = resType;
        }

        public int Accepts(CheezType lhs, CheezType rhs)
        {
            if (lhs is PolyValueType && rhs is PolyValueType)
                return 0;
            if (lhs is PolyValueType || rhs is PolyValueType)
                return 1;
            return -1;
        }

        public override string ToString()
        {
            return $"({ResultType}) {LhsType} {Name} {RhsType}";
        }

        public object Execute(object left, object right)
        {
            throw new NotImplementedException();
        }
    }

    public class BuiltInUnaryOperator : IUnaryOperator
    {
        public CheezType SubExprType { get; private set; }
        public CheezType ResultType { get; private set; }

        public string Name { get; private set; }

        public delegate object ComptimeExecution(object value);
        public ComptimeExecution Execution { get; set; }

        public BuiltInUnaryOperator(string name, CheezType resType, CheezType sub, ComptimeExecution exe = null)
        {
            Name = name;
            ResultType = resType;
            SubExprType = sub;
            this.Execution = exe;
        }

        public override string ToString()
        {
            return $"({ResultType}) {Name} {SubExprType}";
        }

        public int Accepts(CheezType sub)
        {
            return SubExprType.Match(sub, null);
        }

        public object Execute(object value)
        {
            return Execution?.Invoke(value);
        }
    }

    public class UserDefinedUnaryOperator : IUnaryOperator
    {
        public CheezType SubExprType { get; set; }
        public CheezType ResultType { get; set; }
        public string Name { get; set; }

        public AstFuncExpr Declaration { get; set; }

        public UserDefinedUnaryOperator(string name, AstFuncExpr func)
        {
            this.Name = name;
            this.SubExprType = func.Parameters[0].Type;
            this.ResultType = func.ReturnType;
            this.Declaration = func;
        }

        public int Accepts(CheezType sub)
        {
            Dictionary<string, (CheezType type, object value)> polyTypes = null;

            // TODO: necessary?
            //if (SubExprType.IsPolyType)
            //{
            //    polyTypes = new Dictionary<string, CheezType>();
            //    Workspace.CollectPolyTypes(SubExprType, lhs, polyTypes);
            //}


            return SubExprType.Match(sub, polyTypes);
        }

        public object Execute(object value)
        {
            throw new NotImplementedException();
        }
    }

    public class UserDefinedBinaryOperator : IBinaryOperator
    {
        public CheezType LhsType { get; set; }
        public CheezType RhsType { get; set; }
        public CheezType ResultType { get; set; }
        public string Name { get; set; }
        public AstFuncExpr Declaration { get; set; }

        public UserDefinedBinaryOperator(string name, AstFuncExpr func)
        {
            this.Name = name;
            this.LhsType = func.Parameters[0].Type;
            this.RhsType = func.Parameters[1].Type;
            this.ResultType = func.ReturnType;
            this.Declaration = func;
        }

        public int Accepts(CheezType lhs, CheezType rhs)
        {
            Dictionary<string, (CheezType type, object value)> polyTypes = null;

            if (LhsType.IsPolyType || RhsType.IsPolyType)
            {
                polyTypes = new Dictionary<string, (CheezType type, object value)>();
                Workspace.CollectPolyTypes(LhsType, lhs, polyTypes);
                Workspace.CollectPolyTypes(RhsType, rhs, polyTypes);
            }
            

            var ml = LhsType.Match(lhs, polyTypes);
            var mr = RhsType.Match(rhs, polyTypes);
            if (ml == -1 || mr == -1)
                return -1;
            return ml + mr;
        }

        public object Execute(object left, object right)
        {
            throw new NotImplementedException();
        }
    }

    public class UserDefinedNaryOperator : INaryOperator
    {
        public CheezType[] ArgTypes { get; }
        public CheezType ResultType { get; }
        public string Name { get; set; }
        public AstFuncExpr Declaration { get; set; }


        public UserDefinedNaryOperator(string name, AstFuncExpr func)
        {
            this.Name = name;
            this.ArgTypes = func.Parameters.Select(p => p.Type).ToArray();
            this.ResultType = func.ReturnType;
            this.Declaration = func;
        }

        public int Accepts(params CheezType[] types)
        {
            if (types.Length != ArgTypes.Length)
                return -1;

            var polyTypes = new Dictionary<string, (CheezType type, object value)>();

            for (int i = 0; i < ArgTypes.Length; i++)
            {
                Workspace.CollectPolyTypes(ArgTypes[i], types[i], polyTypes);
            }

            var match = 0;
            for (int i = 0; i < ArgTypes.Length; i++)
            {
                var m = ArgTypes[i].Match(types[i], polyTypes);
                if (m == -1)
                    return -1;
                match += m;
            }

            return match;
        }

        public object Execute(params object[] args)
        {
            throw new NotImplementedException();
        }
    }
}
