# Roadmap — Vampscape

A small, feature-complete mod. Status: **maintenance** — shipped and stable, no active feature work.

## Done
- ✅ **1.0.0** — hold-to-zoom while building; wheel preserved for rotate/brush; per-mode levels;
  confiner tracking; controller zoom; Far Sight compatibility.
- ✅ **1.0.1** — arm the zoom on the place-from-inventory build-mode entry path.
- ✅ **1.0.2** — live build-mode state read (no leak); re-assert only-mode-camera-active to fix the
  Far Sight camera-reactivation clash. Published as [mod 128](https://www.nexusmods.com/moonlightpeaks/mods/128).
- ✅ **2026-08-22** — onboarded to the structure-review gate + living-doc set; NaN/Infinity guard
  cleanup.

## In progress
- _(none)_

## Planned / if-warranted
- 💤 Structural polish items in [BACKLOG.md](BACKLOG.md) (all P2, opportunistic).
- 💤 React to game/Far Sight updates that change camera or decorate-state internals — this mod leans
  on several game-internal behaviours ([GOTCHAS.md](GOTCHAS.md)); a game patch could require a fix.

_Living doc — refresh with /project-docs when it drifts._
