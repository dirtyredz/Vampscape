# Releasing Vampscape

Repo-wide rules live at the root; this file only covers what is specific to this mod.

- Versioning and archive layout: [12-versioning-and-release.md](../../12-versioning-and-release.md)
- Nexus page mechanics: [13-nexus-page-standard.md](../../13-nexus-page-standard.md)

Short version on numbering: the version is for players, not a build counter. The first
published version is **1.0.0**; bump only when publishing, one CHANGELOG entry per release.

## Build a release

```powershell
powershell -File pack.ps1
```

Produces `dist/Vampscape-<version>.zip`, reading the version from the csproj so the archive
can never disagree with the DLL; `Plugin.cs` derives that same version at build time via
`ModBuildInfo.Version`.

There is no test project. Every code path here reads live game state — `GameCamera`'s
Cinemachine vcams, the decorate state machine, the decoratable area's confiner — none of which
a headless runner can assert. [TESTING.md](TESTING.md)'s 16-item checklist carries the weight
instead, and **none of it has been run yet.**

## Pre-release checklist

Root checklist first: [12-versioning-and-release.md](../../12-versioning-and-release.md).
Then everything in [TESTING.md](TESTING.md), which is not optional here — unlike a mod that
only reads persistence, this one patches a property getter (`Input.MouseScrollDelta`) by
string name, which is the one thing in the whole mod most likely to fail silently at load if
Harmony cannot resolve it. Confirm the log line before anything else.

### The two that matter most

- [ ] **Rotate still works with nothing held.** TESTING.md step 3. This is the regression that
      matters more than the feature itself — rotating is used on every single placement, and
      Vampscape has no business touching it.
- [ ] **The wheel is swallowed only under the exact gate.** Modifier down, build mode open,
      mod enabled — TESTING.md steps 4–5 and 13. Too broad a gate breaks the calendar and menus;
      too narrow leaves rotate uncontrollable while trying to zoom.

### Before the Nexus page goes live

- [ ] Full [TESTING.md](TESTING.md) pass, all 16 steps
- [ ] At least one in-game screenshot showing the zoomed view against the vanilla one —
      see the gap noted in [NEXUS.md](NEXUS.md)
- [ ] Keyword sweep for the summary's searchable words (`zoom`, `camera`, `build`), same method
      Transplant and Coffin Break used
- [ ] The five open decisions in [NEXUS.md](NEXUS.md) resolved, not left as proposals
- [ ] `<Version>` is the single source of truth — `Plugin.cs` derives from it via `ModBuildInfo.Version`, but check the number is
      the one you meant
- [ ] CHANGELOG has one entry for this version
- [ ] Fresh install: delete `BepInEx/config/com.dirtyredz.moonlightpeaks.vampscape.cfg`, launch,
      confirm sensible defaults
- [ ] Archive extracted onto a clean install and verified in game

## Licence

**MIT** — see [LICENSE](LICENSE). Permissive: anyone may use, modify and redistribute, provided
the copyright notice is kept.

| Nexus permission | Set to |
|---|---|
| Upload to other sites | Allowed |
| Convert to other games | Allowed |
| Modify and release | Allowed |
| Use assets in own files | Allowed |
| Include in mod packs / collections | Allowed |

Credit is customary rather than required under MIT.
