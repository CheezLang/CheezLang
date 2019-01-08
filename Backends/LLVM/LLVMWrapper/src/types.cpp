#include "common.h"
#include "llvm/IR/Type.h"
#include "llvm/IR/Constants.h"
#include "llvm/IR/DerivedTypes.h"
#include "llvm/ADT/ArrayRef.h"

DLL_API llvm::PointerType* llvm_pointer_type(llvm::Type* target) {
    return target->getPointerTo();
}

DLL_API llvm::Type* llvm_void_type() {
    return llvm::Type::getVoidTy(get_global_context());
}

DLL_API llvm::IntegerType* llvm_int_type( int sizeInBits) {
    return llvm::Type::getIntNTy(get_global_context(), (unsigned int)sizeInBits);
}

DLL_API llvm::Type* llvm_float_type(int sizeInBits) {
    switch (sizeInBits) {
    case 32: return llvm::Type::getFloatTy(get_global_context());
    case 64: return llvm::Type::getDoubleTy(get_global_context());
    default: return nullptr;
    }
}

DLL_API llvm::StructType* llvm_named_struct(const char* name) {
    return llvm::StructType::create(get_global_context(), name);
}

DLL_API void llvm_struct_set_body(llvm::StructType* type, llvm::Type** types, int count, bool packed) {
    llvm::ArrayRef<llvm::Type*> arr(types, (size_t)count);
    type->setBody(arr, packed);
}

DLL_API llvm::Type* llvm_function_type(llvm::Type* returnType, llvm::Type** argTypes, int count) {
    llvm::ArrayRef<llvm::Type*> paramTypes(argTypes, count);
    return llvm::FunctionType::get(returnType, paramTypes, false);
}

DLL_API llvm::Type* llvm_function_type_varargs(llvm::Type* returnType, llvm::Type** argTypes, int count) {
    llvm::ArrayRef<llvm::Type*> paramTypes(argTypes, count);
    return llvm::FunctionType::get(returnType, paramTypes, true);
}
