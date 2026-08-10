using Unity.Behavior;
using UnityEngine;

namespace Arcatech.Units
{
    [CreateAssetMenu(menuName = "Game/Enemy Data Config")]
    public class EnemyData_SO : ScriptableObjectID
    {

        [Header("Combat / aggro")] public float AggroRad, DeaggroRad, AggroCooldown;
        public bool NeedLoS;
        public float ViewAngle;
        public float BehaviorUpdateInterval;
        public float MeleeRange, RangedRange;

        [Header("Movement")] public float NonCombatMoveSpeed, CombatMoveSpeed, RotateSpeed;

        [Header("Meta")] 
        public UnitTier Tier;

        [Header("Idle params")] public float WanderRange;
        public float WanderIdleTime;
    }

}