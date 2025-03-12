using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
namespace Arcatech
{
    public class ObservableDictionary<T, T2> : IObservableDictionary<T, T2>
    {
        public List<KeyValuePair<T, T2>> Pairs { get; private set; }
        public event Action<IEnumerable<T>> AnyRecordChanged = delegate { };
        public event Action<IEnumerable<T2>> AnyValueChanged = delegate { };

        public ObservableDictionary(KeyValuePair<T, T2>[] records)
        {
            Pairs = new List<KeyValuePair<T, T2>>(records);
        }
        void Invoke()
        {
            T2[] vals = new T2[Pairs.Count];
            for (int i = 0; i < Pairs.Count; i++)
            {
                vals[i] = Pairs[i].Value;
            }
            AnyValueChanged.Invoke(vals);

        }

        public T this[int index] => Pairs[index].Key;

        public int Count => Pairs.Count;

        public T2 this[uint index] => throw new NotImplementedException();

        public bool HasKey (T key) => Pairs.Any(t=>t.Key.Equals(key));
        public void Clear()
        {
            Pairs = new List<KeyValuePair<T, T2>>();
            Invoke();
        }

        public List <T2> GetAllValues()
        {
            List<T2> list = new List<T2>();
            for (int i = 0; i < Pairs.Count; i++)
            {
                list.Add(Pairs[i].Value);
            }
            return list;
        }
        private List <T> GetAllKeys()
        {
            List<T> list = new List<T>();
            for (int i = 0; i<Pairs.Count; i++)
            {
                list.Add(Pairs[i].Key);
            }
            return list;
        }

        public T TryGetKeyByValue(T2 value)
        {
            return Pairs.FirstOrDefault(t => t.Value.Equals(value)).Key;
        }

        public bool TryRemove(T key)
        {
            int index = Pairs.FindIndex(t => t.Key.Equals(key));
            Pairs.RemoveAt(index);
            return true;
        }

        public void Swap(int index1, int index2)
        {
            throw new NotImplementedException();
        }

        public bool TryAdd(T item)
        {
            throw new NotImplementedException();
        }

        public bool TryAddAt(int index, T item)
        {
            throw new NotImplementedException();
        }



        public bool TryRemoveAt(int index)
        {
            if (index < 0 || index == Pairs.Count()) return false;

            if (Pairs[index].Value == null) return false;

            Pairs[index] = default;
            Invoke();
            return true;
        }

        public void SetPair(T key, T2 value)
        {
            var allKeys = GetAllKeys();
            if (allKeys != null && allKeys.Contains(key))
            {
                Pairs[allKeys.IndexOf(key)] = new KeyValuePair<T, T2>(key,value);
            }
            else
            {
                Pairs.Add(new KeyValuePair<T, T2>(key, value));
            }

            Invoke();
        }

        public bool TryGetValue(T key, out T2 val)
        {
            if (GetAllKeys().Contains(key))
            {
                int index = Array.IndexOf(GetAllKeys().ToArray(), key);
                val = Pairs[index].Value;
                return true;
            }
            else
            {
                val = default(T2);
                return false;
            }
        }
    }
}