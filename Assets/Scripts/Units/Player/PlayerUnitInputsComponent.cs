using Arcatech.EventBus;
using Arcatech.Interactions;
using Arcatech.Scenes.Cameras;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Units.Inputs
{
    [RequireComponent(typeof(PlayerAimingComponent), typeof(DashJumpMovementController),typeof(PlayerUnit))]
    [RequireComponent(typeof(InteractionComponent))]
    public class PlayerUnitInputsComponent : ActiveUnitsInputComponent
    {
        [Space,Header("Required components")]
        [SerializeField, Anywhere] PlayerInputReaderObject _playerInputReader;
        [SerializeField, Self] PlayerAimingComponent _aim;
        [SerializeField, Self] DashJumpMovementController _movement;
        [SerializeField, Self] PlayerUnit _player;
        [SerializeField, Self] protected InteractionComponent _interaction;

        private IsoCamAdjust _adj;

        Vector3 _movementVector;
        Vector3 _lookVector;

        protected override void ControllerStartBindings(bool enabling)
        {
            _adj ??= new IsoCamAdjust();
            _playerInputReader.EnablePlayerInputs();
            if (enabling)

            {
                _playerInputReader.Aim += OnAimAction;
                _playerInputReader.Movement += OnMovementAction;

                _playerInputReader.Melee += OnMeleeAction;
                _playerInputReader.Ranged += OnRangedAction;
                _playerInputReader.Jump += OnJumpAction;

                _playerInputReader.DodgeSpec += OnDodgeSkill;
                _playerInputReader.MeleeSpec += OnMeleeSkill;
                _playerInputReader.RangedSpec += OnRangedSkill;
                _playerInputReader.ShieldSpec += OnShieldSkill;

                _playerInputReader.PausePressed += OnPauseButton;
                _playerInputReader.MountAction += OnUseButton;
            }
            else
            {


                _playerInputReader.Aim -= OnAimAction;
                _playerInputReader.Movement -= OnMovementAction;

                _playerInputReader.Melee -= OnMeleeAction;
                _playerInputReader.Ranged -= OnRangedAction;
                _playerInputReader.Jump -= OnJumpAction;

                _playerInputReader.DodgeSpec -= OnDodgeSkill;
                _playerInputReader.MeleeSpec -= OnMeleeSkill;
                _playerInputReader.RangedSpec -= OnRangedSkill;
                _playerInputReader.ShieldSpec -= OnShieldSkill;
                _playerInputReader.PausePressed -= OnPauseButton;
                _playerInputReader.MountAction -= OnUseButton;
            }
        }

        #region inputs section

        private void Update()
        {
            if (_player.ActionLock)
            {
                _movement.SetDesiredMoveDirection(Vector3.zero);
            }
            else
            {
                _movement.SetDesiredMoveDirection(_movementVector);
                _movement.SetDesiredLookDirection(_lookVector);
            }                
        }
        /// <summary>
        /// called on unit death
        /// </summary>
        protected override void OnDisable()
        {
            _movement.SetDesiredMoveDirection(Vector3.zero);
            base.OnDisable();
        }


        private void OnUseButton()
        {
            if (Paused || Killed) return;
            _interaction.InteractCommand();
        }
        private void OnPauseButton()
        {
            EventBus<PauseToggleEvent>.Raise(new PauseToggleEvent(!Paused));
        }

        private void OnShieldSkill()
        {
            if (Paused || Killed) return;
            RequestCombatAction(UnitActionType.ShieldSkill);
        }
        private void OnRangedSkill()
        {
            if (Paused || Killed) return;
            RequestCombatAction(UnitActionType.RangedSkill);
        }

        private void OnMeleeSkill()
        {
            if (Paused || Killed) return;
            RequestCombatAction(UnitActionType.MeleeSkill);
        }

        private void OnJumpAction()
        {
            if (Paused || Killed) return;
            transform.parent = null;
            _movement.DoJump();
            _player.PlayerJump();
        }

        private void OnDodgeSkill()
        {
            if (Paused || Killed) return;
            RequestCombatAction(UnitActionType.DodgeSkill);
        }

        private void OnMovementAction(Vector2 dir)
        {
            if (Paused || Killed) return;
            
            Vector3 AD = _adj.Isoright * _playerInputReader.InputDirection.x;
            Vector3 WS = _adj.Isoforward * _playerInputReader.InputDirection.z;

            _movementVector = AD + WS;
        }

        private void OnAimAction(Vector2 point)
        {
            if (Paused || Killed) return;
            _lookVector = _aim.GetDirectionToTarget;
        }

        private void OnRangedAction()
        {
            if (Paused || Killed) return;
            RequestCombatAction(UnitActionType.Ranged);
        }

        private void OnMeleeAction()
        {
            if (Paused || Killed) return;
            RequestCombatAction(UnitActionType.Melee);
        }


        #endregion

    }
}