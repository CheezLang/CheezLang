#include <iostream>
#include <fstream>
#include <optional>
#include <filesystem>

#include "cpp_to_cheez.hpp"

enum class ArgumentIndices : int {
    InputFile = 1,
    OutputFile,
    COUNT
};

int main(int argc, char** argv) {
    const char* lua_file = "binding.lua";
    const char* out = ".";
    if (argc >= 2) {
        lua_file = argv[1];
    }

    if (argc >= 3) {
        out = argv[2];
    }

    auto lua_file_path = std::filesystem::path(lua_file);
    auto out_path = std::filesystem::path(out);

    auto lua_file_name = lua_file_path.filename().string();
    lua_file_name = lua_file_name.substr(0, lua_file_name.find_last_of("."));

    //std::stringstream cheez_file_name;
    //cheez_file_name << out_path/lua_file_name << ".che";
    //std::stringstream c_file_name;
    //c_file_name << out_path/lua_file_name << ".cpp";

    out_path.append(lua_file_name);
    auto cheez_file_path = out_path;
    auto c_file_path = out_path;

    cheez_file_path.concat(".che");
    c_file_path.concat(".cpp");

    std::ofstream cheez_file{ cheez_file_path, std::ios::binary };
    std::ofstream c_file{ c_file_path, std::ios::binary };

    if (!cheez_file || !c_file) {
        std::cerr << "Failed to create output files '" << cheez_file_path << "' and '" << c_file_path << "'\n";
        return 2;
    }

    CppToCheezGenerator ctx;
    if (!ctx.generate_bindings_from_lua(lua_file_path, cheez_file, c_file))
        return false;

    std::cout << "Generated bindings in '" << cheez_file_path << "' and '" << c_file_path << "'";

    cheez_file.flush();
    c_file.flush();
    //if (argc < (int)ArgumentIndices::COUNT) {
    //    std::cerr << "Wrong number of arguments\n";
    //    return 1;
    //}

    //auto header_file_path = argv[(int)ArgumentIndices::InputFile];
    //auto output_file_name = argv[(int)ArgumentIndices::OutputFile];

    //std::optional<std::string> lua_file;

    //for (int i = 3; i < argc; i++) {
    //    if (strcmp(argv[i], "-custom") == 0) {
    //        if (i >= argc - 1) {
    //            std::cerr << "[ERROR] Expected file path after -custom\n";
    //            exit(1);
    //        } else {
    //            ++i;
    //            lua_file = std::string(argv[i]);
    //        }
    //    }
    //}

    //std::stringstream cheez_file_name;
    //cheez_file_name << output_file_name << ".che";
    //std::stringstream c_file_name;
    //c_file_name << output_file_name << ".cpp";

    //std::ofstream cheez_file{ cheez_file_name.str() };
    //std::ofstream c_file{ c_file_name.str() };

    //if (!cheez_file || !c_file) {
    //    std::cerr << "Failed to create output files '" << cheez_file_name.str() << "' and '" << c_file_name.str() << "'\n";
    //    return 2;
    //}

    //CppToCheezGenerator ctx;
    //if (lua_file && !ctx.set_custom_callbacks(lua_file.value())) {
    //    return 1;
    //}
    //if (!ctx.generate_bindings(header_file_path, cheez_file, c_file))
    //    return 1;
    //std::cout << "Generated bindings in '" << cheez_file_name.str() << "' and '" << c_file_name.str() << "'";

    //cheez_file.flush();
    //c_file.flush();
}