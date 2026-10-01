using UnityEngine;

namespace Arcatech.Items.Projectiles
{
    public abstract class SerializedProjectileBehavior : ScriptableObject
    {
        public abstract ProjectileBehavior Deserialize(BaseGameEntityComponent owner);
    }

    /// <summary>Что делать снаряду после столкновения - решает его поведение.</summary>
    public enum ProjectileCollisionResult
    {
        Continue,
        Finish
    }

    /// <summary>Почему снаряд завершил жизнь (для VFX/звука: осколки о стену, растворение по сроку).</summary>
    public enum ProjectileFinishReason
    {
        /// <summary>Вышел срок / дальность.</summary>
        Expired,
        /// <summary>Разбился о стену.</summary>
        Environment,
        /// <summary>Исчерпал лимит поражаемых целей.</summary>
        HitsExhausted,
        /// <summary>Поведение завершило себя само (например, бумеранг пойман владельцем).</summary>
        Behavior,
        /// <summary>Отменён снаружи (смена экипировки, уничтожение пула).</summary>
        Cancelled
    }

    public enum EnvironmentResponse
    {
        /// <summary>Снаряд разбивается о стену.</summary>
        Destroy = 0,
        /// <summary>Снаряд пролетает сквозь стену.</summary>
        PassThrough = 1
    }

    /// <summary>
    /// Описание столкновения, уже разобранное компонентом снаряда: цель это стена, владелец
    /// или враг; засчитано ли попадание с учётом правил пробития (ProjectileHitRules).
    /// </summary>
    public readonly struct ProjectileCollision
    {
        public ProjectileCollision(Transform projectile, TriggerHitInfo hit, BaseGameEntityComponent target,
            bool isEnvironment, bool isOwner, bool accepted, bool budgetExhausted)
        {
            Projectile = projectile;
            Hit = hit;
            Target = target;
            IsEnvironment = isEnvironment;
            IsOwner = isOwner;
            Accepted = accepted;
            BudgetExhausted = budgetExhausted;
        }

        public Transform Projectile { get; }
        public TriggerHitInfo Hit { get; }

        /// <summary>Сущность, в которую попали. null для стен.</summary>
        public BaseGameEntityComponent Target { get; }

        /// <summary>Попадание в стену / невалидный слой.</summary>
        public bool IsEnvironment { get; }

        /// <summary>Снаряд коснулся своего владельца.</summary>
        public bool IsOwner { get; }

        /// <summary>Валидное попадание засчитано (не владелец, не повтор, лимит не исчерпан).</summary>
        public bool Accepted { get; }

        /// <summary>Это попадание было последним из разрешённых правилами.</summary>
        public bool BudgetExhausted { get; }
    }

    public abstract class ProjectileBehavior
    {
        protected BaseGameEntityComponent Owner;

        /// <summary>Поведение завершилось само (срок, дальность и т.п.). Компонент завершит снаряд.</summary>
        public bool BehaviorCompleted { get; protected set; }

        public abstract void UpdatePosition(float delta, Transform projectileTransform);

        /// <summary>
        /// Реакция на столкновение. Вызывается ПОСЛЕ того, как эффекты попадания уже применены.
        /// </summary>
        public abstract ProjectileCollisionResult OnCollision(in ProjectileCollision collision);

        public abstract void Reset();
    }
}
