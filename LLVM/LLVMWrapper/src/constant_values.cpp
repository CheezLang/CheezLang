#include "common.h"
#include "llvm/IR/Type.h"
#include "llvm/IR/DerivedTypes.h"
#include "llvm/IR/Constants.h"
#include "llvm/ADT/ArrayRef.h"

DLL_API llvm::Constant* llvm_const_int_signed(llvm::Type* type, int64_t value) {
    uint64_t uval = *reinterpret_cast<uint64_t*>(&value);
    return llvm::ConstantInt::get(type, uval, true);
}

DLL_API llvm::Constant* llvm_const_int(llvm::Type* type, uint64_t value) {
    return llvm::ConstantInt::get(type, value, false);
}

DLL_API llvm::Constant* llvm_const_float(llvm::Type* type, double value) {
    return llvm::ConstantFP::get(type, value);
}

DLL_API llvm::ConstantPointerNull* llvm_const_null_pointer(llvm::PointerType* type) {
    return llvm::ConstantPointerNull::get(type);
}

DLL_API llvm::Constant* llvm_const_struct(llvm::StructType* type, llvm::Constant** bodyValues, int count) {
    llvm::ArrayRef<llvm::Constant*> body(bodyValues, (size_t)count);
    return llvm::ConstantStruct::get(type, body);
}

DLL_API llvm::Constant* llvm_const_array(llvm::ArrayType* type, llvm::Constant** bodyValues, int count) {
    llvm::ArrayRef<llvm::Constant*> body(bodyValues, (size_t)count);
    return llvm::ConstantArray::get(type, body);
}

DLL_API llvm::Constant* llvm_const_aggrerate_zero(llvm::Type* type) {
    return llvm::ConstantAggregateZero::get(type);
}
