using UnityEngine;

namespace Starfall.Input
{
    /// <summary>
    /// Input abstraction consumed by gameplay. Implementations map keyboard, gamepad and touch to these
    /// intents so bindings can be remapped later without touching gameplay code.
    /// </summary>
    public interface IGameInput
    {
        /// <summary>Digital/analog movement in [-1,1] (keyboard, d-pad, sticks).</summary>
        Vector2 MoveAxis { get; }
        /// <summary>True while a touch/pointer drag is controlling the ship.</summary>
        bool DragActive { get; }
        /// <summary>Screen-space delta of the current drag this frame (pixels).</summary>
        Vector2 DragDeltaScreen { get; }
        bool FireHeld { get; }
        bool UltimatePressed { get; }
        bool PausePressed { get; }
        /// <summary>Any confirm action (tap, space, enter, gamepad south). Used to skip briefing.</summary>
        bool ConfirmPressed { get; }
    }
}
