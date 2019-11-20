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
    io.printfln("fib(5) = {}", x)
}
```

Greatest common divisor:
```rust
io :: import std.io

// iterative implementation
gcd_it :: (a: int, b: int) -> int {
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
    io.printfln("gcd_it(9, 6) = {}", gcd_it(9, 6))
    io.printfln("gcd_rec(9, 6) = {}", gcd_rec(9, 6))
}
```

Vectors and trait implementation:
```rust
use import std.string
use import std.printable
io :: import std.io

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
    print :: (ref Self, str: ref String, format: string) {
        str.appendf("({}, {}, {})", (self.x, self.y, self.z))
    }
}

Main :: () {
    a := Vec3(1, 2, 3)
    b := Vec3(x = 4, y = 5, z = 6)

    c := a + b

    io.printfln("
  {}
+ {}
  ------------------------------
= {}", (a, b, c))
}
```

Generic dynamic array:
```rust
use import std.mem.allocator
io :: import std.io

Main :: () {
    ints := IntArray.create()

    ints.add(3)
    ints.add(2)
    ints.add(1)

    for i : 0..ints.length {
        v := ints[i]
        io.printfln("ints[{}] = {}", (i, v))
    }

    ints.dispose()
}

IntArray :: struct {
    data    : &int
    length  : int
    capacity: int
}

impl IntArray {
    create :: () -> Self {
        return IntArray(
            length   = 0
            capacity = 10
            data     = alloc_raw(int, 10)
        )
    }

    dispose :: (ref Self) {
        free(data)
    }

    add :: (ref Self, val: int) {
        if capacity <= length {
            capacity = capacity * 2
            data = realloc_raw(data, u64(capacity))
        }

        data[length] = val
        length += 1
    }

    get :: (ref Self, index: int) -> int #operator("[]") {
        return data[index]
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

- -help - display help screen
