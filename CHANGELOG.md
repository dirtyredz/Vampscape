# Changelog

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
