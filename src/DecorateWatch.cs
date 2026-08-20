using Chicken.Utilities;
using HarmonyLib;

namespace Vampscape
{
    /// <summary>
    /// Tracks whether build mode is open, and which of its two camera modes is showing.
    ///
    /// Far Sight answers the same question by walking
    /// PlayerView.Instance.StateMachine.CurrentState every frame. That is fine at its call rate,
    /// but ScrollGate has to answer it from inside a property getter the game reads several times
    /// a frame, so this keeps a plain bool that two patches maintain instead.
    ///
    /// OnActivate and OnDeactivate are protected, hence the string patch targets - the same shape
    /// Transplant uses against this class.
    ///
    /// PlayerDecorateStateMachine is not the only signal - see DecorateStateGate below for why.
    /// </summary>
    [HarmonyPatch(typeof(PlayerDecorateStateMachine))]
    internal static class DecorateWatch
    {
        /// <summary>The live state machine while build mode is open, otherwise null.</summary>
        internal static PlayerDecorateStateMachine Current { get; private set; }

        /// <summary>
        /// Current alone misses at least one way into build mode: entering it by clicking a path
        /// or floor tile and placing straight from the inventory does not run
        /// PlayerDecorateStateMachine's OnActivate the way opening build mode normally does, so
        /// Current stays null and the mod goes inert. Transplant hit the same gap - its
        /// DecorateStatePatches comment says its first build tracked decorate mode only from this
        /// class and "there was no way to tell from the outside whether that had fired" - and
        /// fixed it with a second signal from DecorateSelectState. This does the same but off
        /// BaseDecorateState, the common parent of every decorate substate, so it does not matter
        /// which one that entry path actually lands in.
        /// </summary>
        internal static bool IsDecorating => Current != null || DecorateStateGate.AnyActive;

        /// <summary>
        /// Decorate (3/4 view, VirtualCameraFar) or Floor (top-down, VirtualCameraTopDown). The
        /// two get their own remembered zoom level, since they frame the room very differently.
        /// </summary>
        internal static DecorateMode Mode => Current != null ? Current.Mode : DecorateMode.Decorate;

        [HarmonyPostfix]
        [HarmonyPatch("OnActivate")]
        internal static void AfterActivate(PlayerDecorateStateMachine __instance)
        {
            // Not simply "Current = __instance".
            //
            // When the player is somewhere they are not allowed to decorate, OnActivate bails out
            // and calls Exit(forceExitCompletely: true) from inside its own body. That runs
            // OnDeactivate - and this postfix runs after the whole body, so the sequence is
            // deactivate-then-activate and the flag would latch on and never clear, leaving the
            // scroll wheel swallowed for the rest of the session.
            //
            // The tell is that the aborted path has already moved the player on to another state,
            // so a successful entry is the one where the player is actually still in this one.
            if (!MonoBehaviourSingleton<PlayerView>.Exists)
            {
                return;
            }

            var playerState = MonoBehaviourSingleton<PlayerView>.Instance.StateMachine.CurrentState;
            if ((object)playerState == __instance)
            {
                Current = __instance;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnDeactivate")]
        internal static void AfterDeactivate()
        {
            Current = null;
        }
    }

    /// <summary>
    /// Fallback "is decorate mode open" signal, counted off the state every decorate substate
    /// (select, move, floor, ...) actually activates rather than the outer state machine. Needed
    /// because DecorateWatch's own postfix does not fire for every route into build mode - see
    /// its class comment.
    ///
    /// A count rather than a bool because substates can nest one deactivating into another
    /// activating within the same frame; a plain bool set by whichever posts last could land on
    /// false while a substate is still live. Clamped at zero so a stray extra deactivate - the
    /// same kind of mismatch DecorateWatch itself guards against - cannot wrap it negative and
    /// leave AnyActive permanently true.
    /// </summary>
    [HarmonyPatch(typeof(BaseDecorateState))]
    internal static class DecorateStateGate
    {
        private static int activeCount;

        internal static bool AnyActive => activeCount > 0;

        [HarmonyPostfix]
        [HarmonyPatch("OnActivate")]
        internal static void AfterActivate()
        {
            activeCount++;
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnDeactivate")]
        internal static void AfterDeactivate()
        {
            if (activeCount > 0)
            {
                activeCount--;
            }
        }
    }
}
