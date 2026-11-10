using Arcatech.Units.Control;
using UnityEngine;

namespace Arcatech.Level
{
    /// <summary>
    /// Повесить на объект с обычным (не trigger) коллайдером, по которому ходит персонаж.
    /// Контроллер сам находит ленту под ногами и применяет снос через MotionRequest.Conveyor.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ConveyorSurface : MonoBehaviour
    {
        [Header("Conveyor")]
        [Tooltip("Направление движения в локальных координатах конвейера")]
        public Vector3 localDirection = Vector3.forward;

        [Tooltip("Скорость ленты, м/с. Бег по ленте даёт maxWalkSpeed + beltSpeed, против - maxWalkSpeed - beltSpeed")]
        public float beltSpeed = 4f;

        [Tooltip("Сколько секунд снос продолжает действовать после последнего обновления запроса")]
        public float holdTime = 0.1f;

        [Tooltip("Сохранять ли скорость ленты при прыжке/падении с неё (до приземления)")]
        public bool impartVelocityOnExit = true;

        /// <summary>Скорость ленты в мировых координатах (только горизонтальная плоскость).</summary>
        public Vector3 GetWorldBeltVelocity()
        {
            Vector3 v = transform.TransformDirection(localDirection);
            v.y = 0f;
            return v.sqrMagnitude > 0.0001f ? v.normalized * beltSpeed : Vector3.zero;
        }

        public MotionRequest CreateMotionRequest()
            => MotionRequest.Conveyor(GetWorldBeltVelocity(), holdTime, impartVelocityOnExit);

#if UNITY_EDITOR
        private void OnDrawGizmos() => DrawConveyorGizmo(false);
        private void OnDrawGizmosSelected() => DrawConveyorGizmo(true);

        private void DrawConveyorGizmo(bool isSelected)
        {
            if (localDirection.sqrMagnitude < 0.0001f)
                return;

            // Цвет из дизайн-документа: Циан ARCA #29D7FF
            Color baseColor = new Color(0x29 / 255f, 0xD7 / 255f, 0xFF / 255f, 0.6f);
            Color selectedColor = new Color(0x29 / 255f, 0xD7 / 255f, 0xFF / 255f, 1.0f);
            Gizmos.color = isSelected ? selectedColor : baseColor;

            Vector3 origin = transform.position;
            BoxCollider box = null;
            if (TryGetComponent(out box))
                origin = transform.TransformPoint(box.center);

            Vector3 worldDir = transform.TransformDirection(localDirection).normalized;

            float gizmoLength = 1.5f;
            if (box != null)
            {
                Vector3 n = localDirection.normalized;
                Vector3 absLocal = new Vector3(Mathf.Abs(n.x), Mathf.Abs(n.y), Mathf.Abs(n.z));
                gizmoLength = Mathf.Max(1.0f, Vector3.Scale(box.size, absLocal).magnitude * 0.5f);
            }

            Vector3 end = origin + worldDir * gizmoLength;
            Gizmos.DrawLine(origin, end);

            float arrowSize = 0.15f;
            Gizmos.DrawWireSphere(end, arrowSize);

            if (isSelected)
            {
                Gizmos.color = new Color(0x29 / 255f, 0xD7 / 255f, 0xFF / 255f, 0.35f);
                const int marks = 3;
                for (int i = 1; i <= marks; i++)
                {
                    Vector3 p = Vector3.Lerp(origin, end, i / (float)(marks + 1));
                    Gizmos.DrawWireSphere(p, arrowSize * 0.6f);
                }
            }
        }
#endif
    }
}