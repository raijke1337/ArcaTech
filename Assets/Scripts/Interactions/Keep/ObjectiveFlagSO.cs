// Assets/.../ScriptableObjects/ObjectiveFlagSO.cs
using UnityEngine;

namespace Arcatech.SaveSystem
{
    [CreateAssetMenu(fileName = "Flag_", menuName = "Arcatech/Objective Flag")]
    public class ObjectiveFlagSO : ScriptableObject
    {
        // Чисто runtime-состояние. Сбрасывается доменной перезагрузкой при каждом
        // входе в Play Mode (стандартная настройка Unity). Если у вас отключён
        // "Reload Domain" в Enter Play Mode Settings — добавьте ручной сброс
        // через [InitializeOnEnterPlayMode] или вызовом Reset() на старте уровня.
        [System.NonSerialized] private bool _value;
        public bool IsSet => _value;

        public event System.Action Changed;

        public void Set()   { if (_value)  return; _value = true;  Changed?.Invoke(); }
        public void Reset() { if (!_value) return; _value = false; Changed?.Invoke(); }
        
    }
}