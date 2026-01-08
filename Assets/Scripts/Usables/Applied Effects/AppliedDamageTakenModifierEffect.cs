using UnityEngine;

namespace Arcatech.Stats
{
    [CreateAssetMenu(fileName = "usableEffect_damageTakenMod_", menuName = "Applied Effects/Damage taken mod")]
    public class AppliedDamageTakenModifierEffect : BaseAppliedEffect
    {
        [Range(0.01f, 1f)] public float percentDamageTakenMult = 0.2f;
    }
}