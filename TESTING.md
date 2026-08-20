# Testing Vampscape

No test project. Every code path reads Unity and game types, so verification is by hand.

## Before anything else

Check `BepInEx/LogOutput.log` for:

```
Vampscape 1.0.0 loaded. Hold LeftAlt and scroll to zoom while building.
```

**The one thing most likely to go wrong at load** is the `ScrollGate` patch. It targets a property
getter — `Input.MouseScrollDelta` — by string name and `MethodType.Getter`. If Harmony cannot
resolve it, `PatchAll` throws and the log will say so instead of the line above. Everything else
in the mod is ordinary method patching.

## The core loop

1. Enter build mode. Confirm the camera looks exactly as it always has.
2. Hold Left Alt, scroll. The view should glide out and in, not snap.
3. Release Alt, scroll with an object in hand. **It must still rotate.** This is the regression
   that matters most — rotating is used constantly.
4. Pick up a path or floor tile (something with a brush), release Alt, scroll. **The brush must
   still resize 1–4.** Watch the on-screen brush indicator.
5. Hold Alt and scroll while holding that same path tile. The object must *not* rotate and the
   brush must *not* resize.

## Both camera modes

6. Zoom out in Decorate mode. Switch to Floor mode (top-down). It should have its own level, not
   inherit the one you just set.
7. Zoom Floor mode somewhere different, switch back to Decorate. Each should have kept its own.
8. Leave build mode entirely and re-enter. Both levels should still be there.
9. Quit to menu and back in. Both should be back at default.

## The confiner

10. Zoom out a long way, then pan the camera to the edge of the decoratable area.

The camera should stop before showing much beyond the area's edge. If you can pan well past it
while zoomed out, `RefreshConfiner` is not landing — that is the `DecorateCameraConfiner.Activate()`
call in `BuildZoom`, and it is the one piece of this that depends on stale bounds being refreshed
rather than on anything the game does for us.

Worth trying in a small interior room as well as outdoors: the confiner volume shrinks as the lens
widens, so in a tight room at max zoom the camera may end up pinned near the centre. That is
correct behaviour, not a bug — but confirm it degrades gracefully rather than jittering.

## Leaving cleanly

11. Zoom out, then leave build mode. The normal-play camera must be back to its usual framing —
    no lingering wide lens.
12. With Far Sight installed: zoom during normal play, enter build mode, zoom there, leave.
    Neither mod's zoom should have leaked into the other's, and normal play should return to the
    level Far Sight had.

Step 12 is the one that exercises the baseline-capture race described in the README. If normal
play comes back wider or narrower than you left it, the warmup window in `BuildZoom` is too short.

## Elsewhere in the game

13. Hold Left Alt and scroll in the calendar, the journal, and a quantity popup. All must scroll
    normally — the gate is supposed to be inert outside build mode.
14. Walk somewhere you cannot decorate and try to open build mode. It should refuse as usual, and
    afterwards the wheel must still work everywhere. (This is the aborted-`OnActivate` path that
    `DecorateWatch` guards against; if it regresses, the wheel stays swallowed for the rest of the
    session.)

## Controller

15. On a gamepad, enter build mode and use whatever Zoom is bound to. It should zoom with no
    modifier held.
16. Back on keyboard and mouse, confirm one scroll tick moves the zoom by one step, not two. A
    double step means the joystick guard in `ReadControllerZoom` is not holding.
