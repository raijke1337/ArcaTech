using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Animation
{
    /// <summary>
    /// Убирает проход рук сквозь бёдра/таз: если локоть, предплечье, запястье или кисть входят
    /// в капсулу тела, плечо поворачивается ровно настолько, чтобы рука легла на поверхность.
    /// Коррекция непрерывна (растёт от нуля с глубиной проникновения), поэтому не дёргается.
    /// Работает поверх любых Humanoid-клипов, в т.ч. «витринного» пака.
    /// Капсулы настраиваются под модель — смотрите гизмо при выделенном объекте.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [DefaultExecutionOrder(-50)] // после ProceduralStyleController, до spring/jiggle-костей
    public class ArmAntiClip : MonoBehaviour
    {
        [Header("Веса (ProceduralStyleController перезаписывает их из профиля)")]
        [Range(0f, 1f)] public float leftWeight = 1f;
        [Range(0f, 1f)] public float rightWeight = 1f;

        [Header("Таз (между суставами бёдер). Смещения: x вправо, y вверх, z вперёд, в метрах")]
        public float pelvisRadius = 0.13f;
        [Tooltip("Насколько капсула таза выходит за суставы бёдер в стороны")]
        public float pelvisExtraWidth = 0.04f;
        public Vector3 pelvisOffset = new Vector3(0f, 0.02f, -0.01f);

        [Header("Бёдра (зеркально для обеих ног; x > 0 — наружу)")]
        public float thighRadiusTop = 0.11f;
        public float thighRadiusBottom = 0.075f;
        public Vector3 thighTopOffset = new Vector3(0.02f, 0f, 0f);
        public Vector3 thighBottomOffset = Vector3.zero;
        [Tooltip("Докуда тянется капсула бедра: 0 = сустав бедра, 1 = колено")]
        [Range(0.1f, 1f)] public float thighLength = 0.7f;

        [System.Serializable]
        public class ExtraCapsule
        {
            public HumanBodyBones bone = HumanBodyBones.Hips;
            [Tooltip("Точки в локальном пространстве кости")]
            public Vector3 localStart, localEnd;
            public float radius = 0.05f;
        }
        [Tooltip("Дополнительные объёмы: подсумки, кобура, хвост у основания и т.п.")]
        public List<ExtraCapsule> extraCapsules = new List<ExtraCapsule>();

        [Header("Толщина руки")]
        public float elbowRadius = 0.045f;
        public float wristRadius = 0.035f;
        public float handRadius = 0.03f;
        [Tooltip("Если у рига нет пальцев — длина кисти от запястья")]
        public float handLength = 0.08f;
        public float margin = 0.005f;
        [Tooltip("0 = выталкивать по кратчайшему пути, 1 = всегда наружу в сторону")]
        [Range(0f, 1f)] public float outwardBias = 0.5f;
        [Range(1, 4)] public int iterations = 3;

        [Header("Отладка")]
        public bool drawGizmos = true;

        struct Capsule { public Vector3 a, b; public float ra, rb; }

        class Arm
        {
            public Transform upper, lower, hand, tip;
            public float side; // -1 левая, +1 правая
        }

        Animator anim;
        Arm left, right;
        Transform lUpLeg, lLowLeg, rUpLeg, rLowLeg;
        readonly List<Capsule> capsules = new List<Capsule>(8);
        readonly Vector3[] pts = new Vector3[5];
        readonly float[] rads = new float[5];

        void Awake() => Cache();

        bool Cache()
        {
            if (!anim) anim = GetComponent<Animator>();
            if (!anim || !anim.isHuman) return false;
            left = MakeArm(HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand, HumanBodyBones.LeftMiddleProximal, -1f);
            right = MakeArm(HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand, HumanBodyBones.RightMiddleProximal, 1f);
            lUpLeg = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            lLowLeg = anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            rUpLeg = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            rLowLeg = anim.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            return lUpLeg && rUpLeg && lLowLeg && rLowLeg;
        }

        Arm MakeArm(HumanBodyBones u, HumanBodyBones l, HumanBodyBones h, HumanBodyBones t, float side)
        {
            return new Arm
            {
                upper = anim.GetBoneTransform(u),
                lower = anim.GetBoneTransform(l),
                hand = anim.GetBoneTransform(h),
                tip = anim.GetBoneTransform(t),
                side = side,
            };
        }

        void LateUpdate()
        {
            if (leftWeight <= 0f && rightWeight <= 0f) return;
            if (lUpLeg == null && !Cache()) return;
            BuildCapsules();
            Solve(left, leftWeight);
            Solve(right, rightWeight);
        }

        float Scale => transform.lossyScale.x;
        Vector3 CharVec(Vector3 v) => transform.TransformVector(v);

        void BuildCapsules()
        {
            capsules.Clear();
            float s = Scale;

            capsules.Add(new Capsule
            {
                a = lUpLeg.position + CharVec(pelvisOffset + new Vector3(-pelvisExtraWidth, 0f, 0f)),
                b = rUpLeg.position + CharVec(pelvisOffset + new Vector3(pelvisExtraWidth, 0f, 0f)),
                ra = pelvisRadius * s, rb = pelvisRadius * s,
            });

            AddThigh(lUpLeg, lLowLeg, -1f, s);
            AddThigh(rUpLeg, rLowLeg, 1f, s);

            for (int i = 0; i < extraCapsules.Count; i++)
            {
                var e = extraCapsules[i];
                Transform bone = anim.GetBoneTransform(e.bone);
                if (!bone) continue;
                capsules.Add(new Capsule
                {
                    a = bone.TransformPoint(e.localStart),
                    b = bone.TransformPoint(e.localEnd),
                    ra = e.radius * s, rb = e.radius * s,
                });
            }
        }

        void AddThigh(Transform up, Transform low, float side, float s)
        {
            Vector3 top = thighTopOffset; top.x *= side;
            Vector3 bot = thighBottomOffset; bot.x *= side;
            capsules.Add(new Capsule
            {
                a = up.position + CharVec(top),
                b = Vector3.Lerp(up.position, low.position, thighLength) + CharVec(bot),
                ra = thighRadiusTop * s, rb = thighRadiusBottom * s,
            });
        }

        void Solve(Arm arm, float weight)
        {
            if (weight <= 0f || arm == null || !arm.upper || !arm.lower || !arm.hand) return;

            float s = Scale;
            Quaternion original = arm.upper.rotation;
            Vector3 outward = transform.right * arm.side;
            bool changed = false;

            for (int it = 0; it < iterations; it++)
            {
                Vector3 S = arm.upper.position;
                Vector3 E = arm.lower.position;
                Vector3 H = arm.hand.position;
                Vector3 T = arm.tip ? arm.tip.position : H + (H - E).normalized * handLength * s;

                pts[0] = E;                       rads[0] = elbowRadius;
                pts[1] = Vector3.Lerp(E, H, 0.5f); rads[1] = Mathf.Lerp(elbowRadius, wristRadius, 0.5f);
                pts[2] = H;                       rads[2] = wristRadius;
                pts[3] = Vector3.Lerp(H, T, 0.5f); rads[3] = handRadius;
                pts[4] = T;                       rads[4] = handRadius;

                float bestPen = 0f;
                Vector3 bestP = Vector3.zero, bestTarget = Vector3.zero;

                for (int p = 0; p < pts.Length; p++)
                {
                    for (int c = 0; c < capsules.Count; c++)
                    {
                        Capsule cap = capsules[c];
                        Vector3 ab = cap.b - cap.a;
                        float t = Mathf.Clamp01(Vector3.Dot(pts[p] - cap.a, ab) / Mathf.Max(ab.sqrMagnitude, 1e-8f));
                        Vector3 axis = cap.a + ab * t;
                        float R = Mathf.Lerp(cap.ra, cap.rb, t) + (rads[p] + margin) * s;
                        Vector3 toP = pts[p] - axis;
                        float dist = toP.magnitude;
                        float pen = R - dist;
                        if (pen <= bestPen) continue;

                        Vector3 n = dist > 1e-5f ? toP / dist : outward;
                        n = Vector3.Slerp(n, outward, outwardBias * Mathf.Clamp01(1f - Vector3.Dot(n, outward)));
                        bestPen = pen;
                        bestP = pts[p];
                        bestTarget = axis + n.normalized * R;
                    }
                }

                if (bestPen <= 0f) break;
                Quaternion fix = Quaternion.FromToRotation(bestP - S, bestTarget - S);
                arm.upper.rotation = fix * arm.upper.rotation;
                changed = true;
            }

            if (changed && weight < 1f)
                arm.upper.rotation = Quaternion.Slerp(original, arm.upper.rotation, weight);
        }

        void OnDrawGizmosSelected()
        {
            if (!drawGizmos) return;
            if (lUpLeg == null && !Cache()) return;
            BuildCapsules();
            Gizmos.color = new Color(1f, 0.4f, 0.8f, 0.9f);
            foreach (var c in capsules) DrawCapsule(c);
        }

        static void DrawCapsule(Capsule c)
        {
            Gizmos.DrawWireSphere(c.a, c.ra);
            Gizmos.DrawWireSphere(c.b, c.rb);
            Vector3 dir = (c.b - c.a).normalized;
            Vector3 p1 = Vector3.Cross(dir, Vector3.up);
            if (p1.sqrMagnitude < 1e-4f) p1 = Vector3.Cross(dir, Vector3.right);
            p1.Normalize();
            Vector3 p2 = Vector3.Cross(dir, p1);
            Gizmos.DrawLine(c.a + p1 * c.ra, c.b + p1 * c.rb);
            Gizmos.DrawLine(c.a - p1 * c.ra, c.b - p1 * c.rb);
            Gizmos.DrawLine(c.a + p2 * c.ra, c.b + p2 * c.rb);
            Gizmos.DrawLine(c.a - p2 * c.ra, c.b - p2 * c.rb);
        }
    }
}
