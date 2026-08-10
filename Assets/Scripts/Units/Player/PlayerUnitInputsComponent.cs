using Arcatech.EventBus;
using Arcatech.SaveSystem;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

namespace Arcatech.Units.Control
{
    [RequireComponent(typeof(PlayerAimingComponent))]
    public class PlayerUnitInputsComponent : UnitInputsComponent
    {
        [Space, Header("Required components")] 
        [SerializeField, Anywhere]
        private PlayerInputReaderObject playerInputReader;

        private IMove _mover;
        private readonly IsoCamAdjust _adj = new IsoCamAdjust();

        private bool _inCameraBlend = false;
        
        protected override void ControllerStartBindings(bool enabling)
        {
            if (enabling)
            {
                _mover = GetComponent<IMove>();
                playerInputReader.PausePressed += OnPause;
                playerInputReader.CombatAction += OnValidatedStateMachineAction;
                
                // Подписываемся на событие обновления активной камеры Cinemachine
                // CameraActivatedEvent вызывается ТОЛЬКО когда Brain переключает виртуальную камеру
               
                CinemachineCore.BlendCreatedEvent.AddListener(OnCameraBlendStarted);
                CinemachineCore.BlendFinishedEvent.AddListener(OnCameraBlendCompleted);
                
                // Форсируем первичный расчет, чтобы не ждать первого переключения
                _adj.UpdateBasis();
                
            }
            else
            {
                playerInputReader.PausePressed -= OnPause;
                playerInputReader.CombatAction -= OnValidatedStateMachineAction;
                
                // ОБЯЗАТЕЛЬНО отписываемся от статического события!
                CinemachineCore.BlendCreatedEvent.RemoveListener(OnCameraBlendStarted);
                CinemachineCore.BlendFinishedEvent.RemoveListener(OnCameraBlendCompleted);
            }

            base.ControllerStartBindings(enabling);
        }

        private void OnCameraBlendStarted(CinemachineCore.BlendEventParams arg0)
        {
            _inCameraBlend = true;
            InputMovement = Vector3.zero;
            if (_mover != null)
                _mover.MovementVector = InputMovement;
        }

        private void OnCameraBlendCompleted(ICinemachineMixer m, ICinemachineCamera c)
        {
            _inCameraBlend = false;
            _adj.UpdateBasis();
        }

        private void OnValidatedStateMachineAction(InputAction.CallbackContext ctx, UnitActionType type)
        {
            if (_inCameraBlend) return;
            if (type == UnitActionType.Movement)
            {
                UpdateCachedInputVector(ctx);
                
                if (_mover != null && _mover.ActualMovementVelocity <= 0.1f) 
                    RequestCombatAction(type);
            }
            else 
            {
                if (ctx.performed) 
                    RequestCombatAction(type);
            }
        }

        private void OnPause(InputAction.CallbackContext arg0)
        {
            if (arg0.phase == InputActionPhase.Performed)
                EventBus<PauseToggleEvent>.Raise(new PauseToggleEvent(!Paused));
        }

        private void Update()
        {
            if (_inCameraBlend) return;
            if (_mover != null) 
                _mover.MovementVector = InputMovement;
        }

        private void UpdateCachedInputVector(InputAction.CallbackContext arg0)
        {
            switch (arg0.phase)
            {
                case InputActionPhase.Performed:
                    Vector2 rawInput = arg0.ReadValue<Vector2>();
                    Vector3 localDirection = new Vector3(rawInput.x, 0f, rawInput.y);
                    
                    // RotateInput теперь просто использует закэшированные значения — без пересчетов!
                    InputMovement = RotateInput(localDirection);
                    break;
                    
                default:
                    InputMovement = Vector3.zero;
                    break;
            }

            if (_mover != null)
                _mover.MovementVector = InputMovement;
        }
        
        private Vector3 RotateInput(Vector3 input)
        {
            // Больше никаких вызовов UpdateBasis() здесь — используем закэшированный базис
            var move = (_adj.Isoright * input.x) + (_adj.Isoforward * input.z);
            return move.sqrMagnitude > 0.0001f ? Vector3.Normalize(move) : Vector3.zero;
        }

        internal class IsoCamAdjust
        {
            public Vector3 Isoforward { get; private set; } = Vector3.forward;
            public Vector3 Isoright { get; private set; } = Vector3.right;

            public void UpdateBasis()
            {
                if (Camera.main == null) 
                    return; 

                Vector3 camForward = Camera.main.transform.forward;
                Vector3 flatForward = new Vector3(camForward.x, 0f, camForward.z);
                
                if (flatForward.sqrMagnitude < 0.0001f)
                    return;

                Isoforward = flatForward.normalized;
                Isoright = Vector3.Cross(Vector3.up, Isoforward).normalized;
            }
        }
    }
}