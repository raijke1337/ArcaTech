using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Animation
{
    /// <summary>
    /// Процедурная «пластика» поверх любых Humanoid-клипов:
    ///  - покачивание таза при ходьбе/беге (фаза шага берётся из положения стоп, клипы размечать не нужно);
    ///  - контрапост в idle с медленным переносом веса;
    ///  - компенсация грудью (ровные или встречно наклонённые плечи);
    ///  - стопы фиксируются IK, поэтому не скользят.
    ///
    /// Требования: компонент на том же объекте, что и Animator; у слоя ikLayer в контроллере включён IK Pass.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [DefaultExecutionOrder(-60)] // раньше ArmAntiClip (-50) и spring/jiggle-костей (обычно 0+)
    public class ProceduralStyleController : MonoBehaviour
    {
        [Header("Профили")]
        [Tooltip("Профиль вне стилизованных состояний (атаки, прыжки, интеракции). Обычно Style_Off. Если пусто — Inline Settings.")]
        public StyleProfile defaultProfile;
        public StyleSettings inlineSettings = StyleSettings.Default;
        [Range(0f, 1f)] public float masterWeight = 1f;

        [Header("Аниматор")]
        [Tooltip("Слой, на котором включён IK Pass (обычно базовый)")]
        public int ikLayer = 0;
        [Tooltip("Необязательно: float-параметр скорости в аниматоре. Если пусто — скорость меряется по движению объекта.")]
        public string speedParameter = "LinearVelocity";

        [Header("Скорость → сила эффекта")]
        [Tooltip("Ниже этой скорости персонаж считается стоящим (включается контрапост)")]
        public float idleSpeed = 0.15f;
        [Tooltip("Множитель покачивания от скорости (м/с). По умолчанию к спринту эффект ослабевает.")]
        public AnimationCurve locomotionBySpeed = new AnimationCurve(
            new Keyframe(0f, 0f), new Keyframe(0.3f, 1f), new Keyframe(2.5f, 1f), new Keyframe(5f, 0.4f));

        [Header("Определение фазы шага")]
        [Tooltip("Разница высот стоп, при которой фаза считается максимальной, м")]
        public float footLiftReference = 0.08f;
        [Tooltip("Разница стоп вперёд-назад, при которой фаза считается максимальной, м")]
        public float strideReference = 0.35f;
        [Tooltip("Сглаживание сигнала фазы, с")]
        public float gaitSmoothing = 0.06f;
        [Tooltip("Опускать таз, чтобы ноги не вытягивались в струну при наклоне таза")]
        [Range(0f, 2f)] public float bodyDropCompensation = 1f;

        [Header("Связи")]
        [Tooltip("Если указан, веса анти-клиппинга берутся из текущего профиля")]
        public ArmAntiClip antiClip;

        public StyleSettings Current => current;

        /// <summary>
        /// Профиль текущей стойки оружия (ставит StanceStyleBinder). null — безоружная стойка.
        /// Заменяет профиль состояния, если у StyleProfileState включён useStanceProfile.
        /// </summary>
        public StyleProfile StanceProfile { get; set; }

        Animator anim;
        Transform spine, chest, leftUpperLeg, rightUpperLeg;
        int speedHash;
        bool useSpeedParam;

        Vector3 lastPosition;
        float speed;
        float lift, stride;
        float idleTime;

        StyleSettings current;
        bool ikRanThisFrame;
        Quaternion appliedBodyDelta = Quaternion.identity;

        class Request { public Object owner; public StyleProfile profile; public float blendTime; public bool useStance; public int refCount; }
        readonly List<Request> requests = new List<Request>();
        float lastBlendTime = 0.25f;

        StyleSettings BaseSettings => defaultProfile ? defaultProfile.settings : inlineSettings;

        void Awake()
        {
            anim = GetComponent<Animator>();
            spine = anim.GetBoneTransform(HumanBodyBones.Spine);
            chest = anim.GetBoneTransform(HumanBodyBones.Chest);
            leftUpperLeg = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            rightUpperLeg = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg);

            useSpeedParam = !string.IsNullOrEmpty(speedParameter);
            if (useSpeedParam) speedHash = Animator.StringToHash(speedParameter);

            current = BaseSettings;
            lastPosition = transform.position;
            idleTime = Random.Range(0f, 100f); // чтобы несколько персонажей не качались синхронно
        }

        void OnDisable() => requests.Clear();

        // ---------- API для StateMachineBehaviour / кода ----------

        public void PushProfile(Object owner, StyleProfile profile, float blendTime, bool useStanceProfile = false)
        {
            for (int i = 0; i < requests.Count; i++)
            {
                if (requests[i].owner == owner) { requests[i].refCount++; return; }
            }
            requests.Add(new Request { owner = owner, profile = profile, blendTime = blendTime, useStance = useStanceProfile, refCount = 1 });
        }

        public void RemoveProfile(Object owner)
        {
            for (int i = requests.Count - 1; i >= 0; i--)
            {
                if (requests[i].owner != owner) continue;
                if (--requests[i].refCount <= 0)
                {
                    lastBlendTime = requests[i].blendTime;
                    requests.RemoveAt(i);
                }
                return;
            }
        }

        // ---------- Update: скорость и бленд профилей (до оценки аниматора) ----------

        void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            if (useSpeedParam)
            {
                speed = anim.GetFloat(speedHash);
            }
            else
            {
                Vector3 p = transform.position;
                Vector3 v = (p - lastPosition) / dt;
                v.y = 0f;
                lastPosition = p;
                speed = Mathf.Lerp(speed, v.magnitude, 1f - Mathf.Exp(-dt / 0.1f));
            }

            StyleSettings target;
            float blend;
            if (requests.Count > 0)
            {
                Request top = requests[requests.Count - 1];
                StyleProfile chosen = top.useStance && StanceProfile ? StanceProfile : top.profile;
                target = chosen ? chosen.settings : BaseSettings;
                blend = top.blendTime;
            }
            else
            {
                target = BaseSettings;
                blend = lastBlendTime;
            }
            float k = blend <= 0.0001f ? 1f : 1f - Mathf.Exp(-dt * 3f / blend);
            current = StyleSettings.Lerp(current, target, k);

            if (antiClip)
            {
                antiClip.leftWeight = current.leftArmAntiClip;
                antiClip.rightWeight = current.rightArmAntiClip;
            }

            idleTime += dt;
        }

        // ---------- IK pass: таз и стопы ----------

        void OnAnimatorIK(int layerIndex)
        {
            if (layerIndex != ikLayer) return;
            ikRanThisFrame = true;
            appliedBodyDelta = Quaternion.identity;

            float w = masterWeight;
            if (w <= 0f) return;

            Vector3 up = transform.up, fwd = transform.forward, right = transform.right;

            // Стопы из анимации, до наших изменений
            Vector3 lPos = anim.GetIKPosition(AvatarIKGoal.LeftFoot);
            Quaternion lRot = anim.GetIKRotation(AvatarIKGoal.LeftFoot);
            Vector3 rPos = anim.GetIKPosition(AvatarIKGoal.RightFoot);
            Quaternion rRot = anim.GetIKRotation(AvatarIKGoal.RightFoot);

            // Фаза шага: lift > 0 — правая стопа выше (опора на левой); stride > 0 — правая впереди
            Vector3 d = rPos - lPos;
            float liftRaw = Mathf.Clamp(Vector3.Dot(d, up) / Mathf.Max(footLiftReference, 1e-4f), -1f, 1f);
            float strideRaw = Mathf.Clamp(Vector3.Dot(d, fwd) / Mathf.Max(strideReference, 1e-4f), -1f, 1f);
            float k = 1f - Mathf.Exp(-Time.deltaTime / Mathf.Max(gaitSmoothing, 1e-4f));
            lift = Mathf.Lerp(lift, liftRaw, k);
            stride = Mathf.Lerp(stride, strideRaw, k);

            float loco = locomotionBySpeed.Evaluate(speed) * current.locomotionWeight * w;
            float idle = (1f - Mathf.InverseLerp(0.02f, Mathf.Max(idleSpeed, 0.03f), speed)) * current.idleWeight * w;
            if (loco + idle <= 0.0001f) return;

            // Локомоция: бедро опорной ноги вверх и наружу. Положительный roll поднимает правую сторону.
            float roll = loco * (-lift * current.hipRollDeg);
            float shift = loco * (-lift * current.hipShift);
            float yaw = loco * (-stride * current.hipYawDeg);

            // Idle: контрапост. side = +1 — вес на правой ноге
            float side = 1f;
            if (current.idleSwayPeriod > 0.01f)
            {
                float s = Mathf.Sin(idleTime * 2f * Mathf.PI / current.idleSwayPeriod);
                side = Mathf.Lerp(1f, Mathf.Clamp(s * 1.6f, -1f, 1f), current.idleSwayAmount); // «держится» в крайних позах
            }
            roll += idle * side * current.idleHipRollDeg;
            shift += idle * side * current.idleHipShift;

            Quaternion delta = Quaternion.AngleAxis(yaw, up) * Quaternion.AngleAxis(roll, fwd);
            anim.bodyRotation = delta * anim.bodyRotation;

            float halfHipWidth = (leftUpperLeg && rightUpperLeg)
                ? Vector3.Distance(leftUpperLeg.position, rightUpperLeg.position) * 0.5f : 0.1f;
            float drop = Mathf.Abs(Mathf.Sin(roll * Mathf.Deg2Rad)) * halfHipWidth * bodyDropCompensation;
            anim.bodyPosition += right * shift - up * drop;
            appliedBodyDelta = delta;

            // Шаг «по линии»
            float cross = loco * current.crossStep;
            lPos += right * cross;
            rPos -= right * cross;

            // Стопы остаются на месте
            SetFoot(AvatarIKGoal.LeftFoot, lPos, lRot);
            SetFoot(AvatarIKGoal.RightFoot, rPos, rRot);
        }

        void SetFoot(AvatarIKGoal goal, Vector3 pos, Quaternion rot)
        {
            anim.SetIKPositionWeight(goal, 1f);
            anim.SetIKRotationWeight(goal, 1f);
            anim.SetIKPosition(goal, pos);
            anim.SetIKRotation(goal, rot);
        }

        // ---------- LateUpdate: компенсация грудью ----------

        void LateUpdate()
        {
            // Если аниматор в этом кадре не обновлялся (culling), кости не перезаписаны — повторно не крутим
            if (!ikRanThisFrame) return;
            ikRanThisFrame = false;
            if (appliedBodyDelta == Quaternion.identity || current.chestCounter <= 0f) return;

            Quaternion full = Quaternion.SlerpUnclamped(Quaternion.identity, Quaternion.Inverse(appliedBodyDelta), current.chestCounter);
            if (spine && chest)
            {
                Quaternion half = Quaternion.SlerpUnclamped(Quaternion.identity, full, 0.5f);
                spine.rotation = half * spine.rotation;
                chest.rotation = half * chest.rotation;
            }
            else if (spine)
            {
                spine.rotation = full * spine.rotation;
            }
        }
    }
}
