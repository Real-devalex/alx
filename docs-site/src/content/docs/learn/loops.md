---
title: "Loops"
description: "Repeat code without copying it."
sidebar:
  order: 5
---

## For loops with a range

```alx
for i in 1..5 {
    print(i)
}
```

This prints 1, 2, 3 and 4. A range like `1..5` starts at 1 and stops **before** 5, so to include 5 write `1..6`.

## While loops

A `while` loop repeats as long as something is true.

```alx
lives = 3
while lives > 0 {
    print("Lives left: {lives}")
    lives = lives - 1
}
```

## Break and continue

- `break` leaves the loop at once.
- `continue` skips to the next round.

```alx
for i in 1..10 {
    if i % 2 == 0 {
        continue      // skip even numbers
    }
    if i > 7 {
        break         // stop after 7
    }
    print(i)
}
```

:::tip[Adding up]
```alx
total = 0
for i in 1..11 {      // 1 up to 10
    total = total + i
}
print(total)   // 55
```
:::
