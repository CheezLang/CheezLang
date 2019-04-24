#include "Windows.h"

extern "C" void* winmacro_GetFiberData() {
    return GetFiberData();
}

extern "C" void* winmacro_GetCurrentFiber() {
    return GetCurrentFiber();
}
