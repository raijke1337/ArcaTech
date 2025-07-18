using Arcatech.Triggers;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Level.Conditions
{
    [CreateAssetMenu(fileName = "Dummy check", menuName = "Level/Event Condition/Always succeed", order = 1)]
    public class DummyEventCondition : EventCondition
    {
        public override ConditionCheckResult PerformConditionChecks(IInteractible user, IInteractible target, Transform place)
        {
            return ConditionCheckResult.Success;
        }
    }


}