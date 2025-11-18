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
        private IMove mover;
        
        protected override void ControllerStartBindings(bool enabling)
        {
            _adj ??= new IsoCamAdjust();

            if (enabling)
            {
                mover = GetComponent<IMove>();
                _playerInputReader.PausePressed += OnPause;
                _playerInputReader.CombatAction += OnValidatedStateMachineAction;
            }
            else
            {
                _playerInputReader.PausePressed -= OnPause;
                _playerInputReader.CombatAction -= OnValidatedStateMachineAction;
            }

            base.ControllerStartBindings(enabling);
        }

        private void OnValidatedStateMachineAction(InputAction.CallbackContext ctx, UnitActionType type)
        {
            if (type == UnitActionType.Movement)
            {
                UpdateCachedInputVector(ctx);
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

        private void Update()
        {
            if (mover != null) 
                mover.MovementVector = InputMovement;
            // to fix the issue with movement not continuing after using attacks or jump
        }

        private void UpdateCachedInputVector(InputAction.CallbackContext arg0)
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
            mover.MovementVector = InputMovement;
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