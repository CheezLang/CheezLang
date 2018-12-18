# Cheez Lang

Cheez is a small programming language I created in 2018. It's more on the low level side, inspired by Rust and uses LLVM as it's backend.

The compiler is written in C#. I also wrote a Language Server, but I haven't maintained it so it's not even compiling right now. There is also a VSCode extension which provides basic syntax highlighting and access to the language server.

You can download it from the releases page.

Heres what a Hello World program:
```rust
#load("std/io/io.che")

fn Main() {
    PrintString("Hello World.`n")
}
```

A fibonacci calculator, starting at index 0:
```rust
#load("std/io/io.che")

fn fib(x: int) -> int {
    if x <= 1 {
        return 1
    }
    return fib(x - 1) + fib(x - 2)
}

fn Main() {
    let x = fib(5)
    Printf("fib(5) = {}`n", [x])
}
```

## Syntax and Features

Cheez's syntax is inspired by rust, but it does not use semicolons.
Instead it uses a simle rule: if a statement/expression is not complete at the end of a line it continues at the next line, otherwise it ends at the end of this line.

### Comments
There are two kinds of comments in Cheez: 
#### Line Comments
```rust
// This is a comment
```
#### Block Comments
```rust
/*
    This is a block comment
    /* They can be nested */
*/
```

### Variable declaration
A variable can be declared using the `let` keyword. Variables can shadow previously declared variables, even with a different type.
```rust
let a = 1           // variable of type i32, inferred by the compiler
let b: i16 = 2      // variable of type i16
let c: bool         // variable of type bool

let a = (i16)a + b  // new variable a of type 16, shadows the previous a
```

### Types
These are the basic types:
- number types
  - `i8`, `i16`, `i32`, `i64`
  - `u8`, `u16`, `u32`, `u64`
  - `f32`, `t64`
- other
  - `string` - a C-style string
  - `bool` - 1 byte boolean
  - `char` - 1 byte character
  - `void`
  - `any`  - a up to 4 byte primitive type

`byte`, `short`, `int`, `long` are aliases for  `i8`, `i16`, `i32`, `i64`

`uyte`, `ushort`, `uint`, `ulong` are aliases for  `u8`, `u16`, `u32`, `u64`

`float` and `double` are aliases for `f32` and `f64`


#### String literals
String literals are surrounded by double quotes `"` and use backtick ` as escape character.
```rust
let str: string = "This is a string."
```

#### Number literals
```rust
-1
0
1

-1.2
0.0
2.6
```

#### Bool literals
```rust
true
false
```

#### Char literals
```rust
'x'
'3'
```

### 

## Examples
```rust
#load("std/io/io.che")

// Greatest common divisor
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

fn gcd_rec(a: int, b: int) -> int {
    if b == 0 {
        return a
    }

    return gcd_rec(b, a % b)
}

fn Main() {
    Printf("gcd_it(9, 6) = {}`n", [gcd_it(9, 6)])
    Printf("gcd_rec(9, 6) = {}`n", [gcd_rec(9, 6)])
}
```

```rust
#load("std/io/io.che")

struct Vec3 {
    x: float
    y: float
    z: float
}

impl Vec3 {
    ref fn add(other: Vec3) -> Vec3 {
        return new Vec3 {
            x = self.x + other.x
            y = self.y + other.y
            z = self.z + other.z
        }
    }
}

impl Printable for Vec3 {
    fn Print(str: String&, format: char[]) {
        Sprintf(str, "({}, {}, {})", [self.x, self.y, self.z])
    }
}

fn Main() {
    let a = new Vec3 { 1, 2, 3 }
    let b = new Vec3 { x = 4, y = 5, z = 6 }

    let c = a.add(b)

    Printf("
  {}
+ {}
  ------------------------------
= {}", [a, b, c])
}
```
