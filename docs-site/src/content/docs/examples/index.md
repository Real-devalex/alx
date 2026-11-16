---
title: "Examples"
description: "Small programs to copy and change."
---

Every example here also lives in the [`examples` folder](https://github.com/Real-devalex/alx/tree/main/examples) and is run by the test pipeline on every change, so they always work.

## FizzBuzz

```alx
for i in 1..16 {
    if i % 15 == 0 {
        print("FizzBuzz")
    } else if i % 3 == 0 {
        print("Fizz")
    } else if i % 5 == 0 {
        print("Buzz")
    } else {
        print(i)
    }
}
```

## Dice game

```alx
player = randomInt(1, 6)
enemy = randomInt(1, 6)
print("You rolled {player}, the enemy rolled {enemy}")

if player > enemy {
    print("You win!")
} else if player == enemy {
    print("Draw")
} else {
    print("You lose")
}
```

## Battle

```alx
class Fighter {
    function constructor(name, health, power) {
        this.name = name
        this.health = health
        this.power = power
    }
    function hit(other) {
        other.health = other.health - this.power
        print("{this.name} hits {other.name}. {other.name} has {max(other.health, 0)} HP left")
    }
    function alive() { return this.health > 0 }
}

hero = new Fighter("Hero", 50, 12)
slime = new Fighter("Slime", 30, 5)

while hero.alive() and slime.alive() {
    hero.hit(slime)
    if slime.alive() { slime.hit(hero) }
}
```

## Moving entity

```alx
hero = new Entity("Hero", 0, 0)
hero.velocity = new Vec2(4, 3)
for tick in 1..4 {
    hero.update(1)
    print("tick {tick}: {hero.position.toString()}")
}
```
