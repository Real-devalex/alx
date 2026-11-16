---
title: "Built-in functions"
description: "Every function that comes with ALX."
sidebar:
  order: 1
---

These work in every program. No `import` needed.

## Output and input

| Function | What it does |
| --- | --- |
| `print(value)` | Shows a value on the screen |
| `input(prompt)` | Asks the player to type something and returns the text |
| `type(value)` | Returns `"integer"`, `"float"`, `"string"`, `"boolean"`, `"array"` and so on |

## Math

| Function | What it does |
| --- | --- |
| `sqrt(x)` | Square root |
| `pow(x, y)` | `x` to the power `y` |
| `abs(x)` | Positive version of `x` |
| `sign(x)` | `-1`, `0` or `1` |
| `floor(x)` `ceil(x)` `round(x)` | Round down, up or to the nearest whole number |
| `min(a, b)` `max(a, b)` | The smaller or larger of two numbers |
| `clamp(x, low, high)` | Keeps `x` between `low` and `high` |
| `lerp(a, b, t)` | A point between `a` and `b`. `t` 0 is `a`, `t` 1 is `b` |
| `sin(x)` `cos(x)` `tan(x)` `atan2(y, x)` | Angles in radians |
| `toRadians(deg)` `toDegrees(rad)` | Convert angles |
| `PI` `TAU` | Constants. `TAU` is `PI * 2` |

## Random and time

| Function | What it does |
| --- | --- |
| `random()` | A float from 0 up to (not including) 1 |
| `randomInt(low, high)` | A whole number from `low` to `high`, both included |
| `time()` | Seconds since 1970, as a float. Subtract two times to measure |
| `sleep(ms)` | Waits for `ms` milliseconds (up to 10000) |

## Arrays

`length`, `push(x)`, `pop()`, `first`, `last`, `contains(x)`, `indexOf(x)`, `join(separator)`, `reverse()`.

## Maps

`length`, `keys()`, `values()`, `containsKey(key)`, `get(key, default)`.
