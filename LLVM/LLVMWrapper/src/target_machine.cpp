#include "common.h"
#include "llvm/IR/Module.h"
#include "llvm/Target/TargetMachine.h"
#include "llvm/Support/TargetRegistry.h"
#include "llvm/Support/TargetSelect.h"
#include "llvm/Target/TargetMachine.h"
#include "llvm/Target/TargetOptions.h"
#include "llvm/IR/LegacyPassManager.h"
#include "llvm/Support/FileSystem.h"
#include <llvm/Transforms/IPO/PassManagerBuilder.h>

#include <iostream>

DLL_API bool llvm_module_emit_to_obj(llvm::Module* module, const char* filename, const char* cpu, const char* features) {
    llvm::InitializeAllTargetInfos();
    llvm::InitializeAllTargets();
    llvm::InitializeAllTargetMCs();
    llvm::InitializeAllAsmParsers();
    llvm::InitializeAllAsmPrinters();

    auto& targetTriple = module->getTargetTriple();

    std::string error;
    auto target = llvm::TargetRegistry::lookupTarget(targetTriple, error);
    if (!target) {
        std::cerr << "Could not create target from target triple '" << targetTriple << "': " << error << std::endl;
        return false;
    }

    llvm::TargetOptions options;
    auto rm = llvm::Optional<llvm::Reloc::Model>();
    auto cm = llvm::Optional<llvm::CodeModel::Model>();
    auto targetMachine = target->createTargetMachine(targetTriple, cpu, features, options, rm, cm);
    
    std::error_code ec;
    llvm::raw_fd_ostream dest(filename, ec, llvm::sys::fs::F_None);

    if (ec) {
        std::cerr << "Could not open file: " << ec.message() << std::endl;
        return false;
    }

    llvm::PassManagerBuilder pmb;
    pmb.OptLevel = targetMachine->getOptLevel();

    llvm::legacy::PassManager MPM;
    pmb.populateModulePassManager(MPM);

    auto fileType = llvm::TargetMachine::CGFT_ObjectFile;

    if (targetMachine->addPassesToEmitFile(MPM, dest, nullptr, fileType)) {
        std::cerr << "TargetMachine can't emit a file of this type" << std::endl;
        return false;
    }

    std::cout << "test" << std::endl;

    MPM.run(*module);
    dest.flush();

    return true;
}
