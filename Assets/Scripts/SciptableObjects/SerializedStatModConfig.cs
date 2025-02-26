
using Arcatech.Stats;
using UnityEngine;
using UnityEngine.Assertions;
namespace Arcatech.Triggers
{
    [CreateAssetMenu(fileName = "New Serialized Stat Mod", menuName = "Items/Stats/Stat mod", order = 1)]
    public class SerializedStatModConfig : ScriptableObject
    {
        [SerializeField] BaseStatType _stat;
        [SerializeField] SerializedStatModCondition _condition;
        [SerializeField] int _initValue; // value change
        [SerializeField] int _perSecValue;

        private void OnValidate()
        {
            Assert.IsFalse(_initValue == 0);
        }
        public BaseStatType GetStatType { get => _stat; }
        public int GetBaseValue { get => _initValue; }
        public int GetPerSecValue { get => _perSecValue; }

        public bool CheckCondition(StatValueContainer cont)
        {
            if (_condition != null)
            {
                return _condition.CheckCondition(cont);
            }
            else return true;
        }
    }

    public abstract class SerializedStatModCondition : ScriptableObject
    {
        public abstract bool CheckCondition(StatValueContainer c);
    }


}