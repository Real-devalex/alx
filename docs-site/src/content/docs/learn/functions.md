---
title: "Functions"
description: "Name a piece of code and reuse it."
sidebar:
  order: 6
---

A **function** is a named set of instructions. You write it once and use it many times.

```alx
function greet(name) {
    print("Hello, {name}!")
}

greet("Daniel")
greet("Alexion")
```

## Giving a value back

```alx
function add(a, b) {
    return a + b
}

print(add(10, 20))   // 30
```

## Real game example

```alx
function damage(base, multiplier) {
    return base * multiplier
}

print(damage(50, 2.5))   // 125
```

`afun` is a shorter way to write `function`. Both do the same thing.

## Lambdas

A **lambda** is a small function with no name.

```alx
double = lambda(x) { x * 2 }
print(double(21))   // 42
```

## Closures

A function can remember variables from where it was made.

```alx
function makeCounter() {
    count = 0
    function next() {
        count = count + 1
        return count
    }
    return next
}

counter = makeCounter()
print(counter())   // 1
print(counter())   // 2
```
