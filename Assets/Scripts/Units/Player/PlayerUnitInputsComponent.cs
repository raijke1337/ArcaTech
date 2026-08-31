using Arcatech.Cameras;
using Arcatech.EventBus;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

namespace Arcatech.Units.Control
{
    [RequireComponent(typeof(PlayerAimingComponent))]
    [RequireComponent(typeof(PlayerInputGateway))]
    public sealed class PlayerUnitInputsComponent :
        UnitInputsComponent
    {
        [Header("Required components")]
        [SerializeField, Self]
        private PlayerInputGateway inputGateway;

        [SerializeField, Self]
        private PlayerAimingComponent aiming;

        private IMove _mover;
        private IAim _aim;
        private readonly IsoCamAdjust _cameraAdjust = new();

        private bool _inCameraBlend;

        protected override void Awake()
        {
            base.Awake();
            _mover = GetComponent<IMove>();
            _aim  = GetComponent<IAim>();
            aiming.Initialize(inputGateway);
        }

        private void OnEnable()
        {
            inputGateway.MoveChanged += OnMoveChanged;

            inputGateway.MeleeAttackPressed += OnMeleeAttack;
            inputGateway.RangedAttackPressed += OnRangedAttack;
            inputGateway.MeleeSkillPressed += OnMeleeSkill;
            inputGateway.RangedSkillPressed += OnRangedSkill;
            inputGateway.ShieldSkillPressed += OnShieldSkill;
            inputGateway.DodgePressed += OnDodge;
            inputGateway.JumpPressed += OnJump;
            inputGateway.InteractPressed += OnInteract;
            inputGateway.PausePressed += OnPause;
            inputGateway.CameraRotateLeftPressed += () => RotateCamera(true);
            inputGateway.CameraRotateRightPressed += () => RotateCamera(false);
            
            
            // CinemachineCore.BlendCreatedEvent
            //     .AddListener(OnCameraBlendStarted);
            //
            // CinemachineCore.BlendFinishedEvent
            //     .AddListener(OnCameraBlendFinished);

            _cameraAdjust.UpdateBasis();
            
            
        }

        private void RotateCamera(bool clockwise)
        {
            CamerasController.Instance.SwitchCamera(
                clockwise,
                () =>
                { 
                    _cameraAdjust.UpdateBasis();
                });
        }


        private void OnDisable()
        {
            if (inputGateway != null)
            {
                inputGateway.MoveChanged -= OnMoveChanged;

                inputGateway.MeleeAttackPressed -= OnMeleeAttack;
                inputGateway.RangedAttackPressed -= OnRangedAttack;
                inputGateway.MeleeSkillPressed -= OnMeleeSkill;
                inputGateway.RangedSkillPressed -= OnRangedSkill;
                inputGateway.ShieldSkillPressed -= OnShieldSkill;
                inputGateway.DodgePressed -= OnDodge;
                inputGateway.JumpPressed -= OnJump;
                inputGateway.InteractPressed -= OnInteract;
                inputGateway.PausePressed -= OnPause;
                
                inputGateway.CameraRotateLeftPressed -= () => RotateCamera(true);
                inputGateway.CameraRotateRightPressed -= () => RotateCamera(false);
            }

            // CinemachineCore.BlendCreatedEvent
            //     .RemoveListener(OnCameraBlendStarted);
            //
            // CinemachineCore.BlendFinishedEvent
            //     .RemoveListener(OnCameraBlendFinished);
        }

        private void Update()
        {
            if (_inCameraBlend)
                return;

            if (_mover != null)
            {
                _mover.MovementVector = InputMovement;
                _mover.IsGamepadInput = inputGateway.IsGamepad;
            }

            if (_aim != null)
            {
                _aim.HasLockedTarget = aiming.CurrentTarget;
            }
        }

        private void OnMoveChanged(Vector2 input)
        {
            if (_inCameraBlend)
            {
                SetMovement(Vector3.zero);
                return;
            }

            Vector3 localDirection = new Vector3(
                input.x,
                0f,
                input.y);

            SetMovement(RotateInput(localDirection));
        }

        private void OnMeleeAttack()
        {
            RequestCombatAction(UnitActionType.Melee);
        }

        private void OnRangedAttack()
        {
            RequestCombatAction(UnitActionType.Ranged);
        }

        private void OnMeleeSkill()
        {
            RequestCombatAction(UnitActionType.MeleeSkill);
        }

        private void OnRangedSkill()
        {
            RequestCombatAction(UnitActionType.RangedSkill);
        }

        private void OnShieldSkill()
        {
            RequestCombatAction(UnitActionType.ShieldSkill);
        }

        private void OnJump()
        {
            RequestCombatAction(UnitActionType.Jump);
        }

        private void OnInteract()
        {
            RequestCombatAction(UnitActionType.Use);
        }

        private void OnDodge()
        {
            if (InputMovement.sqrMagnitude < 0.01f)
                return;

            UnitCommand command = new UnitCommand(
                UnitActionType.DodgeSkill,
                InputMovement);

            RequestCombatAction(command);
        }

        private void OnPause()
        {
            EventBus<PauseToggleEvent>.Raise(
                new PauseToggleEvent(!Paused));
        }

        private void SetMovement(Vector3 movement)
        {
            InputMovement = movement;

            if (_mover != null)
                _mover.MovementVector = InputMovement;
        }

        private Vector3 RotateInput(Vector3 input)
        {
            Vector3 move =
                _cameraAdjust.Isoright * input.x +
                _cameraAdjust.Isoforward * input.z;

            return Vector3.ClampMagnitude(move, 1f);
        }

        // private void OnCameraBlendStarted(
        //     CinemachineCore.BlendEventParams parameters)
        // {
        //     _inCameraBlend = true;
        //     SetMovement(Vector3.zero);
        // }
        //
        // private void OnCameraBlendFinished(
        //     ICinemachineMixer mixer,
        //     ICinemachineCamera camera)
        // {
        //     _inCameraBlend = false;
        //     _cameraAdjust.UpdateBasis();
        // }

        private sealed class IsoCamAdjust
        {
            public Vector3 Isoforward { get; private set; } =
                Vector3.forward;

            public Vector3 Isoright { get; private set; } =
                Vector3.right;

            public void UpdateBasis()
            {
              //  Debug.Log("UpdateBasis");
                Camera camera = Camera.main;

                if (camera == null)
                    return;

                Vector3 forward = camera.transform.forward;
                forward.y = 0f;

                if (forward.sqrMagnitude < 0.0001f)
                    return;

                Isoforward = forward.normalized;
                Isoright = Vector3.Cross(
                    Vector3.up,
                    Isoforward).normalized;
            }
        }
        
        
    }
}