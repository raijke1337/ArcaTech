using Arcatech.Triggers;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Level.Conditions
{
    public interface IEventCondition
    {
        ConditionCheckResult PerformConditionChecks(IInteractible user, IInteractible target, Transform place); // just in case
    }

    public abstract class EventCondition : ScriptableObject, IEventCondition
    {
        public abstract ConditionCheckResult PerformConditionChecks(IInteractible user, IInteractible target, Transform place);
    }

}