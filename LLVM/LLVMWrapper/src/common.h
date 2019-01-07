#pragma once

#include "llvm/IR/LLVMContext.h"

#define DLL_API extern "C" __declspec(dllexport)
#define DEBUG 0

llvm::LLVMContext& get_global_context();
