#include <memory>

typedef void* (*GLADloadproc)(const char* name);
extern "C" int gladLoadGL(void);
extern "C" int gladLoadGLLoader(GLADloadproc);


extern "C" void __c__gladLoadGL(int32_t *ret) {
    *ret = (int32_t )gladLoadGL();
}
extern "C" void __c__gladLoadGLLoader(int32_t *ret, GLADloadproc _0) {
    *ret = (int32_t )gladLoadGLLoader(_0);
}
