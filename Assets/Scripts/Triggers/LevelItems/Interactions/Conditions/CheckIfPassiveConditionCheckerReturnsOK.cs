using Arcatech.Triggers;
using UnityEngine;

namespace Arcatech.Level
{
    [CreateAssetMenu(fileName = "Check if a passive condition fulfulls", menuName = "Level/Event Condition/Passive cond. check")]
    public class CheckIfPassiveConditionCheckerReturnsOK : EventCondition
    {

        public override bool PerformConditionChecks(IInteractible user, IInteractible target, Transform place)
        {
            throw new System.NotImplementedException();
        }
    }
}