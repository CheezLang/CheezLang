.\count_loc.ps1 ..\..\ .\Compiler_all.csv *.cs -exclude TemporaryGeneratedFile*
.\count_loc.ps1 ..\..\CompilerLibrary .\CompilerLibrary.csv *.cs -exclude TemporaryGeneratedFile*
.\count_loc.ps1 ..\..\Backends\DummyBackend .\Backends_DummyBackend.csv *.cs -exclude TemporaryGeneratedFile*
.\count_loc.ps1 ..\..\Backends\LLVM\Linker .\Backends_LLVM_Linker.csv *.cs -exclude TemporaryGeneratedFile*
.\count_loc.ps1 ..\..\Backends\LLVM\LLLVMCS .\Backends_LLVM_LLLVMCS.csv *.cs -exclude TemporaryGeneratedFile*
.\count_loc.ps1 ..\..\Backends\LLVM\LLVMSharpBackend .\Backends_LLVM_LLVMSharpBackend.csv *.cs -exclude TemporaryGeneratedFile*
.\count_loc.ps1 ..\..\Backends\LLVM\LLVMWrapper\src .\Backends_LLVM_LLVMWrapper.csv  -exclude TemporaryGeneratedFile*
.\count_loc.ps1 ..\..\Backends\LLVM\TestNet .\Backends_LLVM_TestNet.csv *.cs -exclude TemporaryGeneratedFile*
.\count_loc.ps1 ..\..\CompilerCLI .\CompilerCLI.csv *.cs -exclude TemporaryGeneratedFile*
.\count_loc.ps1 ..\..\examples .\cheez_all.csv *.che
.\count_loc.ps1 ..\..\examples\examples .\cheez_examples.csv *.che
.\count_loc.ps1 ..\..\examples\libraries .\cheez_libraries.csv *.che
.\count_loc.ps1 ..\..\examples\libraries\compiler .\cheez_compiler.csv *.che
.\count_loc.ps1 ..\..\examples\std .\cheez_std.csv *.che
.\count_loc.ps1 ..\..\examples\tests .\cheez_tests.csv *.che
.\count_loc.ps1 ..\LanguageServer\VSCodeExtension\src .\Tools_LanguageServer_VSCodeExtension.csv *.ts
.\count_loc.ps1 ..\LanguageServer\Server .\Tools_LanguageServer_Server.csv *.cs -exclude TemporaryGeneratedFile*