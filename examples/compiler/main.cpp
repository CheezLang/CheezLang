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

void _flush_cout() {
    std::cout.flush();
}


// type declarations
struct String;
struct TokenLocation;
struct Lexer;
struct Token;
enum class TokenType;

struct String {
    u32 length;
    u8* data;
};
struct TokenLocation {
    string file;
    u32 line;
    u32 start;
    u32 end;
};
struct Lexer {
    String text;
    TokenLocation location;
};
struct Token {
    TokenType type;
    TokenLocation location;
};
enum class TokenType {
    Unknown,
    NewLine,
    EndOfFile,
    StringLiteral,
    NumberLiteral,
    Identifier,
    Semicolon,
    DoubleColon,
    Colon,
    Comma,
    Period,
    Equal,
    Ampersand,
    HashTag,
    Plus,
    Minus,
    Asterisk,
    ForwardSlash,
    Percent,
    Less,
    LessEqual,
    Greater,
    GreaterEqual,
    DoubleEqual,
    NotEqual,
    OpenParen,
    ClosingParen,
    OpenBrace,
    ClosingBrace,
    OpenBracket,
    ClosingBracket,
    KwReturn,
    KwCast,
    KwRef,
    KwFn,
    KwStruct,
    KwEnum,
    KwImpl,
    KwConstant,
    KwVar,
    KwIf,
    KwElse,
    KwFor,
    KwWhile,
    KwAnd,
    KwOr,
    KwTrue,
    KwFalse,
    KwUsing,
    KwPrint,
    KwPrintln,

};

// forward declarations
void Main();
bool LoadString(string filename, String* content);

namespace String_impl {
    void Print(String& self);
}
namespace TokenLocation_impl {
    TokenLocation Clone(TokenLocation& self);
}

// global variables

// function implementations
void Main() {
    string filename = "D:\\Programming\\CS\\CheezLang\\examples\\example_1.che";
    String file;
    if (LoadString(filename, &file)) {
    } else {
        std::cout << "Failed to load file '" << filename << "'" << std::endl;
    }
    Lexer lexer;
    lexer.text = file;
    ;
}
bool LoadString(string filename, String* content) {
    i32 SeekCur = 1;
    i32 SeekEnd = 2;
    i32 SeekSet = 0;
    FILE* file;
    i32 err = fopen_s(&file, filename, "r");
    if ((err) != (0)) {
        return false;
    }
    fseek(file, 0, SeekEnd);
    (*content).length = ((u32)(ftell(file)));
    fseek(file, 0, SeekSet);
    (*content).data = ((u8*)(malloc(((*content).length) + (1))));
    fread((*content).data, 1, ((u64)((*content).length)), file);
    (*content).data[(*content).length] = 0;
    return true;
}

namespace String_impl {
    void Print(String& self) {
        std::cout << self.data << std::endl;
    }
}
namespace TokenLocation_impl {
    TokenLocation Clone(TokenLocation& self) {
        TokenLocation t;
        t.file = self.file;
        t.line = self.line;
        t.start = self.start;
        t.end = self.end;
        ;
        return t;
    }
}

// entry point to the program
int main()
{
    // call user main function
    Main();
}

