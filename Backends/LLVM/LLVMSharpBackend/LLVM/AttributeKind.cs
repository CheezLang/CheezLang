namespace Cheez.CodeGeneration.LLVMCodeGen
{
    public static class AttributeKindExt
    {
        public static uint ToUInt(this LLVMAttributeKind k)
        {
            return (uint)k;
        }
    }

    public enum LLVMAttributeKind
    {
        None = 0,
        Alignment,
        AllocSize,
        AlwaysInline,
        ArgMemOnly,
        Builtin,
        ByVal,
        Cold,
        Convergent,
        Dereferenceable,
        DereferenceableOrNull,
        InAlloca,
        InReg,
        InaccessibleMemOnly,
        InaccessibleMemOrArgMemOnly,
        InlineHint,
        JumpTable,
        MinSize,
        Naked,
        Nest,
        NoAlias,
        NoBuiltin,
        NoCapture,
        //NoCfCheck,
        NoDuplicate,
        NoImplicitFloat,
        NoInline,
        NoRecurse,
        NoRedZone,
        NoReturn,
        NoUnwind,
        NonLazyBind,
        NonNull,
        //OptForFuzzing,
        OptimizeForSize,
        OptimizeNone,
        ReadNone,
        ReadOnly,
        Returned,
        ReturnsTwice,
        SExt,
        SafeStack,
        SanitizeAddress,
        //SanitizeHWAddress,
        SanitizeMemory,
        SanitizeThread,
        //ShadowCallStack,
        Speculatable,
        StackAlignment,
        StackProtect,
        StackProtectReq,
        StackProtectStrong,
        //StrictFP,
        StructRet,
        SwiftError,
        SwiftSelf,
        UWTable,
        WriteOnly,
        ZExt
    }
}
