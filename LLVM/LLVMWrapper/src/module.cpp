#include "common.h"
#include "llvm/IR/Module.h"
#include "llvm/IR/IRPrintingPasses.h"
#include "llvm/Analysis/InstructionSimplify.h"
#include "llvm/IR/PassManager.h"

#include <fstream>

DLL_API llvm::Module* llvm_create_module(const char* name, llvm::LLVMContext* context) {
    auto modPtr = new llvm::Module(name, *context);
#if DEBUG
    std::cout << "llvm_create_module() => " << (long long)modPtr << std::endl;
#endif
    return modPtr;
}

DLL_API void llvm_delete_module(llvm::Module* mod) {
#if DEBUG
    std::cout << "llvm_delete_module(" << (long long)modPtr << ")" << std::endl;
#endif
}

DLL_API void llvm_module_set_target_triple(llvm::Module* mod, const char* targetTriple) {
    mod->setTargetTriple(targetTriple);
}

DLL_API void llvm_module_get_target_triple(llvm::Module* mod, const char** data, int* length) {
    auto& tt = mod->getTargetTriple();
    *data = tt.data();
    *length = tt.size();
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

void test() {
    llvm::LLVMContext context;
    llvm::Module m("test", context);
    //m.set
}
