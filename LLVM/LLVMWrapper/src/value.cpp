#include "common.h"
#include "llvm/IR/Module.h"
#include "llvm/IR/Value.h"
#include "llvm/IR/Verifier.h"

#include <string>
#include <iostream>

DLL_API void llvm_value_set_linkage(llvm::GlobalValue* value, int linkage) {
    auto linkageType = (llvm::GlobalValue::LinkageTypes)linkage;
    value->setLinkage(linkageType);
}

DLL_API llvm::BasicBlock* llvm_value_append_basic_block(const char* name, llvm::Function* value) {
    
    return llvm::BasicBlock::Create(get_global_context(), name, value);
}

DLL_API llvm::BasicBlock* llvm_value_get_first_basic_block(llvm::Function* value) {
    return &value->getEntryBlock();
}

DLL_API bool llvm_value_verify_function(llvm::Function* value) {
    std::string errors;
    llvm::raw_string_ostream ss(errors);
    bool result = llvm::verifyFunction(*value, &ss);
    if (result) {
        std::cerr << "[LLVM verify function] " << errors << std::endl;
    }

    return result;
}
