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

This prints 1, 2, 3, 4 and 5. `1..5` means "from 1 to 5".

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
for i in 1..10 {
    total = total + i
}
print(total)   // 55
```
:::
