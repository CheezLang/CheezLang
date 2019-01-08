namespace LLVMCS
{
    // copied from llvm GobalValue.h
    public enum LinkageTypes
    {
        ExternalLinkage = 0,///< Externally visible function
        AvailableExternallyLinkage, ///< Available for inspection, not emission.
        LinkOnceAnyLinkage, ///< Keep one copy of function when linking (inline)
        LinkOnceODRLinkage, ///< Same, but only replaced by something equivalent.
        WeakAnyLinkage,     ///< Keep one copy of named function when linking (weak)
        WeakODRLinkage,     ///< Same, but only replaced by something equivalent.
        AppendingLinkage,   ///< Special purpose, only applies to global arrays
        InternalLinkage,    ///< Rename collisions when linking (static functions).
        PrivateLinkage,     ///< Like Internal, but omit from symbol table.
        ExternalWeakLinkage,///< ExternalWeak linkage description.
        CommonLinkage       ///< Tentative definitions.
    };
}
