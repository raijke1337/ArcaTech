using System;
using System.Collections.Generic;
using Arcatech.Units;
using Arcatech.Units.Behaviour;
using UnityEngine.Assertions;

namespace Arcatech.BlackboardSystem
{
    [Serializable]
    public readonly struct BlackboardKey : IEquatable<BlackboardKey> // for better performance
    {
        readonly string name;
        readonly int hashedLookupKey;

        public BlackboardKey(string name) : this()
        {
            this.name = name;
            hashedLookupKey = name.ComputeHash();
        }

        public bool Equals(BlackboardKey other)
        {
            return hashedLookupKey == other.hashedLookupKey;
        }
        public override bool Equals(object obj)
        {
            return obj is BlackboardKey bb && Equals(bb);
        }

        public override int GetHashCode()
        {
            return hashedLookupKey;
        }
        public override string ToString()
        {
            return name;
        }

        public static bool operator == (BlackboardKey l, BlackboardKey r) => l.hashedLookupKey == r.hashedLookupKey;
        public static bool operator != (BlackboardKey l, BlackboardKey r) => !(l == r);

    }
   
    [Serializable]
    public class BlackboardEntry<T>
    {
        public BlackboardKey Key { get; }
        public T Value { get; }
        public Type ValueType { get; }

        public BlackboardEntry(BlackboardKey key, T value)
        {
            Key = key;
            Value = value;
            ValueType = typeof(T);
        }

        public override bool Equals(object obj)
        {
            return (obj is BlackboardEntry<T> entry) && entry.Key == Key;
        }
        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    }

    [Serializable]
    public class Blackboard
    {
        #region too complex
        Dictionary<string, BlackboardKey> keysRegistry = new(); // for external lookup if entry alreay exists
        Dictionary<BlackboardKey, object> entries = new Dictionary<BlackboardKey, object>(); // for entires

        public bool TryGetValue<T>(BlackboardKey key, out T value)
        {
            if (entries.TryGetValue(key, out var entry) && entry is BlackboardEntry<T> casted)
            {
                value = casted.Value;
                return true;
            }
            value = default;
            return false;
        }

        public void SetValue<T>(BlackboardKey keyy, T value)
        {
            entries[keyy] = new BlackboardEntry<T>(keyy, value);
           // Debug();
        }


        public BlackboardKey GetOrRegisterKey (string keyName)
        {
            Assert.IsNotNull(keyName);

            if (!keysRegistry.TryGetValue(keyName, out var key))
            {
                key = new BlackboardKey(keyName);
                keysRegistry[keyName] = key;
            }
            //Debug();
            return key;
        }

        public bool ContainsKey(BlackboardKey key) => entries.ContainsKey(key);
        public void RemoveKey(BlackboardKey key) => entries.Remove(key);

        public void Debug()
        {
            foreach (var e in entries)
            {
                var t = e.Value.GetType(); // check if all values are correct
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof (BlackboardEntry<>))
                {
                    var property = t.GetProperty("Value");
                    if (property == null) continue;
                    var val = property.GetValue(e.Value);
                    UnityEngine.Debug.Log($"Key {e.Key} / Value {val}");
                }
            }
        }
        #endregion

        public NPCUnitComponent gatherAround { get; private set; }
        public void SetGatherPointUnit (NPCUnitComponent g)
        {
            if (g == null)
            gatherAround = g;
        }
        public void ResetGatherPoint ()
        {
            gatherAround = null;
        }

        #region arbiter section

        public List<Action> PassedActions { get; } = new();
        public void AddAction (Action a)
        {
            Assert.IsNotNull(a);
            PassedActions.Add(a);
        }
        public void ClearActions() => PassedActions.Clear();

        #endregion


    }


}
