using UnityEngine;

namespace Arcatech.Usables.Effects
{
    [CreateAssetMenu(fileName = "usableEffect_modifier_", menuName = "Usables/Applied Effects/Modifier")]
    public class AppliedModifierEffect : BaseAppliedEffect
    {
        [Header("Modifier")]
        public ModifierParam param;
        [Tooltip("Multiplier applied per stack. 0.8 = -20%, 1.2 = +20%.")]
        public float multiplier = 0.8f;
        public ModifierStackCounting counting = ModifierStackCounting.PerSource;
    }
    
    public enum ModifierStackCounting
    {
        PerSource,   // "всего от сущности-источника"
        OnTarget     // "всего на цели"
    }
}