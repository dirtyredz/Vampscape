# Vampscape — Nexus page source

**Not yet live — mod ID not assigned.** This is the first draft, written directly in the
current house style (`15-page-style.md`), so unlike the original six it needs no later
restyle pass. Once a page exists, its live BBCode becomes the source of truth and this file
gets the same "superseded" banner the others carry.

Structure: [14-description-review.md](../../14-description-review.md).
Look: [15-page-style.md](../../15-page-style.md).
Mechanics: [13-nexus-page-standard.md](../../13-nexus-page-standard.md).

---

## Fields

| Field | Value |
|---|---|
| **Name** | Vampscape |
| **Summary** (short, shows in listings) | Can't see enough of your build? Hold a key and scroll to zoom out — rotating and the brush still use the plain wheel, untouched. |
| **Category** | *Proposed:* User Interface — it is purely a camera mod, nothing it does is a gameplay rule |
| **Version** | 1.0.2 |
| **Requirements** | BepInEx 5 (win_x64), 5.4.23.5 or newer — required |
| | [Mod Nook](https://www.nexusmods.com/moonlightpeaks/mods/127) — optional, for in-game settings |
| | Mod Menu — optional, the alternative to Mod Nook |
| **Tags** | *Proposed, unverified live:* quality of life, user interface, save-safe — check the actual per-game tag vocabulary in the edit form before setting these |
| **Licence** | MIT (matches [LICENSE](LICENSE)) |

---

## Full description — paste into Nexus

```bbcode
[size=6][color=#F7D994]🔭  Vampscape[/color][/size]
[color=#C7A25B][i]Can't see enough of your build? Hold a key and scroll to zoom out — rotating and the brush still use the plain wheel, untouched.[/i][/color]
[color=#C7A25B]💾 Save-safe  ·  🖱️ Hold to zoom  ·  🎮 Controller supported  ·  🎛️ Configurable[/color]
[color=#7A6A9B]────────────────────────────────────────[/color]
[quote]🖱️  [color=#F7D994][b]The promise.[/b][/color] Rotate and the brush keep the plain wheel — only the modifier changes.[/quote]

[size=5][color=#F7D994]🔭  What it does[/color][/size]
[color=#D4D4D8]Build mode has no zoom, and the scroll wheel is already spoken for.

GameCamera does not have a zoom level to turn up — it owns three fixed cameras, and build mode only ever shows you two of them. Meanwhile the wheel already rotates whatever you are holding and resizes the brush on paths and floors. So the fix could not simply hand the wheel over — retraining rotate would be a worse mod than no mod at all.

Vampscape adds a modifier instead. Hold it and scroll to zoom the camera out, as far or as close as you want, smoothly, in both the regular decorate view and the top-down floor view — each remembers its own level. Let go, and the wheel goes straight back to rotating and resizing exactly as it always has.

It is not a replacement for a zoom mod like Far Sight — it does the opposite job. Far Sight handles the camera during normal play and deliberately stands down the moment you open build mode. This is scoped to build mode alone, and the two hand off cleanly.

Nothing is written to your save.[/color]

[size=5][color=#F7D994]✨  Main features[/color][/size]
[list]
[*]Hold a modifier (Left Alt by default) and scroll to zoom the camera while you build
[*]Let go and rotate / brush resizing work exactly as they always did — nothing retrained
[*]Decorate view and the top-down Floor view each remember their own zoom level
[*]Camera bounds keep up as you zoom, so you cannot pan past the edge of the area while zoomed out
[*]Zoom with a controller on the game's own Zoom control — no modifier needed
[*]Complementary to Far Sight — it covers normal play, this covers build mode, and the two never fight over the wheel
[*]Nothing is written to your save
[/list]

[size=5][color=#F7D994]📋  Requirements[/color][/size]
[list]
[*][b]BepInEx 5 (win_x64)[/b], version 5.4.23.5 or newer — the only thing this mod needs
[/list]
[color=#D4D4D8]PC/Steam only. The Switch and mobile builds cannot load BepInEx.[/color]

[size=5][color=#F7D994]📥  Installation[/color][/size]
[b]🟢 With Vortex[/b]
[color=#D4D4D8]Open the Files tab, click the Vortex button, and enable the mod. Done.[/color]

[b]🔧 Manually[/b]
[list=1]
[*]Install [b]BepInEx 5 (win_x64)[/b] into your Moonlight Peaks folder, if you do not have it already. The BepInEx folder sits beside Moonlight Peaks.exe.
[*]Launch the game once, then quit. This creates the BepInEx/plugins folder.
[*]Download the archive from the Files tab and extract it over your Moonlight Peaks folder, so the file ends up at BepInEx/plugins/Vampscape/Vampscape.dll
[*]Launch the game.
[/list]
[color=#D4D4D8]To uninstall, delete the BepInEx/plugins/Vampscape folder. Nothing was ever written to your save.[/color]

[size=5][color=#F7D994]🎛️  Configuration[/color][/size]
[color=#D4D4D8]Settings are written to BepInEx/config/com.dirtyredz.moonlightpeaks.vampscape.cfg on first launch. The defaults are meant to be left alone.[/color]

[quote]🎯  [color=#F7D994][b]Install Mod Nook[/b][/color] and change these in game instead. Vampscape shows up in it on its own — bind the zoom modifier by pressing the key you want, and dial in the zoom range and speed on sliders. Nothing here needs it; it just makes this mod easier to live with.[/quote]

[size=5][color=#F7D994]🤝  Compatibility[/color][/size]
[color=#D4D4D8]Far Sight is not just compatible, it is complementary — the two are never active at the same time, by design, and each hands the camera to the other cleanly.

Vampscape does not touch persistence, so it cannot conflict with anything that changes what can be placed, moved or decorated. It only reads the scroll wheel and scales the camera's lens while build mode is open.[/color]

[size=5][color=#F7D994]💜  Shout outs[/color][/size]
[list]
[*][b]Little Chicken Game Company[/b], for a camera and build system worth digging this far into.
[*]The [b]BepInEx[/b] and [b]HarmonyX[/b] teams, without whom none of this scene exists.
[*][b]Elsiabeth[/b] for Mod Menu — it made the case that in-game settings were worth having, and it is why none of these mods had to build a settings screen of their own.
[*][b]My Mate[/b], for being my inspiration.
[/list]
```

---

## Changelog entry for the Nexus page

Player-facing. Describe the **symptom**, not the cause — the repo README names the Harmony
patches; that belongs in the repo.

### 1.0.2

```
- Fixed the build-mode zoom doing nothing in floor (top-down) mode when Far Sight is also
  installed. The game was left showing the angled camera instead of the top-down one, so
  zooming moved a view that was not on screen. Floor mode now shows the correct top-down
  camera again, and the zoom works on it. Far Sight can stay enabled.
- Tidied up how the mod detects build mode, so it no longer keeps a grip on the camera after
  you leave build mode.
```

### 1.0.1

```
- Fixed the zoom not turning on when you entered build mode by placing a path or floor tile
  straight from your inventory, rather than opening decorate mode the normal way.
```

### 1.0.0

```
First release.

- Hold a modifier (Left Alt by default) and scroll to zoom the camera while in build mode.
- Rotating and brush resizing keep the bare wheel, unchanged.
- Decorate mode and Floor mode each remember their own zoom level.
- Camera pan bounds are refreshed as the zoom changes, so you cannot pan past the edge of the
  decoratable area while zoomed out.
- Controller zoom on the game's existing Zoom control.
- Complementary to Far Sight, which covers normal play and stands down in build mode.
```
