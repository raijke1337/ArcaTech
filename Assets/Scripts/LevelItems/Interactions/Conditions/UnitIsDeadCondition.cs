using Arcatech.Triggers;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Level.Conditions
{
    [CreateAssetMenu(fileName = "Check if unit is dead", menuName = "Level/Event Condition/Unit died", order = 3)]
    public class UnitIsDeadCondition : EventCondition
    {//checks target
        public override ConditionCheckResult PerformConditionChecks(IInteractible user, IInteractible target, Transform place)
        {
            if (target is BaseEntityOLD entity && (entity.UnitDead || !entity.isActiveAndEnabled)) return ConditionCheckResult.Success;
            else return ConditionCheckResult.Fail;
        }
    }
}