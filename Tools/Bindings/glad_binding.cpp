#include <memory>
#include "glad_binding_source.cpp"

extern "C" void __c__gladLoadGL(int32_t *ret) {
    *ret = (int32_t )gladLoadGL();
}
extern "C" void __c__gladLoadGLLoader(int32_t *ret, GLADloadproc _0) {
    *ret = (int32_t )gladLoadGLLoader(_0);
}
