using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Actions
{
    [CreateAssetMenu(fileName = "New unit action result ", menuName = "Actions/Action Result/Do unit action")]
    public class SerializedUnitActionResult : SerializedActionResult
    {
        [SerializeField] SerializedUnitAction _action;

        public override IActionResult BuildActionResult()
        {
            return new UnitActionResult(_action);
        }
    }

    public class UnitActionResult : IActionResult
    {
        SerializedUnitAction act;
        public UnitActionResult (SerializedUnitAction a)
        {
            act = a;
        }
        public void ProduceResult(BaseEntity user, BaseEntity target, Transform place)
        {
            user.ForceUnitAction(act.ProduceAction(user, place));
        }
    }

}