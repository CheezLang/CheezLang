using Cheez.Ast.Statements;
using Cheez.Types;
using Cheez.Types.Primitive;
using System;
using System.Collections.Generic;

namespace Cheez
{
    public interface IOperator
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

    public class BuiltInPointerOperator : IOperator
    {
        public CheezType LhsType => PointerType.GetPointerType(CheezType.Any);
        public CheezType RhsType => PointerType.GetPointerType(CheezType.Any);
        public CheezType ResultType { get; private set; }

        public string Name { get; private set; }

        public BuiltInPointerOperator(string name)
        {
            this.Name = name;
            switch (name)
            {
                case "==": ResultType = CheezType.Bool; break;
                case "!=": ResultType = CheezType.Bool; break;

                default: ResultType = PointerType.GetPointerType(CheezType.Any); break;
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

    public class BuiltInOperator : IOperator
    {
        public CheezType LhsType { get; private set; }
        public CheezType RhsType { get; private set; }
        public CheezType ResultType { get; private set; }

        public string Name { get; private set; }

        public delegate object ComptimeExecution(object left, object right);
        public ComptimeExecution Execution { get; set; }

        public BuiltInOperator(string name, CheezType resType, CheezType lhs, CheezType rhs, ComptimeExecution exe = null)
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

        public AstFunctionDecl Declaration { get; set; }

        public UserDefinedUnaryOperator(string name, AstFunctionDecl func)
        {
            this.Name = name;
            this.SubExprType = func.Parameters[0].Type;
            this.ResultType = func.ReturnValue?.Type;
            this.Declaration = func;
        }

        public int Accepts(CheezType sub)
        {
            Dictionary<string, CheezType> polyTypes = null;

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

    public class UserDefinedBinaryOperator : IOperator
    {
        public CheezType LhsType { get; set; }
        public CheezType RhsType { get; set; }
        public CheezType ResultType { get; set; }
        public string Name { get; set; }
        public AstFunctionDecl Declaration { get; set; }

        public UserDefinedBinaryOperator(string name, AstFunctionDecl func)
        {
            this.Name = name;
            this.LhsType = func.Parameters[0].Type;
            this.RhsType = func.Parameters[1].Type;
            this.ResultType = func.ReturnValue?.Type;
            this.Declaration = func;
        }

        public int Accepts(CheezType lhs, CheezType rhs)
        {
            Dictionary<string, CheezType> polyTypes = null;

            if (LhsType.IsPolyType || RhsType.IsPolyType)
            {
                polyTypes = new Dictionary<string, CheezType>();
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
}
