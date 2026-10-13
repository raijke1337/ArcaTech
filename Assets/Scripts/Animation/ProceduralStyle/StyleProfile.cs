using UnityEngine;

namespace Arcatech.Animation
{
    /// <summary>
    /// Набор параметров процедурного слоя. Блендится между состояниями аниматора.
    /// </summary>
    [System.Serializable]
    public struct StyleSettings
    {
        [Header("Локомоция: покачивание таза")]
        [Tooltip("Общая сила эффекта при движении")]
        [Range(0f, 1f)] public float locomotionWeight;
        [Tooltip("Подъём бедра опорной ноги, градусы")]
        public float hipRollDeg;
        [Tooltip("Сдвиг таза к опорной ноге, метры")]
        public float hipShift;
        [Tooltip("Поворот таза за выносимой вперёд ногой, градусы")]
        public float hipYawDeg;
        [Tooltip("Компенсация грудью: 0 = корпус качается вместе с тазом, 1 = плечи ровные, >1 = встречный наклон плеч")]
        [Range(0f, 2f)] public float chestCounter;
        [Tooltip("Шаг «по линии»: стопы смещаются к центру, метры")]
        public float crossStep;

        [Header("Idle: контрапост")]
        [Range(0f, 1f)] public float idleWeight;
        [Tooltip("Наклон таза в сторону опорной ноги, градусы")]
        public float idleHipRollDeg;
        [Tooltip("Сдвиг таза в сторону опорной ноги, метры")]
        public float idleHipShift;
        [Tooltip("Период смены опорной ноги, секунды (0 = статичная поза)")]
        public float idleSwayPeriod;
        [Tooltip("0 = всегда стоит на правой ноге, 1 = полностью переносит вес с ноги на ногу")]
        [Range(0f, 1f)] public float idleSwayAmount;

        [Header("Анти-клиппинг рук")]
        [Range(0f, 1f)] public float leftArmAntiClip;
        [Range(0f, 1f)] public float rightArmAntiClip;

        public static StyleSettings Default => new StyleSettings
        {
            locomotionWeight = 1f,
            hipRollDeg = 6f,
            hipShift = 0.025f,
            hipYawDeg = 6f,
            chestCounter = 1.3f,
            crossStep = 0.03f,

            idleWeight = 1f,
            idleHipRollDeg = 5f,
            idleHipShift = 0.04f,
            idleSwayPeriod = 7f,
            idleSwayAmount = 0.6f,

            leftArmAntiClip = 1f,
            rightArmAntiClip = 1f,
        };

        public static StyleSettings Lerp(in StyleSettings a, in StyleSettings b, float t)
        {
            return new StyleSettings
            {
                locomotionWeight = Mathf.Lerp(a.locomotionWeight, b.locomotionWeight, t),
                hipRollDeg = Mathf.Lerp(a.hipRollDeg, b.hipRollDeg, t),
                hipShift = Mathf.Lerp(a.hipShift, b.hipShift, t),
                hipYawDeg = Mathf.Lerp(a.hipYawDeg, b.hipYawDeg, t),
                chestCounter = Mathf.Lerp(a.chestCounter, b.chestCounter, t),
                crossStep = Mathf.Lerp(a.crossStep, b.crossStep, t),

                idleWeight = Mathf.Lerp(a.idleWeight, b.idleWeight, t),
                idleHipRollDeg = Mathf.Lerp(a.idleHipRollDeg, b.idleHipRollDeg, t),
                idleHipShift = Mathf.Lerp(a.idleHipShift, b.idleHipShift, t),
                idleSwayPeriod = Mathf.Lerp(a.idleSwayPeriod, b.idleSwayPeriod, t),
                idleSwayAmount = Mathf.Lerp(a.idleSwayAmount, b.idleSwayAmount, t),

                leftArmAntiClip = Mathf.Lerp(a.leftArmAntiClip, b.leftArmAntiClip, t),
                rightArmAntiClip = Mathf.Lerp(a.rightArmAntiClip, b.rightArmAntiClip, t),
            };
        }
    }

    [CreateAssetMenu(menuName = "Arcatech/Animation/Style Profile", fileName = "StyleProfile")]
    public class StyleProfile : ScriptableObject
    {
        public StyleSettings settings = StyleSettings.Default;
    }
}
