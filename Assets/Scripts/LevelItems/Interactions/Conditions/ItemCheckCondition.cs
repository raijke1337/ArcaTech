using Arcatech.Items;
using Arcatech.Triggers;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Level.Conditions
{
    [CreateAssetMenu(fileName = "Check item in inventory", menuName = "Level/Event Condition/Item check", order = 2)]
    public class ItemCheckCondition : EventCondition
    {
        [SerializeField] ItemSO _checked;
        public override ConditionCheckResult PerformConditionChecks(IInteractible user, IInteractible target, Transform place)
        {
            if (user is EquippedUnit eq)
            {
                var ok = eq.HasItem(_checked);
                Debug.Log(ok);
                return ConditionCheckResult.Success;
            }
            else return ConditionCheckResult.Fail;
        }
    }
}