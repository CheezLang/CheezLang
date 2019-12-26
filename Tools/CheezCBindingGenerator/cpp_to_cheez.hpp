#pragma once

#include <sstream>
#include <string>
#include <unordered_map>
#include <clang-c/Index.h>

struct Declaration {
    CXCursor declaration;
    size_t namespac = 0;
};

class Context {
private:
    std::stringstream cheez_buffer, c_buffer;
    std::stringstream cheez_type_decl;
    std::stringstream cheez_drop_impl;
    std::stringstream cheez_impl;
    std::stringstream cheez_c_bindings;
    std::ostream* buffer = nullptr;
    uint64_t param_index = 0;
    uint64_t member_index = 0;
    bool no_includes = true;

    CXTranslationUnit tu;

    std::vector<Declaration> structs;
    std::vector<Declaration> enums;
    std::vector<Declaration> functions;
    std::vector<Declaration> typedefs;
    std::vector<Declaration> macros;
    std::vector<Declaration> variables;
    std::vector<Declaration> macro_expansions;
    std::vector<Declaration> namespaces;
    std::unordered_map<std::string, int> duplicateFunctionNames;

public:
    bool generate_bindings(const std::string& source_file_path, std::ostream& cheez_file, std::ostream& cpp_file);

private:
    void emit_function_decl(const Declaration& decl);
    void emit_variable_decl(const Declaration& decl);
    void emit_macro_expansion(const Declaration& decl);
    void emit_struct_decl(const Declaration& cursor);
    void emit_typedef_decl(CXCursor cursor);
    void emit_enum_decl(CXCursor cursor);
    void emit_macro(CXCursor cursor);
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
    void emit_parameter_default_value(std::ostream& stream, CXCursor c, CXToken* tokens, int num_tokens, int default_value_start, bool emit_equals);

    std::string get_unique_function_name(CXString cxstr) {
        std::string str(clang_getCString(cxstr));

        auto found = duplicateFunctionNames.find(str);

        if (found == duplicateFunctionNames.end()) {
            duplicateFunctionNames.insert_or_assign(str, 1);
            return str;
        }
        else {
            int count = std::get<1>(*found) + 1;
            duplicateFunctionNames.insert_or_assign(str, count);

            std::stringstream ss;
            ss << str << "_" << count;
            return ss.str();
        }
    }

    void indent(std::ostream& stream, int amount) {
        for (int i = 0; i < amount; i++) {
            stream << " ";
        }
    }

    void reset() {
        cheez_type_decl.str(std::string());
        cheez_drop_impl.str(std::string());
        cheez_impl.str(std::string());
    }
};
