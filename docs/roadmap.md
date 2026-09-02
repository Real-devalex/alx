# ALX Development Roadmap

**Last Updated:** September 2026

---

## Current Version: ALX 0.1.0 — Foundation ✅

### Status: COMPLETE

### Implemented

- **.NET Solution** — Professional project structure
- **CLI** — `alx <file.alx>`, `alx version`, `alx help`
- **Lexer** — Full tokenization with error recovery
- **Token System** — All token types for 0.1.0
- **Parser** — Recursive descent parser, operator precedence
- **AST** — Complete abstract syntax tree
- **Interpreter** — Tree-walking interpreter
- **Types** — String, Integer, Float, Boolean, Null
- **Variables** — Assignment, reassignment, constants
- **Expressions** — Arithmetic, comparisons, logical operators
- **print()** — Built-in statement
- **Conditionals** — if, else, else if
- **While Loops** — With proper scope handling
- **Functions** — Parameters, return values, recursion
- **Diagnostics** — Error codes, line/column info, helpful messages
- **Tests** — 103 automated tests (76 compiler, 27 integration)

### Test Results

```
Passed!  - Failed: 0, Passed: 103, Skipped: 0, Total: 103
```

---

## Next: ALX 0.2.0 — Enhanced Control Flow

### Planned

- **For Loops** — `for i in 1..10 { }`
- **Ranges** — `1..10`, `0..100`
- **Break / Continue** — Loop control
- **Enhanced Scope** — Block scoping improvements
- **String Interpolation** — `"Hello, {name}"` (if architecture supports it)
- **Better Diagnostics** — Suggestions, did-you-mean

---

## Future Milestones

### ALX 0.3.0 ✅ — Functions Improvements
- Closures, higher-order functions, lambda expressions
- afun keyword, expression-bodied functions

### ALX 0.4.0 ✅ — Collections
- Arrays: `[1, 2, 3]` with push/pop/contains/indexOf/join/reverse
- Maps: `{name: "Hero", health: 100}` with keys/values/containsKey/get
- Member & index assignment
- VS Code syntax highlighting extension

### ALX 0.5.0 — Objects
- Classes
- Objects
- Methods
- Properties
- Constructors
- Object references

### ALX 0.6.0 — Modules
- Imports
- Module system
- Standard library
- Package architecture

### ALX 0.7.0 — Game Runtime Foundation
- Vectors
- Math library
- Time
- Input
- Transform
- Entity architecture

### ALX 0.8.0 — Tooling
- Language Server
- Formatter
- Debugger
- Code completion
- Better diagnostics

### ALX 0.9.0 — Bytecode VM
- Bytecode compiler
- ALX Virtual Machine
- Performance optimization

### ALX 1.0.0 — Stable Release
- Stable syntax
- Stable runtime
- Standard library
- Tooling
- Documentation

---

## Long-Term Vision

```
ALEXION STUDIOS
       │
       ↓
     ALX LANGUAGE
       │
       ├─→ ALX STANDARD LIBRARY
       ├─→ ALX RUNTIME
       └─→ ALX STUDIO (IDE)
              │
              ↓
       ALEXION ENGINE
              │
              ↓
            🎮 GAMES
```

---

*This roadmap is a living document and will be updated as ALX evolves.*
