#include <iostream>
#include <fstream>
#include <sstream>
#include <string>
#include <clang-c/Index.h>

#if 0
#define LOG_CURSOR(c) std::cout << "Cursor '" << clang_getCursorSpelling(c) << "' of kind '" << clang_getCursorKindSpelling(clang_getCursorKind(c)) << "'\n";
#else
#define LOG_CURSOR(c)
#endif

std::ostream& operator<<(std::ostream& stream, const CXString& str)
{
    stream << clang_getCString(str);
    clang_disposeString(str);
    return stream;
}

enum class ArgumentIndices : int {
    InputFile = 1,
    OutputFile,
    COUNT
};

struct Context {
    std::ofstream &cheez_file, &c_file;
    std::ostream* buffer = nullptr;
    bool requires_c_wrapper = false;
    uint64_t args_which_need_wrapping = 0;
    uint64_t param_index = 0;
};

void emit_function_decl(Context& ctx, CXCursor cursor);
void emit_struct_decl(Context& ctx, CXCursor cursor);
void emit_typedef_decl(Context& ctx, CXCursor cursor);
void emit_enum_decl(Context& ctx, CXCursor cursor);

int main(int argc, char** argv)
{
    std::cout << "ok\n";
    if (argc != (int) ArgumentIndices::COUNT) {
        std::cerr << "Wrong number of arguments\n";
        return 1;
    }

    auto header_file_path = argv[(int)ArgumentIndices::InputFile];
    auto output_file_name = argv[(int)ArgumentIndices::OutputFile];

    std::stringstream cheez_file_name;
    cheez_file_name << output_file_name << ".che";
    std::stringstream c_file_name;
    c_file_name << output_file_name << ".c";

    std::ofstream cheez_file{ cheez_file_name.str() };
    std::ofstream c_file{ c_file_name.str() };

    if (!cheez_file || !c_file) {
        std::cerr << "Failed to create output files '" << cheez_file_name.str() << "' and '" << c_file_name.str() << "'\n";
        return 2;
    }

    Context ctx{ cheez_file, c_file };

    {
        std::string header(header_file_path);
        size_t last_slash = header.find_last_of('/');
        size_t last_backslash = header.find_last_of('\\');
        size_t last = -1;
        if (last_slash == std::string::npos) last = last_backslash;
        if (last_backslash == std::string::npos) last = last_slash;
        if (last == std::string::npos) last = 0;
        else last += 1;
        std::string header_name = header.substr(last);
        c_file << "#include \"" << header_name << "\"\n\n";
    }

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

    CXCursor cursor = clang_getTranslationUnitCursor(unit);
    clang_visitChildren(
        cursor,
        [](CXCursor c, CXCursor parent, CXClientData client_data)
    {
        LOG_CURSOR(c);
        Context& ctx = *(Context*)client_data;

        switch (c.kind) {
        case CXCursor_FunctionDecl:
            emit_function_decl(ctx, c);
            break;

        case CXCursor_StructDecl:
            emit_struct_decl(ctx, c);
            break;

        case CXCursor_TypedefDecl:
            emit_typedef_decl(ctx, c);
            break;

        case CXCursor_EnumDecl:
            emit_enum_decl(ctx, c);
            break;

        default: {
            std::cout << "[ERROR] TODO: " << "Cursor '" << clang_getCursorSpelling(c) << "' of kind '" << clang_getCursorKindSpelling(clang_getCursorKind(c)) << "'\n";
            break;
        }
        }

        return CXChildVisit_Continue;
        //return CXChildVisit_Recurse;
    }, &ctx);

    clang_disposeTranslationUnit(unit);
    clang_disposeIndex(index);
}

void emit_struct_decl(Context& ctx, CXCursor cursor) {
    auto name = clang_getCString(clang_getCursorSpelling(cursor));
    if (name == nullptr)
        return;

    ctx.cheez_file << name << " :: struct {}\n";
}

void emit_typedef_decl(Context& ctx, CXCursor cursor) {
    auto name = clang_getCString(clang_getCursorSpelling(cursor));
    if (name == nullptr)
        return;

    //ctx.cheez_file << name << " :: struct {}\n";
}

void emit_enum_decl(Context& ctx, CXCursor cursor) {
    auto name = clang_getCString(clang_getCursorSpelling(cursor));
    if (name == nullptr)
        return;

    ctx.cheez_file << name << " :: enum {}\n";
}

std::string get_anonymous_param_name(int index) {
    std::stringstream ss;
    ss << "p" << index;
    return ss.str();
}

void emit_type(Context& ctx, const CXType& type, bool behind_pointer = false) {
    long long size = clang_Type_getSizeOf(type);

    switch (type.kind) {
    case CXTypeKind::CXType_Void:       (*ctx.buffer) << "void"; break;

    case CXTypeKind::CXType_UChar:      (*ctx.buffer) << "u" << (size * 8); break;
    case CXTypeKind::CXType_UShort:     (*ctx.buffer) << "u" << (size * 8); break;
    case CXTypeKind::CXType_UInt:       (*ctx.buffer) << "u" << (size * 8); break;
    case CXTypeKind::CXType_ULong:      (*ctx.buffer) << "u" << (size * 8); break;
    case CXTypeKind::CXType_ULongLong:  (*ctx.buffer) << "u" << (size * 8); break;

    case CXTypeKind::CXType_Char_S:     (*ctx.buffer) << "i" << (size * 8); break;
    case CXTypeKind::CXType_Short:      (*ctx.buffer) << "i" << (size * 8); break;
    case CXTypeKind::CXType_Int:        (*ctx.buffer) << "i" << (size * 8); break;
    case CXTypeKind::CXType_Long:       (*ctx.buffer) << "i" << (size * 8); break;
    case CXTypeKind::CXType_LongLong:   (*ctx.buffer) << "i" << (size * 8); break;

    case CXTypeKind::CXType_Float:      (*ctx.buffer) << "f32"; break;
    case CXTypeKind::CXType_Double:     (*ctx.buffer) << "f64"; break;

    case CXTypeKind::CXType_Bool:       (*ctx.buffer) << "bool"; break;

    case CXTypeKind::CXType_ConstantArray: {
        auto target_type = clang_getArrayElementType(type);
        auto array_size = clang_getArraySize(type);
        (*ctx.buffer) << "&";
        emit_type(ctx, target_type);
        break;
    }

    case CXTypeKind::CXType_IncompleteArray: {
        auto target_type = clang_getArrayElementType(type);
        (*ctx.buffer) << "&";
        emit_type(ctx, target_type);
        break;
    }

    case CXTypeKind::CXType_Pointer: {
        auto target_type = clang_getPointeeType(type);
        (*ctx.buffer) << "&";
        emit_type(ctx, target_type, true);
        break;
    }

    case CXTypeKind::CXType_Typedef: {

        if (behind_pointer) {
            (*ctx.buffer) << clang_getTypeSpelling(type);
        } else {
            auto typedef_decl = clang_getTypeDeclaration(type);
            auto elo = clang_getTypedefDeclUnderlyingType(typedef_decl);
            auto actual_type = clang_Type_getNamedType(elo);
            auto type_decl = clang_getTypeDeclaration(actual_type);

            switch (type_decl.kind) {
            case CXCursorKind::CXCursor_StructDecl:
            case CXCursorKind::CXCursor_UnionDecl:
            case CXCursorKind::CXCursor_ClassDecl:
                if (ctx.param_index == 0) {
                    (*ctx.buffer) << "void";
                } else {
                    (*ctx.buffer) << "&" << clang_getTypeSpelling(type);
                }
                ctx.requires_c_wrapper = true;
                ctx.args_which_need_wrapping |= (uint64_t)1 << ctx.param_index;
                break;

            default:
                (*ctx.buffer) << clang_getTypeSpelling(type);
                break;
            }
        }
        break;
    }

    default: {
        auto spelling = clang_getTypeSpelling(type);
        (*ctx.buffer) << "__UNKNOWN__";
        std::cout << "[ERROR] Failed to translate type '" << spelling << "' (" << clang_getTypeKindSpelling(type.kind) << ")\n";
        break;
    }
    }
}

void emit_function_decl_c_wrapper(Context& ctx, CXCursor cursor) {
    auto function_type = clang_getCursorType(cursor);
    auto return_type = clang_getResultType(function_type);

    if (ctx.args_which_need_wrapping & 1) {
        ctx.c_file << "void";
    } else {
        ctx.c_file << clang_getTypeSpelling(return_type);
    }
    ctx.c_file << " _" << clang_getCursorSpelling(cursor) << "(";

    // emit parameters
    if (ctx.args_which_need_wrapping & 1)
        ctx.c_file << clang_getTypeSpelling(return_type) << "* ret";
    ctx.param_index = 1;
    clang_visitChildren(cursor, [](CXCursor c, CXCursor parent, CXClientData client_data) {
        if (c.kind != CXCursorKind::CXCursor_ParmDecl)
            return CXChildVisit_Continue;

        Context& ctx = *(Context*)client_data;
        if (ctx.param_index > 1 || (ctx.args_which_need_wrapping & 1))
            ctx.c_file << ", ";

        // type
        if (ctx.args_which_need_wrapping & ((uint64_t)1 << ctx.param_index)) {
            ctx.c_file << clang_getTypeSpelling(clang_getCursorType(c)) << "* ";
        } else {
            ctx.c_file << clang_getTypeSpelling(clang_getCursorType(c)) << " ";
        }

        // name
        auto param_name_length = strlen(clang_getCString(clang_getCursorSpelling(c)));
        if (param_name_length == 0)
            ctx.c_file << get_anonymous_param_name(ctx.param_index - 1);
        else if (ctx.args_which_need_wrapping & 1 && ctx.param_index == 1)
            ctx.c_file << "ret";
        else
            ctx.c_file << clang_getCursorSpelling(c);

        ctx.param_index++;
        return CXChildVisit_Continue;
    }, &ctx);

    ctx.c_file << ") { ";
    if (return_type.kind != CXTypeKind::CXType_Void) {
        if (ctx.args_which_need_wrapping & 1)
            ctx.c_file << "*ret = ";
        else
            ctx.c_file << "return ";
    }

    // emit call to original function
    ctx.c_file << clang_getCursorSpelling(cursor) << "(";
    // emit arguments
    ctx.param_index = 1;
    clang_visitChildren(cursor, [](CXCursor c, CXCursor parent, CXClientData client_data) {
        if (c.kind != CXCursorKind::CXCursor_ParmDecl)
            return CXChildVisit_Continue;

        Context& ctx = *(Context*)client_data;
        if (ctx.param_index > 1)
            ctx.c_file << ", ";

        if (ctx.args_which_need_wrapping & ((uint64_t)1 << ctx.param_index))
            ctx.c_file << "*";

        // name
        auto param_name = clang_getCString(clang_getCursorSpelling(c));
        auto param_name_length = strlen(param_name);
        if (param_name_length == 0)
            ctx.c_file << get_anonymous_param_name(ctx.param_index - 1);
        else
            ctx.c_file << param_name;

        ctx.param_index++;
        return CXChildVisit_Continue;
    }, &ctx);

    ctx.c_file << ")";
    ctx.c_file << "; }\n";
}

void emit_function_decl(Context& ctx, CXCursor cursor) {
    ctx.requires_c_wrapper = false;
    ctx.args_which_need_wrapping = 0;
    auto function_type = clang_getCursorType(cursor);
    auto return_type = clang_getResultType(function_type);


    std::stringstream tmp;
    ctx.buffer = &tmp;
    ctx.param_index = 0;
    emit_type(ctx, return_type);

    std::stringstream buffer;
    ctx.buffer = &buffer;
    buffer << " :: (";

    // emit parameters
    if (ctx.args_which_need_wrapping & 1) {
        buffer << "ret: &";
        emit_type(ctx, return_type, true);
    }
    ctx.param_index = 1;
    clang_visitChildren(cursor, [](CXCursor c, CXCursor parent, CXClientData client_data) {
        if (c.kind != CXCursorKind::CXCursor_ParmDecl)
            return CXChildVisit_Continue;
        LOG_CURSOR(c);
        Context& ctx = *(Context*)client_data;

        auto param_name = clang_getCString(clang_getCursorSpelling(c));
        auto param_type = clang_getCursorType(c);

        auto param_name_length = strlen(param_name);

        if (ctx.param_index > 1 || ctx.args_which_need_wrapping & 1) {
            *ctx.buffer << ", ";
        }

        if (param_name_length == 0) {
            *ctx.buffer << get_anonymous_param_name(ctx.param_index - 1);
        } else {
            *ctx.buffer << "_" << param_name;
        }

        *ctx.buffer << ": ";

        emit_type(ctx, param_type);

        ctx.param_index++;
        return CXChildVisit_Continue;
    }, &ctx);

    buffer << ") -> ";


    if ((ctx.args_which_need_wrapping & 1) == 0) {
        ctx.param_index = 0;
        emit_type(ctx, return_type);
    } else {
        buffer << "void";
    }
    
    buffer << ";\n";

    if (ctx.requires_c_wrapper) {
        ctx.cheez_file << "_" << clang_getCursorSpelling(cursor) << buffer.str();
        emit_function_decl_c_wrapper(ctx, cursor);
    } else {
        ctx.cheez_file << clang_getCursorSpelling(cursor) << buffer.str();
    }
}
