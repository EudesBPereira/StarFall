using UnityEngine;
using UnityEngine.InputSystem;

namespace Starfall.Input
{
    /// <summary>
    /// Single entry point for gameplay input. Builds Input System actions in code (keyboard + gamepad)
    /// and merges them with touch drag (fed by <see cref="TouchPad"/>) and on-screen buttons.
    /// Remapping later only needs to change the bindings created in <see cref="BuildActions"/>.
    /// </summary>
    [DefaultExecutionOrder(-250)]
    public sealed class GameInputReader : MonoBehaviour, IGameInput
    {
        private InputAction _move;
        private InputAction _fire;
        private InputAction _ultimate;
        private InputAction _pause;
        private InputAction _confirm;

        private Vector2 _dragDelta;
        private Vector2 _dragDeltaAccumulated;
        private bool _dragActive;
        private bool _uiUltimate;
        private bool _uiPause;
        private bool _uiConfirm;
        private bool _uiFireHeld;

        public Vector2 MoveAxis { get; private set; }
        public bool DragActive => _dragActive;
        public Vector2 DragDeltaScreen => _dragDelta;
        public bool FireHeld { get; private set; }
        public bool UltimatePressed { get; private set; }
        public bool PausePressed { get; private set; }
        public bool ConfirmPressed { get; private set; }

        /// <summary>When enabled, the ship fires automatically (mobile default). Persisted in the save.</summary>
        public bool AutoFire { get; set; } = true;

        private void Awake()
        {
            BuildActions();
        }

        private void OnEnable()
        {
            _move.Enable();
            _fire.Enable();
            _ultimate.Enable();
            _pause.Enable();
            _confirm.Enable();
        }

        private void OnDisable()
        {
            _move.Disable();
            _fire.Disable();
            _ultimate.Disable();
            _pause.Disable();
            _confirm.Disable();
        }

        private void OnDestroy()
        {
            _move?.Dispose();
            _fire?.Dispose();
            _ultimate?.Dispose();
            _pause?.Dispose();
            _confirm?.Dispose();
        }

        private void BuildActions()
        {
            _move = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
            _move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/s").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/a").With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/d").With("Right", "<Keyboard>/rightArrow");
            _move.AddBinding("<Gamepad>/leftStick");
            _move.AddBinding("<Gamepad>/dpad");

            _fire = new InputAction("Fire", InputActionType.Button);
            _fire.AddBinding("<Keyboard>/space");
            _fire.AddBinding("<Gamepad>/buttonSouth");
            _fire.AddBinding("<Gamepad>/rightTrigger");

            _ultimate = new InputAction("Ultimate", InputActionType.Button);
            _ultimate.AddBinding("<Keyboard>/e");
            _ultimate.AddBinding("<Gamepad>/buttonWest");
            _ultimate.AddBinding("<Gamepad>/leftTrigger");

            _pause = new InputAction("Pause", InputActionType.Button);
            _pause.AddBinding("<Keyboard>/escape");
            _pause.AddBinding("<Gamepad>/start");

            _confirm = new InputAction("Confirm", InputActionType.Button);
            _confirm.AddBinding("<Keyboard>/enter");
            _confirm.AddBinding("<Keyboard>/space");
            _confirm.AddBinding("<Gamepad>/buttonSouth");
            _confirm.AddBinding("<Gamepad>/start");
        }

        private void Update()
        {
            Vector2 axis = _move.ReadValue<Vector2>();
            MoveAxis = Vector2.ClampMagnitude(axis, 1f);

            FireHeld = AutoFire || _fire.IsPressed() || _uiFireHeld || _dragActive;
            UltimatePressed = _ultimate.WasPressedThisFrame() || _uiUltimate;
            PausePressed = _pause.WasPressedThisFrame() || _uiPause;
            ConfirmPressed = _confirm.WasPressedThisFrame() || _uiConfirm || TapThisFrame();

            _dragDelta = _dragDeltaAccumulated;
            _dragDeltaAccumulated = Vector2.zero;
            _uiUltimate = false;
            _uiPause = false;
            _uiConfirm = false;
        }

        private static bool TapThisFrame()
        {
            var pointer = Pointer.current;
            return pointer != null && pointer.press.wasPressedThisFrame;
        }

        // ---- Touch / UI feeds --------------------------------------------------------------

        public void BeginDrag() => _dragActive = true;

        public void FeedDrag(Vector2 screenDelta) => _dragDeltaAccumulated += screenDelta;

        public void EndDrag() => _dragActive = false;

        public void PressUltimate() => _uiUltimate = true;

        public void PressPause() => _uiPause = true;

        public void PressConfirm() => _uiConfirm = true;

        public void SetFireHeld(bool held) => _uiFireHeld = held;

        /// <summary>Clears transient state (used on pause/unpause and respawn).</summary>
        public void ResetTransient()
        {
            _dragActive = false;
            _dragDelta = Vector2.zero;
            _dragDeltaAccumulated = Vector2.zero;
            _uiFireHeld = false;
        }
    }
}
