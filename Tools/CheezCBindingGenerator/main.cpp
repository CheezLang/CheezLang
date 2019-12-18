#include <iostream>
#include <string>
#include <clang-c/Index.h>

std::ostream& operator<<(std::ostream& stream, const CXString& str)
{
    stream << clang_getCString(str);
    clang_disposeString(str);
    return stream;
}

struct Context {
    CXTranslationUnit tu;
};

int main(int argc, char** argv)
{
    if (argc != 2) {
        std::cerr << "Wrong number of arguments\n";
        return 1;
    }

    auto header_file_path = argv[1];


    
    CXIndex index = clang_createIndex(0, 0);
    CXTranslationUnit unit = clang_parseTranslationUnit(
        index,
        header_file_path, nullptr, 0,
        nullptr, 0,
        CXTranslationUnit_None);
    if (unit == nullptr)
    {
        std::cerr << "Unable to parse translation unit. Quitting.\n";
        return 3;
    }

    Context ctx{unit};

    CXCursor cursor = clang_getTranslationUnitCursor(unit);
    clang_visitChildren(
        cursor,
        [](CXCursor c, CXCursor parent, CXClientData client_data)
    {
        Context& ctx = *(Context*)client_data;


        auto location = clang_getCursorLocation(c);
        if (clang_Location_isFromMainFile(location) == 0)
            return CXChildVisit_Continue;


        switch (c.kind) {

        case CXCursorKind::CXCursor_UnexposedDecl: {
            std::cout << "[ERROR] TODO: " << "Cursor '" << clang_getCursorSpelling(c) << "' of kind '" << clang_getCursorKindSpelling(clang_getCursorKind(c)) << "'\n";
            auto location = clang_getCursorLocation(c);
            auto source_range = clang_getRange(location, location);

            CXToken* tokens;
            unsigned num_tokens;
            clang_tokenize(ctx.tu, source_range, &tokens, &num_tokens);
            return CXChildVisit_Recurse;
            break;
        }

        default: {
            std::cout << "[ERROR] TODO: " << "Cursor '" << clang_getCursorSpelling(c) << "' of kind '" << clang_getCursorKindSpelling(clang_getCursorKind(c)) << "'\n";
            break;
        }
        }

        return CXChildVisit_Continue;
    }, &ctx);

    clang_disposeTranslationUnit(unit);
    clang_disposeIndex(index);
}