using UnityEngine;

namespace Arcatech.Stats
{
    [CreateAssetMenu(fileName = "usableEffect_knockBack_", menuName = "Applied Effects/Knockback")]
    public class AppliedKnockbackEffect : BaseAppliedEffect
    {
        [Range(0.01f, 10f)] public float distance = 2f;
    }
}