using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Stats
{
    /*
    [CreateAssetMenu(fileName = "New Serialized Stat Mod", menuName = "Items/Stats/Stat mod", order = 1)]
    public class SerializedStatModConfig : ScriptableObject
    {
        [SerializeField] ResourceStatType _stat;

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
            //_guid = SerializableGuid.NewGuid();
           // Debug.Log($"assign {_guid.ToString()} to mod config {this}");
        }
    }

    public class StatsMod
    {
        internal StatsMod() { }
        public StatsMod(ResourceStatType type, SerializedStatModCondition cond, int max, int init, int persec,SerializableGuid id)
        {
            GetStatType = type; condition = cond; GetMaxValue = max; GetInitValue = init; GetPerSecValue = persec; ID = id;
        }
        SerializedStatModCondition condition;
        public ResourceStatType GetStatType { get; }
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
        // Overload the == operator
        public static bool operator == (StatsMod left, StatsMod right)
        {
            if (ReferenceEquals(left, right)) return true;   // same reference or both null
            if (left is null || right is null) return false; // one null, one not
            return left.ID == right.ID;
        }
        public static bool operator != (StatsMod left, StatsMod right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"{GetStatType}:max {GetMaxValue}+ {GetPerSecValue} per second";
        }

        public override int GetHashCode() => base.GetHashCode();

    }
    */

}