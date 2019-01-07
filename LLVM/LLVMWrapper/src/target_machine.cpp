#include "common.h"
#include "llvm/Target/TargetMachine.h"
#include "llvm/Support/TargetRegistry.h"
#include "llvm/Support/TargetSelect.h"
#include "llvm/Target/TargetMachine.h"
#include "llvm/Target/TargetOptions.h"
#include "llvm/IR/LegacyPassManager.h"
#include "llvm/Support/FileSystem.h"

#include <iostream>

DLL_API void llvm_initialize_all_targets() {
    llvm::InitializeAllTargetInfos();
    llvm::InitializeAllTargets();
    llvm::InitializeAllTargetMCs();
    llvm::InitializeAllAsmParsers();
    llvm::InitializeAllAsmPrinters();
}

DLL_API const llvm::Target* llvm_get_target(const char* target_triple) {
    std::string error;
    return llvm::TargetRegistry::lookupTarget(target_triple, error);
}

DLL_API const llvm::TargetMachine* llvm_create_target_machine(llvm::Target* target, const char* target_triple, const char* cpu, const char* features) {
    llvm::TargetOptions options;
    auto rm = llvm::Optional<llvm::Reloc::Model>();
    auto cm = llvm::Optional<llvm::CodeModel::Model>();
    auto targetMachine = target->createTargetMachine(target_triple, cpu, features, options, rm, cm);
    return targetMachine;
}

DLL_API void llvm_delete_target_machine(llvm::TargetMachine* targetMachine) {
    delete targetMachine;
}

DLL_API bool llvm_emit_object_code(llvm::TargetMachine* targetMachine, llvm::Module* module, const char* filename) {
    std::error_code ec;
    llvm::raw_fd_ostream dest(filename, ec, llvm::sys::fs::F_None);

    if (ec) {
        std::cerr << "Could not open file: " << ec.message() << std::endl;
        return false;
    }

    llvm::legacy::PassManager pass;
    auto fileType = llvm::TargetMachine::CGFT_ObjectFile;

    if (targetMachine->addPassesToEmitFile(pass, dest, &dest, fileType)) {
        std::cerr << "TargetMachine can't emit a file of this type" << std::endl;
        return false;
    }


    pass.run(*module);
    dest.flush();

    return true;
}
