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
#load("std:io/io.che")

fn Main() {
    println("Hello World.")
}
```

Generic dynamic array:
```rust
#load("std:mem/std_heap_allocator")
#load("std:io/io")

fn Main() {
    let ints: Array(int) = create_array()
    defer ints.dispose()

    ints.add(3)
    ints.add(2)
    ints.add(1)

    while let i: u32 = 0; i < ints.length; i += 1 {
        print_f("ints[{}] = {}`n", [i, ints.get(i)])
    }
}

fn create_array() -> Array($T) {
    let arr: Array(T)
    arr.init()
    return arr
}

struct Array(ElementType: type) {
    data: ElementType&
    length: uint
    capacity: uint
    allocator: Allocator
}

impl Array($ElementType) {
    ref fn init() {
        allocator = new StdHeapAllocator{}
        length = 0
        capacity = 10
        data = allocator.allocate((ulong)capacity, @sizeof(ElementType), @alignof(ElementType))
    }

    ref fn dispose() {
        allocator.free(data)
    }

    ref fn add(val: ElementType) {
        if capacity <= length {
            capacity = capacity * 2
            data = allocator.reallocate(data, (ulong)capacity, @sizeof(ElementType), @alignof(ElementType))
        }

        data[length] = val
        length += 1
    }

    ref fn get(index: uint) -> ElementType {
        return data[index]
    }
}
```

A fibonacci calculator, starting at index 0:
```rust
#load("std:io/io.che")

fn fib(x: int) -> int {
    if x <= 1 {
        return 1
    }
    return fib(x - 1) + fib(x - 2)
}

fn Main() {
    let x = fib(5)
    print_f("fib(5) = {}`n", [x])
}
```

Greatest common divisor:
```rust
#load("std:io/io.che")

// iterative implementation
fn gcd_it(a: int, b: int) -> int {
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
fn gcd_rec(a: int, b: int) -> int {
    if b == 0 {
        return a
    }

    return gcd_rec(b, a % b)
}

fn Main() {
    print_f("gcd_it(9, 6) = {}`n", [gcd_it(9, 6)])
    print_f("gcd_rec(9, 6) = {}`n", [gcd_rec(9, 6)])
}
```

Vectors and trait implementation:
```rust
#load("std:io/io.che")

struct Vec3 {
    x: float
    y: float
    z: float
}

impl Vec3 {
    // ref - pass self by reference instead of by value
    ref fn add(other: Vec3) -> Vec3 {
        return new Vec3 {
            x = self.x + other.x
            y = self.y + other.y
            z = self.z + other.z
        }
    }
}

impl Printable for Vec3 {
    // trait functions are ref by default
    fn print(str: String&, format: string) {
        sprint_f(str, "({}, {}, {})", [self.x, self.y, self.z])
    }
}

fn Main() {
    let a = new Vec3 { 1, 2, 3 }
    let b = new Vec3 { x = 4, y = 5, z = 6 }

    let c = a.add(b)

    print_f("
  {}
+ {}
  ------------------------------
= {}", [a, b, c])
}
```

## Run
Run the compiler using this command in Window Command Line:
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
