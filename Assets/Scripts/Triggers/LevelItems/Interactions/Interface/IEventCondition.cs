using Arcatech.Triggers;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Level
{
    public interface IEventCondition
    {
        bool PerformConditionChecks(IInteractible user, IInteractible target, Transform place); // just in case
    }

    public abstract class EventCondition : ScriptableObject, IEventCondition
    {
        public abstract bool PerformConditionChecks(IInteractible user, IInteractible target, Transform place);
    }

}