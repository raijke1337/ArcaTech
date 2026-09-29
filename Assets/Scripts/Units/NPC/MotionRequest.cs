using UnityEngine;

namespace Arcatech.Units.Control
{
    public enum MotionKind : byte
    {
        /// <summary>Внешний толчок (удар, взрыв). Перебивает текущую скорость. Может выбросить NPC с навмеша.</summary>
        Knockback,

        /// <summary>Рывок/додж. Добавляется к текущей скорости. NPC не покидает навмеш (упирается в край).</summary>
        Dash,
    }

    /// <summary>
    /// Единый способ сказать юниту «тебя двигают». Направление и скорость (м/с) считает тот,
    /// кто знает контекст (ActionResult), а исполнитель (ECM2 / NavMesh) только исполняет.
    /// </summary>
    public readonly struct MotionRequest
    {
        /// <summary>Мировая скорость, м/с. Для Dash вертикальная часть игнорируется.</summary>
        public readonly Vector3 Velocity;

        /// <summary>Сколько секунд держать толчок. 0 = значение по умолчанию у исполнителя.</summary>
        public readonly float Duration;

        public readonly MotionKind Kind;

        /// <summary>Можно ли выбросить NPC за край навмеша (в пропасть). Игрока не касается.</summary>
        public readonly bool CanLeaveNavMesh;

        private MotionRequest(Vector3 velocity, float duration, MotionKind kind, bool canLeaveNavMesh)
        {
            Velocity = velocity;
            Duration = duration;
            Kind = kind;
            CanLeaveNavMesh = canLeaveNavMesh;
        }

        public static MotionRequest Knockback(Vector3 velocity, float duration = 0f, bool canLeaveNavMesh = true)
            => new MotionRequest(velocity, duration, MotionKind.Knockback, canLeaveNavMesh);

        public static MotionRequest Dash(Vector3 velocity, float duration = 0f)
            => new MotionRequest(velocity, duration, MotionKind.Dash, false);
    }
}