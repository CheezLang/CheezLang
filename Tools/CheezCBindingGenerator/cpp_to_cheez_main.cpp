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

bool c_string_starts_with(const char* str, const char* pre) {
    return strncmp(pre, str, strlen(pre)) == 0;
}

std::ostream& operator<<(std::ostream& stream, const CXString& str)
{
    stream << clang_getCString(str);
    return stream;
}

enum class ArgumentIndices : int {
    InputFile = 1,
    OutputFile,
    COUNT
};

struct Context {
    std::stringstream cheez_file, c_file;
    std::stringstream cheez_type_decl;
    std::stringstream cheez_drop_impl;
    std::stringstream cheez_impl;
    std::stringstream cheez_c_bindings;
    std::ostream* buffer = nullptr;
    bool requires_c_wrapper = false;
    uint64_t args_which_need_wrapping = 0;
    uint64_t param_index = 0;
    uint64_t member_index = 0;
    bool start_with_comma = false;
    bool prefer_pointers = false;
    bool no_includes = true;

    void reset() {
        cheez_type_decl.str(std::string());
        cheez_drop_impl.str(std::string());
        cheez_impl.str(std::string());
    }

    void emit_function_decl(CXCursor cursor);
    void emit_struct_decl(CXCursor cursor);
    void emit_typedef_decl(CXCursor cursor);
    void emit_enum_decl(CXCursor cursor);
    void emit_function_decl_c_wrapper(CXCursor cursor);
    void emit_c_function_parameter_list(std::ostream& stream, CXCursor func, bool start_with_comma = false);
    void emit_c_function_argument_list(std::ostream& stream, CXCursor func, bool start_with_comma = false);
    void emit_cheez_function_parameter_list(std::ostream& stream, CXCursor func, bool start_with_comma = false, bool prefer_pointers = false);
    void emit_cheez_function_argument_list(std::ostream& stream, CXCursor func, bool start_with_comma = false);
    void emit_param_name(std::ostream& stream, CXCursor cursor, int index);
    std::string get_param_name(CXCursor cursor, int index);
    void emit_cheez_type(std::ostream& stream, const CXType& type, bool is_func_param, bool behind_pointer = false, bool prefer_pointers = false);
    void emit_c_type(std::ostream& stream, const CXType& type, const char* name, bool is_func_param, bool behind_pointer = false);
    bool pass_type_by_pointer(const CXType& type);
};

int main(int argc, char** argv)
{
    if (argc != (int) ArgumentIndices::COUNT) {
        std::cerr << "Wrong number of arguments\n";
        return 1;
    }

    auto header_file_path = argv[(int)ArgumentIndices::InputFile];
    auto output_file_name = argv[(int)ArgumentIndices::OutputFile];

    std::stringstream cheez_file_name;
    cheez_file_name << output_file_name << ".che";
    std::stringstream c_file_name;
    c_file_name << output_file_name << ".cpp";

    std::ofstream cheez_file{ cheez_file_name.str() };
    std::ofstream c_file{ c_file_name.str() };

    if (!cheez_file || !c_file) {
        std::cerr << "Failed to create output files '" << cheez_file_name.str() << "' and '" << c_file_name.str() << "'\n";
        return 2;
    }

    Context ctx{  };

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
        c_file << "#include <memory>\n";
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
        ctx.reset();

        //if (ctx.no_includes && c.kind != CXCursorKind::CXCursor_UnexposedDecl) {
        //    // skip functions from other files
        //    auto source_location = clang_getCursorLocation(c);
        //    if (clang_Location_isFromMainFile(source_location) == 0) {
        //        CXFile file;
        //        uint32_t line, column;
        //        clang_getFileLocation(source_location, &file, &line, &column, nullptr);
        //        std::cout << "Skipping (" << clang_getCursorKindSpelling(c.kind) << ")\t\t" << clang_getCursorSpelling(c) << "\t\tfrom " << clang_getFileName(file) << ":" << line << ":" << column << "\n";
        //        return CXChildVisitResult::CXChildVisit_Continue;
        //    }
        //}

        CXString name = clang_getCursorSpelling(c);

        if (strcmp("FILE", clang_getCString(name)) == 0) {
            int a = 0;
        }

        switch (c.kind) {
        case CXCursor_Namespace:
            return CXChildVisit_Recurse;

        case CXCursor_FunctionDecl:
            ctx.emit_function_decl(c);
            break;

        case CXCursor_ClassDecl:
        case CXCursor_StructDecl: {
            if (strlen(clang_getCString(name)) != 0)
                ctx.emit_struct_decl(c);
            break;
        }

        case CXCursor_TypedefDecl:
            ctx.emit_typedef_decl(c);
            break;

        case CXCursor_EnumDecl: {
            if (strlen(clang_getCString(name)) != 0)
                ctx.emit_enum_decl(c);
            break;
        }

        case CXCursor_Constructor:
        case CXCursor_Destructor:
        case CXCursor_CXXMethod:
            // do nothing, handled somewhere else
            break;

        case CXCursor_ClassTemplate:
        case CXCursor_VarDecl:
        case CXCursor_UnexposedDecl:
        case CXCursor_FunctionTemplate:
            // can't handle those
            break;

        default: {
            std::cout << "[ERROR] TODO: " << "Cursor '" << clang_getCursorSpelling(c) << "' of kind '" << clang_getCursorKindSpelling(clang_getCursorKind(c)) << "'\n";
            break;
        }
        }

        clang_disposeString(name);
        return CXChildVisit_Continue;
        //return CXChildVisit_Recurse;
    }, &ctx);

    clang_disposeTranslationUnit(unit);
    clang_disposeIndex(index);

    cheez_file << "#lib(\"./__TODO__.lib\")\n\n";
    cheez_file << "#export_scope\n\n";
    cheez_file << "__UNKNOWN__ :: struct #copy {}\n\n";
    cheez_file << ctx.cheez_file.str() << "\n";
    cheez_file << "// ==========================================================\n";
    cheez_file << "// ==========================================================\n";
    cheez_file << "// ==========================================================\n\n";
    cheez_file << "#file_scope\n\n";
    cheez_file << ctx.cheez_c_bindings.str();
    c_file << ctx.c_file.str();
}

void Context::emit_typedef_decl(CXCursor cursor) {
    auto elo = clang_getTypedefDeclUnderlyingType(cursor);
    auto actual_type = clang_Type_getNamedType(elo);
    auto type_decl = clang_getTypeDeclaration(actual_type);

    switch (type_decl.kind) {
    case CXCursor_StructDecl: {
        //auto struct_name = clang_getCursorSpelling(type_decl);
        //if (strlen(clang_getCString(struct_name)) == 0) {
        //    emit_struct_decl(type_decl, clang_getCursorSpelling(cursor));
        //}
        cheez_file << clang_getCursorSpelling(cursor) << " :: struct #copy {}\n";
        break;
    }

    case CXCursor_EnumDecl: {
        //auto struct_name = clang_getCursorSpelling(type_decl);
        //if (strlen(clang_getCString(struct_name)) == 0) {
        //    emit_enum_decl(type_decl, clang_getCursorSpelling(cursor));
        //}
        cheez_file << clang_getCursorSpelling(cursor) << " :: enum #copy {}\n";
        break;
    }

    default: {
        cheez_file << clang_getCursorSpelling(cursor) << " :: ";
        emit_cheez_type(cheez_file, elo, false);
        cheez_file << "\n";
        break;
    }
    }

    //ctx.cheez_file << name << " :: struct {}\n";
}

void Context::emit_struct_decl(CXCursor cursor) {
    auto name = clang_getCursorSpelling(cursor);

    // check if is forward decl
    member_index = 0;
    clang_visitChildren(cursor, [](CXCursor c, CXCursor parent, CXClientData client_data) {
        Context& ctx = *(Context*)client_data;
        ctx.member_index += 1;
        return CXChildVisit_Continue;
    }, this);

    if (member_index == 0) {
        // this is either an empy struct or a forward decl
        return;
    }

    // type declaration
    cheez_file << name << " :: struct #copy {\n";
    member_index = 0;
    clang_visitChildren(cursor, [](CXCursor c, CXCursor parent, CXClientData client_data) {
        Context& ctx = *(Context*)client_data;

        auto struct_name = clang_getCursorSpelling(parent);
        auto name = clang_getCursorSpelling(c);
        auto type = clang_getCursorType(c);

        switch (c.kind) {
        case CXCursorKind::CXCursor_FieldDecl: {
            if (strcmp("LogFile", clang_getCString(name)) == 0) {
                int a = 0;
            }

            ctx.cheez_type_decl << "    " << name << " : ";
            ctx.emit_cheez_type(ctx.cheez_type_decl, type, false);
            ctx.cheez_type_decl << " = default";
            ctx.cheez_type_decl << "\n";
            break;
        }

        case CXCursorKind::CXCursor_CXXMethod: {
            if (c_string_starts_with(clang_getCString(name), "operator"))
                break;

            CXType return_type = clang_getResultType(type);

            // cheez
            {
                ctx.cheez_impl << "    " << name << " :: (ref Self";
                ctx.emit_cheez_function_parameter_list(ctx.cheez_impl, c, true);
                ctx.cheez_impl << ") ";
                if (return_type.kind != CXTypeKind::CXType_Void) {
                    ctx.cheez_impl << "-> ";
                    ctx.emit_cheez_type(ctx.cheez_impl, return_type, true);
                    ctx.cheez_impl << " ";
                }
                ctx.cheez_impl << "{\n";

                if (return_type.kind != CXTypeKind::CXType_Void) {
                    ctx.cheez_impl << "        result : ";
                    ctx.emit_cheez_type(ctx.cheez_impl, return_type, false, true, true);
                    ctx.cheez_impl << " = default\n";
                }
                ctx.cheez_impl << "        __c__" << struct_name << "_" << name << "_" << ctx.member_index << "(&self";
                if (return_type.kind != CXTypeKind::CXType_Void) {
                    ctx.cheez_impl << ", &result";
                }
                ctx.emit_cheez_function_argument_list(ctx.cheez_impl, c, true);
                ctx.cheez_impl << ")\n";
                if (return_type.kind != CXTypeKind::CXType_Void) {
                    ctx.cheez_impl << "        return ";
                    if (return_type.kind == CXTypeKind::CXType_LValueReference)
                        ctx.cheez_impl << "<<";
                    ctx.cheez_impl << "result\n";
                }
                ctx.cheez_impl << "    }\n";
            }

            // cheez binding
            {
                ctx.cheez_c_bindings << "__c__" << struct_name << "_" << name << "_" << ctx.member_index << " :: (self: &" << struct_name;

                if (return_type.kind != CXTypeKind::CXType_Void) {
                    ctx.cheez_c_bindings << ", ret: &";
                    ctx.emit_cheez_type(ctx.cheez_c_bindings, return_type, false, true, true);
                }

                ctx.emit_cheez_function_parameter_list(ctx.cheez_c_bindings, c, true, true);
                ctx.cheez_c_bindings << ");\n";
            }

            // c
            {
                ctx.c_file << "extern \"C\" void __c__" << struct_name << "_" << name << "_" << ctx.member_index << "(" << struct_name << "* self";
                if (return_type.kind != CXTypeKind::CXType_Void) {
                    ctx.c_file << ", ";
                    ctx.emit_c_type(ctx.c_file, return_type, "*ret", true, true);
                }
                ctx.emit_c_function_parameter_list(ctx.c_file, c, true);
                ctx.c_file << ") {\n";
                ctx.c_file << "    ";
                if (return_type.kind != CXTypeKind::CXType_Void) {
                    ctx.c_file << "*ret = (";
                    ctx.emit_c_type(ctx.c_file, return_type, "", false, false);
                    ctx.c_file << ")";
                    if (return_type.kind == CXTypeKind::CXType_LValueReference)
                        ctx.c_file << "&";
                }
                ctx.c_file << "self->" << name << "(";
                ctx.emit_c_function_argument_list(ctx.c_file, c, false);
                ctx.c_file << ");\n";
                ctx.c_file << "}\n";
            }
            break;
        }
        
        case CXCursorKind::CXCursor_Constructor: {
            CXType return_type = clang_getResultType(type);

            // cheez
            {
                ctx.cheez_impl << "    new :: (";
                ctx.emit_cheez_function_parameter_list(ctx.cheez_impl, c, false);
                ctx.cheez_impl << ") -> " << struct_name     << " {\n";
                ctx.cheez_impl << "        result : " << struct_name << " = default\n";
                ctx.cheez_impl << "        __c__" << struct_name << "_new_" << ctx.member_index << "(&result";
                ctx.emit_cheez_function_argument_list(ctx.cheez_impl, c, true);
                ctx.cheez_impl << ")\n";
                ctx.cheez_impl << "        return result\n";
                ctx.cheez_impl << "    }\n";
            }

            // cheez binding
            {
                ctx.cheez_c_bindings << "__c__" << struct_name << "_new_" << ctx.member_index << " :: (self: &" << struct_name;
                ctx.emit_cheez_function_parameter_list(ctx.cheez_c_bindings, c, true, true);
                ctx.cheez_c_bindings << ");\n";
            }

            // c
            {
                ctx.c_file << "extern \"C\" void __c__" << struct_name << "_new_" << ctx.member_index << "(" << struct_name << "* self";
                ctx.emit_c_function_parameter_list(ctx.c_file, c, true);
                ctx.c_file << ") {\n";
                ctx.c_file << "    new (self) " << struct_name << "(";
                ctx.emit_c_function_argument_list(ctx.c_file, c, false);
                ctx.c_file << ");\n";
                ctx.c_file << "}\n";
            }
            break;
        }
        
        case CXCursorKind::CXCursor_Destructor: {
            // cheez
            {
                ctx.cheez_drop_impl << "    drop :: (ref Self) {\n";
                ctx.cheez_drop_impl << "        __c__" << struct_name << "_dtor(&self)\n";
                ctx.cheez_drop_impl << "    }\n";
            }

            // cheez binding
            {
                ctx.cheez_c_bindings << "__c__" << struct_name << "_dtor :: (self: &" << struct_name << ");\n";
            }

            // c
            {
                ctx.c_file << "extern \"C\" void __c__" << struct_name << "_dtor(" << struct_name << "* self) {\n";
                ctx.c_file << "    self->~" << struct_name << "();\n";
                ctx.c_file << "}\n";
            }
            break;
        }

        case CXCursorKind::CXCursor_EnumDecl:
        case CXCursorKind::CXCursor_UnionDecl:
        case CXCursorKind::CXCursor_ClassDecl:
        case CXCursorKind::CXCursor_StructDecl:
            // can't handle nested types yet, have to refactore the code
            // @TODO: return CXChildVisit_Recurse;
            break;

        case CXCursorKind::CXCursor_CXXAccessSpecifier:
            // ignore for now, don't even have private members in cheez yet
            break;

        case CXCursorKind::CXCursor_VarDecl:
        case CXCursorKind::CXCursor_TypedefDecl:
        case CXCursorKind::CXCursor_ConversionFunction:
            // ignore this stuff
            break;

        default: {
            std::cout << "[ERROR] struct member: " << "Cursor '" << clang_getCursorSpelling(c) << "' of kind '" << clang_getCursorKindSpelling(clang_getCursorKind(c)) << "'\n";
            break;
        }
        }

        clang_disposeString(name);
        clang_disposeString(struct_name);

        ctx.member_index += 1;
        return CXChildVisit_Continue;
    }, this);
    cheez_file << cheez_type_decl.str();
    cheez_file << "}\n";

    // function implementations
    auto impl = cheez_impl.str();
    if (impl.size() > 0) {
        cheez_file << "impl " << name << " {\n";
        cheez_file << impl;
        cheez_file << "}\n";
    }

    auto drop_impl = cheez_drop_impl.str();
    if (drop_impl.size() > 0) {
        cheez_file << "impl Drop for " << name << " {\n";
        cheez_file << drop_impl;
        cheez_file << "}\n";
    }
    clang_disposeString(name);
}

void Context::emit_enum_decl(CXCursor cursor) {
    auto name = clang_getCString(clang_getCursorSpelling(cursor));
    if (name == nullptr)
        return;

    cheez_file << name << " :: enum #copy {}\n";
}

void Context::emit_function_decl_c_wrapper(CXCursor cursor) {
    //auto function_type = clang_getCursorType(cursor);
    //auto return_type = clang_getResultType(function_type);

    //if (args_which_need_wrapping & 1) {
    //    c_file << "void";
    //} else {
    //    c_file << clang_getTypeSpelling(return_type);
    //}
    //c_file << " _" << clang_getCursorSpelling(cursor) << "(";

    //// emit parameters
    //if (args_which_need_wrapping & 1)
    //    c_file << clang_getTypeSpelling(return_type) << "* ret";
    //param_index = 1;
    //clang_visitChildren(cursor, [](CXCursor c, CXCursor parent, CXClientData client_data) {
    //    if (c.kind != CXCursorKind::CXCursor_ParmDecl)
    //        return CXChildVisit_Continue;

    //    Context& ctx = *(Context*)client_data;
    //    if (ctx.param_index > 1 || (ctx.args_which_need_wrapping & 1))
    //        ctx.c_file << ", ";

    //    // type
    //    if (ctx.args_which_need_wrapping & ((uint64_t)1 << ctx.param_index)) {
    //        ctx.c_file << clang_getTypeSpelling(clang_getCursorType(c)) << "* ";
    //    } else {
    //        ctx.c_file << clang_getTypeSpelling(clang_getCursorType(c)) << " ";
    //    }

    //    // name
    //    auto param_name_length = strlen(clang_getCString(clang_getCursorSpelling(c)));
    //    if (param_name_length == 0)
    //        ctx.c_file << ctx.get_anonymous_param_name(ctx.param_index - 1);
    //    else if (ctx.args_which_need_wrapping & 1 && ctx.param_index == 1)
    //        ctx.c_file << "ret";
    //    else
    //        ctx.c_file << clang_getCursorSpelling(c);

    //    ctx.param_index++;
    //    return CXChildVisit_Continue;
    //}, this);

    //c_file << ") { ";
    //if (return_type.kind != CXTypeKind::CXType_Void) {
    //    if (args_which_need_wrapping & 1)
    //        c_file << "*ret = ";
    //    else
    //        c_file << "return ";
    //}

    //// emit call to original function
    //c_file << clang_getCursorSpelling(cursor) << "(";
    //// emit arguments
    //param_index = 1;
    //clang_visitChildren(cursor, [](CXCursor c, CXCursor parent, CXClientData client_data) {
    //    if (c.kind != CXCursorKind::CXCursor_ParmDecl)
    //        return CXChildVisit_Continue;

    //    Context& ctx = *(Context*)client_data;
    //    if (ctx.param_index > 1)
    //        ctx.c_file << ", ";

    //    if (ctx.args_which_need_wrapping & ((uint64_t)1 << ctx.param_index))
    //        ctx.c_file << "*";

    //    // name
    //    auto param_name = clang_getCString(clang_getCursorSpelling(c));
    //    auto param_name_length = strlen(param_name);
    //    if (param_name_length == 0)
    //        ctx.c_file << ctx.get_anonymous_param_name(ctx.param_index - 1);
    //    else
    //        ctx.c_file << param_name;

    //    ctx.param_index++;
    //    return CXChildVisit_Continue;
    //}, this);

    //c_file << ")";
    //c_file << "; }\n";
}

void Context::emit_function_decl(CXCursor cursor) {
    //ctx.requires_c_wrapper = false;
    //ctx.args_which_need_wrapping = 0;
    //auto function_type = clang_getCursorType(cursor);
    //auto return_type = clang_getResultType(function_type);


    //std::stringstream tmp;
    //ctx.buffer = &tmp;
    //ctx.param_index = 0;
    //emit_type(ctx, return_type);

    //std::stringstream buffer;
    //ctx.buffer = &buffer;
    //buffer << " :: (";

    //// emit parameters
    //if (ctx.args_which_need_wrapping & 1) {
    //    buffer << "ret: &";
    //    emit_type(ctx, return_type, true);
    //}
    //ctx.param_index = 1;
    //clang_visitChildren(cursor, [](CXCursor c, CXCursor parent, CXClientData client_data) {
    //    if (c.kind != CXCursorKind::CXCursor_ParmDecl)
    //        return CXChildVisit_Continue;
    //    LOG_CURSOR(c);
    //    Context& ctx = *(Context*)client_data;

    //    auto param_name = clang_getCString(clang_getCursorSpelling(c));
    //    auto param_type = clang_getCursorType(c);

    //    auto param_name_length = strlen(param_name);

    //    if (ctx.param_index > 1 || ctx.args_which_need_wrapping & 1) {
    //        *ctx.buffer << ", ";
    //    }

    //    if (param_name_length == 0) {
    //        *ctx.buffer << get_anonymous_param_name(ctx.param_index - 1);
    //    } else {
    //        *ctx.buffer << "_" << param_name;
    //    }

    //    *ctx.buffer << ": ";

    //    emit_type(ctx, param_type);

    //    ctx.param_index++;
    //    return CXChildVisit_Continue;
    //}, &ctx);

    //buffer << ") -> ";


    //if ((ctx.args_which_need_wrapping & 1) == 0) {
    //    ctx.param_index = 0;
    //    emit_type(ctx, return_type);
    //} else {
    //    buffer << "void";
    //}
    //
    //buffer << ";\n";

    //if (ctx.requires_c_wrapper) {
    //    ctx.cheez_file << "_" << clang_getCursorSpelling(cursor) << buffer.str();
    //    emit_function_decl_c_wrapper(ctx, cursor);
    //} else {
    //    ctx.cheez_file << clang_getCursorSpelling(cursor) << buffer.str();
    //}
}

void Context::emit_cheez_function_parameter_list(std::ostream& stream, CXCursor func, bool start_with_comma, bool prefer_pointers) {
    buffer = &stream;
    param_index = 0;
    this->start_with_comma = start_with_comma;
    this->prefer_pointers = prefer_pointers;

    clang_visitChildren(func, [](CXCursor c, CXCursor parent, CXClientData client_data) {
        Context& ctx = *(Context*)client_data;

        switch (c.kind) {
        case CXCursorKind::CXCursor_ParmDecl: {
            CXString name = clang_getCursorSpelling(c);
            CXType type = clang_getCursorType(c);

            if (ctx.param_index > 0 || ctx.start_with_comma)
                *ctx.buffer << ", ";
            ctx.emit_param_name(*ctx.buffer, c, ctx.param_index);
            *ctx.buffer << ": ";
            ctx.emit_cheez_type(*ctx.buffer, type, true, false, ctx.prefer_pointers);

            clang_disposeString(name);
            ctx.param_index += 1;
            break;
        }
        }

        return CXChildVisit_Continue;
    }, this);
}

void Context::emit_cheez_function_argument_list(std::ostream& stream, CXCursor func, bool start_with_comma) {
    buffer = &stream;
    param_index = 0;
    this->start_with_comma = start_with_comma;

    clang_visitChildren(func, [](CXCursor c, CXCursor parent, CXClientData client_data) {
        Context& ctx = *(Context*)client_data;

        switch (c.kind) {
        case CXCursorKind::CXCursor_ParmDecl: {
            if (ctx.param_index > 0 || ctx.start_with_comma)
                *ctx.buffer << ", ";

            CXType type = clang_getCursorType(c);
            if (ctx.pass_type_by_pointer(type) || type.kind == CXType_LValueReference)
                *ctx.buffer << "&";
            ctx.emit_param_name(*ctx.buffer, c, ctx.param_index);
            ctx.param_index += 1;
            break;
        }
        }

        return CXChildVisit_Continue;
    }, this);
}

void Context::emit_c_function_parameter_list(std::ostream& stream, CXCursor func, bool start_with_comma) {
    buffer = &stream;
    param_index = 0;
    this->start_with_comma = start_with_comma;

    clang_visitChildren(func, [](CXCursor c, CXCursor parent, CXClientData client_data) {
        Context& ctx = *(Context*)client_data;

        switch (c.kind) {
        case CXCursorKind::CXCursor_ParmDecl: {
            if (ctx.param_index > 0 || ctx.start_with_comma)
                *ctx.buffer << ", ";
            ctx.emit_c_type(*ctx.buffer, clang_getCursorType(c), ctx.get_param_name(c, ctx.param_index).c_str(), true);
            ctx.param_index += 1;
            break;
        }
        }

        return CXChildVisit_Continue;
    }, this);
}

void Context::emit_c_function_argument_list(std::ostream& stream, CXCursor func, bool start_with_comma) {
    buffer = &stream;
    param_index = 0;
    this->start_with_comma = start_with_comma;

    clang_visitChildren(func, [](CXCursor c, CXCursor parent, CXClientData client_data) {
        Context& ctx = *(Context*)client_data;

        switch (c.kind) {
        case CXCursorKind::CXCursor_ParmDecl: {
            CXString name = clang_getCursorSpelling(c);
            CXType type = clang_getCursorType(c);

            if (ctx.param_index > 0 || ctx.start_with_comma)
                *ctx.buffer << ", ";

            if (ctx.pass_type_by_pointer(type) || type.kind == CXType_LValueReference)
                *ctx.buffer << "*";
            ctx.emit_param_name(*ctx.buffer, c, ctx.param_index);

            clang_disposeString(name);
            ctx.param_index += 1;
            break;
        }
        }

        return CXChildVisit_Continue;
    }, this);
}

void Context::emit_cheez_type(std::ostream& stream, const CXType& type, bool is_func_param, bool behind_pointer, bool prefer_pointers) {
    long long size = clang_Type_getSizeOf(type);

    switch (type.kind) {
    case CXTypeKind::CXType_Void:       stream << "void"; break;

    case CXTypeKind::CXType_UChar:      stream << "u" << (size * 8); break;
    case CXTypeKind::CXType_UShort:     stream << "u" << (size * 8); break;
    case CXTypeKind::CXType_UInt:       stream << "u" << (size * 8); break;
    case CXTypeKind::CXType_ULong:      stream << "u" << (size * 8); break;
    case CXTypeKind::CXType_ULongLong:  stream << "u" << (size * 8); break;

    case CXTypeKind::CXType_SChar:      stream << "i" << (size * 8); break;
    case CXTypeKind::CXType_Short:      stream << "i" << (size * 8); break;
    case CXTypeKind::CXType_Int:        stream << "i" << (size * 8); break;
    case CXTypeKind::CXType_Long:       stream << "i" << (size * 8); break;
    case CXTypeKind::CXType_LongLong:   stream << "i" << (size * 8); break;

    case CXTypeKind::CXType_Char_S:     stream << "char" << (size * 8); break;
    case CXTypeKind::CXType_WChar:      stream << "char" << (size * 8); break;

    case CXTypeKind::CXType_Float:      stream << "f32"; break;
    case CXTypeKind::CXType_Double:     stream << "f64"; break;

    case CXTypeKind::CXType_Bool:       stream << "bool"; break;

    case CXTypeKind::CXType_ConstantArray: {
        auto target_type = clang_getArrayElementType(type);
        auto array_size = clang_getArraySize(type);

        if (is_func_param) {
            stream << "&";
        }
        else {
            stream << "[" << array_size << "]";
        }
        emit_cheez_type(stream, target_type, is_func_param, true, prefer_pointers);
        break;
    }

    case CXTypeKind::CXType_IncompleteArray: {
        auto target_type = clang_getArrayElementType(type);

        if (is_func_param) {
            stream << "&";
        }
        else {
            stream << "[]";
        }
        emit_cheez_type(stream, target_type, is_func_param, true, prefer_pointers);
        break;
    }

    case CXTypeKind::CXType_Pointer: {
        auto target_type = clang_getPointeeType(type);
        stream << "&";
        emit_cheez_type(stream, target_type, is_func_param, true, false);
        break;
    }

    case CXTypeKind::CXType_LValueReference: {
        auto target_type = clang_getPointeeType(type);
        if (prefer_pointers)
            stream << "&";
        else
            stream << "ref ";
        emit_cheez_type(stream, target_type, is_func_param, true, false);
        break;
    }

    // function type
    case CXTypeKind::CXType_FunctionProto: {
        stream << "fn(";

        for (int arg_index = 0; true; arg_index++) {
            CXType arg_type = clang_getArgType(type, arg_index);
            if (arg_type.kind == CXTypeKind::CXType_Invalid)
                break;

            if (arg_index > 0) {
                stream << ", ";
            }
            emit_cheez_type(stream, arg_type, true, false, prefer_pointers);
        }

        stream << ") -> ";
        CXType return_type = clang_getResultType(type);
        emit_cheez_type(stream, return_type, false, false, prefer_pointers);
        break;
    }

    case CXTypeKind::CXType_Elaborated: {
        CXType actual_type = clang_Type_getNamedType(type);
        CXCursor type_decl = clang_getTypeDeclaration(actual_type);
        stream << clang_getCursorSpelling(type_decl);
        break;
    }

    // struct
    case CXTypeKind::CXType_Record: {
        CXCursor type_decl = clang_getTypeDeclaration(type);
        if (prefer_pointers && !behind_pointer) {
            stream << "&" << clang_getCursorSpelling(type_decl);
        }
        else {
            stream << clang_getCursorSpelling(type_decl);
        }
        break;
    }

    // enum
    case CXTypeKind::CXType_Enum: {
        stream << clang_getTypeSpelling(type);
        break;
    }

    case CXTypeKind::CXType_Typedef: {

        if (behind_pointer) {
            auto typedef_decl = clang_getTypeDeclaration(type);
            //auto elo = clang_getTypedefDeclUnderlyingType(typedef_decl);
            //auto actual_type = clang_Type_getNamedType(elo);
            //auto type_decl = clang_getTypeDeclaration(actual_type);

            //std::stringstream ss;
            //ss << clang_getTypeSpelling(type) << "\n";
            //ss << clang_getCursorSpelling(typedef_decl) << "\n";
            //ss << clang_getTypeSpelling(elo) << "\n";
            //ss << clang_getTypeSpelling(actual_type) << "\n";
            //ss << clang_getCursorSpelling(type_decl) << "\n";
            //auto str = ss.str();
            //stream << clang_getTypeSpelling(type);
            stream << clang_getCursorSpelling(typedef_decl);
        }
        else {
            auto typedef_decl = clang_getTypeDeclaration(type);
            auto elo = clang_getTypedefDeclUnderlyingType(typedef_decl);
            auto actual_type = clang_Type_getNamedType(elo);
            auto type_decl = clang_getTypeDeclaration(actual_type);

            switch (type_decl.kind) {
            case CXCursorKind::CXCursor_StructDecl:
            case CXCursorKind::CXCursor_UnionDecl:
            case CXCursorKind::CXCursor_ClassDecl:
                if (prefer_pointers && !behind_pointer) {
                    stream << "&" << clang_getTypeSpelling(type);
                }
                else {
                    stream << clang_getTypeSpelling(type);
                }
                break;

            default:
                stream << clang_getCursorSpelling(typedef_decl);
                //emit_cheez_type(stream, elo, is_func_param, behind_pointer, prefer_pointers);
                break;
            }
        }
        break;
    }

    default: {
        auto spelling = clang_getTypeSpelling(type);
        if (prefer_pointers && !behind_pointer)
            stream << "&";
        stream << "__UNKNOWN__";
        std::cout << "[ERROR] Failed to translate type '" << spelling << "' (" << clang_getTypeKindSpelling(type.kind) << ")\n";
        break;
    }
    }
}

void Context::emit_c_type(std::ostream& stream, const CXType& type, const char* name, bool is_func_param, bool behind_pointer) {
    long long size = clang_Type_getSizeOf(type);

    switch (type.kind) {
    case CXTypeKind::CXType_Void:       stream << "void " << name; break;

    case CXTypeKind::CXType_UChar:      stream << "uint" << (size * 8) << "_t " << name; break;
    case CXTypeKind::CXType_UShort:     stream << "uint" << (size * 8) << "_t " << name; break;
    case CXTypeKind::CXType_UInt:       stream << "uint" << (size * 8) << "_t " << name; break;
    case CXTypeKind::CXType_ULong:      stream << "uint" << (size * 8) << "_t " << name; break;
    case CXTypeKind::CXType_ULongLong:  stream << "uint" << (size * 8) << "_t " << name; break;

    case CXTypeKind::CXType_SChar:      stream << "int" << (size * 8) << "_t " << name; break;
    case CXTypeKind::CXType_Short:      stream << "int" << (size * 8) << "_t " << name; break;
    case CXTypeKind::CXType_Int:        stream << "int" << (size * 8) << "_t " << name; break;
    case CXTypeKind::CXType_Long:       stream << "int" << (size * 8) << "_t " << name; break;
    case CXTypeKind::CXType_LongLong:   stream << "int" << (size * 8) << "_t " << name; break;

    //case CXTypeKind::CXType_Char_S:     stream << "char" << (size * 8) << "_t " << name; break;
    //case CXTypeKind::CXType_WChar:      stream << "char" << (size * 8) << "_t " << name; break;
    case CXTypeKind::CXType_Char_S:     stream << "char " << name; break;
    case CXTypeKind::CXType_WChar:      stream << "wchar_t " << name; break;

    case CXTypeKind::CXType_Float:      stream << "float " << name; break;
    case CXTypeKind::CXType_Double:     stream << "double " << name; break;

    case CXTypeKind::CXType_Bool:       stream << "bool " << name; break;

    case CXTypeKind::CXType_ConstantArray: {
        auto target_type = clang_getArrayElementType(type);
        auto array_size = clang_getArraySize(type);

        emit_c_type(stream, target_type, "", is_func_param, true);
        if (is_func_param) {
            stream << "* " << name;
        }
        else {
            stream << name << "[" << array_size << "]";
        }
        break;
    }

    case CXTypeKind::CXType_IncompleteArray: {
        auto target_type = clang_getArrayElementType(type);

        emit_c_type(stream, target_type, "", is_func_param, true);
        if (is_func_param) {
            stream << "* " << name;
        }
        else {
            stream << "" << name << "[]";
        }
        break;
    }

    case CXTypeKind::CXType_Pointer: {
        auto target_type = clang_getPointeeType(type);
        emit_c_type(stream, target_type, "", is_func_param, true);
        stream << "* " << name;
        break;
    }

    case CXTypeKind::CXType_LValueReference: {
        auto target_type = clang_getPointeeType(type);
        emit_c_type(stream, target_type, "", is_func_param, true);
        stream << "* " << name;
        break;
    }

    // function type
    case CXTypeKind::CXType_FunctionProto: {
        CXType return_type = clang_getResultType(type);
        emit_c_type(stream, return_type, "", false);
        stream << "(*" << name << ")(";

        for (int arg_index = 0; true; arg_index++) {
            CXType arg_type = clang_getArgType(type, arg_index);
            if (arg_type.kind == CXTypeKind::CXType_Invalid)
                break;

            if (arg_index > 0) {
                stream << ", ";
            }
            emit_c_type(stream, arg_type, "", true);
        }

        stream << ")";
        break;
    }

    case CXTypeKind::CXType_Elaborated: {
        CXType actual_type = clang_Type_getNamedType(type);
        CXCursor type_decl = clang_getTypeDeclaration(actual_type);
        stream << clang_getCursorSpelling(type_decl);
        break;
    }

    // struct
    case CXTypeKind::CXType_Record: {
        CXCursor type_decl = clang_getTypeDeclaration(type);
        if (is_func_param && !behind_pointer) {
            stream << clang_getCursorSpelling(type_decl) << "* " << name;
        } else {
            stream << clang_getCursorSpelling(type_decl) << " " << name;
        }
        break;
    }

    // enum
    case CXTypeKind::CXType_Enum: {
        stream << clang_getTypeSpelling(type) << " " << name;
        break;
    }

    case CXTypeKind::CXType_Typedef: {

        if (behind_pointer) {
            stream << clang_getTypeSpelling(type) << " " << name;
        }
        else {
            auto typedef_decl = clang_getTypeDeclaration(type);
            auto elo = clang_getTypedefDeclUnderlyingType(typedef_decl);
            auto actual_type = clang_Type_getNamedType(elo);
            auto type_decl = clang_getTypeDeclaration(actual_type);

            switch (type_decl.kind) {
            case CXCursorKind::CXCursor_StructDecl:
            case CXCursorKind::CXCursor_UnionDecl:
            case CXCursorKind::CXCursor_ClassDecl:
                if (is_func_param && !behind_pointer) {
                    stream << clang_getTypeSpelling(type) << "* " << name;
                }
                else {
                    stream << clang_getTypeSpelling(type) << " " << name;
                }
                break;

            default:
                if (elo.kind == CXTypeKind::CXType_Pointer && clang_getPointeeType(elo).kind == CXTypeKind::CXType_FunctionProto) {
                    stream << clang_getTypeSpelling(type) << " " << name;
                } else {
                    emit_c_type(stream, elo, name, is_func_param, behind_pointer);
                }
                break;
            }
        }
        break;
    }

    default: {
        auto spelling = clang_getTypeSpelling(type);
        stream << "__UNKNOWN__";
        if (!behind_pointer)
            stream << "*";
        stream << " " << name;
        std::cout << "[ERROR] Failed to translate type '" << spelling << "' (" << clang_getTypeKindSpelling(type.kind) << ")\n";
        break;
    }
    }
}

bool Context::pass_type_by_pointer(const CXType& type) {
    switch (type.kind) {
    case CXTypeKind::CXType_Void:
    case CXTypeKind::CXType_UChar:
    case CXTypeKind::CXType_UShort:
    case CXTypeKind::CXType_UInt:
    case CXTypeKind::CXType_ULong:
    case CXTypeKind::CXType_ULongLong:
    case CXTypeKind::CXType_SChar:
    case CXTypeKind::CXType_Short:
    case CXTypeKind::CXType_Int:
    case CXTypeKind::CXType_Long:
    case CXTypeKind::CXType_LongLong:
    case CXTypeKind::CXType_Char_S:
    case CXTypeKind::CXType_WChar:
    case CXTypeKind::CXType_Float:
    case CXTypeKind::CXType_Double:
    case CXTypeKind::CXType_Bool:
    case CXTypeKind::CXType_ConstantArray:
    case CXTypeKind::CXType_IncompleteArray:
    case CXTypeKind::CXType_Pointer:
    case CXTypeKind::CXType_LValueReference:
    case CXTypeKind::CXType_FunctionProto:
    case CXTypeKind::CXType_Enum:
        return false;

    case CXTypeKind::CXType_Elaborated: {
        CXType actual_type = clang_Type_getNamedType(type);
        CXCursor type_decl = clang_getTypeDeclaration(actual_type);
        switch (type_decl.kind) {
            case CXCursorKind::CXCursor_StructDecl: return true;
            default: return false;
        }
    }

    case CXTypeKind::CXType_Record: return true;

    case CXTypeKind::CXType_Typedef: {
        auto typedef_decl = clang_getTypeDeclaration(type);
        auto elo = clang_getTypedefDeclUnderlyingType(typedef_decl);
        auto actual_type = clang_Type_getNamedType(elo);
        auto type_decl = clang_getTypeDeclaration(actual_type);

        switch (type_decl.kind) {
        case CXCursorKind::CXCursor_StructDecl:
        case CXCursorKind::CXCursor_ClassDecl:
            return true;

        default:
            return false;
            break;
        }
    }

    default: {
        auto spelling = clang_getTypeSpelling(type);
        std::cout << "[ERROR] unhandled pass_type_by_pointer '" << spelling << "' (" << clang_getTypeKindSpelling(type.kind) << ")\n";
        clang_disposeString(spelling);
        return true;
    }
    }
}

void Context::emit_param_name(std::ostream& stream, CXCursor cursor, int index) {
    CXString name = clang_getCursorSpelling(cursor);
    if (strlen(clang_getCString(name)) == 0) {
        stream << "_" << index;
    } else {
        stream << "_" << name;
    }
}

std::string Context::get_param_name(CXCursor cursor, int index) {
    std::stringstream stream;
    emit_param_name(stream, cursor, index);
    return stream.str();
}