using Arcatech.Triggers;
using UnityEngine;

namespace Arcatech.Level.Conditions
{
    [CreateAssetMenu(fileName = "Check if a passive condition fulfulls", menuName = "Level/Event Condition/Passive cond. check")]
    public class CheckIfPassiveConditionCheckerReturnsOK : EventCondition
    {

        public override ConditionCheckResult PerformConditionChecks(IInteractible user, IInteractible target, Transform place)
        {
            throw new System.NotImplementedException();
        }
    }
}