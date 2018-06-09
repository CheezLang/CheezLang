#include "lld/Common/Driver.h"
#include "llvm/ADT/ArrayRef.h"

extern "C" {
    __declspec(dllexport) bool llvm_link_coff(const char** argv, int argc)
    {
        llvm::ArrayRef<const char*> arr(argv, argc);
        return lld::coff::link(arr, false);
    }

    __declspec(dllexport) bool llvm_link_elf(const char** argv, int argc)
    {
        llvm::ArrayRef<const char*> arr(argv, argc);
        return lld::elf::link(arr, false);
    }

    
}
