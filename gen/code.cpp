#include <string>
#include <iostream>
#include <cstdint>

// type defininions
using u8 = uint8_t;
using u16 = uint16_t;
using u32 = uint32_t;
using u64 = uint64_t;

using i8 =  int8_t;
using i16 = int16_t;
using i32 = int32_t;
using i64 = int64_t;

using f32 = float_t;
using f64 = double_t;

using string = const char*;


// type declarations
struct Vec3 {
    i32 x;
    i32 y;
    i32 z;
};


// forward declarations
namespace Vec3_impl {
    void _print_me(Vec3 self);
    void _normalize(Vec3 self);
}
void _main();


// compiled statements
auto a = 1;
auto b = 10;
auto c = 9;

namespace Vec3_impl {
    void _print_me(Vec3 self) {
        std::cout << "(";
        std::cout << self.x << ", " << self.y << ", " << self.z;
        std::cout << ")\n";
    }
    void _normalize(Vec3 self) {
        std::cout << "normalize\n";
    }
}
void _main() {
    a = 2;
    std::cout << a << " " << b << " " << c << " " << "\n";
    std::cout << 1 << " + " << 2 << " + " << 3;
    std::cout << " = 6\n";
    if (1) {
        std::cout << "hello ";
    }
    if (1) {
        std::cout << "world";
    } else {
        std::cout << "you";
    }
    std::cout << "\n";
}


// entry point to the program
int main()
{
    // call user main function
    _main();
}

