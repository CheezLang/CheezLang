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
i32* malloc(i32 size);
void add(int a, int b);
void test();
i32 main();

// global variables
i32 a;
auto b = 10;
auto c = 9;
i32* arr = malloc(4);

// function implementations
void add(int a, int b) {
    std::cout << a << " + " << b;
}
void test() {
    std::cout << "was geht?";
}
i32 main() {
    i32* arr = malloc(4);
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
    add(1, 2);
}

