using System;
using System.Collections.Generic;
using com.cyborgAssets.inspectorButtonPro;
using UnityEngine;

namespace Arcatech.Items
{
    /// <summary>
    /// Привязывает слоты стойки к клипам базового контроллера (клипам безоружной Теилс).
    /// AnimatorOverrideController подменяет клипы по оригиналу, поэтому оригиналы нужно знать точно.
    /// Заполняется автоматически: контекстное меню ассета -> "Auto-fill from controller".
    /// </summary>
    [CreateAssetMenu(fileName = "stance_slot_map", menuName = "Items/Stance slot map")]
    public class StanceSlotMap : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public StanceSlot slot;
            [Tooltip("Клип, который сейчас стоит в базовом контроллере")]
            public AnimationClip baseClip;
        }

        [SerializeField] private Entry[] entries = Array.Empty<Entry>();

        public IReadOnlyList<Entry> Entries => entries;

        public bool TryGetBaseClip(StanceSlot slot, out AnimationClip clip)
        {
            foreach (var e in entries)
            {
                if (e.slot != slot) continue;
                clip = e.baseClip;
                return clip != null;
            }

            clip = null;
            return false;
        }

#if UNITY_EDITOR
        [Header("Editor: автозаполнение")]
        [SerializeField] private RuntimeAnimatorController sourceController;
        [SerializeField] private string idleStateName = "Idle";
        [SerializeField] private string locomotionStateName = "Locomotion";
        [Tooltip("Стейт с блендтри старта/остановки. Пока не используется в игре: если стейта нет, слоты StartStop останутся пустыми.")]
        [SerializeField] private string startStopStateName = "LocomotionStart";
        [Tooltip("Стейт реакции на урон: одномерный блендтри по DamageFront (порог +1 - Front, -1 - Back).")]
        [SerializeField] private string damageInterruptStateName = "DamageInterrupt";

        private void OnValidate()
        {
            var seen = new HashSet<StanceSlot>();
            foreach (var e in entries)
            {
                if (!seen.Add(e.slot))
                    Debug.LogWarning($"[{name}] слот {e.slot} указан несколько раз.", this);
            }
        }

        /// <summary>
        /// Берёт клипы из стейтов контроллера. Для блендтри считается, что X = ForwardMove, Y = SideMove:
        /// X>0 вперёд, X&lt;0 назад, Y&lt;0 влево, Y>0 вправо (так построен Locomotion).
        /// </summary>
        [ProButton]
        public void AutoFill()
        {
            var controller = sourceController as UnityEditor.Animations.AnimatorController;
            if (controller == null)
            {
                Debug.LogWarning($"[{name}] укажите sourceController (ассет AnimatorController).", this);
                return;
            }

            var found = new Dictionary<StanceSlot, AnimationClip>();

            var idle = FindState(controller, idleStateName);
            if (idle != null && idle.motion is AnimationClip idleClip) found[StanceSlot.Idle] = idleClip;
            else Debug.LogWarning($"[{name}] стейт '{idleStateName}' не найден или в нём не одиночный клип.", this);

            var loco = FindState(controller, locomotionStateName);
            if (loco != null)
            {
                FillDirections(loco.motion, found,
                    StanceSlot.RunForward, StanceSlot.RunBackward, StanceSlot.RunLeft, StanceSlot.RunRight);
            }
            else Debug.LogWarning($"[{name}] стейт '{locomotionStateName}' не найден.", this);

            var startStop = FindState(controller, startStopStateName);
            if (startStop != null)
            {
                FillDirections(startStop.motion, found,
                    StanceSlot.StartStopForward, StanceSlot.StartStopBackward,
                    StanceSlot.StartStopLeft, StanceSlot.StartStopRight);
            }

            var damage = FindState(controller, damageInterruptStateName);
            if (damage != null && damage.motion is AnimationClip damageClip)
            {
                found[StanceSlot.DamageInterrupt] = damageClip;
            }

            var list = new List<Entry>();
            foreach (StanceSlot slot in Enum.GetValues(typeof(StanceSlot)))
            {
                if (found.TryGetValue(slot, out var clip))
                    list.Add(new Entry { slot = slot, baseClip = clip });
                else
                    Debug.Log($"[{name}] слот {slot}: клип не найден (оставлен пустым).", this);
            }

            UnityEditor.Undo.RecordObject(this, "Auto-fill stance slot map");
            entries = list.ToArray();
            UnityEditor.EditorUtility.SetDirty(this);

            ValidateAgainstController();
        }

        /// <summary>
        /// Предупреждает о клипах, которых нет в контроллере, и о клипах, которые играют несколько стейтов:
        /// подмена заденет их все.
        /// </summary>
        [ContextMenu("Validate against controller")]
        private void ValidateAgainstController()
        {
            var controller = sourceController as UnityEditor.Animations.AnimatorController;
            if (controller == null)
            {
                Debug.LogWarning($"[{name}] укажите sourceController (ассет AnimatorController).", this);
                return;
            }

            var inController = new HashSet<AnimationClip>(controller.animationClips);

            foreach (var e in entries)
            {
                if (e.baseClip == null) continue;

                if (!inController.Contains(e.baseClip))
                {
                    Debug.LogWarning($"[{name}] слот {e.slot}: клипа '{e.baseClip.name}' нет в контроллере '{controller.name}'.", this);
                    continue;
                }

                var users = new List<string>();
                foreach (var layer in controller.layers)
                {
                    EachState(layer.stateMachine, state =>
                    {
                        if (Uses(state.motion, e.baseClip)) users.Add(state.name);
                    });
                }

                if (users.Count > 1)
                {
                    Debug.LogWarning(
                        $"[{name}] слот {e.slot}: клип '{e.baseClip.name}' играют несколько стейтов " +
                        $"({string.Join(", ", users)}). Подмена заденет их все.", this);
                }
            }
        }

        private static void FillDirections(
            Motion motion,
            Dictionary<StanceSlot, AnimationClip> into,
            StanceSlot forward, StanceSlot backward, StanceSlot left, StanceSlot right)
        {
            var tree = motion as UnityEditor.Animations.BlendTree;
            if (tree == null) return;

            foreach (var child in tree.children)
            {
                var clip = child.motion as AnimationClip;
                if (clip == null) continue;

                Vector2 p = child.position;
                if (Mathf.Abs(p.x) >= Mathf.Abs(p.y))
                    into[p.x > 0f ? forward : backward] = clip;
                else
                    into[p.y > 0f ? right : left] = clip;
            }
        }

        private static UnityEditor.Animations.AnimatorState FindState(
            UnityEditor.Animations.AnimatorController controller, string stateName)
        {
            UnityEditor.Animations.AnimatorState result = null;
            foreach (var layer in controller.layers)
            {
                EachState(layer.stateMachine, s =>
                {
                    if (result == null && s.name == stateName) result = s;
                });
            }

            return result;
        }

        private static void EachState(
            UnityEditor.Animations.AnimatorStateMachine machine,
            Action<UnityEditor.Animations.AnimatorState> action)
        {
            foreach (var s in machine.states) action(s.state);
            foreach (var sub in machine.stateMachines) EachState(sub.stateMachine, action);
        }

        private static bool Uses(Motion motion, AnimationClip clip)
        {
            if (motion == null) return false;
            if (motion == clip) return true;

            var tree = motion as UnityEditor.Animations.BlendTree;
            if (tree == null) return false;

            foreach (var child in tree.children)
            {
                if (Uses(child.motion, clip)) return true;
            }

            return false;
        }
#endif
    }
}
