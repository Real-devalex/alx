# ALX Language Specification

**Version:** 0.1.0
**Status:** Draft
**Last Updated:** September 2026

---

## 1. Overview

ALX (Alexion Language) is a dynamically-typed, interpreted programming language designed for general-purpose use with special emphasis on game development. ALX programs are executed locally on the developer's machine through an interpreter.

---

## 2. Lexical Structure

### 2.1 Source Files

ALX source files use the `.alx` extension.

### 2.2 Comments

Single-line comments begin with `//`:

```alx
// This is a comment
x = 42  // Inline comment
```

### 2.3 Identifiers

Identifiers start with a letter or underscore, followed by letters, digits, or underscores:

```
name
_private
myVar2
MAX_HEALTH
```

### 2.4 Keywords

| Keyword    | Description              |
|------------|--------------------------|
| `function` | Function declaration     |
| `if`       | Conditional branch       |
| `else`     | Alternative branch       |
| `while`    | While loop               |
| `for`      | For loop (planned)       |
| `in`       | Iterator (planned)       |
| `return`   | Return from function     |
| `true`     | Boolean true literal     |
| `false`    | Boolean false literal    |
| `null`     | Null literal             |
| `const`    | Constant declaration     |
| `and`      | Logical AND              |
| `or`       | Logical OR               |
| `not`      | Logical NOT              |
| `print`    | Print statement          |

---

## 3. Types

### 3.1 Primitive Types

| Type    | Example           | Description                     |
|---------|-------------------|---------------------------------|
| Integer | `42`, `-7`, `0`   | 64-bit signed integer          |
| Float   | `3.14`, `-0.5`    | 64-bit double-precision float  |
| String  | `"hello"`, `'hi'` | UTF-8 text string              |
| Boolean | `true`, `false`   | Boolean value                  |
| Null    | `null`            | Absence of value               |

### 3.2 Type Checking

ALX is dynamically typed. Variables can hold values of any type. Type checking is performed at runtime during operations.

```alx
x = 42
x = "hello"  // Valid — x is now a string
```

---

## 4. Operators

### 4.1 Arithmetic Operators

| Operator | Description | Example    |
|----------|-------------|------------|
| `+`      | Addition    | `10 + 5`   |
| `-`      | Subtraction | `10 - 5`   |
| `*`      | Multiplication | `10 * 5` |
| `/`      | Division    | `10 / 5`   |
| `%`      | Modulo      | `10 % 3`   |

### 4.2 Comparison Operators

| Operator | Description     | Example     |
|----------|-----------------|-------------|
| `==`     | Equal           | `x == y`    |
| `!=`     | Not equal       | `x != y`    |
| `>`      | Greater than    | `x > y`     |
| `<`      | Less than       | `x < y`     |
| `>=`     | Greater or equal| `x >= y`    |
| `<=`     | Less or equal   | `x <= y`    |

### 4.3 Logical Operators

| Operator | Description | Example         |
|----------|-------------|-----------------|
| `and`    | Logical AND | `a and b`       |
| `or`     | Logical OR  | `a or b`        |
| `not`    | Logical NOT | `not a`         |

### 4.4 Assignment Operator

| Operator | Description | Example |
|----------|-------------|---------|
| `=`      | Assignment  | `x = 5` |

### 4.5 Operator Precedence (Highest to Lowest)

1. `()` — Grouping
2. `-` (unary), `not` — Unary operators
3. `*`, `/`, `%` — Multiplicative
4. `+`, `-` — Additive
5. `<`, `>`, `<=`, `>=` — Comparison
6. `==`, `!=` — Equality
7. `and` — Logical AND
8. `or` — Logical OR
9. `=` — Assignment

---

## 5. Variables

### 5.1 Declaration

Variables are created by assignment:

```alx
name = "ALX"
version = 1
```

### 5.2 Reassignment

```alx
health = 100
health = 75
```

### 5.3 Constants

Constants cannot be reassigned:

```alx
const MAX_HEALTH = 100
MAX_HEALTH = 200  // Error: Cannot reassign constant
```

---

## 6. Expressions

### 6.1 Literals

```alx
42          // Integer
3.14        // Float
"hello"     // String
true        // Boolean
null        // Null
```

### 6.2 String Concatenation

Strings can be concatenated with `+`:

```alx
name = "ALX"
print("Hello, " + name + "!")  // Hello, ALX!
```

### 6.3 Function Calls

```alx
function greet(name) {
    print("Hello " + name)
}

greet("World")
```

---

## 7. Statements

### 7.1 Print Statement

Outputs a value to the console:

```alx
print("Hello")
print(42)
print(3.14)
print(true)
print(null)
```

### 7.2 If Statement

```alx
if condition {
    // code
}
```

### 7.3 If-Else Statement

```alx
if condition {
    // code
} else {
    // code
}
```

### 7.4 If-Else If-Else Statement

```alx
if condition1 {
    // code
} else if condition2 {
    // code
} else {
    // code
}
```

### 7.5 While Loop

```alx
while condition {
    // code
}
```

### 7.6 Function Declaration

```alx
function name(param1, param2) {
    return expression
}
```

### 7.7 Return Statement

```alx
function add(a, b) {
    return a + b
}
```

---

## 8. Scope

### 8.1 Global Scope

Variables defined at the top level are global:

```alx
x = 10
print(x)  // 10
```

### 8.2 Block Scope

Variables defined inside blocks (if, while, function) are local to that block:

```alx
x = 10
if true {
    x = 20  // Modifies the global x
}
print(x)  // 20
```

### 8.3 Function Scope

Function parameters and local variables exist only within the function:

```alx
function add(a, b) {
    return a + b
}
result = add(10, 20)
print(result)  // 30
```

---

## 9. Functions

### 9.1 Declaration

```alx
function functionName(parameter1, parameter2) {
    // body
}
```

### 9.2 Parameters

Functions accept zero or more parameters:

```alx
function noArgs() {
    print("no args")
}

function oneArg(x) {
    print(x)
}

function twoArgs(a, b) {
    print(a + b)
}
```

### 9.3 Return Values

Functions can return values:

```alx
function add(a, b) {
    return a + b
}

result = add(10, 20)
```

### 9.4 Recursion

Functions can call themselves:

```alx
function factorial(n) {
    if n <= 1 {
        return 1
    }
    return n * factorial(n - 1)
}

print(factorial(5))  // 120
```

---

## 10. Diagnostics

### 10.1 Error Codes

| Code     | Description                     |
|----------|---------------------------------|
| ALX1001  | Unexpected character            |
| ALX1002  | Unterminated string literal     |
| ALX1003  | Invalid number literal          |
| ALX2001  | Unexpected token                |
| ALX3001  | Undefined variable              |
| ALX3002  | Type mismatch                   |
| ALX3003  | Cannot call non-function        |
| ALX3004  | Wrong argument count            |
| ALX3005  | Return outside function         |
| ALX3006  | Cannot convert type             |

### 10.2 Error Format

```
ALX Error ALX3001

    hello.alx:4:12

    Undefined variable 'healthh'.

        print(healthh)
              ^^^^^^^
```

---

## 11. Reserved for Future Use

The following features are planned for future versions:

- **for loops** with ranges
- **Arrays** and **Maps**
- **Classes** and **Objects**
- **Modules** and **Imports**
- **String interpolation**: `"Hello, {name}"`
- **Lambda expressions**
- **Closures**
- **Error handling** with try/catch
- **Game development APIs**
- **Bytecode compilation**

---

*This document is a living specification and will be updated as ALX evolves.*
