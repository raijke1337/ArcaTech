using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Arcatech.Interactions
{
    [CreateAssetMenu(fileName = "interaction_", menuName = "Interactions/Animation profile")]
    public sealed class InteractionAnimationProfileSO : ScriptableObject
    {
        [SerializeField, Tooltip("Нет записи — базовый клип. Запись с None — пропустить анимацию.")]
        private SerializedDictionary<InteractionState, AnimationClip> clips = new();

        public AnimationClip GetClip(InteractionState state) =>
            clips.TryGetValue(state, out var clip) ? clip : null;

        public bool SkipsAnimation(InteractionState state) =>
            clips.TryGetValue(state, out var clip) && clip == null;
    }
}
