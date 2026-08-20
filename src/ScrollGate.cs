using HarmonyLib;
using UnityEngine;

namespace Vampscape
{
    /// <summary>
    /// Hides the scroll wheel from the game while the zoom modifier is held.
    ///
    /// Three things read the wheel in build mode, and all three go through the same one-line
    /// wrapper - Input.MouseScrollDelta, which is just UnityEngine.Input.mouseScrollDelta:
    ///
    ///     GridObjectHelper.TryRotateGridObject   rotate / mirror the held object
    ///     DecorateMoveObjectState.ProcessBrush   brush size 1-4, paths and floors only
    ///     DecorateSelectState.ProcessBrush       brush size with nothing in hand
    ///
    /// Zeroing the wrapper suppresses all three at once, which is why this is one postfix rather
    /// than three. The cost of patching something that central is that it is also what the
    /// calendar, the menus and the quantity popup scroll with - hence the gate below is as narrow
    /// as it can be: build mode open, modifier down, mod switched on.
    ///
    /// ShouldSwallow() is deliberately recomputed on every call rather than latched once per
    /// frame in an Update. Unity gives no ordering guarantee between this plugin's Update and the
    /// game's state machines, and a latched flag would be read before it was written on whichever
    /// frames the order happened to flip. Everything it reads is already frame-stable.
    /// </summary>
    [HarmonyPatch(typeof(global::Input), "MouseScrollDelta", MethodType.Getter)]
    internal static class ScrollGate
    {
        internal static bool ShouldSwallow()
        {
            return Plugin.Enabled != null
                && Plugin.Enabled.Value
                && DecorateWatch.IsDecorating
                && Hotkey.IsHeld(Plugin.Modifier.Value);
        }

        [HarmonyPostfix]
        internal static void Postfix(ref Vector2 __result)
        {
            if (ShouldSwallow())
            {
                __result = Vector2.zero;
            }
        }
    }
}
