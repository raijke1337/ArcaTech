using Arcatech.Actions;
using Arcatech.Units.Control;
using UnityEngine;

namespace Arcatech.Units
{
    [CreateAssetMenu(menuName = "States/Actions/Set player movement vector")]
    public class SetMovementSO : SerializedActionResult
    {
        UnitActionType actionType;
        public override ActionResult BuildActionResult()
        {
            return new SetMovementAction();
        }
    }
    public class SetMovementAction : ActionResult
    {        
        private IMove _mover;
        private UnitInputsComponent _inputs;
        public override bool ProduceResult(BaseGameEntityComponent user, BaseGameEntityComponent target,
            Transform place)
        {
            if (_mover == null)
            {
                _mover = user.GetComponent<IMove>();
                if (_mover == null) return false;
            }

            if (_inputs == null)
            {
                _inputs = user.GetComponent<UnitInputsComponent>();
                if (_inputs == null) return false;
            }

            _mover.MovementVector = _inputs.InputMovement;
            return true;
        }
    }
}