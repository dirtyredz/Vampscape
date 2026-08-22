# Features — Vampscape

What the mod does. Status: ✅ shipped · 🚧 in progress · 💤 planned. All current features shipped as
of v1.0.2 ([mod 128 on Nexus](https://www.nexusmods.com/moonlightpeaks/mods/128)).

## Zoom
- ✅ **Hold-to-zoom while building** — hold the modifier (Left Alt default) and scroll to zoom the
  build-mode camera. `ScrollGate` + `BuildZoom`.
- ✅ **Wheel stays the wheel when released** — rotate/mirror and brush size (1–4) keep the bare
  wheel; only the modifier-held wheel is taken.
- ✅ **Per-mode zoom levels** — Decorate (3/4) and Floor (top-down) each remember their own level,
  across leaving/re-entering build mode; reset on quit.
- ✅ **Smooth glide** — lens is lerped toward the target, not snapped (`Damping`).
- ✅ **Pan bounds track the zoom** — the decoratable area's confiner is re-`Activate()`d as the zoom
  changes, so zooming out doesn't let the view drift past the area edge.

## Controller
- ✅ **Controller zoom, no modifier** — reads the game's existing Zoom axis (Rewired action 24),
  which nothing else reads during build mode. Joystick-only guard prevents double-counting the wheel.
- ✅ **Deadzone + speed tuning** — `ControllerDeadzone`, `ControllerZoomSpeed`.

## Configuration (Mod Menu + `.cfg`)
- ✅ **9 settings** — Enabled, Modifier, MinZoom, MaxZoom, ZoomStep, InvertZoom, ControllerZoom,
  ControllerZoomSpeed, ControllerDeadzone. Sectioned for Mod Menu via `ConfigDescription` tags.
- ✅ **Invert direction** — `InvertZoom` for players who want scroll-up to zoom out.

## Compatibility & safety
- ✅ **Complementary to Far Sight** — never active at the same time; two specific interactions
  handled (baseline-capture race, camera-reactivation clash). See [GOTCHAS.md](GOTCHAS.md).
- ✅ **Save-safe** — nothing written to the save; all state is in-memory and session-scoped.
- ✅ **Clean restore** — lens baselines restored on leaving build mode and on plugin destroy.

## Not planned
- ❌ General free-look / normal-play zoom — deliberately out of scope; Far Sight covers that.

_Living doc — refresh with /project-docs when it drifts._
