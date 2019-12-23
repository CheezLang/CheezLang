#include <iostream>
#include <fstream>
#include <sstream>
#include <string>
#include <unordered_map>
#include <clang-c/Index.h>

#if 0
#define LOG_CURSOR(c) std::cout << "Cursor '" << clang_getCursorSpelling(c) << "' of kind '" << clang_getCursorKindSpelling(clang_getCursorKind(c)) << "'\n";
#else
#define LOG_CURSOR(c)
#endif

bool c_string_starts_with(const char* str, const char* pre) {
    return strncmp(pre, str, strlen(pre)) == 0;
}

template <typename T>
struct function_traits
    : public function_traits<decltype(&T::operator())>
{};

template <typename ClassType, typename ReturnType, typename... Args>
struct function_traits<ReturnType(ClassType::*)(Args...) const>
{
    template <size_t i>
    using arg_t = std::tuple_element_t<i, std::tuple<Args...>>;
};

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

struct Declaration {
    CXCursor declaration;
    size_t namespac = 0;
};

struct Context {
    std::stringstream cheez_file, c_file;
    std::stringstream cheez_type_decl;
    std::stringstream cheez_drop_impl;
    std::stringstream cheez_impl;
    std::stringstream cheez_c_bindings;
    std::ostream* buffer = nullptr;
    uint64_t param_index = 0;
    uint64_t member_index = 0;
    bool start_with_comma = false;
    bool prefer_pointers = false;
    bool no_includes = true;

    std::vector<Declaration> structs;
    std::vector<Declaration> enums;
    std::vector<Declaration> functions;
    std::vector<Declaration> typedefs;
    std::vector<Declaration> namespaces;
    std::unordered_map<std::string, int> duplicateFunctionNames;

    void reset() {
        cheez_type_decl.str(std::string());
        cheez_drop_impl.str(std::string());
        cheez_impl.str(std::string());
    }

    void emit_function_decl(const Declaration& decl);
    void emit_struct_decl(const Declaration& cursor);
    void emit_typedef_decl(CXCursor cursor);
    void emit_enum_decl(CXCursor cursor);
    void emit_c_function_parameter_list(std::ostream& stream, CXCursor func, bool start_with_comma = false);
    void emit_c_function_argument_list(std::ostream& stream, CXCursor func, bool start_with_comma = false);
    void emit_cheez_function_parameter_list(std::ostream& stream, CXCursor func, bool start_with_comma = false, bool prefer_pointers = false);
    void emit_cheez_function_argument_list(std::ostream& stream, CXCursor func, bool start_with_comma = false);
    void emit_param_name(std::ostream& stream, CXCursor cursor, int index);
    std::string get_param_name(CXCursor cursor, int index);
    void emit_cheez_type(std::ostream& stream, const CXType& type, bool is_func_param, bool behind_pointer = false, bool prefer_pointers = false);
    void emit_c_type(std::ostream& stream, const CXType& type, const char* name, bool is_func_param, bool behind_pointer = false);
    bool pass_type_by_pointer(const CXType& type);
    void emit_namespace(std::ostream& stream, size_t ns);

    void sort_stuff_into_lists(CXCursor tu, size_t namespac);

    std::string get_unique_function_name(CXString cxstr) {
        std::string str(clang_getCString(cxstr));

        auto found = duplicateFunctionNames.find(str);

        if (found == duplicateFunctionNames.end()) {
            duplicateFunctionNames.insert_or_assign(str, 1);
            return str;
        } else {
            int count = std::get<1>(*found) + 1;
            duplicateFunctionNames.insert_or_assign(str, count);

            std::stringstream ss;
            ss << str << "_" << count;
            return ss.str();
        }
    }
};

template <typename F>
struct VSCallback {
    F function;
    CXClientData data;
};

template <typename F>
auto getFreeCallbackData(F f, CXClientData ctx)
{
    return VSCallback<F>{f, ctx};
}

template <typename F>
auto getFreeCallback(F f)
{
    return [](CXCursor c, CXCursor parent, CXClientData client_data) {
        VSCallback<F>* cb = static_cast<VSCallback<F>*>(client_data);
        return (*cb->function)(c, parent, cb->data);
    };
}

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

    Context ctx{  };
    ctx.namespaces.push_back({ clang_getNullCursor(), 0 });
    ctx.sort_stuff_into_lists(clang_getTranslationUnitCursor(unit), 0);

    for (auto td : ctx.typedefs) {
        ctx.reset();
        ctx.emit_typedef_decl(td.declaration);
    }

    for (auto td : ctx.enums) {
        ctx.reset();
        ctx.emit_enum_decl(td.declaration);
    }

    for (auto td : ctx.structs) {
        ctx.reset();
        ctx.emit_struct_decl(td);
    }

    for (auto td : ctx.functions) {
        ctx.reset();
        ctx.emit_function_decl(td);
    }

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

void Context::sort_stuff_into_lists(CXCursor tu, size_t namespac) {
    auto visitAllChildren = [this, namespac](CXCursor c, CXCursor parent, CXClientData client_data)
    {
        //if (strcmp("FILE", clang_getCString(name)) == 0) {
        //    int a = 0;
        //}

        //std::cout << "[ERROR] TODO: " << "Cursor '" << clang_getCursorSpelling(c) << "' of kind '" << clang_getCursorKindSpelling(clang_getCursorKind(c)) << "'\n";

        switch (c.kind) {
        case CXCursor_Namespace:
            namespaces.push_back({ c, namespac });
            sort_stuff_into_lists(c, namespaces.size() - 1);
            break;

        case CXCursor_FunctionDecl:
            functions.push_back({ c, namespac });
            break;

        case CXCursor_ClassDecl:
        case CXCursor_StructDecl:
            namespaces.push_back({ c, namespac });
            structs.push_back({ c, namespac });
            sort_stuff_into_lists(c, namespaces.size() - 1);
            break;

        case CXCursor_TypedefDecl:
            typedefs.push_back({ c, namespac });
            break;

        case CXCursor_EnumDecl:
            enums.push_back({ c, namespac });
            break;

        case CXCursor_Constructor:
        case CXCursor_Destructor:
        case CXCursor_CXXMethod:
        case CXCursor_FieldDecl:
        case CXCursor_ConversionFunction:
        case CXCursor_CXXAccessSpecifier:
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

        return CXChildVisit_Continue;
    };

    auto data = getFreeCallbackData(&visitAllChildren, nullptr);
    clang_visitChildren(tu, getFreeCallback(&visitAllChildren), &data);
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

void Context::emit_struct_decl(const Declaration& decl) {
    auto cursor = decl.declaration;
    auto name = clang_getCursorSpelling(cursor);

    if (strlen(clang_getCString(name)) == 0)
        return;

    if (!clang_isCursorDefinition(cursor))
        return;

    // type declaration
    cheez_file << name << " :: struct #copy {\n";
    member_index = 0;
    auto visitAllChildren = [this, &decl](CXCursor c, CXCursor parent, CXClientData client_data) {
        auto struct_name = clang_getCursorSpelling(parent);
        auto name = clang_getCursorSpelling(c);
        auto type = clang_getCursorType(c);

        switch (c.kind) {
        case CXCursorKind::CXCursor_FieldDecl: {
            if (strcmp("LogFile", clang_getCString(name)) == 0) {
                int a = 0;
            }

            cheez_type_decl << "    " << name << " : ";
            emit_cheez_type(cheez_type_decl, type, false);
            cheez_type_decl << " = default";
            cheez_type_decl << "\n";
            break;
        }

        case CXCursorKind::CXCursor_CXXMethod: {
            if (c_string_starts_with(clang_getCString(name), "operator"))
                break;

            CXType return_type = clang_getResultType(type);

            // cheez
            {
                cheez_impl << "    " << name << " :: (ref Self";
                emit_cheez_function_parameter_list(cheez_impl, c, true);
                cheez_impl << ") ";
                if (return_type.kind != CXTypeKind::CXType_Void) {
                    cheez_impl << "-> ";
                    emit_cheez_type(cheez_impl, return_type, true);
                    cheez_impl << " ";
                }
                cheez_impl << "{\n";

                if (return_type.kind != CXTypeKind::CXType_Void) {
                    cheez_impl << "        result : ";
                    emit_cheez_type(cheez_impl, return_type, false, true, true);
                    cheez_impl << " = default\n";
                }
                cheez_impl << "        __c__" << struct_name << "_" << name << "_" << member_index << "(&self";
                if (return_type.kind != CXTypeKind::CXType_Void) {
                    cheez_impl << ", &result";
                }
                emit_cheez_function_argument_list(cheez_impl, c, true);
                cheez_impl << ")\n";
                if (return_type.kind != CXTypeKind::CXType_Void) {
                    cheez_impl << "        return ";
                    if (return_type.kind == CXTypeKind::CXType_LValueReference)
                        cheez_impl << "<<";
                    cheez_impl << "result\n";
                }
                cheez_impl << "    }\n";
            }

            // cheez binding
            {
                cheez_c_bindings << "__c__" << struct_name << "_" << name << "_" << member_index << " :: (self: &" << struct_name;

                if (return_type.kind != CXTypeKind::CXType_Void) {
                    cheez_c_bindings << ", ret: &";
                    emit_cheez_type(cheez_c_bindings, return_type, false, true, true);
                }

                emit_cheez_function_parameter_list(cheez_c_bindings, c, true, true);
                cheez_c_bindings << ");\n";
            }

            // c
            {
                c_file << "extern \"C\" void __c__" << struct_name << "_" << name << "_" << member_index << "(" << struct_name << "* self";
                if (return_type.kind != CXTypeKind::CXType_Void) {
                    c_file << ", ";
                    emit_c_type(c_file, return_type, "*ret", true, true);
                }
                emit_c_function_parameter_list(c_file, c, true);
                c_file << ") {\n";
                c_file << "    ";
                if (return_type.kind != CXTypeKind::CXType_Void) {
                    c_file << "*ret = (";
                    emit_c_type(c_file, return_type, "", false, false);
                    c_file << ")";
                    if (return_type.kind == CXTypeKind::CXType_LValueReference)
                        c_file << "&";
                }
                c_file << "self->" << name << "(";
                emit_c_function_argument_list(c_file, c, false);
                c_file << ");\n";
                c_file << "}\n";
            }
            break;
        }

        case CXCursorKind::CXCursor_Constructor: {
            CXType return_type = clang_getResultType(type);

            // cheez
            {
                cheez_impl << "    new :: (";
                emit_cheez_function_parameter_list(cheez_impl, c, false);
                cheez_impl << ") -> " << struct_name << " {\n";
                cheez_impl << "        result : " << struct_name << " = default\n";
                cheez_impl << "        __c__" << struct_name << "_new_" << member_index << "(&result";
                emit_cheez_function_argument_list(cheez_impl, c, true);
                cheez_impl << ")\n";
                cheez_impl << "        return result\n";
                cheez_impl << "    }\n";
            }

            // cheez binding
            {
                cheez_c_bindings << "__c__" << struct_name << "_new_" << member_index << " :: (self: &" << struct_name;
                emit_cheez_function_parameter_list(cheez_c_bindings, c, true, true);
                cheez_c_bindings << ");\n";
            }

            // c
            {
                c_file << "extern \"C\" void __c__" << struct_name << "_new_" << member_index << "(" << struct_name << "* self";
                emit_c_function_parameter_list(c_file, c, true);
                c_file << ") {\n";


                c_file << "    new (self) ";
                emit_namespace(c_file, decl.namespac);
                c_file << struct_name << "(";
                emit_c_function_argument_list(c_file, c, false);
                c_file << ");\n";
                c_file << "}\n";
            }
            break;
        }

        case CXCursorKind::CXCursor_Destructor: {
            // cheez
            {
                cheez_drop_impl << "    drop :: (ref Self) {\n";
                cheez_drop_impl << "        __c__" << struct_name << "_dtor(&self)\n";
                cheez_drop_impl << "    }\n";
            }

            // cheez binding
            {
                cheez_c_bindings << "__c__" << struct_name << "_dtor :: (self: &" << struct_name << ");\n";
            }

            // c
            {
                c_file << "extern \"C\" void __c__" << struct_name << "_dtor(" << struct_name << "* self) {\n";
                c_file << "    self->~" << struct_name << "();\n";
                c_file << "}\n";
            }
            break;
        }

        case CXCursorKind::CXCursor_CXXAccessSpecifier:
            // ignore for now, don't even have private members in cheez yet
            break;


        case CXCursorKind::CXCursor_EnumDecl:
        case CXCursorKind::CXCursor_UnionDecl:
        case CXCursorKind::CXCursor_ClassDecl:
        case CXCursorKind::CXCursor_StructDecl:
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

        member_index += 1;
        return CXChildVisit_Continue;
    };
    auto data = getFreeCallbackData(&visitAllChildren, nullptr);
    clang_visitChildren(cursor, getFreeCallback(&visitAllChildren), &data);

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
    auto name = clang_getCursorSpelling(cursor);

    if (strlen(clang_getCString(name)) == 0)
        return;

    if (!clang_isCursorDefinition(cursor))
        return;

    cheez_file << name << " :: enum #copy {}\n";
}

void Context::emit_function_decl(const Declaration& decl) {
    auto cursor = decl.declaration;
    auto name_raw = clang_getCursorSpelling(cursor);
    auto name = get_unique_function_name(name_raw);

    // ignore definitions
    // this apparently doesn't ignore functions defined inside structs, only global function defininitions
    // which is what we want
    if (clang_isCursorDefinition(cursor))
        return;

    // ignore operators
    if (c_string_starts_with(name.c_str(), "operator"))
        return;

    auto function_type = clang_getCursorType(cursor);
    auto return_type = clang_getResultType(function_type);

    if (strcmp("RenderFrame", clang_getCString(name_raw)) == 0) {
        int a = 0;
    }

    // cheez
    {
        cheez_impl << name << " :: (";
        emit_cheez_function_parameter_list(cheez_impl, cursor, false);
        cheez_impl << ") ";
        if (return_type.kind != CXTypeKind::CXType_Void) {
            cheez_impl << "-> ";
            emit_cheez_type(cheez_impl, return_type, true);
            cheez_impl << " ";
        }
        cheez_impl << "{\n";

        if (return_type.kind != CXTypeKind::CXType_Void) {
            cheez_impl << "    result : ";
            emit_cheez_type(cheez_impl, return_type, false, true, true);
            cheez_impl << " = default\n";
        }
        cheez_impl << "    __c__" << name << "(";
        if (return_type.kind != CXTypeKind::CXType_Void) {
            cheez_impl << "&result";
        }
        emit_cheez_function_argument_list(cheez_impl, cursor, return_type.kind != CXTypeKind::CXType_Void);
        cheez_impl << ")\n";
        if (return_type.kind != CXTypeKind::CXType_Void) {
            cheez_impl << "    return ";
            if (return_type.kind == CXTypeKind::CXType_LValueReference)
                cheez_impl << "<<";
            cheez_impl << "result\n";
        }
        cheez_impl << "}\n";
    }

    // cheez binding
    {
        cheez_c_bindings << "__c__" << name << " :: (";

        if (return_type.kind != CXTypeKind::CXType_Void) {
            cheez_c_bindings << "ret: &";
            emit_cheez_type(cheez_c_bindings, return_type, false, true, true);
        }

        emit_cheez_function_parameter_list(cheez_c_bindings, cursor, return_type.kind != CXTypeKind::CXType_Void, true);
        cheez_c_bindings << ");\n";
    }

    // c
    {
        c_file << "extern \"C\" void __c__" << name << "(";
        if (return_type.kind != CXTypeKind::CXType_Void) {
            emit_c_type(c_file, return_type, "*ret", true, true);
        }
        emit_c_function_parameter_list(c_file, cursor, return_type.kind != CXTypeKind::CXType_Void);
        c_file << ") {\n";
        c_file << "    ";
        if (return_type.kind != CXTypeKind::CXType_Void) {
            c_file << "*ret = (";
            emit_c_type(c_file, return_type, "", false, false);
            c_file << ")";
            if (return_type.kind == CXTypeKind::CXType_LValueReference)
                c_file << "&";
        }

        emit_namespace(c_file, decl.namespac);

        c_file << name_raw << "(";
        emit_c_function_argument_list(c_file, cursor, false);
        c_file << ");\n";
        c_file << "}\n";
    }

    // function implementations
    cheez_file << cheez_impl.str();

    clang_disposeString(name_raw);
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
        if (target_type.kind == CXTypeKind::CXType_FunctionProto) {
            emit_c_type(stream, target_type, name, is_func_param, behind_pointer);
        } else {
            emit_c_type(stream, target_type, "", is_func_param, true);
            stream << "* " << name;
        }
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

void Context::emit_namespace(std::ostream& stream, size_t ns) {
    while (!clang_Cursor_isNull(namespaces[ns].declaration)) {
        auto n = clang_getCursorSpelling(namespaces[ns].declaration);
        stream << n << "::";
        clang_disposeString(n);
        ns = namespaces[ns].namespac;
    }
}
