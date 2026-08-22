# Architecture — Vampscape

How the system works. For the code-shape map see [STRUCTURE.md](../STRUCTURE.md); for the
human-facing explanation see [README.md](../README.md).

## System overview

Vampscape is a single BepInEx 5 plugin DLL that adds a build-mode camera zoom to Moonlight Peaks
(a Unity Mono game). It writes nothing to the save. The whole feature is: *while build mode is open
and a modifier is held, scroll the wheel to scale the live camera's lens instead of resizing the
brush / rotating the held object.*

The game gives us no zoom lever to turn up. `GameCamera` owns three fixed Cinemachine cameras — Far,
Close, TopDown — and build mode switches between two of them (`Decorate`→Far, `Floor`→TopDown) in
`PlayerDecorateStateMachine.SetMode()`. So "zoom" here means **scaling `m_Lens.OrthographicSize` and
`FieldOfView`** on whichever of those two cameras is live — the same lever the Far Sight mod pulls
during normal play.

## Runtime shape

Two runtime pieces, wired up in `VampscapePlugin.Awake()`:

1. **A Harmony postfix** (`ScrollGate`) on `Input.MouseScrollDelta`'s getter — the single one-line
   wrapper all three build-mode wheel consumers read through. It zeroes the wheel while the gate is
   open (mod on + build mode + modifier held), suppressing rotate and both brush sizers at once.
2. **A MonoBehaviour** (`BuildZoom`) added to the plugin GameObject — the per-frame `Update` loop
   that reads zoom input, glides the lens, refreshes the pan confiner, and restores lenses on exit.

Both consult two stateless helpers — `DecorateWatch` (is build mode open? which mode?) and `Hotkey`
(is the hold-binding down?) — and a static `Plugin` config/log holder.

## Data model

No persistence. All state is in-memory on the `BuildZoom` instance and resets when the game quits:

- **Per-mode zoom levels.** `Decorate` and `Floor` each keep their own `target`/`current` pair
  (the top-down and 3/4 framings want different zoom, so they don't share a number). They survive
  leaving/re-entering build mode because the MonoBehaviour lives for the whole session.
- **Lens baselines.** A `Dictionary<CinemachineVirtualCamera, Baseline>` caches each camera's
  original ortho size + FOV the first time it's zoomed, so `Restore()` can put them back exactly.
- **Per-frame cache.** `DecorateWatch` caches the live `PlayerDecorateStateMachine` once per frame
  (`ScrollGate` asks from inside a getter the game reads several times a frame).

## Key flows

**Wheel event while building, modifier held:**
```
game reads Input.MouseScrollDelta
  → ScrollGate.Postfix: gate open? → zero the wheel  (game sees nothing: no rotate, no brush)
BuildZoom.Update (same frame, order-independent):
  → gate open? read the real wheel delta → adjust target zoom
  → Lerp current→target → write lens ortho/FOV → re-run confiner Activate()
```

**Leaving build mode:** `DecorateWatch.IsDecorating` goes false → `BuildZoom.Restore()` writes every
cached baseline back → the normal-play camera returns to its prior framing.

**Controller:** no modifier needed (a stick has no brush-vs-zoom conflict). `BuildZoom` reads
Rewired action 24 (the game's Zoom control, unread during build mode) directly, only when the last
active controller is a joystick — otherwise the wheel would be counted twice.

## External interfaces

Everything is against the shipped game + BepInEx; no network, no files. Referenced game/engine
assemblies (see [src/Vampscape.csproj](../src/Vampscape.csproj)): `UnityEngine.*` (incl.
`InputLegacyModule` for `Input.GetKey`/`mouseScrollDelta`), `Cinemachine` (lens edits),
`Vampire.Runtime` (`GameCamera`, `PlayerView`, `PlayerDecorateStateMachine`),
`chicken-utilities.runtime` (`MonoBehaviourSingleton`), `Rewired_Core` (controller axis),
`BepInEx` + `0Harmony`. All `<Private>false</Private>` — the game ships its own copies.

## Design notes

- **Read live state, never latch.** Build-mode state is read from the player's current state every
  frame, not tracked via patch activate/deactivate counters — the game's `StateMachine.OnDeactivate`
  is empty, so a counter leaks a permanent "still decorating". See [DECISIONS.md](DECISIONS.md).
- **Recompute the gate every read.** Unity gives no `Update` ordering guarantee between this plugin
  and the game's state machines, so `ScrollGate.ShouldSwallow()` is recomputed on every call rather
  than latched once per frame.
- **Complementary to Far Sight, not conflicting.** Far Sight covers normal play and stands down in
  build mode. Two interactions with it are handled: a baseline-capture race (warmup frames) and a
  camera-reactivation clash (re-assert only-mode-camera-active). See [GOTCHAS.md](GOTCHAS.md).

_Living doc — refresh with /project-docs when it drifts._
