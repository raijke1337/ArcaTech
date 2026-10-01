using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Arcatech.Items
{
    /// <summary>
    /// Стойка оружия: какими клипами заменить idle и бег, пока предмет в руках.
    /// Хранит только заменяющие клипы по слотам; какие клипы заменяются, знает StanceSlotMap.
    /// Пустой слот - играет клип безоружной стойки.
    /// </summary>
    [CreateAssetMenu(fileName = "stance_", menuName = "Items/Weapon stance")]
    public class WeaponStanceSO : ScriptableObject
    {
        [SerializeField, Tooltip("Если в руках несколько предметов со стойкой, побеждает стойка с большим приоритетом.")]
        private int priority;

        [SerializeField] private SerializedDictionary<StanceSlot, AnimationClip> clips;

        public int Priority => priority;

        public bool TryGetClip(StanceSlot slot, out AnimationClip clip)
        {
            clip = null;
            return clips != null && clips.TryGetValue(slot, out clip) && clip != null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            WarnIfPartial(StanceSlotGroups.Run, "Run");
            WarnIfPartial(StanceSlotGroups.StartStop, "StartStop");
            WarnIfPartial(StanceSlotGroups.DamageInterrupt, "DamageInterrupt");
        }

        private void WarnIfPartial(StanceSlot[] group, string label)
        {
            if (clips == null) return;

            int filled = 0;
            foreach (var slot in group)
            {
                if (TryGetClip(slot, out _)) filled++;
            }

            if (filled != 0 && filled != group.Length)
            {
                Debug.LogWarning(
                    $"[{name}] группа '{label}' заполнена частично ({filled}/{group.Length}): " +
                    "остальные направления будут играть клипы безоружной стойки.", this);
            }
        }
#endif
    }
}
