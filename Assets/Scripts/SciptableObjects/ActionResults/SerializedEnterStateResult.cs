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
        SerializedUnitState serState;
        private UnitState state;
        
        public EnterStateActionResult (SerializedUnitState a)
        {
            serState = a;
        }
        public void ProduceResult(BaseGameEntityComponent user, BaseGameEntityComponent target, Transform place)
        {
            if (user.TryGetComponent<ActiveGameUnitComponent>(out var actor))
            {
                state ??= serState.DeserializeState(actor, place);
                actor.ForceUnitState(state);
            }
        }
    }

}