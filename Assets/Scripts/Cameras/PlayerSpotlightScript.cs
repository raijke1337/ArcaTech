using UnityEngine;

namespace Arcatech.Cameras
{
    public class PlayerSpotlightScript : MonoBehaviour
    {
        [Header("Target Settings")] [Tooltip("Назначьте игрока вручную или оставьте пустым для автопоиска по тегу")]
        public Transform player;

        [Header("Follow Settings")] [Tooltip("Смещение относительно позиции игрока")]
        public Vector3 offset = new Vector3(0f, 8f, 0f);

        [Tooltip("Плавность следования (0 = мгновенно, выше = плавнее)")] [Range(0f, 10f)]
        public float smoothSpeed = 5f;

        [Tooltip("Должен ли свет всегда смотреть на игрока")]
        public bool lookAtPlayer = true;

        void Start()
        {
            // Если игрок не назначен, ищем автоматически
            if (player == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

                if (playerObject != null)
                {
                    player = playerObject.transform;
                   // Debug.Log($"Spot Light нашел игрока: {playerObject.name}");
                }
                else
                {
                    Debug.LogWarning("Игрок с тегом 'Player' не найден на сцене!");
                }
            }
        }

        void LateUpdate()
        {
            if (player == null)
                return;

            // Целевая позиция с учетом смещения
            Vector3 targetPosition = player.position + offset;

            // Плавное следование или мгновенное перемещение
            if (smoothSpeed > 0f)
            {
                transform.position = Vector3.Lerp(
                    transform.position,
                    targetPosition,
                    smoothSpeed * Time.deltaTime
                );
            }
            else
            {
                transform.position = targetPosition;
            }

            // Направляем свет на игрока
            if (lookAtPlayer)
            {
                transform.LookAt(player);
            }
        }
    }
}