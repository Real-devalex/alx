---
title: "Vec2 and Entity"
description: "The built-in vector and entity classes."
sidebar:
  order: 2
---

Both classes are written in ALX itself and loaded before every program.

## Vec2

```alx
v = new Vec2(3, 4)
```

| Member | What it does |
| --- | --- |
| `x`, `y` | The two numbers |
| `add(other)` | A new vector: this plus other |
| `sub(other)` | A new vector: this minus other |
| `scale(k)` | A new vector: both numbers times `k` |
| `dot(other)` | The dot product |
| `length()` | How long the vector is |
| `distanceTo(other)` | Distance between two points |
| `normalized()` | The same direction with length 1 (or `(0, 0)`) |
| `lerp(other, t)` | A point between the two vectors |
| `toString()` | Text like `(3, 4)` |

## Entity

```alx
hero = new Entity("Hero", 10, 20)
```

| Member | What it does |
| --- | --- |
| `name` | The entity's name |
| `position` | A `Vec2` |
| `velocity` | A `Vec2`, units per second |
| `alive` | `true` until you set it to `false` |
| `update(dt)` | Moves by `velocity * dt` |
| `moveTo(x, y)` | Jumps to a position |
| `distanceTo(other)` | Distance to another entity |
