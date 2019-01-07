#include "common.h"
#include "llvm\IR\DataLayout.h"
#include "llvm\IR\Module.h"

DLL_API llvm::DataLayout* llvm_create_data_layout_1(const char* layoutDesc) {
    return new llvm::DataLayout(layoutDesc);
}

DLL_API llvm::DataLayout* llvm_create_data_layout_2(llvm::Module* mod) {
    return new llvm::DataLayout(mod);
}

DLL_API void llvm_delete_data_layout(llvm::DataLayout* dataLayout) {
    delete dataLayout;
}
