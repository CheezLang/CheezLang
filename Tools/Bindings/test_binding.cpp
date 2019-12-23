#include <memory>
#include "D:/Programming/CheezLang/data/test.hpp"

extern "C" void __c__Vector_new_3(Vector* self) {
    new (self) Vector();
}
extern "C" void __c__Vector_new_4(Vector* self, Vector  (*_other)) {
    new (self) Vector(*_other);
}
extern "C" void __c__Vector_new_5(Vector* self, Vector  (*_other)) {
    new (self) Vector(_other);
}
extern "C" void __c__Vector_new_6(Vector* self, float _x, float _y, float _z) {
    new (self) Vector(_x, _y, _z);
}
extern "C" void __c__Vector_dtor(Vector* self) {
    self->~Vector();
}
extern "C" void __c__Vector_getX_8(Vector* self, float *ret) {
    *ret = self->getX();
}
extern "C" void __c__Vector_getY_9(Vector* self, float *ret) {
    *ret = self->getY();
}
extern "C" void __c__Vector_foo_10(Vector* self, float *ret, Vector (*_uiae), float _s) {
    *ret = self->foo(*_uiae, _s);
}
extern "C" void __c__Vector_bar_11(Vector* self, Vector *ret, Vector (*_uiae), float _s) {
    *ret = self->bar(*_uiae, _s);
}
extern "C" void __c__Vector_mul_12(Vector* self, Vector (*_other)) {
    self->mul(*_other);
}
extern "C" void __c__Vector_div_13(Vector* self, Vector (*_other)) {
    self->div(*_other);
}
extern "C" void __c__Vector_add_14(Vector* self, Vector  (*_other)) {
    self->add(*_other);
}
extern "C" void __c__Vector_sub_15(Vector* self, Vector  (*_other)) {
    self->sub(*_other);
}
extern "C" void __c__Vector_mod_16(Vector* self, Vector  (*_other)) {
    self->mod(_other);
}
