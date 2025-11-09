using System;
using Arcatech.EventBus;
using Arcatech.Interactions;
using Arcatech.Scenes.Cameras;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Arcatech.Units.Control
{
    [RequireComponent(typeof(PlayerAimingComponent),typeof(PlayerUnit))]
    [RequireComponent(typeof(InteractionComponent))]
    public class PlayerUnitInputsComponent : UnitInputsComponent
    {
        [Space,Header("Required components")]
        [SerializeField, Anywhere] PlayerInputReaderObject _playerInputReader;
        [SerializeField, Self] protected InteractionComponent _interaction;

        private IMove _movement;
        private IJump _jump;

        private void Start()
        {
            _movement = GetComponentInChildren<IMove>();
            _jump =  GetComponentInChildren<IJump>();
        }

        protected override void ControllerStartBindings(bool enabling)
        {
            _adj ??= new IsoCamAdjust();
            
            if (enabling)
            {
                _playerInputReader.Movement += OnMovement;
                _playerInputReader.Jump += OnJump;
                _playerInputReader.PausePressed += OnPause;
                _playerInputReader.CombatAction += OnCombatAction;
                _playerInputReader.UseAction += OnUseAction;
            }
            else
            {
                _playerInputReader.Movement -= OnMovement;
                _playerInputReader.Jump -= OnJump;
                _playerInputReader.PausePressed -= OnPause;
                _playerInputReader.CombatAction -= OnCombatAction;
                _playerInputReader.UseAction -= OnUseAction;
            }
        }

        private void OnUseAction(InputAction.CallbackContext arg0) => _interaction.InteractCommand();

        private void OnCombatAction(InputAction.CallbackContext ctx, UnitActionType type)
        {
            if (ctx.performed)
                RequestCombatAction(type);
        }

        private void OnPause(InputAction.CallbackContext arg0)
        {
            if (arg0.phase == InputActionPhase.Performed)
                EventBus<PauseToggleEvent>.Raise(new PauseToggleEvent(!Paused));
        }

        private void OnJump(InputAction.CallbackContext arg0)
        {
            switch (arg0.phase)
            {
                case InputActionPhase.Performed:
                    _jump.JumpCommand = true;
                    break;
                default:
                    _jump.JumpCommand = false;
                    break;
            }
        }

        private void OnMovement(InputAction.CallbackContext arg0)
        {
            switch (arg0.phase)
            {
                case InputActionPhase.Performed:
                    _feedVector.x =arg0.ReadValue<Vector2>().x;
                    _feedVector.z =arg0.ReadValue<Vector2>().y;
                    _movement.MovementVector = RotateInput(_feedVector);
                    break;
                default:
                    Debug.Log("reset movement vector OK");
                    _movement.MovementVector = Vector3.zero;
                    _feedVector = Vector3.zero;
                    break;
            }
        }
        
        
        private IsoCamAdjust _adj;
        Vector3 _feedVector = Vector3.zero;
        private Vector3 RotateInput(Vector3 input)
        {
            var AD = _adj.Isoright * input.x;
            var WS = _adj.Isoforward * input.z;
            
            return AD + WS;
        }
        
    }
}