using UnityEngine;

namespace Arcatech.Stats
{
    [CreateAssetMenu(fileName = "usableEffect_stun_", menuName = "Applied Effects/Stun")]
    public class AppliedStunEffect : BaseAppliedEffect
    {
        [Range(0.01f, 1f)] public float time = 1f;
    }
}