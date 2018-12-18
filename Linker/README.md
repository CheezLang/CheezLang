# LLVM Linker as DLL

This project wraps the LLVM linker library (lld) into a dll that gets called by the compiler.

## Build
To build this project, LLVM must be installed under '`D:\Program Files (x86)\LLVM`'. If it is not installed under this directory, you can changed the path in the project settings:
- AdditionalLibraryDirectories (`<path to llvm>\lib`)
- AdditionalIncludeDirectories (`<path to llvm>\include`)

If you don't want to build the dll yourself, there is a prebuilt one in the root directory of this project.

The compiler should work with that. If it doesn't that's a bug.
