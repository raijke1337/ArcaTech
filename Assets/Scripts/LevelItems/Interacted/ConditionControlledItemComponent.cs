using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Level.Conditions
{
    public class ConditionControlledItemComponent : MonoBehaviour, IConditionControlled
    {
        [SerializeField] ConditionBehaviorStrategy strategy;
        IConditionControlledStrat _s;
        [SerializeField, Tooltip("can only produce results once for success result")] bool SucceedOnce = true;
        bool triggered = false;
        private void OnValidate()
        {
            Assert.IsNotNull(strategy);
        }
        private void OnEnable()
        {
            _s = strategy.Build(this);
        }

        public virtual void SetState(ConditionCheckResult result)
        {
            if (triggered && SucceedOnce && result == ConditionCheckResult.Success) return;
            Debug.Log(name+" "+result.ToString());
            
            _s.SetState(result);
            if (result == ConditionCheckResult.Success) { triggered = true; }
        }
    }

}