#include <iostream>
#include <fstream>
#include <unordered_map>
#include <filesystem>

#include <clang-c/Index.h>

#include "lua/lua.hpp"

#include "cpp_to_cheez.hpp"

bool c_string_starts_with(const char* str, const char* pre) {
    return strncmp(pre, str, strlen(pre)) == 0;
}

#define BREAK_ON(var, name) if (std::string(var) == (name)) {\
        __debugbreak();\
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

bool CheckLua(lua_State* L, int r) {
    if (r) {
        std::string errormsg = lua_tostring(L, -1);
        std::cerr << "[LUA] " << errormsg << std::endl;
        return false;
    }
    return true;
}

bool CppToCheezGenerator::set_custom_callbacks(const std::string& path) {
    lua_state = luaL_newstate();
    
    // register this in lua extra space
    *(CppToCheezGenerator**)lua_getextraspace(lua_state) = this;
    luaL_openlibs(lua_state);

    luaL_newlib(lua_state, lua_lib);
    lua_setglobal(lua_state, "Type");

    return CheckLua(lua_state, luaL_dofile(lua_state, path.c_str()));
}

CppToCheezGenerator::~CppToCheezGenerator() {
    if (lua_state != nullptr) {
        *(CppToCheezGenerator**)lua_getextraspace(lua_state) = nullptr;
        lua_close(lua_state);
    }

    clang_disposeTranslationUnit(m_translation_unit);
    clang_disposeIndex(m_index);
}

void CppToCheezGenerator::call_custom_handler(std::ostream& stream, const char* handler_name) {
    lua_getglobal(lua_state, handler_name);
    if (lua_isfunction(lua_state, -1)) {
        lua_call(lua_state, 0, 1);
        if (lua_isstring(lua_state, -1)) {
            auto str = lua_tostring(lua_state, -1);
            stream << str << "\n";
            lua_pop(lua_state, 1);
        }
        else {
            std::cerr << "[LUA ERROR] " << handler_name << " did not return a string\n";
            lua_pop(lua_state, 1);
        }
    }
    else if (!lua_isnil(lua_state, -1)) {
        std::cerr << "[LUA ERROR] " << handler_name << " is not a function\n";
    }

    // pop value which is either nil or not a function
    // if it was a function we already popped it by calling it
    lua_pop(lua_state, 1);
}

bool CppToCheezGenerator::call_custom_handler(std::ostream& stream, const char* name, CXCursor cursor, const char* decl_name) {
    lua_getglobal(lua_state, name);
    if (lua_isfunction(lua_state, -1)) {
        lua_pushlightuserdata(lua_state, &cursor);
        lua_pushstring(lua_state, decl_name);
        lua_call(lua_state, 2, 2);
        if (lua_isboolean(lua_state, -2)) {
            bool done = lua_toboolean(lua_state, -2);

            if (lua_isstring(lua_state, -1)) {
                auto str = lua_tostring(lua_state, -1);
                stream << str << "\n";
                lua_pop(lua_state, 2);
                return done;
            }
            else if (lua_isnil(lua_state, -1)) {
                lua_pop(lua_state, 2);
                return done;
            }
            else {
                std::cerr << "[LUA ERROR] " << name << " did not return a string or nil as second parameter\n";
                lua_pop(lua_state, 2);
                return done;
            }

            return done;
        }
        else {
            std::cerr << "[LUA ERROR] " << name << " did not return a boolean\n";
            return true;
        }
    }
    else if (!lua_isnil(lua_state, -1)) {
        std::cerr << "[LUA ERROR] " << name << " is not a function\n";
    }

    // pop value which is either nil or not a function
    // if it was a function we already popped it by calling it
    lua_pop(lua_state, 1);

    return false;
}

std::string CppToCheezGenerator::call_custom_transformer(std::ostream& stream, const char* name, CXCursor cursor, const char* decl_name, const char* member_name) {
    lua_getglobal(lua_state, name);
    if (lua_isfunction(lua_state, -1)) {
        lua_pushlightuserdata(lua_state, &cursor);
        lua_pushstring(lua_state, decl_name);
        lua_pushstring(lua_state, member_name);
        lua_call(lua_state, 3, 1);

        if (lua_isstring(lua_state, -1)) {
            auto str = lua_tostring(lua_state, -1);
            lua_pop(lua_state, 1);
            return str;
        }
        else {
            std::cerr << "[LUA ERROR] " << name << " did not return a string\n";
            lua_pop(lua_state, 1);
            return member_name;
        }
    }
    else if (!lua_isnil(lua_state, -1)) {
        std::cerr << "[LUA ERROR] " << name << " is not a function\n";
    }

    // pop value which is either nil or not a function
    // if it was a function we already popped it by calling it
    lua_pop(lua_state, 1);

    return member_name;
}

bool CppToCheezGenerator::call_macro_handler(std::ostream& stream, const char* handler_name, CXCursor cursor, const std::string& decl_name, const std::string& macro_text) {
    lua_getglobal(lua_state, handler_name);
    if (lua_isfunction(lua_state, -1)) {
        lua_pushlightuserdata(lua_state, &cursor);
        lua_pushstring(lua_state, decl_name.c_str());
        lua_pushstring(lua_state, macro_text.c_str());
        lua_call(lua_state, 3, 1);

        if (lua_isstring(lua_state, -1)) {
            auto str = lua_tostring(lua_state, -1);
            lua_pop(lua_state, 1);
            if (*str)
            stream << str << "\n";
            return true;
        }
        else {
            lua_pop(lua_state, 1);
            return false;
        }
    }
    else if (!lua_isnil(lua_state, -1)) {
        std::cerr << "[LUA ERROR] " << handler_name << " is not a function\n";
    }

    // pop value which is either nil or not a function
    // if it was a function we already popped it by calling it
    lua_pop(lua_state, 1);

    return false;
}

bool CppToCheezGenerator::call_typedef_handler(std::ostream& stream, const char* handler_name, CXCursor cursor, const std::string& decl_name, const std::string& typedef_text) {
    lua_getglobal(lua_state, handler_name);
    if (lua_isfunction(lua_state, -1)) {
        lua_pushlightuserdata(lua_state, &cursor);
        lua_pushstring(lua_state, decl_name.c_str());
        lua_pushstring(lua_state, typedef_text.c_str());
        lua_call(lua_state, 3, 1);

        if (lua_isstring(lua_state, -1)) {
            auto str = lua_tostring(lua_state, -1);
            lua_pop(lua_state, 1);
            if (*str) {
                stream << str << "\n";
                return true;
            }
            return false;
        }
        else {
            lua_pop(lua_state, 1);
            return true;
        }
    }
    else if (!lua_isnil(lua_state, -1)) {
        std::cerr << "[LUA ERROR] " << handler_name << " is not a function\n";
    }

    // pop value which is either nil or not a function
    // if it was a function we already popped it by calling it
    lua_pop(lua_state, 1);

    return false;
}

bool CppToCheezGenerator::call_custom_handler(std::ostream& stream, const char* name, CXCursor cursor, const char* decl_name, CXType decl_type) {
    lua_getglobal(lua_state, name);
    if (lua_isfunction(lua_state, -1)) {
        lua_pushlightuserdata(lua_state, &cursor);
        lua_pushstring(lua_state, decl_name);
        lua_pushlightuserdata(lua_state, &decl_type);
        lua_call(lua_state, 3, 2);
        if (lua_isboolean(lua_state, -2)) {
            bool done = lua_toboolean(lua_state, -2);

            if (lua_isstring(lua_state, -1)) {
                auto str = lua_tostring(lua_state, -1);
                stream << str << "\n";
                lua_pop(lua_state, 2);
                return done;
            }
            else if (lua_isnil(lua_state, -1)) {
                lua_pop(lua_state, 2);
                return done;
            }
            else {
                std::cerr << "[LUA ERROR] " << name << " did not return a string or nil as second parameter\n";
                lua_pop(lua_state, 2);
                return done;
            }

            return done;
        } else {
            std::cerr << "[LUA ERROR] " << name << " did not return a boolean\n";
            return true;
        }
    } else if (!lua_isnil(lua_state, -1)) {
        std::cerr << "[LUA ERROR] " << name << " is not a function\n";
    }

    // pop value which is either nil or not a function
    // if it was a function we already popped it by calling it
    lua_pop(lua_state, 1);

    return false;
}

bool CppToCheezGenerator::generate_bindings_from_lua(std::filesystem::path lua_file, std::ostream& cheez_file, std::ostream& cpp_file) {
    if (!set_custom_callbacks(lua_file.string()))
        return false;

    lua_getglobal(lua_state, "source_file");
    if (lua_isstring(lua_state, -1)) {
        auto source = lua_tostring(lua_state, -1);
        auto source_path = lua_file.parent_path() / source;
        if (!load_translation_unit_from_file(source_path.string()))
            return false;
        return do_generate_bindings(cheez_file, cpp_file);
    }
    else {
        std::cerr << "[LUA] No global string 'source_file' found\n";
        lua_pop(lua_state, 1);
        return false;
    }

    return true;
}

bool CppToCheezGenerator::generate_bindings_from_file(const std::string& source_file_path, std::ostream& cheez_file, std::ostream& cpp_file)
{
    {
        std::string header(source_file_path);
        size_t last_slash = header.find_last_of('/');
        size_t last_backslash = header.find_last_of('\\');
        size_t last = -1;
        if (last_slash == std::string::npos) last = last_backslash;
        if (last_backslash == std::string::npos) last = last_slash;
        if (last == std::string::npos) last = 0;
        else last += 1;
        std::string header_name = header.substr(last);
        cpp_file << "#include \"" << header_name << "\"\n\n";
    }

    if (!load_translation_unit_from_file(source_file_path))
        return false;
    return do_generate_bindings(cheez_file, cpp_file);
}

bool CppToCheezGenerator::load_translation_unit_from_file(const std::string& file_name) {
    m_index = clang_createIndex(0, 0);
    m_translation_unit = clang_parseTranslationUnit(
        m_index,
        file_name.c_str(), nullptr, 0,
        nullptr, 0,
        (CXTranslationUnit_None
            | CXTranslationUnit_DetailedPreprocessingRecord
            ));
    if (m_translation_unit == nullptr)
    {
        std::cerr << "[ERROR] Unable to parse translation unit. Quitting.\n";
        return false;
    }

    return true;
}

bool CppToCheezGenerator::do_generate_bindings(std::ostream& cheez_file, std::ostream& cpp_file) {
    call_custom_handler(m_cheez_buffer, "prepend_to_cheez");
    call_custom_handler(m_cpp_buffer, "prepend_to_cpp");

    m_namespaces.push_back({ clang_getNullCursor(), 0 });
    sort_stuff_into_lists(clang_getTranslationUnitCursor(m_translation_unit), 0);

    for (auto td : m_typedefs) {
        reset();
        emit_typedef_decl(td);
    }

    for (auto td : m_macros) {
        reset();
        emit_macro(td);
    }

    for (auto td : m_enums) {
        reset();
        emit_enum_decl(td);
    }

    for (auto td : m_structs) {
        reset();
        emit_struct_decl(td);
    }

    for (auto td : m_unions) {
        reset();
        emit_union_decl(td);
    }

    for (auto td : m_functions) {
        reset();
        emit_function_decl(td);
    }

    for (auto td : m_variables) {
        reset();
        emit_variable_decl(td);
    }

    for (auto td : m_macro_expansions) {
        reset();
        emit_macro_expansion(td);
    }

    cheez_file << m_cheez_unknown_types.str() << "\n";
    cheez_file << this->m_cheez_buffer.str() << "\n";
    cheez_file << "// ==========================================================\n";
    cheez_file << "// ==========================================================\n";
    cheez_file << "// ==========================================================\n\n";
    cheez_file << "#file_scope\n\n";
    cheez_file << m_cheez_c_bindings.str();
    cpp_file << m_cpp_buffer.str();

    call_custom_handler(cheez_file, "append_to_cheez");
    call_custom_handler(cpp_file, "append_to_cpp");

    return true;
}

std::string tokensToString(CXTranslationUnit unit, CXCursor cursor, int offset = 0, std::string sep = "") {

    auto range = clang_getCursorExtent(cursor);
    CXToken* tokens;
    unsigned num_tokens;
    clang_tokenize(unit, range, &tokens, &num_tokens);

    // first token should be the name of the macro
    // if there is only one token, it's a macro like
    // #define IDK
    // so we don't need to translate it
    if (num_tokens <= 1)
        return "";

    std::stringstream stream;

    bool comma = false;
    for (int i = offset; i < num_tokens; i++) {
        if (comma)
            stream << sep;
        auto token_str = clang_getTokenSpelling(unit, tokens[i]);
        auto token_str_c = clang_getCString(token_str);
        stream << token_str;
        clang_disposeString(token_str);

        comma = true;
    }
    return stream.str();
}

bool CppToCheezGenerator::sort_stuff_into_lists(CXCursor tu, size_t namespac) {
    bool ok = true;
    auto visitAllChildren = [&](CXCursor c, CXCursor parent, CXClientData client_data)
    {
        std::string uiae = clang_getCString(clang_getCursorSpelling(c));
        //BREAK_ON(uiae, "ImGui_ImplGlfw_InitForOpenGL")
        //if (strcmp("FILE", clang_getCString(name)) == 0) {
        //    int a = 0;
        //}

        //std::cout << "[ERROR] TODO: " << "Cursor '" << clang_getCursorSpelling(c) << "' of kind '" << clang_getCursorKindSpelling(clang_getCursorKind(c)) << "'\n";

        auto cursor_spelling = clang_getCursorSpelling(c);
        auto name = std::string(clang_getCString(cursor_spelling));
        clang_disposeString(cursor_spelling);


        switch (c.kind) {
        case CXCursor_Namespace:
            m_namespaces.push_back({ c, namespac, name });
            sort_stuff_into_lists(c, m_namespaces.size() - 1);
            break;

        case CXCursor_FunctionDecl:
            m_functions.push_back({ c, namespac, name });
            break;

        case CXCursor_ClassDecl:
        case CXCursor_StructDecl:
            m_namespaces.push_back({ c, namespac, name });
            m_structs.push_back({ c, namespac, name });
            sort_stuff_into_lists(c, m_namespaces.size() - 1);
            break;
        case CXCursor_UnionDecl:
            m_namespaces.push_back({ c, namespac, name });
            m_unions.push_back({ c, namespac, name });
            sort_stuff_into_lists(c, m_namespaces.size() - 1);
            break;

        case CXCursor_TypedefDecl:
            m_typedefs.push_back({ c, namespac, name });
            break;

        case CXCursor_EnumDecl:
            m_enums.push_back({ c, namespac, name });
            break;

        case CXCursor_MacroDefinition:
            m_macros.push_back({ c, namespac, name });
            break;

        case CXCursor_VarDecl:
            m_variables.push_back({ c, namespac, name });
            break;

        case CXCursor_MacroExpansion:
            m_macro_expansions.push_back({ c, namespac, name });
            break;

        case CXCursor_InclusionDirective:
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
        case CXCursor_FunctionTemplate:
            // can't handle those
            break;

        case CXCursor_UnexposedDecl:
        {
            std::string stringRepr = tokensToString(m_translation_unit, c, 0, " ");
            // handle extern "C"
            if (stringRepr.starts_with("extern \"C\"")) {
                //sort_stuff_into_lists(c, m_namespaces.size() - 1);
                return CXChildVisit_Recurse;
            }
            else {
                //auto location = clang_getCursorLocation(c);
                //CXFile file;
                //unsigned line, column;
                //clang_getFileLocation(location, &file, &line, &column, nullptr);
                //std::cout << clang_getCursorKindSpelling(clang_getCursorKind(c)) << " at " << clang_getFileName(file) << ":" << line << ":" << column << "\n";
                //std::cout << stringRepr << "\n";
            }

            break;
        }

        case CXCursor_CXXBaseSpecifier:
        {
            //ok = false;
            std::cout << "[WARNING] TODO: " << "Cursor '" << clang_getCursorSpelling(c) << "' of kind '" << clang_getCursorKindSpelling(clang_getCursorKind(c)) << "'\n";
            auto location = clang_getCursorLocation(c);
            CXFile file;
            unsigned line, column;
            clang_getFileLocation(location, &file, &line, &column, nullptr);
            std::cout << "  at " << clang_getFileName(file) << ":" << line << ":" << column << "\n";
            break;
        }

        default: {
            std::cout << "[ERROR] TODO: " << "Cursor '" << clang_getCursorSpelling(c) << "' of kind '" << clang_getCursorKindSpelling(clang_getCursorKind(c)) << "'\n";
            auto location = clang_getCursorLocation(c);
            CXFile file;
            unsigned line, column;
            clang_getFileLocation(location, &file, &line, &column, nullptr);
            std::cout << "  at " << clang_getFileName(file) << ":" << line << ":" << column << "\n";
            break;
        }
        }

        return CXChildVisit_Continue;
    };

    auto data = getFreeCallbackData(&visitAllChildren, nullptr);
    clang_visitChildren(tu, getFreeCallback(&visitAllChildren), &data);

    return ok;
}

void CppToCheezGenerator::emit_typedef_decl(const Declaration& decl) {
    auto cursor = decl.declaration;
    auto elo = clang_getTypedefDeclUnderlyingType(cursor);
    auto actual_type = clang_Type_getNamedType(elo);
    auto type_decl = clang_getTypeDeclaration(actual_type);
    auto name = clang_getCursorSpelling(cursor);
    auto name_c = clang_getCString(name);

    std::stringstream typedefTypeStringStream;
    emit_cheez_type(typedefTypeStringStream, elo, false);
    auto typedefTypeString = typedefTypeStringStream.str();
    if (call_typedef_handler(m_cheez_buffer, "on_typedef", decl.declaration, name_c, typedefTypeString)) {
        clang_disposeString(name);
        return;
    }

    switch (type_decl.kind) {
    case CXCursor_StructDecl: {
        auto type_name = clang_getCursorSpelling(type_decl);
        auto type_name_c = clang_getCString(type_name);
        if (strlen(type_name_c) == 0) {
            Declaration decl = {
                type_decl, // decl
                0, // namespace
                name_c,
            };
            emit_struct_decl(decl);
        }
        break;
    }

    case CXCursor_EnumDecl: {
        auto type_name = clang_getCursorSpelling(type_decl);
        auto type_name_c = clang_getCString(type_name);
        if (strlen(type_name_c) == 0) {
            Declaration decl = {
                type_decl, // decl
                0, // namespace
                name_c,
            };
            emit_enum_decl(decl);
        }
        break;
    }

    case CXCursor_UnionDecl: {
        auto type_name = clang_getCursorSpelling(type_decl);
        auto type_name_c = clang_getCString(type_name);
        if (strlen(type_name_c) == 0) {
            Declaration decl = {
                type_decl, // decl
                0, // namespace
                name_c,
            };
            emit_union_decl(decl);
        }
        break;
    }

    default: {
        m_cheez_buffer << name << " :: ";
        emit_cheez_type(m_cheez_buffer, elo, false);
        m_cheez_buffer << "\n";
        break;
    }
    }

    clang_disposeString(name);
    //ctx.cheez_file << name << " :: struct {}\n";
}

void CppToCheezGenerator::emit_macro_expansion(const Declaration& decl) {
    //auto visitAllChildren = [this](CXCursor c, CXCursor parent, CXClientData client_data) {
    //    switch (c.kind) {

    //    default: {
    //        std::cout << "[ERROR] TODO emit_macro_expansion: " << "Cursor '" << clang_getCursorSpelling(c) << "' of kind '" << clang_getCursorKindSpelling(clang_getCursorKind(c)) << "'\n";
    //        auto location = clang_getCursorLocation(c);
    //        CXFile file;
    //        unsigned line, column;
    //        clang_getFileLocation(location, &file, &line, &column, nullptr);
    //        std::cout << " at " << clang_getFileName(file) << ":" << line << ":" << column << "\n";
    //        break;
    //    }
    //    }
    //    return CXChildVisit_Continue;
    //};

    //auto data = getFreeCallbackData(&visitAllChildren, nullptr);
    //clang_visitChildren(decl.declaration, getFreeCallback(&visitAllChildren), &data);

    //std::cout << "[ERROR] TODO emit_macro_expansion: " << "Cursor '" << clang_getCursorSpelling(decl.declaration) << "' of kind '" << clang_getCursorKindSpelling(clang_getCursorKind(decl.declaration)) << "'\n";
    //auto location = clang_getCursorLocation(decl.declaration);
    //CXFile file;
    //unsigned line, column;
    //clang_getFileLocation(location, &file, &line, &column, nullptr);
    //std::cout << "  at " << clang_getFileName(file) << ":" << line << ":" << column << "\n";
}

void CppToCheezGenerator::emit_variable_decl(const Declaration& decl) {
    auto name = clang_getCursorSpelling(decl.declaration);
    auto name_c = clang_getCString(name);
    auto type = clang_getCursorType(decl.declaration);

    if (call_custom_handler(m_cheez_buffer, "on_global_variable", decl.declaration, name_c, type)) {
        clang_disposeString(name);
        return;
    }

    // TODO: move this to lua
    //if (c_string_starts_with(name_c, "glad_gl")) {
    //    auto sub_name = name_c + 5;

    //    m_cheez_buffer << sub_name << " : ";
    //    emit_cheez_type(m_cheez_buffer, type, false);
    //    m_cheez_buffer << " #extern #linkname(\"" << name << "\")\n";
    //    clang_disposeString(name);
    //    return;
    //}

    m_cheez_buffer << name << " : ";
    emit_cheez_type(m_cheez_buffer, type, false);
    m_cheez_buffer << " #extern\n";
    clang_disposeString(name);
}

void CppToCheezGenerator::emit_macro(const Declaration& decl) {
    auto cursor = decl.declaration;
    auto name = clang_getCursorSpelling(cursor);
    auto name_c = clang_getCString(name);

    auto name_str = std::string(name_c);
    auto macro_text = tokensToString(m_translation_unit, cursor, 1);

    // ignore macros starting with __, builtin macros and function like macros
    if (clang_Cursor_isMacroFunctionLike(cursor) || clang_Cursor_isMacroBuiltin(cursor))
        return;
    if (c_string_starts_with(name_c, "_")) {
        clang_disposeString(name);
        return;
    }

    if (call_macro_handler(m_cheez_buffer, "on_macro2", cursor, name_str, macro_text)) {
        return;
    }

    if (call_custom_handler(m_cheez_buffer, "on_macro", decl.declaration, name_c)) {
        clang_disposeString(name);
        return;
    }

    // temp
    if (c_string_starts_with(name_c, "gl")) {
        clang_disposeString(name);
        return;
    }

    // get tokens
    auto range = clang_getCursorExtent(cursor);
    CXToken* tokens;
    unsigned num_tokens;
    clang_tokenize(m_translation_unit, range, &tokens, &num_tokens);

    // first token should be the name of the macro
    // if there is only one token, it's a macro like
    // #define IDK
    // so we don't need to translate it
    if (num_tokens <= 1)
        return;

    m_cheez_buffer << name << " :: ";

    for (int i = 1; i < num_tokens; i++) {
        auto token_str = clang_getTokenSpelling(m_translation_unit, tokens[i]);
        auto token_str_c = clang_getCString(token_str);
        m_cheez_buffer << token_str;
        clang_disposeString(token_str);
    }

    m_cheez_buffer << "\n";
    clang_disposeString(name);
}

void CppToCheezGenerator::emit_struct_decl(const Declaration& decl) {
    auto cursor = decl.declaration;

    if (decl.name.length() == 0) {
        return;
    }

    if (!clang_isCursorDefinition(cursor)) {
        return;
    }

    if (call_custom_handler(m_cheez_buffer, "on_struct", decl.declaration, decl.name.c_str())) {
        return;
    }

    // type declaration
    m_cheez_buffer << decl.name << " :: struct #copy {\n";
    auto visitAllChildren = [this, &decl, member_index = 0]
        (CXCursor c, CXCursor parent, CXClientData client_data) mutable {
        auto struct_name = clang_getCursorSpelling(parent);
        auto name = clang_getCursorSpelling(c);
        auto type = clang_getCursorType(c);

        switch (c.kind) {
        case CXCursorKind::CXCursor_FieldDecl: {
            if (strcmp("LogFile", clang_getCString(name)) == 0) {
                int a = 0;
            }

            m_cheez_type_decls << "    " << name << " : ";
            emit_cheez_type(m_cheez_type_decls, type, false);
            m_cheez_type_decls << " = default";
            m_cheez_type_decls << "\n";
            break;
        }

        case CXCursorKind::CXCursor_CXXMethod: {
            if (c_string_starts_with(clang_getCString(name), "operator"))
                break;

            CXType return_type = clang_getResultType(type);

            // cheez
            {
                m_cheez_impls << "    " << name << " :: (&mut Self";
                emit_cheez_function_parameter_list(m_cheez_impls, c, true);
                m_cheez_impls << ") ";
                if (return_type.kind != CXTypeKind::CXType_Void) {
                    m_cheez_impls << "-> ";
                    emit_cheez_type(m_cheez_impls, return_type, true);
                    m_cheez_impls << " ";
                }
                m_cheez_impls << "{\n";

                if (return_type.kind != CXTypeKind::CXType_Void) {
                    m_cheez_impls << "        mut result : ";
                    emit_cheez_type(m_cheez_impls, return_type, false, true, true);
                    m_cheez_impls << " = default\n";
                }
                m_cheez_impls << "        __c__" << struct_name << "_" << name << "_" << member_index << "(^mut *self";
                if (return_type.kind != CXTypeKind::CXType_Void) {
                    m_cheez_impls << ", ^mut result";
                }
                emit_cheez_function_argument_list(m_cheez_impls, c, true);
                m_cheez_impls << ")\n";
                if (return_type.kind != CXTypeKind::CXType_Void) {
                    m_cheez_impls << "        return ";
                    if (return_type.kind == CXTypeKind::CXType_LValueReference)
                        m_cheez_impls << "*";
                    m_cheez_impls << "result\n";
                }
                m_cheez_impls << "    }\n";
            }

            // cheez binding
            {
                m_cheez_c_bindings << "__c__" << struct_name << "_" << name << "_" << member_index << " :: (self: ^mut " << struct_name;

                if (return_type.kind != CXTypeKind::CXType_Void) {
                    m_cheez_c_bindings << ", ret: ^";
                    emit_cheez_type(m_cheez_c_bindings, return_type, false, true, true);
                }

                emit_cheez_function_parameter_list(m_cheez_c_bindings, c, true, true);
                m_cheez_c_bindings << ");\n";
            }

            // c
            {
                m_cpp_buffer << "extern \"C\" void __c__" << struct_name << "_" << name << "_" << member_index << "(" << struct_name << "* self";
                if (return_type.kind != CXTypeKind::CXType_Void) {
                    m_cpp_buffer << ", ";
                    emit_c_type(m_cpp_buffer, return_type, "*ret", true, true);
                }
                emit_c_function_parameter_list(m_cpp_buffer, c, true);
                m_cpp_buffer << ") {\n";
                m_cpp_buffer << "    ";
                if (return_type.kind != CXTypeKind::CXType_Void) {
                    m_cpp_buffer << "*ret = (";
                    emit_c_type(m_cpp_buffer, return_type, "", false, false);
                    m_cpp_buffer << ")";
                    if (return_type.kind == CXTypeKind::CXType_LValueReference)
                        m_cpp_buffer << "&";
                }
                m_cpp_buffer << "self->" << name << "(";
                emit_c_function_argument_list(m_cpp_buffer, c, false);
                m_cpp_buffer << ");\n";
                m_cpp_buffer << "}\n";
            }
            break;
        }

        case CXCursorKind::CXCursor_Constructor: {
            CXType return_type = clang_getResultType(type);

            // cheez
            {
                m_cheez_impls << "    new :: (";
                emit_cheez_function_parameter_list(m_cheez_impls, c, false);
                m_cheez_impls << ") -> " << struct_name << " {\n";
                m_cheez_impls << "        result : " << struct_name << " = default\n";
                m_cheez_impls << "        __c__" << struct_name << "_new_" << member_index << "(^mut result";
                emit_cheez_function_argument_list(m_cheez_impls, c, true);
                m_cheez_impls << ")\n";
                m_cheez_impls << "        return result\n";
                m_cheez_impls << "    }\n";
            }

            // cheez binding
            {
                m_cheez_c_bindings << "__c__" << struct_name << "_new_" << member_index << " :: (self: ^mut " << struct_name;
                emit_cheez_function_parameter_list(m_cheez_c_bindings, c, true, true);
                m_cheez_c_bindings << ");\n";
            }

            // c
            {
                m_cpp_buffer << "extern \"C\" void __c__" << struct_name << "_new_" << member_index << "(" << struct_name << "* self";
                emit_c_function_parameter_list(m_cpp_buffer, c, true);
                m_cpp_buffer << ") {\n";


                m_cpp_buffer << "    new (self) ";
                emit_namespace(m_cpp_buffer, decl.namespac);
                m_cpp_buffer << struct_name << "(";
                emit_c_function_argument_list(m_cpp_buffer, c, false);
                m_cpp_buffer << ");\n";
                m_cpp_buffer << "}\n";
            }
            break;
        }

        case CXCursorKind::CXCursor_Destructor: {
            // cheez
            {
                m_cheez_drop_impls << "    drop :: (&mut Self) {\n";
                m_cheez_drop_impls << "        __c__" << struct_name << "_dtor(^*self)\n";
                m_cheez_drop_impls << "    }\n";
            }

            // cheez binding
            {
                m_cheez_c_bindings << "__c__" << struct_name << "_dtor :: (self: ^mut " << struct_name << ");\n";
            }

            // c
            {
                m_cpp_buffer << "extern \"C\" void __c__" << struct_name << "_dtor(" << struct_name << "* self) {\n";
                m_cpp_buffer << "    self->~" << struct_name << "();\n";
                m_cpp_buffer << "}\n";
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
            auto location = clang_getCursorLocation(c);
            CXFile file;
            unsigned line, column;
            clang_getFileLocation(location, &file, &line, &column, nullptr);
            std::cout << "  at " << clang_getFileName(file) << ":" << line << ":" << column << "\n";
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

    m_cheez_buffer << m_cheez_type_decls.str();
    m_cheez_buffer << "}\n";

    // function implementations
    auto impl = m_cheez_impls.str();
    if (impl.size() > 0) {
        m_cheez_buffer << "impl " << decl.name << " {\n";
        m_cheez_buffer << impl;
        m_cheez_buffer << "}\n";
    }

    auto drop_impl = m_cheez_drop_impls.str();
    if (drop_impl.size() > 0) {
        m_cheez_buffer << "impl Drop for " << decl.name << " {\n";
        m_cheez_buffer << drop_impl;
        m_cheez_buffer << "}\n";
    }
}

void CppToCheezGenerator::emit_union_decl(const Declaration& decl) {
    auto cursor = decl.declaration;
    auto name = clang_getCursorSpelling(cursor);

    if (strlen(clang_getCString(name)) == 0)
        return;

    if (!clang_isCursorDefinition(cursor))
        return;

    if (call_custom_handler(m_cheez_buffer, "on_union", decl.declaration, clang_getCString(name))) {
        clang_disposeString(name);
        return;
    }

    int longest_member_name = 0;
    {
        auto calcMaxMemberLength = [this, &longest_member_name](CXCursor c, CXCursor parent, CXClientData client_data) {
            auto member_name = clang_getCursorSpelling(c);
            auto member_name_c = clang_getCString(member_name);

            auto struct_name = clang_getCursorSpelling(parent);
            auto struct_name_c = clang_getCString(struct_name);

            auto member_name_final = call_custom_transformer(m_cheez_buffer, "transform_union_member_name", c, struct_name_c, member_name_c);

            int member_name_len = member_name_final.size();

            //if (c_string_starts_with(member_name_c, struct_name_c)) {
            //    member_name_len -= strlen(struct_name_c);
            //}

            //if (c_string_starts_with(member_name_c, "_")) {
            //    member_name_len -= 1;
            //}

            if (member_name_len > longest_member_name) {
                longest_member_name = member_name_len;
            }
            return CXChildVisit_Continue;
        };

        auto data = getFreeCallbackData(&calcMaxMemberLength, nullptr);
        clang_visitChildren(cursor, getFreeCallback(&calcMaxMemberLength), &data);
    }

    {
        auto visitAllChildren = [this, longest_member_name](CXCursor c, CXCursor parent, CXClientData client_data) {
            auto member_name = clang_getCursorSpelling(c);
            auto member_name_c = clang_getCString(member_name);
            auto member_type = clang_getCursorType(c);

            auto union_name = clang_getCursorSpelling(parent);
            auto union_name_c = clang_getCString(union_name);

            auto member_name_final = call_custom_transformer(m_cheez_buffer, "transform_union_member_name", c, union_name_c, member_name_c);

            switch (c.kind) {
            case CXCursorKind::CXCursor_FieldDecl: {
                m_cheez_type_decls << "    " << member_name_final;
                indent(m_cheez_type_decls, longest_member_name - member_name_final.size());
                m_cheez_type_decls << " : ";
                emit_cheez_type(m_cheez_type_decls, member_type, false);
                m_cheez_type_decls << "\n";
                break;
            }
            }
            return CXChildVisit_Continue;
        };
        auto data = getFreeCallbackData(&visitAllChildren, nullptr);
        clang_visitChildren(cursor, getFreeCallback(&visitAllChildren), &data);
    }

    auto tag_type = clang_getEnumDeclIntegerType(cursor);

    m_cheez_buffer << name << " :: enum #copy #untagged";
    m_cheez_buffer << " {\n" << m_cheez_type_decls.str() << "}\n";
}

struct EnumMember {
    CXCursor cursor;
    std::string name;
    int64_t value;
};

struct Enum {
    std::string name;
    std::vector<std::string> directives;
    std::vector<EnumMember> members;
};

template<typename T, typename F>
std::vector<T> luaArrayToVector(lua_State* L, int32_t index, F converter) {
    std::vector<T> result;
    int32_t len = lua_rawlen(L, index);
    for (int32_t i = 1; i <= len; i++) {
        lua_pushinteger(L, i);
        lua_gettable(L, index - 1);
        result.push_back(converter());
        lua_pop(L, 1);
    }
    return result;
}

bool custom_enum_handler(lua_State* L, Enum& enum_) {
    lua_getglobal(L, "on_enum");
    if (lua_isfunction(L, -1)) {
        lua_newtable(L);

        // push table for entire enum
        lua_pushstring(L, "name");
        lua_pushstring(L, enum_.name.c_str());
        lua_settable(L, -3);

        // directives
        lua_pushstring(L, "directives");
        lua_newtable(L);
        for (int i = 0; i < enum_.directives.size(); i++) {
            lua_pushnumber(L, i + 1);
            lua_pushstring(L, enum_.directives[i].c_str());
            lua_settable(L, -3);
        }
        lua_settable(L, -3);

        // members
        lua_pushstring(L, "members");
        lua_newtable(L);
        for (int i = 0; i < enum_.members.size(); i++) {
            lua_pushnumber(L, i + 1);
            lua_newtable(L);

            lua_pushstring(L, "name");
            lua_pushstring(L, enum_.members[i].name.c_str());
            lua_settable(L, -3);

            lua_pushstring(L, "value");
            lua_pushnumber(L, enum_.members[i].value);
            lua_settable(L, -3);

            lua_settable(L, -3);
        }
        lua_settable(L, -3);

        lua_call(L, 1, 1);

        if (!lua_istable(L, -1))
        {
            lua_pop(L, 1);
            return false;
        }

        lua_pushstring(L, "name");
        lua_gettable(L, -2);
        enum_.name = lua_tostring(L, -1);
        lua_pop(L, 1);

        lua_pushstring(L, "directives");
        lua_gettable(L, -2);
        enum_.directives = luaArrayToVector<std::string>(L, -1, [&]() {
            return std::string(lua_tostring(L, -1));
        });
        lua_pop(L, 1);

        lua_pushstring(L, "members");
        lua_gettable(L, -2);
        enum_.members = luaArrayToVector<EnumMember>(L, -1, [&]() {
            lua_pushstring(L, "name");
            lua_gettable(L, -2);
            auto name_c = lua_tostring(L, -1);
            std::string name = name_c;
            lua_pop(L, 1);
            lua_pushstring(L, "value");
            lua_gettable(L, -2);
            int32_t value = lua_tonumber(L, -1);
            lua_pop(L, 1);
            return EnumMember({ {}, name, value });
        });
        lua_pop(L, 1);
    }

    // pop value which is either nil or not a function
    // if it was a function we already popped it by calling it
    lua_pop(L, 1);

    return true;
}

void CppToCheezGenerator::emit_enum_decl(const Declaration& decl) {
    auto cursor = decl.declaration;
    if (!clang_isCursorDefinition(cursor))
        return;

    Enum enum_;
    enum_.directives = {"#copy", "#repr(\"C\")" };

    {
        auto tag_type = clang_getEnumDeclIntegerType(cursor);
        std::stringstream temp;
        temp << "#tag_type(";
        emit_cheez_type(temp, tag_type, false);
        temp << ")";
        enum_.directives.push_back(temp.str());
    }

    {
        auto name = clang_getCursorSpelling(cursor);
        enum_.name = clang_getCString(name);
        clang_disposeString(name);
        if (enum_.name.empty())
            return;
    }

    //if (call_custom_handler(m_cheez_buffer, "on_enum", decl.declaration, enum_.name.c_str())) {
    //    return;
    //}

    enum_.name = call_custom_transformer(m_cheez_buffer, "transform_enum_name", decl.declaration, enum_.name.c_str(), enum_.name.c_str());

    {
        auto collectMembers = [this, &enum_](CXCursor c, CXCursor parent, CXClientData client_data) {
            auto member_name = clang_getCursorSpelling(c);
            enum_.members.push_back({ c, clang_getCString(member_name), clang_getEnumConstantDeclValue(c) });
            clang_disposeString(member_name);
            return CXChildVisit_Continue;
        };

        auto data = getFreeCallbackData(&collectMembers, nullptr);
        clang_visitChildren(cursor, getFreeCallback(&collectMembers), &data);
    }
    if (!custom_enum_handler(lua_state, enum_))
        return;

    std::sort(enum_.members.begin(), enum_.members.end(), [](auto& a, auto& b) {return a.value < b.value; });

    int longest_member_name = 0;
    for(auto& member : enum_.members) {
        member.name = call_custom_transformer(m_cheez_buffer, "transform_enum_member_name", member.cursor, enum_.name.c_str(), member.name.c_str());
        int member_name_len = member.name.size();
        if (member_name_len > longest_member_name) {
            longest_member_name = member_name_len;
        }
    }

    std::unordered_map<std::string, int64_t> memberMap;
    for (auto& member : enum_.members) {
        if (memberMap.find(member.name) != memberMap.end()) {
            auto otherValue = memberMap[member.name];
            if (member.value != otherValue) {
                std::cout << "Enum '" << enum_.name << "' has duplicate member '" << member.name << "' with different values: " << otherValue << ", " << member.value << "\n";
            }
            else {
                continue;
            }
        }
        memberMap[member.name] = member.value;

        m_cheez_type_decls << "    " << member.name;

        indent(m_cheez_type_decls, longest_member_name - member.name.size());

        if (member.value >= 0) {
            m_cheez_type_decls << " = 0x" << std::hex << member.value << std::dec << "\n";
        } else {
            m_cheez_type_decls << " = " << member.value << "\n";
        }
    }

    m_cheez_buffer << enum_.name << " :: enum";
    for (auto& dir : enum_.directives) {
        m_cheez_buffer << " " << dir;
    }
    m_cheez_buffer << " {\n" << m_cheez_type_decls.str() << "}\n";
}

void CppToCheezGenerator::emit_function_decl(const Declaration& decl) {
    auto cursor = decl.declaration;
    auto name_raw = clang_getCursorSpelling(cursor);
    auto name_raw_c = clang_getCString(name_raw);
    auto name = get_unique_function_name(name_raw);

    // ignore definitions
    // this apparently doesn't ignore functions defined inside structs, only global function defininitions
    // which is what we want
    if (clang_isCursorDefinition(cursor))
        return;

    // ignore operators
    if (c_string_starts_with(name.c_str(), "operator"))
        return;

    if (call_custom_handler(m_cheez_buffer, "on_function", decl.declaration, clang_getCString(name_raw))) {
        clang_disposeString(name_raw);
        return;
    }

    auto function_type = clang_getCursorType(cursor);
    auto return_type = clang_getResultType(function_type);

    // cheez
    {
        m_cheez_impls << name << " :: (";
        emit_cheez_function_parameter_list(m_cheez_impls, cursor, false);
        m_cheez_impls << ") ";
        if (return_type.kind != CXTypeKind::CXType_Void) {
            m_cheez_impls << "-> ";
            emit_cheez_type(m_cheez_impls, return_type, true);
            m_cheez_impls << " ";
        }
        m_cheez_impls << "{\n";

        if (return_type.kind != CXTypeKind::CXType_Void) {
            m_cheez_impls << "    mut result : ";
            emit_cheez_type(m_cheez_impls, return_type, false, true, true);
            m_cheez_impls << " = default\n";
        }
        m_cheez_impls << "    __c__" << name << "(";
        if (return_type.kind != CXTypeKind::CXType_Void) {
            m_cheez_impls << "^result";
        }
        emit_cheez_function_argument_list(m_cheez_impls, cursor, return_type.kind != CXTypeKind::CXType_Void);
        m_cheez_impls << ")\n";
        if (return_type.kind != CXTypeKind::CXType_Void) {
            m_cheez_impls << "    return ";
            if (return_type.kind == CXTypeKind::CXType_LValueReference) {
                m_cheez_impls << "&mut *";
                //m_cheez_impls << "*";
            }
            m_cheez_impls << "result\n";
        }
        m_cheez_impls << "}\n";
    }

    // cheez binding
    {
        m_cheez_c_bindings << "__c__" << name << " :: (";

        if (return_type.kind != CXTypeKind::CXType_Void) {
            m_cheez_c_bindings << "ret: ^mut ";
            emit_cheez_type(m_cheez_c_bindings, return_type, false, true, true);
        }

        emit_cheez_function_parameter_list(m_cheez_c_bindings, cursor, return_type.kind != CXTypeKind::CXType_Void, true);
        m_cheez_c_bindings << ");\n";
    }

    // c
    {
        m_cpp_buffer << "extern \"C\" void __c__" << name << "(";
        if (return_type.kind != CXTypeKind::CXType_Void) {
            emit_c_type(m_cpp_buffer, return_type, "*ret", true, true);
        }
        emit_c_function_parameter_list(m_cpp_buffer, cursor, return_type.kind != CXTypeKind::CXType_Void);
        m_cpp_buffer << ") {\n";
        m_cpp_buffer << "    ";
        if (return_type.kind != CXTypeKind::CXType_Void) {
            m_cpp_buffer << "*ret = (";
            emit_c_type(m_cpp_buffer, return_type, "", false, false);
            m_cpp_buffer << ")";
            if (return_type.kind == CXTypeKind::CXType_LValueReference)
                m_cpp_buffer << "&";
        }

        emit_namespace(m_cpp_buffer, decl.namespac);

        m_cpp_buffer << name_raw << "(";
        emit_c_function_argument_list(m_cpp_buffer, cursor, false);
        m_cpp_buffer << ");\n";
        m_cpp_buffer << "}\n";
    }

    // function implementations
    m_cheez_buffer << m_cheez_impls.str();

    clang_disposeString(name_raw);
}

void CppToCheezGenerator::emit_parameter_default_value(std::ostream& stream, CXCursor c, CXToken* tokens, int num_tokens, int index, bool emit_equals) {
    auto default_value_token = tokens[index];
    auto kind = clang_getTokenKind(default_value_token);

    // only generate default values for literals and NULL
    if (kind == CXTokenKind::CXToken_Literal) {
        auto token_str = clang_getTokenSpelling(m_translation_unit, default_value_token);
        if (emit_equals) stream << " = ";
        stream << token_str;
        clang_disposeString(token_str);
    }
    else if (kind == CXTokenKind::CXToken_Keyword) {
        auto token_str = clang_getTokenSpelling(m_translation_unit, default_value_token);
        auto token_str_c = clang_getCString(token_str);

        if (strcmp("false", token_str_c) == 0) {
            if (emit_equals) stream << " = ";
            stream << "false";
        } else if (strcmp("true", token_str_c) == 0) {
            if (emit_equals) stream << " = ";
            stream << "true";
        } else if (strcmp("sizeof", token_str_c) == 0) {
            // @TODO: ignore for now
        } else {
            auto location = clang_getCursorLocation(c);
            CXFile file;
            unsigned line, column;
            clang_getFileLocation(location, &file, &line, &column, nullptr);
            std::cout << "[ERROR] TODO handle default vaule (keyword): " << token_str << "\n";
            std::cout << "  at " << clang_getFileName(file) << ":" << line << ":" << column << "\n";
        }

        clang_disposeString(token_str);
    }
    else if (kind == CXTokenKind::CXToken_Punctuation) {
        auto token_str = clang_getTokenSpelling(m_translation_unit, default_value_token);
        auto token_str_c = clang_getCString(token_str);

        if (strcmp("-", token_str_c) == 0) {
            if (emit_equals) stream << " = ";
            stream << "-";
            emit_parameter_default_value(stream, c, tokens, num_tokens, index + 1, false);
        } else if (strcmp("+", token_str_c) == 0) {
            if (emit_equals) stream << " = ";
            stream << "+";
            emit_parameter_default_value(stream, c, tokens, num_tokens, index + 1, false);
        } else if (strcmp("~", token_str_c) == 0) {
            if (emit_equals) stream << " = ";
            stream << "@bin_not(";
            emit_parameter_default_value(stream, c, tokens, num_tokens, index + 1, false);
            stream << ")";
        } else {
            auto location = clang_getCursorLocation(c);
            CXFile file;
            unsigned line, column;
            clang_getFileLocation(location, &file, &line, &column, nullptr);
            std::cout << "[ERROR] TODO handle default vaule (punctuation): " << token_str << "\n";
            std::cout << "  at " << clang_getFileName(file) << ":" << line << ":" << column << "\n";
        }

        clang_disposeString(token_str);
    }
    else if (kind == CXTokenKind::CXToken_Identifier) {
        auto token_str = clang_getTokenSpelling(m_translation_unit, default_value_token);
        auto token_str_c = clang_getCString(token_str);

        if (strcmp("NULL", token_str_c) == 0) {
            if (emit_equals) stream << " = ";
            stream << "null";
        } else if (index == num_tokens - 1) {
            // only one token which is an identifier, so probably a constant
            if (emit_equals) stream << " = ";
            stream << token_str;
        } else if (index < num_tokens - 1) {
            // more tokens following after this id, so we'll just ignore it
        } else {
            auto location = clang_getCursorLocation(c);
            CXFile file;
            unsigned line, column;
            clang_getFileLocation(location, &file, &line, &column, nullptr);
            std::cout << "[ERROR] TODO handle default vaule (identifier): " << token_str << "\n";
            std::cout << "  at " << clang_getFileName(file) << ":" << line << ":" << column << "\n";
        }

        clang_disposeString(token_str);
    }
    else {
        auto token_str = clang_getTokenSpelling(m_translation_unit, default_value_token);
        auto location = clang_getCursorLocation(c);
        CXFile file;
        unsigned line, column;
        clang_getFileLocation(location, &file, &line, &column, nullptr);
        std::cout << "[ERROR] TODO handle default vaule (" << kind << "): " << token_str << "\n";
        std::cout << "  at " << clang_getFileName(file) << ":" << line << ":" << column << "\n";
        clang_disposeString(token_str);
    }
}

void CppToCheezGenerator::emit_cheez_function_parameter_list(std::ostream& stream, CXCursor func, bool start_with_comma, bool prefer_pointers) {
    auto visitAllChildren = [this, &stream, start_with_comma, prefer_pointers, param_index = 0]
        (CXCursor c, CXCursor parent, CXClientData client_data) mutable {
        switch (c.kind) {
        case CXCursorKind::CXCursor_ParmDecl: {
            CXString name = clang_getCursorSpelling(c);
            CXType type = clang_getCursorType(c);

            if (param_index > 0 || start_with_comma)
                stream << ", ";
            emit_param_name(stream, c, param_index);
            stream << ": ";
            emit_cheez_type(stream, type, true, false, prefer_pointers);

            // emit default value
            {
                auto range = clang_getCursorExtent(c);
                CXToken* tokens;
                unsigned num_tokens;
                clang_tokenize(m_translation_unit, range, &tokens, &num_tokens);
                    
                bool has_default_value = false;
                int default_value_start = 0;
                for (int i = 0; i < num_tokens; i++) {
                    auto token_str = clang_getTokenSpelling(m_translation_unit, tokens[i]);
                    auto token_str_c = clang_getCString(token_str);
                    if (strcmp(token_str_c, "=") == 0) {
                        has_default_value = true;
                        default_value_start = i + 1;
                        clang_disposeString(token_str);
                        break;
                    }
                    clang_disposeString(token_str);
                }

                if (has_default_value && default_value_start >= num_tokens) {
                    auto location = clang_getCursorLocation(c);
                    CXFile file;
                    unsigned line, column;
                    clang_getFileLocation(location, &file, &line, &column, nullptr);
                    std::cout << "[ERROR] failed to get default value from parameter\n";
                    std::cout << "  at " << clang_getFileName(file) << ":" << line << ":" << column << "\n";
                } else if (has_default_value) {
                    //emit_parameter_default_value(stream, c, tokens, num_tokens, default_value_start, true);
                }
            }

            clang_disposeString(name);
            param_index += 1;
            break;
        }
        }
        return CXChildVisit_Continue;
    };

    auto data = getFreeCallbackData(&visitAllChildren, nullptr);
    clang_visitChildren(func, getFreeCallback(&visitAllChildren), &data);
}

void CppToCheezGenerator::emit_cheez_function_argument_list(std::ostream& stream, CXCursor func, bool start_with_comma) {
    auto visitAllChildren = [this, &stream, start_with_comma, param_index = 0]
        (CXCursor c, CXCursor parent, CXClientData client_data) mutable {
        switch (c.kind) {
        case CXCursorKind::CXCursor_ParmDecl: {
            if (param_index > 0 || start_with_comma)
                stream << ", ";

            CXType type = clang_getCursorType(c);
            if (pass_type_by_pointer(type)) {
                if (type.kind == CXType_LValueReference)
                    stream << "^mut *";
                else
                    stream << "^mut ";
            }
            else {
                if (type.kind == CXType_LValueReference)
                    stream << "^mut *";
                else
                    stream << "";
            }
            emit_param_name(stream, c, param_index);
            param_index += 1;
            break;
        }
        }

        return CXChildVisit_Continue;
    };

    auto data = getFreeCallbackData(&visitAllChildren, nullptr);
    clang_visitChildren(func, getFreeCallback(&visitAllChildren), &data);
}

void CppToCheezGenerator::emit_c_function_parameter_list(std::ostream& stream, CXCursor func, bool start_with_comma) {
    auto visitAllChildren = [this, &stream, start_with_comma, param_index = 0]
        (CXCursor c, CXCursor parent, CXClientData client_data) mutable {
        switch (c.kind) {
        case CXCursorKind::CXCursor_ParmDecl: {
            if (param_index > 0 || start_with_comma)
                stream << ", ";
            emit_c_type(stream, clang_getCursorType(c), get_param_name(c, param_index).c_str(), true);
            param_index += 1;
            break;
        }
        }

        return CXChildVisit_Continue;
    };

    auto data = getFreeCallbackData(&visitAllChildren, nullptr);
    clang_visitChildren(func, getFreeCallback(&visitAllChildren), &data);
}

void CppToCheezGenerator::emit_c_function_argument_list(std::ostream& stream, CXCursor func, bool start_with_comma) {
    auto visitAllChildren = [this, &stream, start_with_comma, param_index = 0]
        (CXCursor c, CXCursor parent, CXClientData client_data) mutable {
        switch (c.kind) {
        case CXCursorKind::CXCursor_ParmDecl: {
            CXString name = clang_getCursorSpelling(c);
            CXType type = clang_getCursorType(c);

            if (param_index > 0 || start_with_comma)
                stream << ", ";

            if (pass_type_by_pointer(type) || type.kind == CXType_LValueReference)
                stream << "*";
            emit_param_name(stream, c, param_index);

            clang_disposeString(name);
            param_index += 1;
            break;
        }
        }
        return CXChildVisit_Continue;
    };

    auto data = getFreeCallbackData(&visitAllChildren, nullptr);
    clang_visitChildren(func, getFreeCallback(&visitAllChildren), &data);
}

void CppToCheezGenerator::emit_cheez_type(std::ostream& stream, const CXType& type, bool is_func_param, bool behind_pointer, bool prefer_pointers, bool add_mutability_mod) {
    long long size = clang_Type_getSizeOf(type);
    auto spelling = clang_getTypeSpelling(type);
    auto spelling_c = clang_getCString(spelling);

    auto is_const = strstr(spelling_c, "const ") == spelling_c;
    auto const_modifier = "mut ";
    if (is_const) {
        spelling_c += strlen("const ");
        switch (type.kind) {
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
            const_modifier = "";
            break;

        default: {
        }
        }
    }

    if (add_mutability_mod) {
        stream << const_modifier;
    }

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
            stream << "^";
            emit_cheez_type(stream, target_type, is_func_param, true, prefer_pointers, true);
        }
        else {
            stream << "[" << array_size << "]";
            emit_cheez_type(stream, target_type, is_func_param, true, prefer_pointers);
        }
        break;
    }

    case CXTypeKind::CXType_IncompleteArray: {
        auto target_type = clang_getArrayElementType(type);

        if (is_func_param) {
            stream << "^";
        }
        else {
            stream << "[]";
        }
        emit_cheez_type(stream, target_type, is_func_param, true, prefer_pointers, true);
        break;
    }

    case CXTypeKind::CXType_Pointer: {
        auto target_type = clang_getPointeeType(type);
        if (target_type.kind == CXTypeKind::CXType_FunctionProto) {
            emit_cheez_type(stream, target_type, is_func_param, behind_pointer, prefer_pointers);
        } else {
            stream << "^";
            emit_cheez_type(stream, target_type, is_func_param, true, false, true);
        }
        break;
    }

    case CXTypeKind::CXType_LValueReference: {
        auto target_type = clang_getPointeeType(type);
        if (prefer_pointers)
            stream << "^";
        else
            stream << "&";
        emit_cheez_type(stream, target_type, is_func_param, true, false, true);
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
            stream << "^" << const_modifier << clang_getCursorSpelling(type_decl);
        }
        else {
            stream << clang_getCursorSpelling(type_decl);
        }
        break;
    }

    // enum
    case CXTypeKind::CXType_Enum: {
        stream << spelling_c;
        break;
    }

    case CXTypeKind::CXType_Typedef: {

        if (behind_pointer) {
            auto typedef_decl = clang_getTypeDeclaration(type);
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
                    stream << "^" << const_modifier << spelling_c;
                }
                else {
                    stream << spelling_c;
                }
                break;

            default:
                stream << clang_getCursorSpelling(typedef_decl);
                break;
            }
        }
        break;
    }

    default: {
        if (size <= 0) {
            size = 0;
        }

        auto unknown = m_unknown_types.find(size);
        if (unknown == m_unknown_types.end()) {
            if (size == 0) {

                m_cheez_unknown_types << "__UNKNOWN_0 :: struct {}\n";
            }
            else {
                m_cheez_unknown_types << "__UNKNOWN_" << size << " :: struct #copy {\n    _: [" << size << "]u8 = default\n}\n";
            }
            m_unknown_types.insert(size);
        }

        auto spelling = spelling_c;
        if (prefer_pointers && !behind_pointer)
            stream << "^" << const_modifier;
        else if (size == 0) {
            std::cerr << "[WARNING] Failed to translate type to cheez '" << spelling << "' (" << clang_getTypeKindSpelling(type.kind) << "), size was negative. Replaced with __UNKNOWN_0\n";
        }
        stream << "__UNKNOWN_" << size;
        //std::cerr << "[ERROR] Failed to translate type to cheez '" << spelling << "' (" << clang_getTypeKindSpelling(type.kind) << ") - replaced with __UNKNOWN_" << size << "\n";
        break;
    }
    }
    clang_disposeString(spelling);
}

void CppToCheezGenerator::emit_c_type(std::ostream& stream, const CXType& type, const char* name, bool is_func_param, bool behind_pointer) {
    long long size = clang_Type_getSizeOf(type);
    auto spelling = clang_getTypeSpelling(type);
    auto spelling_c = clang_getCString(spelling);

    auto is_const = strstr(spelling_c, "const ") == spelling_c;
    if (is_const) {
        switch (type.kind) {
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
            stream << "const ";
            break;

        default: {
        }
        }
    }

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
        stream << clang_getCursorSpelling(type_decl) << " " << name;
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
        std::string actual_name = clang_getCString(spelling);
        //stream << "__UNKNOWN__";
        stream << actual_name;
        if (!behind_pointer)
            stream << "*";
        stream << " " << name;
        //std::cout << "[ERROR] Failed to translate type '" << spelling << "' (" << clang_getTypeKindSpelling(type.kind) << ")\n";
        break;
    }
    }

    clang_disposeString(spelling);
}

bool CppToCheezGenerator::pass_type_by_pointer(const CXType& type) {
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

void CppToCheezGenerator::emit_param_name(std::ostream& stream, CXCursor cursor, int index) {
    CXString name = clang_getCursorSpelling(cursor);
    if (strlen(clang_getCString(name)) == 0) {
        stream << "_" << index;
    } else {
        stream << "_" << name;
    }
}

std::string CppToCheezGenerator::get_param_name(CXCursor cursor, int index) {
    std::stringstream stream;
    emit_param_name(stream, cursor, index);
    return stream.str();
}

void CppToCheezGenerator::emit_namespace(std::ostream& stream, size_t ns) {
    while (!clang_Cursor_isNull(m_namespaces[ns].declaration)) {
        auto n = clang_getCursorSpelling(m_namespaces[ns].declaration);
        stream << n << "::";
        clang_disposeString(n);
        ns = m_namespaces[ns].namespac;
    }
}

std::string CppToCheezGenerator::get_unique_function_name(CXString cxstr) {
    std::string str(clang_getCString(cxstr));

    auto found = m_duplicate_function_names.find(str);

    if (found == m_duplicate_function_names.end()) {
        m_duplicate_function_names.insert_or_assign(str, 1);
        return str;
    }
    else {
        int count = std::get<1>(*found) + 1;
        m_duplicate_function_names.insert_or_assign(str, count);

        std::stringstream ss;
        ss << str << "_" << count;
        return ss.str();
    }
}

void CppToCheezGenerator::indent(std::ostream& stream, int amount) {
    for (int i = 0; i < amount; i++) {
        stream << " ";
    }
}

void CppToCheezGenerator::reset() {
    m_cheez_type_decls.str(std::string());
    m_cheez_drop_impls.str(std::string());
    m_cheez_impls.str(std::string());
}