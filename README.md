# Cheez Lang

Cheez is a small programming language I created in 2018. It's more on the low level side, inspired by Rust and uses LLVM as it's backend.

It is statically and strongly typed with C-like memory management, so no garbage collector. It doesn't use semicolons, has parametric polymorphism (I think that's what it's called), `defer`, __virtual functions__ using trait objects, slices, type inference and more.

Some features I plan to implement someday are lambdas, pattern matching, something like Rust's borrow checker but less restrictive and compile time code execution.

A detailed description of each feature can be found [here](https://github.com/Nimaoth/CheezLang/wiki) (early in progress).

The compiler is written in C#. I also wrote a Language Server, but I haven't maintained it so it's not even compiling right now. There is also a VSCode extension which provides basic syntax highlighting and access to the language server.

[You can download it from the releases page.](https://github.com/Nimaoth/CheezLang/releases)

## Examples

Here are some simple examples, more advanced examples can be found [here](https://github.com/Nimaoth/CheezLang/tree/release/examples/examples)

Here's what a Hello World program looks like:
```rust
io :: import std.io

Main :: () {
    io.println("Hello World.")
}
```

A fibonacci calculator, starting at index 0:
```rust
io :: import std.io

fib :: (x: int) -> int {
    if x <= 1 {
        return 1
    }
    return fib(x - 1) + fib(x - 2)
}

Main :: () {
    x := fib(5)
    io.formatln("fib(5) = {}", [x])
}
```

Greatest common divisor:
```rust
io :: import std.io

// iterative implementation
gcd_it :: (mut a: int, mut b: int) -> int {
    if a == 0 {
        return b
    }

    while b != 0 {
        if a > b {
            a = a - b
        } else {
            b = b - a
        }
    }

    return a
}

// recursive implementation
gcd_rec :: (a: int, b: int) -> int {
    if b == 0 {
        return a
    }

    return gcd_rec(b, a % b)
}

Main :: () {
    io.formatln("gcd_it(9, 6) = {}", [gcd_it(9, 6)])
    io.formatln("gcd_rec(9, 6) = {}", [gcd_rec(9, 6)])
}
```

Vectors and trait implementation:
```rust
use import std.string
use import std.printable

io  :: import std.io
fmt :: import std.fmt

Vec3 :: struct #copy {
    x : double
    y : double
    z : double
}

impl Vec3 {
    add :: (Self, other: Vec3) -> Vec3 #operator("+") {
        return Vec3(
            x = self.x + other.x
            y = self.y + other.y
            z = self.z + other.z
        )
    }
}

impl Printable for Vec3 {
    print :: (&Self, str: &mut String, format: string) {
        fmt.format_into(str, "({}, {}, {})", [self.x, self.y, self.z])
    }
}

Main :: () {
    a := Vec3(1, 2, 3)
    b := Vec3(x = 4, y = 5, z = 6)

    c := a + b

    io.formatln("
  {}
+ {}
  ------------------------------
= {}", [a, b, c])
}
```

Generic dynamic array:
```rust
mem :: import std.mem.allocator
fmt :: import std.fmt
io  :: import std.io

Main :: () {
    mut ints := Array[int].new()

    ints.add(3)
    ints.add(2)
    ints.add(1)

    for i in 0..ints.length {
        v := ints[i]
        io.formatln("ints[{}] = {}", [i, v])
    }
}

Array :: struct(T: type) {
    data     : ^mut T
    length   : int
    capacity : int
}

impl(T: type) Array[T] {
    new :: () -> Self {
        return Array[T](
            length   = 0
            capacity = 10
            data     = mem.alloc_raw(T, 10)
        )
    }

    add :: (&mut Self, val: int) {
        if capacity <= length {
            capacity = capacity * 2
            data = mem.realloc_raw(data, u64(capacity))
        }

        data[length] = val
        length += 1
    }

    get :: (&Self, index: int) -> int #operator("[]") {
        return self.data[index]
    }
}

impl(T: type) Drop for Array[T] {
    drop :: (&Self) {
        mem.free(self.data)
    }
}
```

## Run
Run the compiler using this command in Windows Command Line:
```bat
cheezc.exe test.che
test.exe
```
Powershell
```ps1
.\cheezc.exe .\test.che
.\test.exe
```

Compiler options:
| Option                    | Description                                                                                                                        |
| ------------------------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| -r, --run                 | (Default: false) Specifies whether the code should be run immediatly                                                               |
| -o, --out                 | (Default: .) Output directory: --out <directory>                                                                                   |
| --int                     | Intermediate directory: --int <directory>                                                                                          |
| -n, --name                | Name of the executable generated: <name>                                                                                           |
| --print-ast-raw           | Print the raw abstract syntax tree to a file: --print-ast-raw <filepath>                                                           |
| --print-ast-analysed      | Print the analysed abstract syntax tree to a file: --print-ast-analysed <filepath>                                                 |
| --no-code                 | (Default: false) Don't generate an executable                                                                                      |
| --no-errors               | (Default: false) Don't show error messages                                                                                         |
| --ld                      | Additional include directories: --ld [<path> [<path>]...]                                                                          |
| --libs                    | Additional Libraries to link to: --libs [<path> [<path>]...]                                                                       |
| --subsystem               | (Default: console) Sub system: --subsystem [windows                                                                                |console]
| --modules                 | Additional modules: --modules [<name>:<path> [<name>:<path>]...]                                                                   |
| --stdlib                  | Path to the standard library: --stdlib <path>                                                                                      |
| --opt                     | (Default: false) Perform optimizations: --opt                                                                                      |
| --emit-llvm-ir            | (Default: false) Output .ll file containing LLVM IR: --emit-llvm-ir                                                                |
| --time                    | (Default: false) Print how long the compilation takes: --time                                                                      |
| --test                    | (Default: false) Run the program as a test.                                                                                        |
| --trace-stack             | (Default: false) Enable stacktrace (potentially big impact on performance): --trace-stack                                          |
| --error-source            | (Default: false) When reporting an error, print the line which contains the error                                                  |
| --preload                 | Path to a .che file used to import by default                                                                                      |
| --print-linker-args       | (Default: false) Print arguments passed to linker                                                                                  |
| --language-server-tcp     | (Default: false) Launch language server over tcp                                                                                   |
| --port                    | (Default: 5007) Port to use for language server                                                                                    |
| --language-server-console | (Default: false) Launch language server over standard input/output                                                                 |
| --parent-pid              | (Default: -1) Process id of process launching this program. Used in language server mode to exit language server when parent exits |
| --help                    | Display this help screen.                                                                                                          |
| --version                 | Display version information.                                                                                                       |

