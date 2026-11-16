---
title: "Arrays and maps"
description: "Hold many values together."
sidebar:
  order: 7
---

## Arrays

An **array** is an ordered list.

```alx
fruits = ["apple", "banana", "cherry"]

print(fruits[0])        // apple
print(fruits.length)    // 3

fruits.push("mango")    // add to the end
last = fruits.pop()     // remove from the end
print(fruits.contains("banana"))   // true
print(fruits.indexOf("cherry"))    // 2
print(fruits.join(", "))
```

Counting starts at **0**, so the first item is `fruits[0]`.

## Maps

A **map** stores values by name.

```alx
player = {
    name: "Hero",
    health: 100,
    speed: 5.5
}

print(player.name)
print(player["health"])
player.health = 80
print(player.keys())
print(player.containsKey("mana"))   // false
print(player.get("mana", 0))        // 0, a default
```

## Loop over an array

```alx
scores = [10, 20, 30]
total = 0
for i in 0..3 {
    total = total + scores[i]
}
print(total)   // 60
```
