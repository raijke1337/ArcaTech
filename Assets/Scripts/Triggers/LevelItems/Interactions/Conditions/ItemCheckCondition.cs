using Arcatech.Items;
using Arcatech.Triggers;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Level
{
    [CreateAssetMenu(fileName = "Check item in inventory", menuName = "Level/Event Condition/Item check", order = 2)]
    public class ItemCheckCondition : EventCondition
    {
        [SerializeField] ItemSO _checked;
        public override bool PerformConditionChecks(IInteractible user, IInteractible target, Transform place)
        {
            if (user is EquippedUnit eq)
            {
                var ok = eq.HasItem(_checked);
                Debug.Log(ok);
                return ok;
            }
            else return false;
        }
    }
}