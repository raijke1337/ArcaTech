using System;
using Arcatech.EventBus;
using Arcatech.Interactions;
using Arcatech.Scenes.Cameras;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Arcatech.Units.Control
{
    [RequireComponent(typeof(PlayerAimingComponent))]
    [RequireComponent(typeof(InteractionComponent))]
    public class PlayerUnitInputsComponent : UnitInputsComponent
    {
        [Space, Header("Required components")] [SerializeField, Anywhere]
        PlayerInputReaderObject _playerInputReader;

        [SerializeField, Self] protected InteractionComponent _interaction;
        private IPlayerMovementController _playerMovementController; // just set the vector

        protected override void ControllerStartBindings(bool enabling)
        {
            _adj ??= new IsoCamAdjust();

            if (enabling)
            {
                _playerMovementController = GetComponent<IPlayerMovementController>();
                _playerInputReader.PausePressed += OnPause;
                _playerInputReader.CombatAction += OnCombatAction;
                _playerInputReader.UseAction += OnUseAction;
            }
            else
            {
                _playerInputReader.PausePressed -= OnPause;
                _playerInputReader.CombatAction -= OnCombatAction;
                _playerInputReader.UseAction -= OnUseAction;
            }

            base.ControllerStartBindings(enabling);
        }

        private void OnUseAction(InputAction.CallbackContext arg0) => _interaction.InteractCommand();

        private void OnCombatAction(InputAction.CallbackContext ctx, UnitActionType type)
        {
            if (type == UnitActionType.Movement)
            {
                OnMovement(ctx);
                RequestCombatAction(type);
            }
            else 
            {
                if (ctx.performed) RequestCombatAction(type);
            }
        }
    

    private void OnPause(InputAction.CallbackContext arg0)
        {
            if (arg0.phase == InputActionPhase.Performed)
                EventBus<PauseToggleEvent>.Raise(new PauseToggleEvent(!Paused));
        }


        private void OnMovement(InputAction.CallbackContext arg0)
        {
            Vector3 direction = Vector3.forward;
            switch (arg0.phase)
            {
                case InputActionPhase.Performed:
                    var x =arg0.ReadValue<Vector2>().x;
                    var z =arg0.ReadValue<Vector2>().y;
                    direction.x = x;
                    direction.z = z;
                    InputMovement = RotateInput(direction);

                    break;
                default:
                    InputMovement = Vector3.zero;
                    break;
            }
            _playerMovementController.MovementVector = InputMovement;
        }
        
        private IsoCamAdjust _adj;
        private Vector3 RotateInput(Vector3 input)
        {
            var AD = _adj.Isoright * input.x;
            var WS = _adj.Isoforward * input.z;
            
            return AD + WS;
        }
        
    }
}