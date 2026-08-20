# Vampscape

Zoom the camera out while you are building. Hold **Left Alt** and scroll.

Far Sight already covers zooming during normal play, and deliberately stands down in build mode.
This fills that gap and nothing else — it is scoped to build mode specifically, not a general
zoom mod.

## The problem

Build mode has no zoom, and the wheel is already taken.

`GameCamera` does not have a zoom level to turn up. It owns three fixed Cinemachine cameras —
Far, Close and TopDown — and build mode picks between two of them in
`PlayerDecorateStateMachine.SetMode()`: `Decorate` gets Far, `Floor` gets TopDown. During normal
play the wheel just toggles Far↔Close, but build mode sets `isCameraToggleEnabled` to false, so
even that is off. Meanwhile three separate things read the wheel while you build:

| Consumer | What it does | When |
|---|---|---|
| `GridObjectHelper.TryRotateGridObject` | rotate / mirror the held object | object held, rotatable |
| `DecorateMoveObjectState.ProcessBrush` | brush size 1–4 | object held, paths and floors only |
| `DecorateSelectState.ProcessBrush` | brush size with nothing in hand | nothing held |

So the wheel cannot simply be taken over — rotating is used constantly, and retraining it would
be a worse mod than no mod.

## How it works

**The modifier.** All three consumers above read the same one-line wrapper,
`Input.MouseScrollDelta`, which is just `UnityEngine.Input.mouseScrollDelta`. A single Harmony
postfix zeroes it while the modifier is down, which suppresses rotate and both brushes at once.
Let go of the modifier and the wheel does exactly what it always did.

That property is also what the calendar, the menus and the quantity popup scroll with, so the
gate is kept as narrow as it can be: build mode open, modifier down, mod switched on. It is
recomputed on every read rather than latched once per frame, because Unity gives no ordering
guarantee between this plugin's `Update` and the game's state machines.

**The zoom.** Scaling `m_Lens.FieldOfView` and `OrthographicSize` on whichever camera is live —
the same lever Far Sight pulls. Decorate mode and Floor mode each keep their own level, since a
top-down floor view and a 3/4 decorate view want quite different framings. Both survive leaving
and re-entering build mode, and reset when you quit the game.

**The confiner.** Outside build mode, `GameCamera.ProcessCameraBounds()` recomputes the camera's
pan bounds every frame from live viewport rays, so a wider lens automatically tightens how far
the camera may travel. Build mode swaps that out for the decoratable area's own confiner via
`OverrideConfiner()`, and `DecorateCameraConfiner` only recomputes inside `Activate()` — once, on
entry. Zoom after that and the bounds still describe the old lens, which lets the camera pan past
the edge of the area. So this re-runs `Activate()` whenever the zoom moves.

**Controller.** Rewired action 24 is the game's Zoom control, and nothing reads it during build
mode, so it is free to borrow — no modifier needed, because a controller has no brush-vs-zoom
conflict to resolve. It is only read when the last active controller is a joystick: on keyboard
and mouse that same action is bound to the wheel, and without the check every tick would count
twice.

## Compatibility

Far Sight is not just compatible, it is complementary — its `IsGameplay()` returns false on
`PlayerDecorateStateMachine`, so the two are never active at the same time. There is one race
worth knowing about: Far Sight restores its lenses when it stands down, on the same frame build
mode opens, and Unity does not order the two plugins' `Update` calls. Capturing a baseline on that
frame could capture Far Sight's zoomed value and treat it as the default, so this waits a few
frames first. The camera is blending over that window anyway.

Nothing is written to your save.

## Settings

Configurable in-game through Mod Menu, or in
`BepInEx/config/com.dirtyredz.moonlightpeaks.vampscape.cfg`.

| Setting | Default | What it does |
|---|---|---|
| `Enabled` | `true` | Off leaves the wheel entirely to the game |
| `Modifier` | `LeftAlt` | Hold this and scroll to zoom |
| `MinZoom` | `0.6` | Closest zoom, as a fraction of the game's own view |
| `MaxZoom` | `2.2` | Farthest zoom, as a multiple of the game's own view |
| `ZoomStep` | `0.12` | How much one scroll tick changes the zoom |
| `InvertZoom` | `false` | Flip the direction, so scrolling up zooms out |
| `ControllerZoom` | `true` | Zoom with a controller, no modifier needed |
| `ControllerZoomSpeed` | `1.6` | A stick is continuous, unlike a scroll tick |
| `ControllerDeadzone` | `0.2` | How far the stick must move before it zooms |

Scrolling up zooms *in* by default, matching Far Sight — on the reasoning that anyone bothered
enough by the build-mode camera to install this already has that one in their hands. That is the
opposite of what `GameCamera`'s own Far/Close toggle does with a positive Zoom axis, so the
preference is genuinely contested; `InvertZoom` is there for the other half.

## Building

```bash
dotnet build src/Vampscape.csproj
```

Deploys to `BepInEx/plugins/MoonlightPeaksMods/Vampscape` automatically. `pack.ps1` builds the
release archive. See [TESTING.md](TESTING.md) for what to check in game.
