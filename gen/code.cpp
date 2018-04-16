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
i64 add(i64 a, i64 b);
void Main();

// global variables

// function implementations
i64 add(i64 a, i64 b) {
    return a * b + a / b;
}
void Main() {
    i64 a = 5;
    i64 b = add(7, 54);
    string stringTest = "Hello world";
    std::cout << stringTest << '\n';
    std::cout << b << '\n';
    std::cout << add(7, 54) << '\n';
}

// entry point to the program
int main()
{
    // call user main function
    Main();
}

