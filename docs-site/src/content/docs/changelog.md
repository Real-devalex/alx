---
title: "Changelog"
description: "What changed in each version."
---

## Next, 0.8.0

- Ranges include their last number: `1..5` counts 1, 2, 3, 4, 5

## 0.7.0, game runtime

- Math: `sqrt`, `pow`, `sin`, `cos`, `tan`, `atan2`, `floor`, `ceil`, `round`, `abs`, `sign`, `min`, `max`, `clamp`, `lerp`, `toRadians`, `toDegrees`, `PI`, `TAU`
- Random: `random()`, `randomInt(low, high)`
- Time: `time()`, `sleep(ms)`. Types: `type(value)`
- `Vec2` and `Entity` classes built in
- CI pipeline: build, test and run every example on each change
- New docs website

## 0.6.0, modules

- `import module` and `import module as alias`, loading modules from files

## 0.5.0, objects

- Classes, constructors, methods, properties and inheritance with `extends` and `super`

## 0.4.0, collections

- Arrays and maps with methods, member and index assignment, VS Code extension

## 0.3.0, functions

- Closures, lambdas, higher-order functions, `afun`

## 0.2.0, control flow

- `for` loops with ranges, `break`, `continue`, string interpolation

## 0.1.0, foundation

- Lexer, parser, tree-walking interpreter, variables, functions, `if`, `while`, diagnostics
