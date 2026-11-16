---
title: "Classes"
description: "Build your own kinds of things."
sidebar:
  order: 8
---

A **class** is a blueprint. An **object** is something made from it.

```alx
class Player {
    function constructor(name, health) {
        this.name = name
        this.health = health
    }

    function takeDamage(amount) {
        this.health = this.health - amount
        if this.health < 0 {
            this.health = 0
        }
        print("{this.name} has {this.health} HP")
    }

    function isAlive() {
        return this.health > 0
    }
}

hero = new Player("Hero", 100)
hero.takeDamage(30)
print(hero.isAlive())
```

- `constructor` runs when you write `new Player(...)`.
- `this` means "this object".

## Inheritance

A class can build on another with `extends`.

```alx
class Zombie extends Player {
    function constructor(name, health, bite) {
        super(name, health)
        this.bite = bite
    }
}

z = new Zombie("Walker", 40, 8)
z.takeDamage(10)
```
