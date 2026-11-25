using System.Collections.Generic;
using UnityEngine;

namespace Arcatech
{
    // Общий для стоек и взаимодействий экземпляр контроллера на конкретном Animator.
    [DisallowMultipleComponent]
    public sealed class AnimatorClipOverrides : MonoBehaviour
    {
        private AnimatorOverrideController controller;
        private RuntimeAnimatorController source;
        private Animator animator;
        private readonly Dictionary<AnimationClip, AnimationClip> defaults = new();
        private readonly List<KeyValuePair<AnimationClip, AnimationClip>> changes = new();

        public static AnimatorClipOverrides For(Animator animator)
        {
            if (animator == null || animator.runtimeAnimatorController == null) return null;
            if (!animator.TryGetComponent(out AnimatorClipOverrides overrides))
                overrides = animator.gameObject.AddComponent<AnimatorClipOverrides>();
            if (overrides.controller == null) overrides.Initialize(animator);
            return overrides;
        }

        private void Initialize(Animator target)
        {
            animator = target;
            source = target.runtimeAnimatorController;
            controller = source is AnimatorOverrideController existing
                ? Instantiate(existing)
                : new AnimatorOverrideController(source);
            controller.name = source.name + " (runtime)";
            controller.GetOverrides(changes);
            foreach (var pair in changes) defaults.Add(pair.Key, pair.Value);
            changes.Clear();
            animator.runtimeAnimatorController = controller;
        }

        public void SetClip(AnimationClip original, AnimationClip replacement)
        {
            if (original == null) return;
            if (!defaults.TryGetValue(original, out var defaultOverride))
            {
                Debug.LogWarning($"[{name}] Клип '{original.name}' отсутствует в '{source.name}'.", this);
                return;
            }
            changes.Add(new(original, replacement != null ? replacement : defaultOverride));
        }

        public void Apply()
        {
            if (changes.Count == 0) return;
            controller.ApplyOverrides(changes);
            changes.Clear();
        }

        private void OnDestroy()
        {
            if (animator != null && animator.runtimeAnimatorController == controller)
                animator.runtimeAnimatorController = source;
            if (controller != null) Destroy(controller);
        }
    }
}
