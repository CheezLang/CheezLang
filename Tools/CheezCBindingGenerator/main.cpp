#include <iostream>
#include <fstream>

#include "cpp_to_cheez.hpp"

enum class ArgumentIndices : int {
    InputFile = 1,
    OutputFile,
    COUNT
};

int main(int argc, char** argv) {
    if (argc != (int)ArgumentIndices::COUNT) {
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
    if (!ctx.generate_bindings(header_file_path, cheez_file, c_file))
        return 1;
    std::cout << "Generated bindings in '" << cheez_file_name.str() << "' and '" << c_file_name.str() << "'";

    cheez_file.flush();
    c_file.flush();
}