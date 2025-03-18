using Arcatech.Triggers;
using UnityEngine;

namespace Arcatech.Level.Conditions
{
    [CreateAssetMenu(fileName = "Invert triggered state", menuName = "Level/Event Condition/Invert triggered state")]
    public class TriggeredStateInverter : EventCondition
    {
        public override ConditionCheckResult PerformConditionChecks(IInteractible user, IInteractible target, Transform place)
        {
            if (target.Triggered)
            {
                return ConditionCheckResult.Fail;
            }
            else
            {
                return ConditionCheckResult.Success;
            }
        }
    }


}