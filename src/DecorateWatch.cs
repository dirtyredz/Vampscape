using Chicken.Utilities;
using UnityEngine;

namespace Vampscape
{
    /// <summary>
    /// Tracks whether build mode is open, and which of its two camera modes is showing.
    ///
    /// This reads the live player state directly - the same question Far Sight answers by walking
    /// PlayerView.Instance.StateMachine.CurrentState every frame - rather than latching a bool off
    /// Harmony patches. An earlier build did the latching, but the game's StateMachine.OnDeactivate
    /// is empty: leaving build mode does not deactivate the decorate substate, so a patch counting
    /// substate activations against deactivations leaks a permanent "still decorating" and the mod
    /// keeps hijacking the camera during normal play. The current player state cannot lie or leak -
    /// build mode is open exactly when PlayerDecorateStateMachine is the state - and it carries the
    /// authoritative Mode, so both signals come straight from it.
    ///
    /// The result is cached per frame because ScrollGate asks from inside a property getter the game
    /// reads several times a frame; the walk itself is cheap, but this keeps it frame-stable and off
    /// the hot path, which is the property the old bool was really after.
    /// </summary>
    internal static class DecorateWatch
    {
        private static int cachedFrame = -1;
        private static PlayerDecorateStateMachine cachedMachine;

        /// <summary>The live state machine while build mode is open, otherwise null.</summary>
        internal static PlayerDecorateStateMachine Current
        {
            get
            {
                var frame = Time.frameCount;
                if (frame == cachedFrame)
                {
                    return cachedMachine;
                }

                cachedFrame = frame;
                cachedMachine = MonoBehaviourSingleton<PlayerView>.Exists
                    ? MonoBehaviourSingleton<PlayerView>.Instance.StateMachine.CurrentState
                        as PlayerDecorateStateMachine
                    : null;
                return cachedMachine;
            }
        }

        /// <summary>
        /// True whenever build mode is open, by any entry path. Reading the current state covers the
        /// routes the old OnActivate patch missed (placing straight from the inventory never ran it)
        /// without the leak the substate counter had, since a decorate substate is only ever live
        /// while PlayerDecorateStateMachine is the player state that owns it.
        /// </summary>
        internal static bool IsDecorating => Current != null;

        /// <summary>
        /// Decorate (3/4 view, VirtualCameraFar) or Floor (top-down, VirtualCameraTopDown). The
        /// two get their own remembered zoom level, since they frame the room very differently. Read
        /// straight off the live machine, so it is always right regardless of how build mode opened.
        /// </summary>
        internal static DecorateMode Mode
        {
            get
            {
                var machine = Current;
                return machine != null ? machine.Mode : DecorateMode.Decorate;
            }
        }
    }
}
