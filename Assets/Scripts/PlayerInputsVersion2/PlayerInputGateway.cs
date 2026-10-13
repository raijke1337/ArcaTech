using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Arcatech.Units.Control
{
    /// <summary>Каким устройством игрок управляет прямо сейчас (для подсказок и иконок кнопок).</summary>
    public enum InputDeviceKind
    {
        KeyboardMouse,
        Gamepad
    }

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

        /// <summary>Устройство последнего события ввода (сырое, используется для обработки прицела).</summary>
        public bool IsGamepad { get; private set; }

        /// <summary>Устройство, которым игрок реально пользуется. Переключается только
        /// на осмысленный ввод: нажатие кнопки или отклонение стика за мёртвую зону.</summary>
        public InputDeviceKind ActiveDeviceKind { get; private set; } = InputDeviceKind.KeyboardMouse;

        /// <summary>Последнее активное устройство (нужно, чтобы отличать Xbox от DualSense).</summary>
        public InputDevice LastUsedDevice { get; private set; }

        /// <summary>Сменилось активное устройство (в том числе геймпад на другой геймпад).</summary>
        public event Action<InputDeviceKind, InputDevice> ActiveDeviceChanged = delegate { };

        /// <summary>Ассет действий этого игрока - по нему строятся подписи кнопок (учитывает ребинды).</summary>
        public InputActionAsset Actions => _controls?.asset;

        private void Awake()
        {
            _controls = new PlayerControls();
            _controls.Game.SetCallbacks(this);
        }

        private void OnEnable()
        {
            _controls.Game.Enable();
            Instance = this; // простая регистрация, см. ниже
        }

        private void OnDisable()
        {
            ResetInputValues();

            _controls.Game.Disable();
            _controls.Minigame.Disable();

            if (Instance == this) Instance = null;
        }

        private void ResetInputValues()
        {
            MoveInput = Vector2.zero;
            AimInput = Vector2.zero;
            CameraModifierHeld = false;
        }

        private void OnDestroy()
        {
            _controls.Game.SetCallbacks(null);
            _controls.Dispose();
        }
        
        #region Input mode switching

        public static PlayerInputGateway Instance { get; private set; }

        public void EnableGameplayInput()
        {
            _controls.Minigame.Disable();
            _controls.Minigame.SetCallbacks(null);
            _controls.Game.Enable();
        }

        public void EnableMinigameInput(PlayerControls.IMinigameActions actions)
        {
            _controls.Game.Disable();
            _controls.Minigame.SetCallbacks(actions);
            _controls.Minigame.Enable();
        }

        /// <summary>Выключает весь игровой ввод: движение, бой, паузу и мини-игры
        /// (например, на экране итогов уровня). Интерфейс продолжает работать через EventSystem.
        /// Вернуть ввод - EnableGameplayInput().</summary>
        public void DisableAllInput()
        {
            _controls.Game.Disable();
            _controls.Minigame.Disable();
            _controls.Minigame.SetCallbacks(null);

            // Персонаж не должен продолжать идти с последним направлением стика.
            ResetInputValues();
            MoveChanged.Invoke(Vector2.zero);
            AimChanged.Invoke(Vector2.zero);
        }

        #endregion
        

        public void OnMoveDirection(InputAction.CallbackContext context)
        {
            IsGamepad = context.control.device is Gamepad;

            Vector2 value = context.ReadValue<Vector2>();
            MoveInput = ApplyDeadZone(value, moveDeadZone);

            TrackDevice(context, MoveInput != Vector2.zero);
            MoveChanged.Invoke(MoveInput);
        }

        public void OnAimDirection(InputAction.CallbackContext context)
        {
            IsGamepad = context.control.device is Gamepad;

            Vector2 value = context.ReadValue<Vector2>();

            if (IsGamepad)
            {
                AimInput = ApplyDeadZone(value, aimDeadZone);
            }
            else
            {
                AimInput = value;
            }

            // Мышь: любое движение курсора. Стик: только за пределами мёртвой зоны.
            TrackDevice(context, IsGamepad ? AimInput != Vector2.zero : context.performed);
            AimChanged.Invoke(AimInput);
        }

        public void OnMeleeWeaponAtk(InputAction.CallbackContext context)
        {
            TrackDevice(context, context.performed);
            if (context.performed)
                MeleeAttackPressed.Invoke();
        }

        public void OnRangedWeaponAtk(InputAction.CallbackContext context)
        {
            TrackDevice(context, context.performed);
            if (context.performed)
                RangedAttackPressed.Invoke();
        }

        public void OnSpecialMelee(InputAction.CallbackContext context)
        {
            TrackDevice(context, context.performed);
            if (context.performed)
                MeleeSkillPressed.Invoke();
        }

        public void OnSpecialRanged(InputAction.CallbackContext context)
        {
            TrackDevice(context, context.performed);
            if (context.performed)
                RangedSkillPressed.Invoke();
        }

        public void OnSpecialBattery(InputAction.CallbackContext context)
        {
            TrackDevice(context, context.performed);
            if (context.performed)
                ShieldSkillPressed.Invoke();
        }

        public void OnEvasion(InputAction.CallbackContext context)
        {
            TrackDevice(context, context.performed);
            if (context.performed)
                DodgePressed.Invoke();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            TrackDevice(context, context.performed);
            if (context.performed)
                JumpPressed.Invoke();
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            TrackDevice(context, context.performed);
            if (context.performed)
                InteractPressed.Invoke();
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            TrackDevice(context, context.performed);
            if (context.performed)
                PausePressed.Invoke();
        }

        public void OnCameraRotateCCW(InputAction.CallbackContext context){
            TrackDevice(context, context.performed);
            if (context.performed)
                CameraRotateLeftPressed.Invoke();
        }

        public void OnCameraRotateCW(InputAction.CallbackContext context)
        {
            TrackDevice(context, context.performed);
            if (context.performed)
                CameraRotateRightPressed.Invoke();
        }

        private void TrackDevice(InputAction.CallbackContext context, bool meaningful)
        {
            if (!meaningful) return;

            InputDevice device = context.control?.device;
            if (device == null || device == LastUsedDevice) return;

            LastUsedDevice = device;
            ActiveDeviceKind = device is Gamepad ? InputDeviceKind.Gamepad : InputDeviceKind.KeyboardMouse;
            ActiveDeviceChanged.Invoke(ActiveDeviceKind, device);
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