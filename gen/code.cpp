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
    i32 SeekCur = 1;
    i32 SeekEnd = 2;
    i32 SeekSet = 0;
    string filename = "../examples/example_1.che";
    FILE* file;
    i32 err = fopen_s(&file, filename, "r");
    if (err != 0) {
        std::cout << "Couldn't open file '" << filename << "'" << '\n';
        return;
    }
    fseek(file, 0, SeekEnd);
    i32 size = ftell(file);
    std::cout << "Loading file " << filename << " with size " << size << " bytes" << '\n';;
    fseek(file, 0, SeekSet);
    i8* buff = (i8*)(malloc(size + 1));
    fread(buff, 1, (u64)(size), file);
    buff[size] = 0;
    string text = (string)(buff);
    std::cout << "===================" << '\n';;
    std::cout << text << '\n';;
    std::cout << "===================" << '\n';;
    free(buff);
}

// entry point to the program
int main()
{
    // call user main function
    Main();
}

