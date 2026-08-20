# Nexus Mod Page — Vampscape

> **Pasting into the upload form? Use [nexus-paste.md](nexus-paste.md), not this file.**
> The copy here is wrapped for reading, and the editor turns every wrap into a `<br>`.
> See [13-nexus-page-standard.md](../../13-nexus-page-standard.md).

Draft for the first-ever Vampscape listing — Mode C in `.claude/skills/nexus-publish/SKILL.md`,
which is unverified and explicitly says to stage everything locally and stop before publishing.
This file is that staging: read it, then say go.

---

## Open decisions — read before this goes anywhere

Everything else below follows the six existing pages mechanically. These five are new
judgment calls specific to a first-ever page, not settled by prior art:

1. **Title emoji: 🔭 (telescope).** Every existing mod's emoji is literal to what it touches
   (📦 chests, 🌱 plants, ⚰️ coffin, 🪓 axe, 🌿 crops, 🎛️ dials). Telescope is the same move for
   a camera/zoom mod. Bat (🦇) was the other candidate, leaning on the name's pun instead of the
   mechanic — happy to switch if you'd rather lean into the vampire joke.

2. **Category: User Interface, proposed.** Transplant moved from User Interface to Gameplay
   because it changes a gameplay rule (what can be moved). Vampscape changes nothing about
   gameplay — it is camera only — so User Interface reads right, but this is the first mod in
   the set that is purely a camera mod and there's no precedent to lean on.

3. **Tags are unverified.** I don't have live access to the per-game tag vocabulary (`13` notes
   `afk`/`idle` returned nothing for this game when tried) — `quality of life` and
   `user interface` are guesses that need checking in the edit form itself, not assumed from the
   draft.

4. **Shout outs — the Elsiabeth/Mod Menu line.** `14-description-review.md` left this open for
   all six pages and it was never fully settled: drop the Mod Menu credit entirely, or keep it
   as past-tense history. I kept it as history in the draft, matching what's live on the
   existing pages, but flagging since the project doc still calls this open.

5. **Little Chicken Game Company credit.** Existing pages each give a mod-specific reason
   ("crops keeping their record", "a game worth spending this much time inside"). I wrote *"a
   camera and build system worth digging this far into"* — since this mod came out of reading
   `GameCamera` and `PlayerDecorateStateMachine` in real detail. Change it if that doesn't sound
   like you.

---

## Fields

| Field | Value |
|---|---|
| **Name** | Vampscape |
| **Summary** (short, shows in listings) | Can't see enough of your build? Hold a key and scroll to zoom out — rotating and the brush still use the plain wheel, untouched. |
| **Category** | *Proposed:* User Interface |
| **Version** | 1.0.0 |
| **Nexus page** | Not yet created |
| **Requirements** | BepInEx 5 (win_x64), 5.4.23.5 or newer — required |
| | [Mod Nook](https://www.nexusmods.com/moonlightpeaks/mods/127) — optional, for in-game settings |
| | Mod Menu — optional, the alternative to Mod Nook |
| **Tags** | *Proposed, unverified:* quality of life, user interface, save-safe |
| **Licence** | MIT — see [LICENSE](LICENSE) |

No keyword sweep has been run for this mod's searchable words (`zoom`, `camera`, `build` and
similar), unlike Transplant and Coffin Break, which each checked what was already taken before
finalizing the summary. Worth doing before this goes live — see the sweep method referenced in
`13-nexus-page-standard.md`.

---

## Full description

See [nexus-paste.md](nexus-paste.md) for the literal BBCode. Section order and content, in
prose:

- **Description** — the hook: build mode has no zoom, the wheel is already taken by rotate and
  brush, so this adds a modifier rather than reassigning the wheel. Explicitly states it is
  *not* a replacement for Far Sight — the opposite job, handled cleanly.
- **Main features** — seven bullets, the same list as `CHANGELOG.md` §1.0.0, written for a
  reader rather than a diff.
- **Requirements** — BepInEx only, standard boilerplate.
- **Installation** — standard boilerplate, `<Name>` = Vampscape throughout.
- **Configuration** — standard boilerplate plus the Mod Nook pitch: bind the modifier by
  pressing it, dial in range and speed on sliders.
- **Compatibility** — the Far Sight relationship in more depth than the one-liner in
  Requirements, plus the save-safety/no-persistence argument for why it can't conflict with
  anything else.
- **Shout outs** — the standard four, see open decisions above for the two mod-specific lines.

---

## Licence

**MIT** — see [LICENSE](LICENSE). Permissive: anyone may use, modify and redistribute, provided
the copyright notice is kept.

Set the Nexus permissions to agree with it:

| Nexus permission | Set to |
|---|---|
| Upload to other sites | Allowed |
| Convert to other games | Allowed |
| Modify and release | Allowed |
| Use assets in own files | Allowed |
| Include in mod packs / collections | Allowed |

---

## Screenshots

Files live in `screenshots/`.

| # | Shot | File | Status |
|---|---|---|---|
| - | Thumbnail, **16:9** | `thumbnail.png` | ✅ 1672x941 |
| - | Title banner | `banner.png` | ✅ 2358x667 |
| 1 | In-game: zoomed out in Decorate mode, showing more of the build than the vanilla camera allows | — | ⬜ not captured |
| 2 | In-game: the same spot in Floor mode, zoomed | — | ⬜ optional |

**No in-game screenshot exists yet.** Both existing images are the marketing banner/thumbnail;
neither shows the actual mod running. Every prior page's launch shots came from decorate mode
with the feature visibly working — worth capturing before this goes live, even one shot of a
wide-zoomed build compared to the vanilla view.

---

## Notes before publishing

- ⬜ **Not play-tested.** Nothing in this mod has been confirmed working in game — see
  `TESTING.md`. The description and feature list state things ("rotate still works", "the
  confiner keeps up") that are currently argued from reading the code, not measured.
- ⬜ **No in-game screenshot**, see above.
- ⬜ **No keyword sweep** for the summary's searchable words.
- ⬜ **Category, tags and two shout-out lines are proposals**, not decisions — see Open
  decisions above.
- State plainly that it is **save-safe** — the mod adds no persistence and this community reads
  for that claim specifically.
- This is genuinely **Mode C**, the unverified flow — the skill has never created a mod page
  this way before, so expect to discover the "Upload" control's actual behaviour live rather
  than following a known recipe.
