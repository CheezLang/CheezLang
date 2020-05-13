using Cheez.Ast;
using Cheez.Ast.Expressions;
using Cheez.Ast.Statements;
using Cheez.Extras;
using Cheez.Types;
using Cheez.Types.Abstract;
using Cheez.Types.Complex;
using Cheez.Types.Primitive;
using Cheez.Util;
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
        private int GetSizeOfType(CheezType type)
        {
            if (type.GetSize() >= 0)
                return type.GetSize();
            var path = new List<CheezType>();
            return ComputeSizeAndAlignmentOfType(type, path).size;
        }
        private int GetAlignOfType(CheezType type)
        {
            if (type.GetSize() >= 0)
                return type.GetAlignment    ();
            var path = new List<CheezType>();
            return ComputeSizeAndAlignmentOfType(type, path).alignment;
        }

        private (int size, int alignment) ComputeSizeAndAlignmentOfType(CheezType type, List<CheezType> path)
        {
            if (type.GetSize() >= 0)
                return (type.GetSize(), type.GetAlignment());

            if (path.Contains(type))
            {
                ReportError($"Failed to calculate size of type {type} because it has a circular dependency on its own size");
                return (0, 0);
            }

            path.Add(type);

            try
            {
                switch (type)
                {
                    case TraitType s:
                        {
                            //computetr(s.Declaration);
                            var alignment = 1;
                            var size = 0;
                            for (int i = 0; i < s.Declaration.Members.Count; i++)
                            {
                                var m = s.Declaration.Members[i];

                                var (ms, ma) = ComputeSizeAndAlignmentOfType(m.Type, path);

                                m.Offset = Utilities.GetNextAligned(size, ma);
                                alignment = Math.Max(alignment, ma);
                                size += ms;
                                size = Utilities.GetNextAligned(size, ma);
                            }

                            s.SetSizeAndAlignment(0, alignment);
                            return (0, alignment);
                        }

                    case StructType s:
                        {
                            ComputeStructMemberSizes(s.Declaration);
                            var alignment = 1;
                            var size = 0;
                            for (int i = 0; i < s.Declaration.Members.Count; i++)
                            {
                                var m = s.Declaration.Members[i];

                                var (ms, ma) = ComputeSizeAndAlignmentOfType(m.Type, path);

                                m.Offset = Utilities.GetNextAligned(size, ma);
                                alignment = Math.Max(alignment, ma);
                                size += ms;
                                size = Utilities.GetNextAligned(size, ma);
                            }

                            size = Utilities.GetNextAligned(size, alignment);

                            s.SetSizeAndAlignment(size, alignment);
                            return (size, alignment);
                        }

                    case EnumType e:
                        {
                            ComputeEnumMembers(e.Declaration);
                            // @todo: force compute member types
                            var (tagSize, tagAlign) = ComputeSizeAndAlignmentOfType(e.Declaration.TagType, path);

                            var alignment = tagAlign;

                            var maxMemberSize = 0;

                            foreach (var m in e.Declaration.Members)
                            {
                                if (m.AssociatedType != null)
                                {
                                    var (memberSize, _) = ComputeSizeAndAlignmentOfType(m.AssociatedType, path);
                                    maxMemberSize = Math.Max(maxMemberSize, memberSize);
                                }
                            }

                            var size = tagSize + maxMemberSize;
                            size = Utilities.GetNextAligned(size, alignment);

                            e.SetSizeAndAlignment(size, alignment);
                            return (size, alignment);
                        }

                    case TupleType t:
                        {
                            var alignment = 1;
                            var size = 0;
                            for (int i = 0; i < t.Members.Length; i++)
                            {
                                var m = t.Members[i];

                                var (ms, ma) = ComputeSizeAndAlignmentOfType(m.type, path);

                                alignment = Math.Max(alignment, ma);
                                size += ms;
                                size = Utilities.GetNextAligned(size, ma);
                            }

                            size = Utilities.GetNextAligned(size, alignment);

                            t.SetSizeAndAlignment(size, alignment);
                            return (size, alignment);
                        }

                    case ArrayType t:
                        {
                            var (subSize, subAlign) = ComputeSizeAndAlignmentOfType(t.TargetType, path);

                            var size = subSize * (int)((NumberData)t.Length).ToLong();
                            var alignment = subAlign;
                            t.SetSizeAndAlignment(size, alignment);
                            return (size, alignment);
                        }

                    case RangeType r:
                        {
                            var (subSize, subAlign) = ComputeSizeAndAlignmentOfType(r.TargetType, path);
                            var size = subSize * 2;
                            var alignment = subAlign;
                            r.SetSizeAndAlignment(size, alignment);
                            return (size, alignment);
                        }

                    case SumType _:
                        return (-1, 0);

                    default:
                        ReportError("ERROR?");
                        return (-1, 0);
                }
            }
            finally
            {
                path.Remove(type);
            }
        }


        private bool IsTypeDefaultConstructable(CheezType type)
        {
            if (type.IsDefaultConstructableComputed())
                return type.GetIsDefaultConstructable();
            var path = new List<CheezType>();
            return ComputeIsDefaultConstructableOfType(type, path);
        }

        private bool ComputeIsDefaultConstructableOfType(CheezType type, List<CheezType> path)
        {
            if (type.IsDefaultConstructableComputed())
                return type.GetIsDefaultConstructable();

            if (path.Contains(type))
            {
                ReportError($"Failed to calculate default constructability of type {type} because it has a circular dependency on its own default constructability");
                return false;
            }

            path.Add(type);

            try
            {
                switch (type)
                {
                    case StructType s:
                        {
                            ComputeStructMembers(s.Declaration);
                            bool isDefaultConstructable = true;
                            foreach (var m in s.Declaration.Members)
                            {
                                isDefaultConstructable &= m.Decl.Initializer != null;
                                //isDefaultConstructable &= ComputeIsDefaultConstructableOfType(m.Type, path);
                            }

                            s.SetIsDefaultConstructable(isDefaultConstructable);
                            return isDefaultConstructable;
                        }

                    case TupleType t:
                        {
                            bool isDefaultConstructable = true;
                            foreach (var m in t.Members)
                            {
                                isDefaultConstructable &= ComputeIsDefaultConstructableOfType(m.type, path);
                            }

                            t.SetIsDefaultConstructable(isDefaultConstructable);
                            return isDefaultConstructable;
                        }

                    case ArrayType t:
                        {
                            bool isDefaultConstructable = ComputeIsDefaultConstructableOfType(t.TargetType, path);
                            t.SetIsDefaultConstructable(isDefaultConstructable);
                            return isDefaultConstructable;
                        }

                    case RangeType r:
                        {
                            bool isDefaultConstructable = ComputeIsDefaultConstructableOfType(r.TargetType, path);
                            r.SetIsDefaultConstructable(isDefaultConstructable);
                            return isDefaultConstructable;
                        }

                    case SumType _:
                        return false;

                    default:
                        ReportError($"ERROR? {type}");
                        return false;
                }
            }
            finally
            {
                path.Remove(type);
            }
        }
    }
}
