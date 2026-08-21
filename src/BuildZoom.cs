using System.Collections.Generic;
using Chicken.Utilities;
using Cinemachine;
using Rewired;
using UnityEngine;

namespace Vampscape
{
    /// <summary>
    /// The zoom itself.
    ///
    /// The game has no zoom to turn up. GameCamera owns three fixed Cinemachine cameras - Far,
    /// Close and TopDown - and build mode picks between two of them in
    /// PlayerDecorateStateMachine.SetMode(): Decorate gets Far, Floor gets TopDown. So "zoom out"
    /// here means scaling the live camera's lens, which is the same lever Far Sight pulls during
    /// normal play.
    /// </summary>
    internal sealed class BuildZoom : MonoBehaviour
    {
        /// <summary>
        /// Far Sight stands down when build mode opens, and restoring its lenses is the first
        /// thing it does. Unity does not order the two plugins' Update calls, so capturing a
        /// baseline on the same frame build mode opens can capture Far Sight's zoomed value and
        /// treat it as the default. Waiting a few frames removes the race; the camera is blending
        /// over that window anyway, so nothing is visible.
        /// </summary>
        private const int WarmupFrames = 5;

        /// <summary>
        /// Cinemachine damping is not involved - the lens is written directly - so the glide has
        /// to come from somewhere. Higher is snappier.
        /// </summary>
        private const float Damping = 8f;

        /// <summary>Below this the confiner refresh is not worth the recompute.</summary>
        private const float ConfinerRefreshEpsilon = 0.002f;

        private readonly Dictionary<CinemachineVirtualCamera, Baseline> baselines =
            new Dictionary<CinemachineVirtualCamera, Baseline>();

        // Separate levels per mode: the top-down floor view and the 3/4 decorate view want very
        // different framings, and sharing one number makes both feel wrong. Instance fields, so
        // they survive leaving and re-entering build mode but reset with the game.
        private float decorateTarget = 1f;
        private float decorateCurrent = 1f;
        private float floorTarget = 1f;
        private float floorCurrent = 1f;

        private int framesDecorating;
        private float lastConfinerFactor = -1f;
        private bool applied;

        private void Update()
        {
            if (!Plugin.Enabled.Value || !DecorateWatch.IsDecorating || !MonoBehaviourSingleton<GameCamera>.Exists)
            {
                Restore();
                return;
            }

            framesDecorating++;
            if (framesDecorating < WarmupFrames)
            {
                return;
            }

            var gameCamera = MonoBehaviourSingleton<GameCamera>.Instance;
            var floor = DecorateWatch.Mode == DecorateMode.Floor;
            var vcam = floor ? gameCamera.VirtualCameraTopDown : gameCamera.VirtualCameraFar;
            if (vcam == null)
            {
                return;
            }

            KeepModeCameraAlone(gameCamera, vcam);

            var target = floor ? floorTarget : decorateTarget;
            var current = floor ? floorCurrent : decorateCurrent;

            target = Mathf.Clamp(target + ReadZoomInput(), Plugin.MinZoom.Value, Plugin.MaxZoom.Value);
            if (float.IsNaN(target) || float.IsInfinity(target))
            {
                target = 1f;
            }

            current = Mathf.Lerp(current, target, Mathf.Clamp01(Time.unscaledDeltaTime * Damping));
            if (float.IsNaN(current) || float.IsInfinity(current))
            {
                current = target;
            }

            if (floor)
            {
                floorTarget = target;
                floorCurrent = current;
            }
            else
            {
                decorateTarget = target;
                decorateCurrent = current;
            }

            Apply(vcam, current);
            RefreshConfiner(current);
        }

        private void OnDestroy()
        {
            Restore();
        }

        /// <summary>
        /// Keeps the mode's camera the only build camera on screen.
        ///
        /// The game's SetMode leaves exactly one of Far/Close/TopDown active, but Far Sight
        /// reactivates the Close gameplay camera when it stands down to hand build mode over, so
        /// Close ends up live alongside the mode's camera. The brain then renders Close - the wrong
        /// angle in floor mode - and the zoom lands on a camera that is not on screen, which reads
        /// as the zoom being dead. Re-asserting the game's own rule fixes both: the right camera
        /// shows, and it is the one being zoomed.
        ///
        /// Only ever deactivates, and only once the mode's own camera is confirmed live, so it can
        /// never blank the view by switching everything off during a blend.
        /// </summary>
        private static void KeepModeCameraAlone(GameCamera gameCamera, CinemachineVirtualCamera keep)
        {
            if (keep == null || !keep.gameObject.activeSelf)
            {
                return;
            }

            Deactivate(gameCamera.VirtualCameraFar, keep);
            Deactivate(gameCamera.VirtualCameraClose, keep);
            Deactivate(gameCamera.VirtualCameraTopDown, keep);
        }

        private static void Deactivate(CinemachineVirtualCamera vcam, CinemachineVirtualCamera keep)
        {
            if (vcam != null && vcam != keep && vcam.gameObject.activeSelf)
            {
                vcam.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// One scroll tick, or a controller axis scaled into something tick-sized. Positive zooms
        /// out, matching the direction the wheel already zooms out in normal play.
        /// </summary>
        private float ReadZoomInput()
        {
            var delta = 0f;

            // ScrollGate has already zeroed the game's own view of this under exactly the same
            // condition, so the wheel drives the zoom or the brush, never both.
            if (Hotkey.IsHeld(Plugin.Modifier.Value))
            {
                var wheel = UnityEngine.Input.mouseScrollDelta.y;
                if (Mathf.Abs(wheel) > 0.01f)
                {
                    delta -= wheel * Plugin.ZoomStep.Value;
                }
            }

            var stick = ReadControllerZoom();
            if (Mathf.Abs(stick) > 0f)
            {
                delta -= stick * Plugin.ZoomStep.Value * Plugin.ControllerZoomSpeed.Value
                         * Time.unscaledDeltaTime * 60f;
            }

            // Scroll up zooms in, matching Far Sight, on the reasoning that anyone bothered enough
            // by the build-mode camera to install this already has that one and its direction in
            // their hands. Note that is the opposite of what GameCamera's own Far/Close toggle
            // does with a positive Zoom axis, so the preference is genuinely contested - hence the
            // setting rather than a hardcoded sign.
            return Plugin.InvertZoom.Value ? -delta : delta;
        }

        /// <summary>
        /// Rewired action 24 is the game's Zoom control. Nothing reads it during build mode -
        /// PlayerDecorateStateMachine sets isCameraToggleEnabled false, which leaves GameCamera's
        /// camera-toggle blocker in place for the whole session - so it is free to borrow, and a
        /// controller needs no modifier because it has no brush-vs-zoom conflict to resolve.
        ///
        /// The joystick check is not optional: on keyboard and mouse that same action is bound to
        /// the wheel, and without it every scroll tick would be counted twice.
        /// </summary>
        private static float ReadControllerZoom()
        {
            if (!Plugin.ControllerZoom.Value || !ReInput.isReady)
            {
                return 0f;
            }

            var last = ReInput.controllers.GetLastActiveController();
            if (last == null || last.type != ControllerType.Joystick)
            {
                return 0f;
            }

            var player = ReInput.players.GetPlayer(0);
            if (player == null)
            {
                return 0f;
            }

            var axis = player.GetAxis(24);
            if (float.IsNaN(axis) || float.IsInfinity(axis))
            {
                return 0f;
            }

            var deadzone = Mathf.Clamp(Plugin.ControllerDeadzone.Value, 0.05f, 0.8f);
            if (Mathf.Abs(axis) < deadzone)
            {
                return 0f;
            }

            var scaled = Mathf.Sign(axis) * (Mathf.Abs(axis) - deadzone) / (1f - deadzone);
            return float.IsNaN(scaled) || float.IsInfinity(scaled) ? 0f : Mathf.Clamp(scaled, -1f, 1f);
        }

        private void Apply(CinemachineVirtualCamera vcam, float factor)
        {
            if (factor <= 0.01f || float.IsNaN(factor) || float.IsInfinity(factor))
            {
                return;
            }

            if (!baselines.TryGetValue(vcam, out var baseline))
            {
                baseline = new Baseline(vcam.m_Lens.OrthographicSize, vcam.m_Lens.FieldOfView);
                baselines[vcam] = baseline;
            }

            var lens = vcam.m_Lens;
            lens.OrthographicSize = Mathf.Clamp(baseline.Ortho * factor, 0.5f, 200f);
            lens.FieldOfView = Mathf.Clamp(baseline.Fov * factor, 1f, 120f);
            vcam.m_Lens = lens;
            applied = true;
        }

        /// <summary>
        /// Keeps panning honest after a zoom change.
        ///
        /// Outside build mode GameCamera.ProcessCameraBounds() recomputes the confiner volume
        /// every Update from live viewport rays, so a wider lens automatically tightens how far
        /// the camera may pan. Build mode swaps that out for the decoratable area's own confiner
        /// via OverrideConfiner(), and DecorateCameraConfiner only recomputes inside Activate() -
        /// once, on entry. Zoom after that and the bounds still describe the old lens, which lets
        /// the camera pan past the edge of the area. Activate() is public, so re-running it is all
        /// this takes.
        ///
        /// It reads the brain camera's blended FOV, which is a frame behind the lens write above.
        /// That is fine here: the zoom glides over several frames, so the bounds converge with it.
        /// </summary>
        private void RefreshConfiner(float factor)
        {
            if (Mathf.Abs(factor - lastConfinerFactor) < ConfinerRefreshEpsilon)
            {
                return;
            }

            lastConfinerFactor = factor;

            // Null when decorating outside a confined area, in which case the default volumes are
            // live and those already recompute every frame.
            var area = DecorateWatch.Current != null ? DecorateWatch.Current.ConfinedGridArea : null;
            if (area != null && area.CameraConfiner != null)
            {
                area.CameraConfiner.Activate();
            }
        }

        private void Restore()
        {
            framesDecorating = 0;
            lastConfinerFactor = -1f;

            if (!applied)
            {
                return;
            }

            applied = false;

            // A destroyed camera means GameCamera itself was rebuilt, so every baseline in here
            // describes an object that no longer exists.
            var stale = false;

            foreach (var entry in baselines)
            {
                var vcam = entry.Key;
                if (vcam == null)
                {
                    stale = true;
                    continue;
                }

                var lens = vcam.m_Lens;
                lens.OrthographicSize = entry.Value.Ortho;
                lens.FieldOfView = entry.Value.Fov;
                vcam.m_Lens = lens;
            }

            if (stale)
            {
                baselines.Clear();
            }
        }

        private readonly struct Baseline
        {
            internal Baseline(float ortho, float fov)
            {
                Ortho = ortho;
                Fov = fov;
            }

            internal float Ortho { get; }

            internal float Fov { get; }
        }
    }
}
