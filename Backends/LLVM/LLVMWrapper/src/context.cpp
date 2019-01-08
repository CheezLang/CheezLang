#include "common.h"

#include "llvm\IR\LLVMContext.h"

llvm::LLVMContext& get_global_context() {
    static llvm::LLVMContext* global_context = nullptr;
    if (global_context == nullptr) {
        global_context = new llvm::LLVMContext();
    }

    return *global_context;
}
