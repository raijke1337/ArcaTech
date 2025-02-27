
using Arcatech.Stats;
using UnityEngine;
using UnityEngine.Assertions;
namespace Arcatech.Triggers
{
    [CreateAssetMenu(fileName = "New Serialized Stat Mod", menuName = "Items/Stats/Stat mod", order = 1)]
    public class SerializedStatModConfig : ScriptableObject
    {
        [SerializeField] BaseStatType _stat;
        

        [SerializeField] int MaxValueChange;
        [SerializeField] int InitValueChange;
        [Space, SerializeField] SerializedStatModCondition PerSecondValueChangeCondition;
        [SerializeField] int PerSecondValueChange;


        public BaseStatType GetStatType { get => _stat; }
        public int GetMaxValue { get => MaxValueChange; }
        public int GetPerSecValue { get => PerSecondValueChange; }
        public int GetInitValue { get => InitValueChange; }

        public bool CheckCondition(StatValueContainer cont)
        {
            if (PerSecondValueChangeCondition != null)
            {
                return PerSecondValueChangeCondition.CheckCondition(cont);
            }
            else return true;
        }
    }

    public abstract class SerializedStatModCondition : ScriptableObject
    {
        public abstract bool CheckCondition(StatValueContainer c);
    }


}