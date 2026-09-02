# ALX — Alexion Language

**Version:** 0.2.0
**Status:** Enhanced Control Flow
**Platform:** Windows 10/11 x64 (initial)
**Technology:** C# / .NET 8

---

## What is ALX?

ALX (Alexion Language) is a **native, locally running programming language** designed and owned by [Alexion Studios](https://alexionstudios.com).

ALX is designed to be:

- **Easy to learn** — Clean, readable syntax
- **Strongly typed** — With type-safe operations
- **Locally executed** — No internet required, no web dependency
- **Game-ready** — Designed for eventual game development
- **Extensible** — Architecture supports growth

ALX is the foundation for the **Alexion Engine**, a future game-development environment.

---

## Documentation

**[📖 ALX Documentation Site](docs/site/index.html)**

- [Getting Started](docs/site/getting-started.html) — Installation & first program
- [Language Syntax](docs/site/syntax.html) — Complete language reference
- [Examples](docs/site/examples.html) — Runnable ALX programs
- [Roadmap](docs/site/roadmap.html) — Version history & future plans

---

## Quick Start

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later

### Build

```bash
dotnet build ALX.sln
```

### Run your first program

```bash
dotnet run --project src/ALX.CLI -- examples/hello.alx
```

Output:

```
Hello, ALEXION!
```

---

## ALX CLI

```bash
alx <file.alx>        # Run an ALX source file
alx version           # Show ALX version
alx help              # Show help
```

---

## Language Features (0.2.0)

### Hello World

```alx
print("Hello, ALEXION!")
```

### Variables

```alx
name = "ALEXION STUDIOS"
version = 1
health = 100
speed = 5.5
alive = true

print(name)     // ALEXION STUDIOS
print(version)  // 1
print(health)   // 100
print(speed)    // 5.5
print(alive)    // true
```

### Arithmetic

```alx
a = 10
b = 20
print(a + b)    // 30

// Operator precedence
print(10 + 5 * 2)   // 20 (not 30)
```

### String Interpolation (NEW in 0.2.0)

```alx
name = "ALX"
version = 2
print("Welcome to {name} {version}!")   // Welcome to ALX 2!
print("Sum: {10 + 5}")                 // Sum: 15
```

### Conditionals

```alx
health = 100

if health <= 0 {
    print("Player died")
} else if health > 50 {
    print("Healthy")
} else {
    print("Weak")
}
```

### For Loops with Ranges (NEW in 0.2.0)

```alx
// Basic for loop
for i in 1..5 {
    print(i)  // 1, 2, 3, 4
}

// Break — exit early
for i in 1..10 {
    if i == 5 { break }
    print(i)  // 1, 2, 3, 4
}

// Continue — skip iterations
for i in 1..6 {
    if i % 2 == 0 { continue }
    print(i)  // 1, 3, 5
}

// Sum with for loop
total = 0
for i in 1..10 {
    total = total + i
}
print(total)  // 45
```

### While Loops

```alx
x = 5
while x > 0 {
    x = x - 1
    print(x)
}
```

### Functions

ALX supports both `function` and the shorter `afun` keyword for declaring functions. Both are equivalent.

```alx
// Using afun (preferred, shorter)
afun greet(name) {
    print("Hello " + name + "!")
}

// Using function (still works for backward compatibility)
function greetLegacy(name) {
    print("Hello " + name + "!")
}

greet("World")        // Hello World!
greetLegacy("World")  // Hello World!

// Return values
afun add(a, b) {
    return a + b
}

result = add(10, 20)
print(result)   // 30

// Expression-bodied functions (simple case)
afun addOne(x) = x + 1

// Recursion
afun factorial(n) {
    if n <= 1 { return 1 }
    return n * factorial(n - 1)
}

print(factorial(5))   // 120
```

### Boolean Logic

```alx
print(true and false)   // false
print(true or false)    // true
print(not false)        // true
```

### Comments

```alx
// Single line comment
```

---

## Project Structure

```
ALX/
├── ALX.sln
├── src/
│   ├── ALX.Compiler/      # Lexer, Parser, AST, Diagnostics
│   ├── ALX.Runtime/        # Interpreter, Values, Environment
│   ├── ALX.CLI/            # Command-line interface
│   └── ALX.StandardLibrary/
├── tests/
│   ├── ALX.Compiler.Tests/    # Lexer & Parser tests
│   ├── ALX.Runtime.Tests/     # Runtime tests
│   └── ALX.Integration.Tests/ # End-to-end tests
├── examples/
│   ├── hello.alx
│   ├── variables.alx
│   ├── math.alx
│   ├── conditions.alx
│   ├── functions.alx
│   ├── loops.alx
│   ├── for_loops.alx          # NEW
│   └── string_interpolation.alx  # NEW
├── docs/
│   ├── site/                 # Documentation website
│   ├── language-spec.md
│   └── roadmap.md
├── build.ps1                # Windows build script
├── build.sh                 # Linux/Mac build script
└── README.md
```

---

## Architecture

```
ALX Source (.alx)
       ↓
    Lexer (Tokenization)
       ↓
    Tokens
       ↓
    Parser (Syntax Analysis)
       ↓
    AST (Abstract Syntax Tree)
       ↓
    Interpreter (Evaluation)
       ↓
    Output
```

---

## Testing

Run all tests:

```bash
dotnet test ALX.sln
```

Test results:

```
Total: 121 tests (87 compiler + 34 integration)
Status: All passing
```

---

## Roadmap

### ALX 0.1.0 ✅ — Foundation
### ALX 0.2.0 ✅ — Enhanced Control Flow
- For loops with ranges, break & continue, string interpolation

### ALX 0.3.0 ✅ — Functions Improvements
- Closures, lambdas, higher-order functions, `afun` keyword

### ALX 0.4.0 ✅ — Collections (Current)
- Arrays with indexing, push/pop/contains/indexOf/join/reverse
- Maps with dot access, indexing, keys/values/containsKey/get
- Member & index assignment (`player.health = 75`, `arr[0] = 99`)
- VS Code syntax highlighting extension

### ALX 0.5.0 — Objects
- Classes, methods, constructors

### ALX 1.0.0 — Stable Release

See the [full roadmap](docs/site/roadmap.html) for details.

---

## License

© 2026 Alexion Studios. All rights reserved.

---

*Built with 💜 by Alexion Studios*
