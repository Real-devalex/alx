# ALX and Alexion Engine: master plan

Agreed through a 50 question planning interview. This file is the source of truth. Update it when a decision changes.

## The idea in one paragraph

ALX is a **beginner-first programming language built to make games**. The Alexion Engine is the 2D game engine that runs it. A complete beginner should be able to open a window, move a character and make a sound in a few lines, and an expert should still find the power they need. The first goal is to ship real 2D games on Windows. The long goal is a "Unity, but easy" experience: an editor you press Play in, with 3D, built for people who have never made a game.

## Direction chosen: Raylib, ALX Studio and 3D

Three ways to build the engine were compared: **A** Raylib plus our own editor, **B** build on an existing engine such as Stride, **C** everything from scratch. The owner chose **A, with ALX Studio built earlier and 3D designed in from the start**.

- We do not try to match every Unity feature. We win on being easy.
- The "easy" feeling comes from the **editor**: drag things in, press Play, change a value while the game runs.
- 3D is **designed in from day one** (a 3D-ready `Transform`, `Camera` and renderer interface) but **built after 2D works**.
- We use proven libraries for hard parts (Raylib for drawing, a mature library such as Jolt or Bepu for 3D physics, glTF for 3D models) instead of writing them.

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
| Ranges | `1..5` includes 5 (decided). Loops count 1, 2, 3, 4, 5 |
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
| 3D | Designed in from day one, built after the 2D alpha: `Vec3`, cameras, glTF models, basic lighting, 3D physics through a proven library |
| Server mode | Later: headless `alx serve` with player events, in the spirit of SA-MP and FiveM scripting |

### Tools and delivery
| Topic | Decision |
| --- | --- |
| Editor help | VS Code extension now. Language Server soon. **ALX Studio** (Avalonia, C#): scene view, inspector, Play button, live value editing. Built right after the 2D engine alpha |
| Installing | Each release publishes a ready Windows zip. Run `install.ps1`. No .NET needed on the owner's PC |
| Delivery | One pull request per release, CI green, owner merges |
| Testing | A short test checklist per release. The owner replies with what worked or broke |
| Docs website | Astro Starlight, dark and purple with game-style touches. Sections: Learn, Reference, Examples, Changelog and roadmap. A devlog post per release. A browser playground later |

## Releases

### 0.7.0 Game runtime foundation (done)
Math, random, time, `type()`, `Vec2`, `Entity`, tests, CI, docs website, VS Code extension.

### 0.8.0 Language polish
1. **Ranges include the last number** (`1..5` counts to 5)
2. Friendly error messages (line, arrow, did you mean, suggested fix)
3. Richer strings and arrays: `upper lower trim split replace startsWith endsWith`, `sort map filter find sum`
4. `try` and `catch`
5. Optional type hints with friendly warnings
6. Timers: `after` and `every`
7. `save` and `load`
8. Division `/` and `//`, `repeat`, `forever`, `elif`, optional `let`
9. Release zip built by GitHub, ready for `install.ps1`, no .NET needed
10. Test checklist for the owner

### 0.9.0 Engine alpha (2D)
`alx new`, `alx run`, `alx build`. Window, `Rect`, `Circle`, `Text`, `Sprite`, keyboard and gamepad, fullscreen and resizing, collision and simple physics, sound and music, animation, tile maps (Tiled), scenes, UI, particles, hot reload, debug overlay. The renderer sits behind an interface that can draw 3D later. Proof game: **Pong**.

### 0.10.0 ALX Studio alpha
The editor: project browser, scene view you can click and drag in, inspector for values, Play and Stop, live editing while the game runs, console. Language Server for VS Code. Proof game: **Coin Run** platformer, with templates and CC0 art.

### 0.11.0 ALX Studio and templates
Prefabs (reusable objects), a tile map editor, an animation editor, platformer, top-down and shooter templates, project export from the editor.

### 0.12.0 3D foundations
`Vec3`, 3D `Transform`, `Camera3D`, glTF model loading, basic lighting and shadows, a 3D scene view in Studio, 3D collision and physics through a proven library. Proof: a small 3D scene you can walk around in.

### 0.13.0 Roleplay prototype
A small top-down (then 3D) roleplay prototype that proves maps, dialogue, inventory and saving, close to the owner's SA-MP and FiveM background.

### 0.14.0 and beyond
Browser playground (ALX to WebAssembly). Bytecode VM when measured. Multiplayer rooms and `alx serve`. Android and web export. Package list. Visual scripting for people who do not want to type.

## How each release works

1. I write a plan for the release and build it on a branch.
2. GitHub CI builds, runs every test and runs every example.
3. I open a pull request and give a short test checklist.
4. The owner installs the zip, runs the checklist and reports.
5. I fix what broke, the owner merges, and I tag the release and write a devlog post.

## Open risks

- Raylib-cs needs native libraries bundled in the Windows zip. Checked in 0.9.0 planning.
- An editor is a big product. Studio starts small (scene view, inspector, Play) and grows a feature at a time.
- 3D multiplies the work (models, lighting, physics, editor tools). It comes after 2D ships, and it uses proven libraries.
- The tree-walking interpreter may be slow for busy games. We measure on Pong and Coin Run before deciding on the VM.
- No .NET on the owner's PC means every change is verified by CI, not locally. We keep tests thorough.
