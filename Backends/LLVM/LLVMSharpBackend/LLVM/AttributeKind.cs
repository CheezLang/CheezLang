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
        Alignment = 1,
        AllocSize = 2,
        AlwaysInline = 3,
        ArgMemOnly = 4,
        Builtin = 5,
        ByVal = 6,
        Cold = 7,
        Convergent = 8,
        Dereferenceable = 9,
        DereferenceableOrNull = 10,
        InAlloca = 11,
        InReg = 12,
        InaccessibleMemOnly = 13,
        InaccessibleMemOrArgMemOnly = 14,
        InlineHint = 15,
        JumpTable = 16,
        MinSize = 17,
        Naked = 18,
        Nest = 19,
        NoAlias = 20,
        NoBuiltin = 21,
        NoCapture = 22,
        // NoCfCheck = ,
        NoDuplicate = 23,
        NoImplicitFloat = 24,
        NoInline = 25,
        NoRecurse = 26,
        NoRedZone = 27,
        NoReturn = 28,
        NoUnwind = 29,
        NonLazyBind = 30,
        NonNull = 31,
        // OptForFuzzing = ,
        OptimizeForSize = 32,
        OptimizeNone = 33,
        ReadNone = 34,
        ReadOnly = 35,
        Returned = 36,
        ReturnsTwice = 37,
        SExt = 38,
        SafeStack = 39,
        SanitizeAddress = 40,
        // SanitizeHWAddress = ,
        SanitizeMemory = 41,
        SanitizeThread = 42,
        //ShadowCallStack = ,
        Speculatable = 43,
        // SpeculativeLoadHardening = ,
        StackAlignment = 44,
        StackProtect = 45,
        StackProtectReq = 46,
        StackProtectStrong = 47,
        // StrictFP = ,
        StructRet = 48,
        SwiftError = 49,
        SwiftSelf = 50,
        UWTable = 51,
        WriteOnly = 52,
        ZExt = 53,
    }
}
