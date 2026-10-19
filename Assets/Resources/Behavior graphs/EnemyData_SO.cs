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

        [Header("Aggression (CombatCoordinator)")]
        [Tooltip("Агрессия при вступлении в бой, случайно в диапазоне. 1 = полная шкала, токен можно получить.")]
        public Vector2 AggressionOnEngage = new(0.3f, 0.8f);
        [Tooltip("Прирост агрессии в секунду, пока враг в бою и не атакует.")]
        public float AggressionGainPerSec = 0.3f;
        [Tooltip("Изменение агрессии, когда игрок наносит урон.")]
        public float AggressionOnDamaged = -0.2f;
        [Tooltip("Значение агрессии после атаки (возврат токена).")]
        public float AggressionAfterAttack = 0f;
        [Tooltip("Прибавка агрессии, когда в бою погибает союзник.")]
        public float AggressionOnAllyDeath = 0.25f;
    }

}