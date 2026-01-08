using UnityEngine;

namespace Arcatech.Stats
{
    [CreateAssetMenu(fileName = "usableEffect_speed_", menuName = "Applied Effects/Speed change")]
    public class AppliedSpeedDeltaEffect : BaseAppliedEffect
    {
        [Range(0.01f, 1f)] public float percentSpeedMult = 0.2f;
    }
}