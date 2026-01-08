using UnityEngine;

namespace Arcatech.Stats
{
    [CreateAssetMenu(fileName = "usableEffect_damageDealtMod_", menuName = "Applied Effects/Damage dealt mod")]
    public class AppliedDamageDealtModifierEffect : BaseAppliedEffect
    {
        [Range(0.01f, 1f)] public float percentDamageMult = 0.2f;
    }
}