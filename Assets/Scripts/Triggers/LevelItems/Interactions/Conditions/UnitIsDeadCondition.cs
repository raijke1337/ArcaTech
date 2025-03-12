using Arcatech.Triggers;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Level
{
    [CreateAssetMenu(fileName = "Check if unit is dead", menuName = "Level/Event Condition/Unit died", order = 3)]
    public class UnitIsDeadCondition : EventCondition
    {//checks target
        public override bool PerformConditionChecks(IInteractible user, IInteractible target, Transform place)
        {
            if (target is BaseEntity entity)  return entity.UnitDead || !entity.isActiveAndEnabled;
            else return false;
        }
    }
}