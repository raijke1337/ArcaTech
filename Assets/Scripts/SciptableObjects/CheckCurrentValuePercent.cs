using UnityEngine;
namespace Arcatech.Stats
{
    [CreateAssetMenu(fileName = "New Serialized Stat Mod Condition", menuName = "Items/Stats/Stat mod condition/Check current value", order = 2)]
    public class CheckCurrentValuePercent : SerializedStatModCondition
    {
        [SerializeField] Comparer Comparison;
        [SerializeField, Range (0,100)] float PercentCutoff;
        public override bool CheckCondition(StatValueContainer c)
        {
            switch (Comparison)
            {
                case Comparer.Greater:
                    return (c.GetPercent > PercentCutoff/100);
                case Comparer.Less:
                    return (c.GetPercent < PercentCutoff/100);
            }
            return false;
        }
    }


    
}