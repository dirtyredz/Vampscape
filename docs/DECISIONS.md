# Decisions — Vampscape

Design decisions worth not re-litigating. Newest first. See also
[STRUCTURE.md](../STRUCTURE.md) and [ARCHITECTURE.md](ARCHITECTURE.md).

## 2026-08-22 — Fold the repeated NaN/Infinity guard into one `IsBad(float)` helper
**Why:** `float.IsNaN(x) || float.IsInfinity(x)` was written out five times in `BuildZoom.cs`;
a full-depth structural review flagged it as the one clearly cheap+safe cleanup (easy to typo when
a sixth call site appears). One private static predicate removes the duplication with no behaviour
change. **Rejected:** `float.IsFinite` — netstandard2.1 surface the game's Mono runtime doesn't
reliably ship; a "Sanitize-with-default" helper — the fallback differs per site (1f / prev / 0f /
clamp / early-return), so only the boolean test is genuinely common.

## 2026-08-22 — Adopt the structure-review gate + living-doc set for this mod repo
**Why:** first mod repo in the workspace to be onboarded; establishes the push-checkpoint review
and the doc set so later sessions land oriented. **Rejected:** leaving it un-gated — the workspace
CLAUDE.md wants every repo gated once it warrants it.

## 1.0.2 — Read build-mode state live, never latch it off Harmony patches
**Why:** an earlier build counted decorate-substate activations vs deactivations, but the game's
`StateMachine.OnDeactivate` is empty — leaving build mode never deactivates the substate, so the
counter leaked a permanent "still decorating" and the mod kept hijacking the camera during normal
play. Reading `PlayerView…CurrentState as PlayerDecorateStateMachine` each frame (cached) cannot
leak and always reports the authoritative mode. **Rejected:** the activate/deactivate counter (leaks);
watching only `PlayerDecorateStateMachine.OnActivate` (missed the place-from-inventory entry path).

## 1.0.2 — Re-assert "only the current mode's camera is active" while building
**Why:** Far Sight reactivates the Close gameplay camera as it stands down to hand build mode over,
leaving Close live alongside the mode's camera; the Cinemachine brain then renders Close — the wrong
angle in Floor mode — while the zoom drove the top-down camera no longer on screen (read as a dead
zoom). The mod re-asserts the game's own rule. **Rejected:** doing nothing (floor zoom looks dead
with Far Sight installed). Guarded to only ever deactivate once the mode's own camera is confirmed
live, so it can never blank the view mid-blend.

## 1.0.1 — Track `BaseDecorateState`, not just `PlayerDecorateStateMachine.OnActivate`
**Why:** entering build mode by placing straight from the inventory (a path/floor tile) never fires
`OnActivate`, so the zoom didn't arm on that path. (Superseded by the live-read approach in 1.0.2,
which covers every entry path by construction.)

## 1.0.0 — Zoom by scaling the live camera's lens; suppress the wheel with one central postfix
**Why:** the game has no zoom lever — `GameCamera` owns three fixed Cinemachine cameras and build
mode disables the Far↔Close toggle. Scaling `m_Lens.OrthographicSize`/`FieldOfView` is the only
lever (the same one Far Sight uses). All three build-mode wheel consumers read one wrapper
(`Input.MouseScrollDelta`), so a single Harmony postfix suppresses rotate + both brushes at once.
**Rejected:** taking over the wheel outright (rotating is used constantly — a worse mod than none);
patching each of the three consumers separately (one central getter is narrower and simpler).

## 1.0.0 — A hold-modifier for the wheel, but no modifier for the controller
**Why:** on keyboard+mouse the wheel is contested (rotate/brush), so zoom needs a modifier to
disambiguate. A controller's Zoom axis (Rewired action 24) is unread during build mode — no
conflict — so it needs no modifier. Direction defaults to "scroll up zooms in" to match Far Sight,
with `InvertZoom` for the other half since the preference is genuinely contested. **Rejected:**
hardcoding the zoom direction (contested — hence the setting); a controller modifier (unnecessary).

## 1.0.0 — Per-mode zoom levels (Decorate vs Floor kept separate)
**Why:** the top-down Floor view and the 3/4 Decorate view frame the room very differently; one
shared number makes both feel wrong. Each keeps its own target/current, surviving build-mode
re-entry, resetting on quit. **Rejected:** one shared zoom level.

_Living doc — refresh with /project-docs when it drifts._
