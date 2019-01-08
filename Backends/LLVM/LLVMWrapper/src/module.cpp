#include "common.h"
#include "llvm/IR/Module.h"
#include "llvm/IR/IRPrintingPasses.h"
#include "llvm/Analysis/InstructionSimplify.h"
#include "llvm/IR/PassManager.h"

#include <fstream>

DLL_API llvm::Module* llvm_create_module(const char* name) {
    auto modPtr = new llvm::Module(name, get_global_context());
    return modPtr;
}

DLL_API void llvm_delete_module(llvm::Module* mod) {
    delete mod;
}

DLL_API void llvm_module_set_target_triple(llvm::Module* mod, const char* targetTriple) {
    mod->setTargetTriple(targetTriple);
}

DLL_API void llvm_module_get_target_triple(llvm::Module* mod, const char** data, int* length) {
    auto& tt = mod->getTargetTriple();
    *data = tt.data();
    *length = tt.size();
}

DLL_API llvm::Constant* llvm_module_get_or_add_global(llvm::Module* mod, const char* name, llvm::Type* type) {
    return mod->getOrInsertGlobal(name, type);
}

DLL_API llvm::Constant* llvm_module_get_or_add_function(llvm::Module* mod, const char* name, llvm::FunctionType* type) {
    return mod->getOrInsertFunction(name, type);
}

DLL_API bool llvm_module_print_to_file(llvm::Module* mod, const char* path) {
    std::error_code err;
    llvm::raw_fd_ostream file{ path, err, llvm::sys::fs::OpenFlags() };

    if (err) {
        return false;
    }

    llvm::PrintModulePass pmp{ file };

    llvm::AnalysisManager<llvm::Module> am;
    pmp.run(*mod, am);

    return true;
}
