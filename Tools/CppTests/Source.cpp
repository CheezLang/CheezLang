//#define _ITERATOR_DEBUG_LEVEL 0

#include <string>
#include <iostream>
#include <cstdint>
#include <memory>

#include "llvm/ExecutionEngine/Orc/LLJIT.h"
#include "llvm/IR/Module.h"
#include "llvm/Target/TargetMachine.h"
#include "llvm/Target/TargetOptions.h"


int main() {
    using namespace llvm;
    using namespace llvm::orc;
    //InitializeNativeTarget();

    LLVMContext ctx;
    auto module = std::make_unique<Module>("MyModule", ctx);

    auto jit = LLJITBuilder().setNumCompileThreads(1).create();
    if (!jit) {
        std::cout << "error" << std::endl;
        return 1;
    }

    std::cout << "ok" << std::endl;
}