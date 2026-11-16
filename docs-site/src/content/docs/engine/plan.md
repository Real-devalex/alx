---
title: "Master plan"
description: "The decisions behind ALX and the Alexion Engine."
sidebar:
  order: 2
---

Agreed through a 50 question planning interview. This file is the source of truth. Update it when a decision changes.

## The idea in one paragraph

ALX is a **beginner-first programming language built to make games**. The Alexion Engine is the 2D game engine that runs it. A complete beginner should be able to open a window, move a character and make a sound in a few lines, and an expert should still find the power they need. The first goal is to ship real 2D games on Windows. Everything else follows from that.

## Golden rules

1. **Beginner first.** If a noob cannot understand it, it is too complicated. We make the language the owner built better, simpler and more organised, not different.
2. **Friendly always.** Errors show the line, point at the problem and suggest a fix.
3. **Finish small things.** Every release is playable or testable on the owner's PC.
4. **Safe delivery.** One pull request per release, CI must pass, the owner merges.

## Decisions

### Direction
| Topic | Decision |
| --- | --- |
| Purpose | Game language and engine for the owner's own games |
| First game type | 2D on PC |
| Graphics and window | Raylib through Raylib-cs (C#) |
| Platforms | Windows first, Linux and Mac later, keep code portable |
| Engine name | **Alexion Engine** (ALX is the language) |
| Licence | All rights reserved for now, open it later if wanted |

### Language
| Topic | Decision |
| --- | --- |
| Typing | Dynamic, with optional type hints such as `health: int = 100` |
| Syntax | Keep braces and the current style. Make it simpler and more consistent. Add friendly shortcuts (below) |
| Shortcuts | `repeat 5 times { }`, `when key pressed("space") { }`, `forever { }`, `elif` |
| Variables | Keep `score = 0`. Add optional `let` for explicit scope |
| Division | `10 / 4` is `2.5`. `10 // 4` is `2` |
| Text and numbers | `"Score: " + 10` just works |
| Errors | Friendly messages with the code line, an arrow and a suggested fix |
| Error handling | `try` and `catch` |
| Timers | `after 2 seconds { }` and `every 1 second { }` |
| Saving | Built in `save(name, data)` and `load(name)` |
| Libraries | Keep `import`. A standard library folder now, a package list much later |
| Speed | Stay on the tree-walking interpreter. Build a bytecode VM only when we measure a real slowdown |

### Engine (Alexion Engine)
| Topic | Decision |
| --- | --- |
| Game loop | The engine owns it and calls `update(dt)` and `draw()` |
| First features | Window, sprites, keyboard, collision, sound and music, sprite animation, tile maps |
| Extras for 0.9 | Gamepad, fullscreen and resizable windows, particle effects |
| Beginner API | Sprites first (`hero = Sprite("hero", 100, 100)`), draw commands for advanced users |
| No-art mode | Built in `Rect`, `Circle` and `Text` with colours |
| Structure | Simple objects a noob understands, with the `Entity` base from 0.7.0. No jargon like ECS in the beginner path |
| Physics | Simple built in: gravity, speed, box and circle collisions |
| Scenes | `changeScene("game")`. A menu scene, a game scene, a game over scene |
| UI | Built in text, buttons, health bars and menus |
| Levels | Maps made in the free Tiled editor, loaded with `Map("level1")` |
| Projects | `alx new mygame` creates `game.alx` and `assets/`. `alx run` plays. `alx build` makes a folder or exe players can double click |
| Templates | Blank, Platformer, Top-down adventure, Shooter, with free CC0 art and sound (Kenney style) |
| Hot reload | Save the file and the running game updates |
| Debugging | A debug overlay key now (FPS, positions, collision boxes). A step debugger later |
| Multiplayer | After single-player games ship: simple online rooms |
| 3D | After 2D games ship, using Raylib 3D, adding `Vec3` |
| Server mode | Later: headless `alx serve` with player events, in the spirit of SA-MP and FiveM scripting |

### Tools and delivery
| Topic | Decision |
| --- | --- |
| Editor help | VS Code extension now. Language Server right after friendly errors. ALX Studio (Avalonia, C#) later |
| Installing | Each release publishes a ready Windows zip. Run `install.ps1`. No .NET needed on the owner's PC |
| Delivery | One pull request per release, CI green, owner merges |
| Testing | A short test checklist per release. The owner replies with what worked or broke |
| Docs website | Astro Starlight, dark and purple with game-style touches. Sections: Learn, Reference, Examples, Changelog and roadmap. A devlog post per release. A browser playground later |

## Releases

### 0.7.0 Game runtime foundation (done, in review)
Math, random, time, `type()`, `Vec2`, `Entity`, tests, CI.

### 0.8.0 Language polish
1. Friendly error messages (line, arrow, did you mean, suggested fix)
2. Richer strings and arrays: `upper lower trim split replace startsWith endsWith`, `sort map filter find sum`
3. `try` and `catch`
4. Optional type hints with friendly warnings
5. Timers: `after` and `every`
6. `save` and `load`
7. Division `/` and `//`, `repeat`, `forever`, `elif`, optional `let`
8. Release zip built by GitHub, ready for `install.ps1`
9. Test checklist for the owner

### 0.9.0 Engine alpha
`alx new`, `alx run`, `alx build`. Window, `Rect`, `Circle`, `Text`, `Sprite`, keyboard and gamepad, fullscreen and resizing, collision and simple physics, sound and music, animation, tile maps (Tiled), scenes, UI, particles, hot reload, debug overlay. Proof game: **Pong**.

### 0.10.0 Templates and Language Server
Platformer, top-down and shooter templates with CC0 art. Language Server for VS Code. Proof game: **Coin Run** platformer.

### 0.11.0 Roleplay prototype
A small top-down roleplay prototype that proves maps, dialogue, inventory and saving, close to the owner's SA-MP and FiveM background.

### 0.12.0 and beyond
ALX Studio editor (Avalonia). Browser playground (ALX to WebAssembly). Bytecode VM when measured. Multiplayer rooms and `alx serve`. 3D. Android and web export. Package list.

## How each release works

1. I write a plan for the release and build it on a branch.
2. GitHub CI builds, runs every test and runs every example.
3. I open a pull request and give a short test checklist.
4. The owner installs the zip, runs the checklist and reports.
5. I fix what broke, the owner merges, and I tag the release and write a devlog post.

## Open risks

- Raylib-cs needs native libraries bundled in the Windows zip. Checked in 0.9.0 planning.
- The tree-walking interpreter may be slow for busy games. We measure on Pong and Coin Run before deciding on the VM.
- No .NET on the owner's PC means every change is verified by CI, not locally. We keep tests thorough.
