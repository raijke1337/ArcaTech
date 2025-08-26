using UnityEngine;
namespace Arcatech.Stats
{
    [CreateAssetMenu(fileName = "New Serialized Stat Mod Condition", menuName = "Items/Stats/Stat mod condition/Dummy Condition", order = 1)]
    public class DummyCondition : SerializedStatModCondition
    {
        [SerializeField] bool result;
        public override bool CheckCondition(StatValueContainer c)
        {
            return result;
        }
    }

}