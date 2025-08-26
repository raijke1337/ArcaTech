
using Arcatech.Stats;
using UnityEngine;
using UnityEngine.Assertions;
namespace Arcatech.Stats
{
    [CreateAssetMenu(fileName = "New Serialized Stat Mod", menuName = "Items/Stats/Stat mod", order = 1)]
    public class SerializedStatModConfig : ScriptableObject
    {
        [SerializeField] BaseStatType _stat;

        SerializableGuid _guid;
        [Space, SerializeField] SerializedStatModCondition _condition;

        [SerializeField] int _changeMax;
        [SerializeField] int _changeInit;
        [SerializeField] int _changePerSec;

        public StatsMod BuildMod { get
            {
                return new StatsMod(_stat, _condition, _changeMax, _changeInit, _changePerSec, _guid);
            }
        }
        private void OnValidate()
        {
            Assert.IsNotNull(_condition, $"Set some condition for {this}");
            _guid = SerializableGuid.NewGuid();
        }
    }

    public class StatsMod
    {
        internal StatsMod() { }
        public StatsMod(BaseStatType type, SerializedStatModCondition cond, int max, int init, int persec,SerializableGuid id)
        {
            GetStatType = type; condition = cond; GetMaxValue = max; GetInitValue = init; GetPerSecValue = persec; ID = id;
        }
        SerializedStatModCondition condition;
        public BaseStatType GetStatType { get; }
        public int GetMaxValue { get ; }
        public int GetPerSecValue { get ; }
        public int GetInitValue { get ; }

        public SerializableGuid ID { get; }

        public bool CheckCondition(StatValueContainer cont)
        {
            if (condition != null)
            {
                return condition.CheckCondition(cont);
            }
            else return true;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is StatsMod s)) return false;
            else if (s.ID.Equals(ID)) return true;
            return false;
        }

    }

    public abstract class SerializedStatModCondition : ScriptableObject
    {
        public abstract bool CheckCondition(StatValueContainer c);
    }


}