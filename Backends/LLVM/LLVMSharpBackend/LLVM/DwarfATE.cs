using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cheez.CodeGeneration.LLVMCodeGen
{
    public static class Dwarf
    {
        // DWARF attribute type encodings.
        public const uint ATE_address           = 0x01;
        public const uint ATE_boolean           = 0x02;
        public const uint ATE_complex_float     = 0x03;
        public const uint ATE_float             = 0x04;
        public const uint ATE_signed            = 0x05;
        public const uint ATE_signed_char       = 0x06;
        public const uint ATE_unsigned          = 0x07;
        public const uint ATE_unsigned_char     = 0x08;
        // New in DWARF v3:
        public const uint ATE_imaginary_float   = 0x09;
        public const uint ATE_packed_decimal    = 0x0a;
        public const uint ATE_numeric_string    = 0x0b;
        public const uint ATE_edited            = 0x0c;
        public const uint ATE_signed_fixed      = 0x0d;
        public const uint ATE_unsigned_fixed    = 0x0e;
        public const uint ATE_decimal_float     = 0x0f;
        // New in DWARF v4:
        public const uint ATE_UTF               = 0x10;
        // New in DWARF v5:
        public const uint ATE_UCS               = 0x11;
        public const uint ATE_ASCII             = 0x12;


        // DWARF languages.
        public const uint LANG_C89 = 0x0001;
        public const uint LANG_C = 0x0002;
        public const uint LANG_Ada83 = 0x0003;
        public const uint LANG_C_plus_plus = 0x0004;
        public const uint LANG_Cobol74 = 0x0005;
        public const uint LANG_Cobol85 = 0x0006;
        public const uint LANG_Fortran77 = 0x0007;
        public const uint LANG_Fortran90 = 0x0008;
        public const uint LANG_Pascal83 = 0x0009;
        public const uint LANG_Modula2 = 0x000a;
        // New in DWARF v3:
        public const uint LANG_Java = 0x000b;
        public const uint LANG_C99 = 0x000c;
        public const uint LANG_Ada95 = 0x000d;
        public const uint LANG_Fortran95 = 0x000e;
        public const uint LANG_PLI = 0x000f;
        public const uint LANG_ObjC = 0x0010;
        public const uint LANG_ObjC_plus_plus = 0x0011;
        public const uint LANG_UPC = 0x0012;
        public const uint LANG_D = 0x0013;
        // New in DWARF v4:
        public const uint LANG_Python = 0x0014;
        // New in DWARF v5:
        public const uint LANG_OpenCL = 0x0015;
        public const uint LANG_Go = 0x0016;
        public const uint LANG_Modula3 = 0x0017;
        public const uint LANG_Haskell = 0x0018;
        public const uint LANG_C_plus_plus_03 = 0x0019;
        public const uint LANG_C_plus_plus_11 = 0x001a;
        public const uint LANG_OCaml = 0x001b;
        public const uint LANG_Rust = 0x001c;
        public const uint LANG_C11 = 0x001d;
        public const uint LANG_Swift = 0x001e;
        public const uint LANG_Julia = 0x001f;
        public const uint LANG_Dylan = 0x0020;
        public const uint LANG_C_plus_plus_14 = 0x0021;
        public const uint LANG_Fortran03 = 0x0022;
        public const uint LANG_Fortran08 = 0x0023;
        public const uint LANG_RenderScript = 0x0024;
        public const uint LANG_BLISS = 0x0025;
        // Vendor extensions:
        public const uint LANG_Mips_Assembler = 0x8001;
        public const uint LANG_GOOGLE_RenderScript = 0x8e57;
        public const uint LANG_BORLAND_Delphi = 0xb000;
    }
}
