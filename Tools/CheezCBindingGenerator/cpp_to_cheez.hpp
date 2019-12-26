#pragma once

#include <sstream>
#include <string>
#include <unordered_map>
#include <unordered_set>

#include <clang-c/Index.h>
#include "lua\lua.hpp"

struct lua_State;

struct Declaration {
    CXCursor declaration;
    size_t namespac = 0;
};

class CppToCheezGenerator {
private:
    std::stringstream m_cheez_buffer, m_cpp_buffer;
    std::stringstream m_cheez_unknown_types;
    std::stringstream m_cheez_type_decls;
    std::stringstream m_cheez_drop_impls;
    std::stringstream m_cheez_impls;
    std::stringstream m_cheez_c_bindings;
    bool no_includes = true;

    CXTranslationUnit m_translation_unit;

    std::vector<Declaration> m_structs;
    std::vector<Declaration> m_unions;
    std::vector<Declaration> m_enums;
    std::vector<Declaration> m_functions;
    std::vector<Declaration> m_typedefs;
    std::vector<Declaration> m_macros;
    std::vector<Declaration> m_variables;
    std::vector<Declaration> m_macro_expansions;
    std::vector<Declaration> m_namespaces;
    std::unordered_map<std::string, int> m_duplicate_function_names;
    std::unordered_set<int> m_unknown_types;

    lua_State* lua_state;

public:
    // CppToCheezGenerator();
    ~CppToCheezGenerator();

public:
    bool set_custom_callbacks(const std::string& path);
    bool generate_bindings(const std::string& source_file_path, std::ostream& cheez_file, std::ostream& cpp_file);

private:
    void sort_stuff_into_lists(CXCursor tu, size_t namespac);
    void reset();

    void emit_function_decl(const Declaration& decl);
    void emit_variable_decl(const Declaration& decl);
    void emit_macro_expansion(const Declaration& decl);
    void emit_struct_decl(const Declaration& decl);
    void emit_union_decl(const Declaration& decl);
    void emit_typedef_decl(CXCursor cursor);
    void emit_enum_decl(CXCursor cursor);
    void emit_macro(CXCursor cursor);
    void emit_c_function_parameter_list(std::ostream& stream, CXCursor func, bool start_with_comma = false);
    void emit_c_function_argument_list(std::ostream& stream, CXCursor func, bool start_with_comma = false);
    void emit_cheez_function_parameter_list(std::ostream& stream, CXCursor func, bool start_with_comma = false, bool prefer_pointers = false);
    void emit_cheez_function_argument_list(std::ostream& stream, CXCursor func, bool start_with_comma = false);
    void emit_param_name(std::ostream& stream, CXCursor cursor, int index);
    void emit_cheez_type(std::ostream& stream, const CXType& type, bool is_func_param, bool behind_pointer = false, bool prefer_pointers = false);
    void emit_c_type(std::ostream& stream, const CXType& type, const char* name, bool is_func_param, bool behind_pointer = false);
    void emit_namespace(std::ostream& stream, size_t ns);
    void emit_parameter_default_value(std::ostream& stream, CXCursor c, CXToken* tokens, int num_tokens, int default_value_start, bool emit_equals);

    std::string get_unique_function_name(CXString cxstr);
    std::string get_param_name(CXCursor cursor, int index);
    bool pass_type_by_pointer(const CXType& type);

    void indent(std::ostream& stream, int amount);

    // lua bindings
    static const luaL_Reg lua_lib[4];
    bool call_custom_handler(const char* handler_name, CXCursor cursor, const char* decl_name, CXType decl_type);

    int to_cheez_string_lua(lua_State* L);
    int to_c_string_lua(lua_State* L);
    int get_size_lua(lua_State* L);
};
