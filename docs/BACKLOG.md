# Backlog — Vampscape

Prioritized trough of deferred work and known issues. P0 = do next · P1 = should · P2 = nice-to-have.
Structural items seeded from the 2026-08-22 full-depth review (see
[STRUCTURE.md](../STRUCTURE.md#structural-debt)).

## P0
- _(none)_ — mod is shipped and stable at v1.0.2.

## P1
- _(none)_

## P2 — structural polish (all optional; none forced by the review)
- [ ] **`BuildZoom.cs` sub-concern extraction.** ~335-line MonoBehaviour bundling input reading,
      camera-stack arbitration (`KeepModeCameraAlone`/`Deactivate`), lens/baseline override
      (`Apply`/`Restore`/`Baseline`), and confiner refresh. Natural cut lines: a `ZoomInput` reader,
      a `BuildCameraOverride`/`CameraStack` type. **Deferred:** under the 800-line cap, every part
      serves one feature, each has one caller — extraction now is indirection for its own sake.
      Revisit only if the file grows or a sub-concern gains a second caller.
- [ ] **`ZoomState { Target, Current }` struct-per-mode.** Replace the four parallel
      `decorateTarget/decorateCurrent/floorTarget/floorCurrent` fields (hand-swizzled by a `floor`
      bool in `Update`) with two small structs. Marginal; fold in opportunistically the next time
      `BuildZoom.Update` is touched — don't spend a dedicated change on it.
- [ ] **Split `Plugin.cs` into entry point + `PluginConfig.cs`.** The file holds both
      `VampscapePlugin` and the static `Plugin` config/log holder. Accepted as the composition root;
      split only if either type grows.
- [ ] **Reconcile the `Plugin.Enabled != null` guard in `ScrollGate.ShouldSwallow`.**
      `BuildZoom.Update` dereferences `Plugin.Enabled.Value` with no guard; `Bind()` runs before
      both are reachable, so the `ScrollGate` guard is currently unreachable. Either drop it (matches
      `BuildZoom`) or add a one-line comment on why the postfix specifically guards pre-bind.

## Known issues / watch-list
- [ ] **Far Sight baseline-capture race** — mitigated by `WarmupFrames = 5`. If normal play returns
      at the wrong zoom after building with Far Sight installed, the warmup is too short
      ([GOTCHAS.md](GOTCHAS.md)). No repro currently.
- [ ] **No automated tests** — every path reads Unity/game types; verification is the manual
      [TESTING.md](../TESTING.md) checklist. Structural, not fixable without a game-mock layer;
      not worth it at this size.

_Living doc — refresh with /project-docs when it drifts._
