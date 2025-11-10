using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Actions
{
    [CreateAssetMenu(fileName = "New unit enters state action result ", menuName = "Actions/Action Result/Enter action state")]
    public class SerializedEnterStateResult : SerializedActionResult
    {
        [SerializeField] SerializedUnitState state;

        public override IActionResult BuildActionResult()
        {
            return new EnterStateActionResult(state);
        }
    }

    public class EnterStateActionResult : IActionResult
    {
        private UnitState state;
        
        public EnterStateActionResult (SerializedUnitState a)
        {
            state = a.Build();
        }
        public void ProduceResult(BaseGameEntityComponent user, BaseGameEntityComponent target, Transform place)
        {
            if (user.TryGetComponent<EntityStateMachineComponent>(out var actor))
            {
                actor.ForceUnitState(state);
            }
        }
    }

}