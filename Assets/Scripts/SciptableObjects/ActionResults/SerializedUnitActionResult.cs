using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Actions
{
    [CreateAssetMenu(fileName = "New unit action result ", menuName = "Actions/Action Result/Do unit action")]
    public class SerializedUnitActionResult : SerializedActionResult
    {
        [SerializeField] SerializedUnitState state;

        public override IActionResult BuildActionResult()
        {
            return new UnitActionResult(state);
        }
    }

    public class UnitActionResult : IActionResult
    {
        SerializedUnitState act;
        public UnitActionResult (SerializedUnitState a)
        {
            act = a;
        }
        public void ProduceResult(BaseGameEntityComponent user, BaseGameEntityComponent target, Transform place)
        {
            if (user.TryGetComponent<ActiveGameUnitComponent>(out var actor))
            {

                actor.ForceUnitState(act.DeserializeState(actor, place));
            }
        }
    }

}