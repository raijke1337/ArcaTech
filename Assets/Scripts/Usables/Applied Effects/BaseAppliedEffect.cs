using Arcatech.Texts;
using UnityEngine;

namespace Arcatech.Stats
{
    public abstract class BaseAppliedEffect : ScriptableObject
    {
        public enum StackType
        {
            None,
            Refresh,
            Independent
        }

        [Header("Meta")] public Description description;

        [Header("Lifetime")] public bool infiniteDuration;
        [Tooltip("ignored if infinite")] public float durationSeconds = 3f;

        [Header("Stacking")] public StackType stackType;
        public int maxStacks = 99;
    }
    
    
}