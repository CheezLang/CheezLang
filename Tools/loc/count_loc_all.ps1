.\count_loc.ps1 ..\..\CompilerLibrary .\CompilerLibrary.csv *.cs -exclude TemporaryGeneratedFile*
.\count_loc.ps1 ..\..\Backends\DummyBackend .\Backends_DummyBackend.csv *.cs -exclude TemporaryGeneratedFile*
.\count_loc.ps1 ..\..\Backends\LLVM\Linker .\Backends_LLVM_Linker.csv *.cs -exclude TemporaryGeneratedFile*
.\count_loc.ps1 ..\..\Backends\LLVM\LLLVMCS .\Backends_LLVM_LLLVMCS.csv *.cs -exclude TemporaryGeneratedFile*
.\count_loc.ps1 ..\..\Backends\LLVM\LLVMSharpBackend .\Backends_LLVM_LLVMSharpBackend.csv *.cs -exclude TemporaryGeneratedFile*
.\count_loc.ps1 ..\..\Backends\LLVM\LLVMWrapper\src .\Backends_LLVM_LLVMWrapper.csv  -exclude TemporaryGeneratedFile*
.\count_loc.ps1 ..\..\Backends\LLVM\TestNet .\Backends_LLVM_TestNet.csv *.cs -exclude TemporaryGeneratedFile*
.\count_loc.ps1 ..\..\CompilerCLI .\CompilerCLI.csv *.cs -exclude TemporaryGeneratedFile*
.\count_loc.ps1 ..\..\examples .\examples.csv *.che
.\count_loc.ps1 ..\LanguageServer\VSCodeExtension\src .\Tools_LanguageServer_VSCodeExtension.csv *.ts
.\count_loc.ps1 ..\LanguageServer\Server .\Tools_LanguageServer_Server.csv *.cs -exclude TemporaryGeneratedFile*