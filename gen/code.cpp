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

// forward declarations
void Main();

// global variables

// function implementations
void Main() {
    i64 a = 1;
    std::cout << a << '\n';
    if (a == 1) {
        i64 a___1 = a + 1;
        std::cout << a___1 << '\n';
        i64 a___2 = a___1 + 1;
        std::cout << a___2 << '\n';
    }
    std::cout << a << '\n';
    i64 a___3 = a + 1;
    std::cout << a___3 << '\n';
}

// entry point to the program
int main()
{
    // call user main function
    Main();
}

