# Changelog

## 1.0.2

- Fixed floor-mode zoom appearing dead when Far Sight is also installed. Far Sight reactivates the
  Close gameplay camera as it stands down to hand build mode over, leaving it live alongside the
  mode's camera; the game then renders Close - the wrong angle in floor mode - while the zoom drove
  the top-down camera that was no longer on screen. The mod now re-asserts the game's own rule that
  only the current mode's camera is active while building, so the right camera shows and it is the
  one being zoomed.
- Reworked "is build mode open" tracking. It used to count decorate-substate activations against
  deactivations, but the game's `StateMachine.OnDeactivate` is empty and never deactivates its
  substate on the way out, so the count leaked a permanent "still decorating" and the mod kept
  driving the camera during normal play. Build-mode state and the floor/decorate mode are now read
  straight from the live player state (cached per frame), which cannot leak and always reports the
  right mode. This also removes the separate substate patch and the tracking postfixes it needed.

## 1.0.1

- Fixed the zoom not activating when build mode is entered by placing straight from the
  inventory (e.g. a path or floor tile), rather than opening decorate mode normally.
  `PlayerDecorateStateMachine.OnActivate` does not fire on that path, so tracking now also
  watches `BaseDecorateState`, the common parent every decorate substate actually activates.

## 1.0.0

First release.

- Hold a modifier (Left Alt by default) and scroll to zoom the camera while in build mode.
- Rotating and brush resizing keep the bare wheel, unchanged.
- Decorate mode and Floor mode each remember their own zoom level, across leaving and re-entering
  build mode.
- Camera pan bounds are refreshed as the zoom changes, so zooming out does not let the view drift
  past the edge of the decoratable area.
- Controller zoom on the game's existing Zoom control, which nothing else reads during build mode.
- Complementary to Far Sight, which covers normal play and stands down in build mode.
