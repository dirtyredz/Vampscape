# Structure — Vampscape

Where things live in the code. For how the system works see
[docs/ARCHITECTURE.md](docs/ARCHITECTURE.md); for the human quick-start see [README.md](README.md).

Last full review: 2026-08-22

## Overview

A single BepInEx 5 / HarmonyX plugin DLL (netstandard2.1) that adds a build-mode camera zoom to
Moonlight Peaks. Five small source files, one namespace (`Vampscape`), no sub-namespaces — the
folders below group by responsibility only; C# namespaces are deliberately left flat. `Plugin.cs`
stays beside the `.csproj` at `src/` (BepInEx entry point). Writes nothing to the save.

## Layout

```
Vampscape/
├── src/
│   ├── Vampscape.csproj        # SDK-style; **/*.cs globs recursively, so folders need no edit
│   ├── Plugin.cs               # BepInEx entry point + static config/log holder (stays at root)
│   ├── game/                   # touches the live game: Harmony patches & game-state bridges
│   │   ├── ScrollGate.cs       #   Harmony postfix on Input.MouseScrollDelta
│   │   ├── DecorateWatch.cs    #   reads live PlayerView state — is build mode open?
│   │   └── BuildZoom.cs        #   MonoBehaviour driving GameCamera's Cinemachine lenses
│   └── core/                   # the mod's own logic, independent of game types
│       └── Hotkey.cs           #   hold-binding key check (input)
├── scripts/                    # repo git-hook installers (shell)
├── docs/                       # ARCHITECTURE / DECISIONS / FEATURES / ROADMAP / BACKLOG / GOTCHAS
├── screenshots/                # Nexus banner + thumbnail
└── pack.ps1                    # release archive packer (workspace-synced canonical)
```

There is no `src/ui/` — this mod draws nothing, it only bends the camera. No `tests/` either;
verification is the manual [TESTING.md](TESTING.md) checklist.

**Enforced homes:**

- `src/game/` — Harmony patches and live-game bridges
- `src/core/` — the mod's own logic, state, config and input handling
- `scripts/` — repo git-hook installers
- `src/Plugin.cs` — BepInEx entry point; must sit beside the `.csproj`
- `pack.ps1` — release archive packer; workspace-synced canonical, must stay at the repo root

## Architecture at a glance

```
VampscapePlugin (Awake)  ── wires up ──►  ScrollGate  (Harmony postfix on the wheel getter)
                                    └──►  BuildZoom   (MonoBehaviour: the per-frame zoom loop)

  both read ──►  DecorateWatch  (is build mode open? which mode?)   [leaf]
           ──►  Hotkey          (is the hold-binding down?)         [leaf]
           ──►  Plugin (static) (config entries + log)              [leaf]
```
One-directional: the two runtime pieces depend on the three leaves; the leaves depend on nothing
internal. No cycles.

## Components

### `VampscapePlugin` — entry point & lifecycle
- **Responsibility:** BepInEx entry. `Awake` binds config, patches `ScrollGate`, adds the
  `BuildZoom` component, logs the load line. `OnDestroy` unpatches.
- **Key file:** [src/Plugin.cs:9](src/Plugin.cs) (first type in the file).
- **Depends on:** `Plugin` (static), `ScrollGate`, `BuildZoom`, Harmony.
- **Seam:** add a new runtime piece by wiring it here.

### `Plugin` (static) — config & logging holder
- **Responsibility:** holds the 9 `ConfigEntry` fields + `ManualLogSource`, and `Bind()`s them.
  Kept off the plugin type so patches reach config without carrying a `BaseUnityPlugin` reference.
- **Key file:** [src/Plugin.cs:45](src/Plugin.cs) (second type in the file).
- **Used by:** everything. **Depends on:** BepInEx config only.
- **Seam:** add a setting → a new `config.Bind` in `Bind()` + a field.

### `BuildZoom` — the zoom itself (largest file)
- **Responsibility:** the per-frame `Update` loop. Reads zoom input (wheel + controller), glides
  the live camera's lens, keeps only the mode's camera active, refreshes the pan confiner, and
  restores baselines on exit. Several sub-concerns, all serving the one "zoom the build camera"
  feature (see [Structural debt](#structural-debt)).
- **Key file:** [src/game/BuildZoom.cs](src/game/BuildZoom.cs) (~335 lines).
- **Depends on:** `DecorateWatch`, `Hotkey`, `Plugin`, Cinemachine, Rewired, `GameCamera`.
- **Seam:** input mapping (`ReadZoomInput`/`ReadControllerZoom`), camera arbitration
  (`KeepModeCameraAlone`), and lens/baseline handling (`Apply`/`Restore`) are the three natural cut
  lines if it ever grows.

### `DecorateWatch` — build-mode state [leaf]
- **Responsibility:** answers "is build mode open?" and "Decorate or Floor?" by reading the live
  player state once per frame (cached). No latching — see [DECISIONS.md](docs/DECISIONS.md).
- **Key file:** [src/game/DecorateWatch.cs](src/game/DecorateWatch.cs). **Depends on:** `PlayerView`, game state types.

### `Hotkey` — hold-binding check [leaf]
- **Responsibility:** "is this `KeyboardShortcut` held right now?" without `KeyboardShortcut.IsPressed`'s
  modifier-block behaviour (which fails whenever any other key is down).
- **Key file:** [src/core/Hotkey.cs](src/core/Hotkey.cs). **Note:** intentionally copied verbatim across
  sibling mods (Transplant, Plant Peek) — a deliberate duplication, documented in its header, not debt.

### `ScrollGate` — wheel suppressor [leaf-ish]
- **Responsibility:** one Harmony postfix on `Input.MouseScrollDelta`'s getter; zeroes the wheel
  while the gate is open so rotate + both brush sizers don't also fire.
- **Key file:** [src/game/ScrollGate.cs](src/game/ScrollGate.cs). **Depends on:** `Plugin`, `DecorateWatch`, `Hotkey`.

## Key flows

- **Zoom:** `ScrollGate` zeroes the game's wheel read while the gate is open; `BuildZoom.Update`
  reads the real wheel the same frame and drives the lens. Order-independent by design.
- **Restore:** build mode closes → `BuildZoom.Restore()` writes cached baselines back.

Full sequences in [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md#key-flows).

## Conventions

- Code grouped by responsibility under `src/game/` and `src/core/` (see [Layout](#layout));
  `Plugin.cs` stays at `src/` root; docs + `pack.ps1` at mod root. One flat namespace throughout —
  moving a file never changes its namespace or any `using`.
- Version single-sourced from `<Version>` in [src/Vampscape.csproj](src/Vampscape.csproj) via
  `GenerateModBuildInfo` in [Directory.Build.props](Directory.Build.props); never hardcode it in
  `Plugin.cs`. `pack.ps1` and `Directory.Build.props` are **workspace-synced canonicals** — don't
  hand-edit here (regenerated by `../../tools/sync-mod-files.ps1`).
- Commit identity: `dirtyredz <dirtyredz@live.com>`.

## Where to find things

| Want to… | Go to |
|---|---|
| Add / change a setting | `Plugin.Bind()` in [src/Plugin.cs](src/Plugin.cs) |
| Change zoom feel (damping, warmup, clamps) | consts atop [src/game/BuildZoom.cs](src/game/BuildZoom.cs) |
| Change what suppresses the wheel | [src/game/ScrollGate.cs](src/game/ScrollGate.cs) |
| Change how build-mode state is detected | [src/game/DecorateWatch.cs](src/game/DecorateWatch.cs) |
| Add a referenced game assembly | `<ItemGroup>` in [src/Vampscape.csproj](src/Vampscape.csproj) |
| Manual test plan | [TESTING.md](TESTING.md) |
| Release / pack | [RELEASING.md](RELEASING.md), `pack.ps1` |

## Structural debt

Assessed 2026-08-22 by a full-depth review (componentization + abstraction lenses + Codex
cross-model). **The codebase is structurally sound for its size** — no God-files, no misplaced
logic, no dependency cycles, no wrong-direction deps. All findings are P2 polish; each is
backlogged in [docs/BACKLOG.md](docs/BACKLOG.md), none forced. Fixed in this pass: the repeated
NaN/Infinity guard, folded into a single `IsBad(float)` helper in `BuildZoom.cs`.

Remaining (all P2, deferred):
- **`BuildZoom.cs` bundles several sub-concerns** (input reading, camera-stack arbitration,
  lens/baseline override, confiner refresh) in one ~335-line MonoBehaviour. Under the 800-line cap
  and every part serves the single feature; each has one caller, so extraction now would be
  indirection for its own sake. Revisit only if the file grows or a sub-concern gains a 2nd caller.
- **Four parallel per-mode fields** (`decorateTarget/…/floorCurrent`) hand-swizzled by a `floor`
  bool → a small `ZoomState { Target, Current }` struct-per-mode. Marginal; fold in opportunistically.
- **`Plugin.cs` holds two types** (entry point + static config holder). Accepted as the composition
  root; split to a `PluginConfig.cs` only if either type grows.
- **`ScrollGate.ShouldSwallow` null-guards `Plugin.Enabled`** while `BuildZoom.Update` doesn't —
  the guard is currently unreachable (bind precedes both). Drop it or comment why.

_Living doc — refresh with /project-docs when it drifts._
