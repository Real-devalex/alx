---
title: "Game basics: math, vectors and entities"
description: "The built-in tools every game needs."
sidebar:
  order: 9
---

ALX 0.7.0 added a **game runtime**: math, random numbers, time and two ready classes. No `import` needed.

## Math

```alx
print(sqrt(144))              // 12
print(clamp(150, 0, 100))     // 100, never above 100
print(lerp(0, 100, 0.25))     // 25, a quarter of the way
print(floor(3.7))             // 3
print(abs(-5))                // 5
print(PI)
```

## Random

```alx
roll = randomInt(1, 6)        // a dice roll, 1 to 6
print("You rolled {roll}")

chance = random()             // a number from 0 up to 1
```

## Vectors

A **Vec2** is a point or a direction with an `x` and a `y`.

```alx
position = new Vec2(10, 20)
velocity = new Vec2(2, -1)

position = position.add(velocity.scale(5))
print(position.toString())    // (20, 15)
```

## Entities

An **Entity** is a thing in your game with a position and a speed.

```alx
hero = new Entity("Hero", 0, 0)
hero.velocity = new Vec2(4, 3)

hero.update(1)                      // move for 1 second
print(hero.position.toString())     // (4, 3)
```

## A tiny game loop

```alx
hero = new Entity("Hero", 0, 0)
hero.velocity = new Vec2(4, 3)
slime = new Entity("Slime", 20, 15)

step = 0
while step < 4 {
    hero.update(1)
    step = step + 1
    print("tick {step}: hero is {round(hero.distanceTo(slime))} away from the slime")
}
```

:::note[What is next]
Drawing on a real window comes with the **Alexion Engine** in 0.9.0. See the [roadmap](/alx/engine/roadmap/).
:::
