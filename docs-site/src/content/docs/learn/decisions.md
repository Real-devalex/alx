---
title: "Making decisions"
description: "Run code only when something is true."
sidebar:
  order: 4
---

Use `if` to choose what happens.

```alx
health = 0

if health <= 0 {
    print("Player died")
} else {
    print("Player is alive")
}
```

## Several choices

```alx
score = 85

if score >= 90 {
    print("Grade: A")
} else if score >= 80 {
    print("Grade: B")
} else {
    print("Keep going!")
}
```

## Comparing

| Symbol | Meaning |
| --- | --- |
| `==` | equal to |
| `!=` | not equal to |
| `<` `>` | less than, greater than |
| `<=` `>=` | less or equal, greater or equal |

## And, or, not

```alx
is_active = true
has_key = false

if is_active and has_key {
    print("Door opens")
} else if is_active or has_key {
    print("Something works")
}

if not has_key {
    print("Find a key")
}
```
