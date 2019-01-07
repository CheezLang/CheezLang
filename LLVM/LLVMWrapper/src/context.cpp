#include "common.h"

#include "llvm\IR\LLVMContext.h"

#if DEBUG
#include <iostream>
#endif

DLL_API void* llvm_create_context() {
    auto conPtr =  new llvm::LLVMContext();
#if DEBUG
    std::cout << "llvm_create_context() => " << (long long)conPtr << std::endl;
#endif
    return (void*)conPtr;
}

DLL_API void llvm_delete_context(void* conPtr) {
#if DEBUG
    std::cout << "llvm_delete_context(" << (long long)conPtr << ")" << std::endl;
#endif
    delete static_cast<llvm::LLVMContext*>(conPtr);
}
