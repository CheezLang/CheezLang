#include "common.h"

#include "llvm/IR/IRBuilder.h"

DLL_API llvm::IRBuilder<>* llvm_create_ir_builder() {
    return new llvm::IRBuilder<>(get_global_context());
}

DLL_API void llvm_delete_ir_builder(llvm::IRBuilder<>* builder) {
    delete builder;
}

DLL_API void llvm_ir_builder_position_at_end(llvm::IRBuilder<>* builder, llvm::BasicBlock* bb) {
    builder->SetInsertPoint(bb);
}

DLL_API void llvm_ir_builder_position_before(llvm::IRBuilder<>* builder, llvm::Instruction* inst) {
    builder->SetInsertPoint(inst);
}

DLL_API llvm::AllocaInst* llvm_ir_builder_create_alloca(llvm::IRBuilder<>* builder, llvm::Type* type, const char* name) {
    return builder->CreateAlloca(type, nullptr, name);
}

DLL_API llvm::BranchInst* llvm_ir_builder_create_br(llvm::IRBuilder<>* builder, llvm::BasicBlock* dest) {
    return builder->CreateBr(dest);
}

DLL_API llvm::ReturnInst* llvm_ir_builder_create_ret_void(llvm::IRBuilder<>* builder) {
    return builder->CreateRetVoid();
}

DLL_API llvm::ReturnInst* llvm_ir_builder_create_ret(llvm::IRBuilder<>* builder, llvm::Value* value) {
    return builder->CreateRet(value);
}

DLL_API llvm::CallInst* llvm_ir_builder_create_call(llvm::IRBuilder<>* builder, llvm::Function* callee, llvm::Value** args, int argCount) {
    llvm::ArrayRef<llvm::Value*> arguments(args, (size_t)argCount);
    return builder->CreateCall(callee, arguments);
}
