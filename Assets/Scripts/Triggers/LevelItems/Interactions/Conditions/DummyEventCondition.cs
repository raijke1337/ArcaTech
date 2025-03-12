using Arcatech.Triggers;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Level
{
    [CreateAssetMenu(fileName = "Dummy check", menuName = "Level/Event Condition/Always succeed", order = 1)]
    public class DummyEventCondition : EventCondition
    {
        public override bool PerformConditionChecks(IInteractible user, IInteractible target, Transform place)
        {
            return true;
        }
    }

}