#include "common.h"
#include "llvm/IR/Type.h"
#include "llvm/IR/DerivedTypes.h"
#include "llvm/ADT/ArrayRef.h"

DLL_API llvm::PointerType* llvm_pointer_type(llvm::Type* target) {
    return target->getPointerTo();
}

DLL_API llvm::Type* llvm_void_type(llvm::LLVMContext* context) {
    return llvm::Type::getVoidTy(*context);
}

DLL_API llvm::IntegerType* llvm_int_type(llvm::LLVMContext* context, int sizeInBits) {
    return llvm::Type::getIntNTy(*context, (unsigned int)sizeInBits);
}

DLL_API llvm::Type* llvm_float_type(llvm::LLVMContext* context, int sizeInBits) {
    switch (sizeInBits) {
    case 32: return llvm::Type::getFloatTy(*context);
    case 64: return llvm::Type::getDoubleTy(*context);
    default: return nullptr;
    }
}

DLL_API llvm::StructType* llvm_named_struct(llvm::LLVMContext* context, const char* name) {
    return llvm::StructType::create(*context, name);
}

DLL_API void llvm_struct_set_body(llvm::StructType* type, llvm::Type** types, int count, bool packed) {
    llvm::ArrayRef<llvm::Type*> arr(types, (size_t)count);
    type->setBody(arr, packed);
}
