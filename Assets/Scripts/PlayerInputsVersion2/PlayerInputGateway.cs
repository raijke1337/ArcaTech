using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Arcatech.Units.Control
{
    [DefaultExecutionOrder(-100)]
    public sealed class PlayerInputGateway :
        MonoBehaviour,
        PlayerControls.IGameActions
    {
        [Header("Input settings")]
        [SerializeField] private float moveDeadZone = 0.15f;
        [SerializeField] private float aimDeadZone = 0.2f;

        private PlayerControls _controls;

        public event Action<Vector2> MoveChanged = delegate { };
        public event Action<Vector2> AimChanged = delegate { };

        public event Action MeleeAttackPressed = delegate { };
        public event Action RangedAttackPressed = delegate { };
        public event Action MeleeSkillPressed = delegate { };
        public event Action RangedSkillPressed = delegate { };
        public event Action ShieldSkillPressed = delegate { };
        public event Action DodgePressed = delegate { };
        public event Action JumpPressed = delegate { };
        public event Action InteractPressed = delegate { };
        public event Action PausePressed = delegate { };

        public event Action CameraRotateLeftPressed = delegate { };
        public event Action CameraRotateRightPressed = delegate { };

        public Vector2 MoveInput { get; private set; }
        public Vector2 AimInput { get; private set; }

        public bool CameraModifierHeld { get; private set; }

        public bool IsGamepad { get; private set; }

        private void Awake()
        {
            _controls = new PlayerControls();
            _controls.Game.SetCallbacks(this);
        }

        private void OnEnable()
        {
            _controls.Game.Enable();
        }

        private void OnDisable()
        {
            MoveInput = Vector2.zero;
            AimInput = Vector2.zero;
            CameraModifierHeld = false;

            _controls.Game.Disable();
        }

        private void OnDestroy()
        {
            _controls.Game.SetCallbacks(null);
            _controls.Dispose();
        }

        public void OnMoveDirection(InputAction.CallbackContext context)
        {
            IsGamepad = context.control.device is Gamepad;

            Vector2 value = context.ReadValue<Vector2>();
            MoveInput = ApplyDeadZone(value, moveDeadZone);

            MoveChanged.Invoke(MoveInput);
        }

        public void OnAimDirection(InputAction.CallbackContext context)
        {
            IsGamepad = context.control.device is Gamepad;

            // Для мыши action Aim должен быть Vector2 и читать Mouse Position.
            Vector2 value = context.ReadValue<Vector2>();

            if (IsGamepad)
                AimInput = ApplyDeadZone(value, aimDeadZone);
            else
                AimInput = value;

            AimChanged.Invoke(AimInput);
        }

        public void OnMeleeWeaponAtk(InputAction.CallbackContext context)
        {
            if (context.performed)
                MeleeAttackPressed.Invoke();
        }

        public void OnRangedWeaponAtk(InputAction.CallbackContext context)
        {
            if (context.performed)
                RangedAttackPressed.Invoke();
        }

        public void OnSpecialMelee(InputAction.CallbackContext context)
        {
            if (context.performed)
                MeleeSkillPressed.Invoke();
        }

        public void OnSpecialRanged(InputAction.CallbackContext context)
        {
            if (context.performed)
                RangedSkillPressed.Invoke();
        }

        public void OnSpecialBattery(InputAction.CallbackContext context)
        {
            if (context.performed)
                ShieldSkillPressed.Invoke();
        }

        public void OnEvasion(InputAction.CallbackContext context)
        {
            if (context.performed)
                DodgePressed.Invoke();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
                JumpPressed.Invoke();
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed)
                InteractPressed.Invoke();
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            if (context.performed)
                PausePressed.Invoke();
        }

        public void OnCameraRotateCCW(InputAction.CallbackContext context){
            if (context.performed)
                CameraRotateLeftPressed.Invoke();
        }

        public void OnCameraRotateCW(InputAction.CallbackContext context)
        {
            if (context.performed)
                CameraRotateRightPressed.Invoke();
        }

        private static Vector2 ApplyDeadZone(Vector2 value, float deadZone)
        {
            float magnitude = value.magnitude;

            if (magnitude <= deadZone)
                return Vector2.zero;

            float normalizedMagnitude = Mathf.InverseLerp(
                deadZone,
                1f,
                Mathf.Clamp01(magnitude));

            return value.normalized * normalizedMagnitude;
        }
    }
}