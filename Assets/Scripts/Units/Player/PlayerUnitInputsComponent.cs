using Arcatech.EventBus;
using Arcatech.Scenes.Cameras;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Units.Inputs
{
    [RequireComponent(typeof(PlayerAimingComponent), typeof(DashJumpMovementController),typeof(PlayerUnit))]
    public class PlayerUnitInputsComponent : ActiveUnitsInputComponent
    {
        [Space,Header("Player inputs")]
        [SerializeField, Anywhere] PlayerInputReaderObject _playerInputReader;
        [SerializeField, Self] PlayerAimingComponent _aim;
        [SerializeField, Self] DashJumpMovementController _movement;
        [SerializeField, Self] PlayerUnit _player;
        [SerializeField] SerializedUnitAction _jump;


        [SerializeField] float _interactRange = 3f;
        public PlayerAimingComponent Aiming => _aim;
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
                _playerInputReader.MountAction += OnMountButton;
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
                _playerInputReader.MountAction -= OnMountButton;
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


        private void OnMountButton()
        {
            // interact button pressed
        }
        private void OnPauseButton()
        {
            EventBus<PauseToggleEvent>.Raise(new PauseToggleEvent());
        }

        private void OnShieldSkill()
        {
            RequestCombatAction(UnitActionType.ShieldSkill);
        }
        private void OnRangedSkill()
        {
            RequestCombatAction(UnitActionType.RangedSkill);
        }

        private void OnMeleeSkill()
        {
            RequestCombatAction(UnitActionType.MeleeSkill);
        }

        private void OnJumpAction()
        {
            
            transform.parent = null;
            _movement.DoJump();
            _player.ForceUnitAction(_jump.ProduceAction(_player, transform));
        }

        private void OnDodgeSkill()
        {
            RequestCombatAction(UnitActionType.DodgeSkill);
        }

        private void OnMovementAction(Vector2 dir)
        {

            Vector3 AD = _adj.Isoright * _playerInputReader.InputDirection.x;
            Vector3 WS = _adj.Isoforward * _playerInputReader.InputDirection.z;

            _movementVector = AD + WS;
        }

        private void OnAimAction(Vector2 point)
        {
            _lookVector = _aim.GetDirectionToTarget;
        }

        private void OnRangedAction()
        {
            RequestCombatAction(UnitActionType.Ranged);
        }

        private void OnMeleeAction()
        {
            RequestCombatAction(UnitActionType.Melee);
        }

        #endregion

    }
}