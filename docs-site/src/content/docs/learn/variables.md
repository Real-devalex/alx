---
title: "Variables"
description: "Store things with a name."
sidebar:
  order: 3
---

A **variable** is a name that holds a value, like a labelled box.

```alx
name = "ALEXION STUDIOS"
health = 100
speed = 5.5
alive = true

print(name)
print(health)
```

ALX has five basic kinds of value:

| Kind | Example | What it is |
| --- | --- | --- |
| integer | `100` | a whole number |
| float | `5.5` | a number with a decimal point |
| string | `"Hero"` | text |
| boolean | `true` or `false` | yes or no |
| null | `null` | nothing |

## Change a variable

```alx
health = 100
health = health - 25
print(health)   // 75
```

## Put values inside text

Wrap a variable in `{ }` inside a string:

```alx
name = "Daniel"
print("Hello, {name}!")
print("Two plus two is {2 + 2}")
```

## Constants

Use `const` for a value that must never change:

```alx
const MAX_HEALTH = 100
```
