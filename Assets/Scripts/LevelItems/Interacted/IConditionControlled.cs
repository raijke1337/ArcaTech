using UnityEngine;

namespace Arcatech.Level.Conditions
{

    public interface IConditionControlled
    {
        public void SetState(ConditionCheckResult newstate);
    }

}