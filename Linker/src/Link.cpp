//#include "lld/Common/Driver.h"
//#include "llvm/ADT/ArrayRef.h"

#include <iostream>

extern "C" {
    __declspec(dllexport) bool link_coff(const char** argv, int argc)
    {
        /*llvm::ArrayRef<const char*> arr(argv, argc);
        return lld::coff::link(arr, false);*/

        for (int i = 0; i < argc; i++)
        {
            std::cout << argv[i] << std::endl;
        }

        return true;
    }
}
