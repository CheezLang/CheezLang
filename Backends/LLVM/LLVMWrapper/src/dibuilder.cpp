#include "common.h"

#include "llvm\IR\DIBuilder.h"
#include "llvm\BinaryFormat\Dwarf.h"

using namespace llvm;
typedef const char* string;

DLL_API DIFile* dibuilder_create_file(DIBuilder* dibuilder, string filename, string directory) {
    return dibuilder->createFile(filename, directory);
}

DLL_API DICompileUnit* dibuilder_create_compile_unit(DIBuilder* dibuilder, DIFile* file, string producer, bool isOptimized) {
    return dibuilder->createCompileUnit(dwarf::DW_LANG_C, file, producer, isOptimized, "", 0);
}

DLL_API DIBuilder* dibuilder_new(Module* mod) {
    return new DIBuilder(*mod);
}

DLL_API void dibuilder_finalize(DIBuilder* dibuilder) {
    dibuilder->finalize();
}

DLL_API void dibuilder_delete(DIBuilder* dibuilder) {
    delete dibuilder;
}