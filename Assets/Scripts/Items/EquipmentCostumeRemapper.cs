using System.Collections.Generic;
using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Items
{
    public class EquipmentCostumeRemapper : MonoBehaviour, IEquipmentPart
    {
        [Tooltip("Корень скелета персонажа (например, Hips). Кости костюма ищутся среди его потомков по имени.")]
        [SerializeField] private Transform skeletonRoot;

        [Tooltip("Необязательно: собственная арматура костюма из FBX. Скрывается на время экипировки.")]
        [SerializeField] private Transform costumeArmature;

        [Tooltip("Включить, если костюм пропадает при анимации из-за неверных bounds.")]
        [SerializeField] private bool updateWhenOffscreen;

        private struct SavedSkin
        {
            public Transform[] Bones;
            public Transform RootBone;
            public bool UpdateWhenOffscreen;
        }

        private readonly Dictionary<SkinnedMeshRenderer, SavedSkin> saved = new();
        private readonly Dictionary<string, Transform> boneMap = new();
        private bool isEquipped;

        public void OnEquip()
        {
            if (isEquipped)
                return;
            skeletonRoot = transform.parent.transform;

            if (skeletonRoot == null)
            {
                Debug.LogError($"{nameof(EquipmentCostumeRemapper)}: не назначен skeletonRoot", this);
                return;
            }

            BuildBoneMap();
            if (boneMap.Count == 0)
            {
                Debug.LogError($"{nameof(EquipmentCostumeRemapper)}: в skeletonRoot нет костей", this);
                return;
            }

            foreach (var smr in GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (smr == null || smr.sharedMesh == null || saved.ContainsKey(smr))
                    continue;

                var oldBones = smr.bones;
                if (oldBones == null || oldBones.Length == 0)
                    continue;

                saved[smr] = new SavedSkin
                {
                    Bones = oldBones,
                    RootBone = smr.rootBone,
                    UpdateWhenOffscreen = smr.updateWhenOffscreen
                };

                var newBones = new Transform[oldBones.Length];
                for (int i = 0; i < oldBones.Length; i++)
                {
                    var old = oldBones[i];

                    if (old != null && boneMap.TryGetValue(old.name, out var mapped))
                    {
                        newBones[i] = mapped;
                    }
                    else
                    {
                        // Кость не найдена: оставляем свою, чтобы не было null в массиве
                        // (null-кости ломают рендер меша целиком)
                        newBones[i] = old;
                        Debug.LogWarning(
                            $"{nameof(EquipmentCostumeRemapper)}: кость '{(old != null ? old.name : "null")}' " +
                            $"({smr.name}) не найдена в скелете персонажа", this);
                    }
                }

                smr.bones = newBones;

                if (smr.rootBone != null && boneMap.TryGetValue(smr.rootBone.name, out var root))
                    smr.rootBone = root;
                else
                    smr.rootBone = skeletonRoot;

                if (updateWhenOffscreen)
                    smr.updateWhenOffscreen = true;
            }

            if (costumeArmature != null)
                costumeArmature.gameObject.SetActive(false);

            isEquipped = true;
        }

        public void OnRemove()
        {
            if (!isEquipped)
                return;

            foreach (var pair in saved)
            {
                var smr = pair.Key;
                if (smr == null)
                    continue; // рендерер уже уничтожен

                smr.bones = pair.Value.Bones;
                smr.rootBone = pair.Value.RootBone;
                smr.updateWhenOffscreen = pair.Value.UpdateWhenOffscreen;
            }

            saved.Clear();
            boneMap.Clear();

            if (costumeArmature != null)
                costumeArmature.gameObject.SetActive(true);

            isEquipped = false;
        }

        public void TriggerState(StateMachineNotifyType notification)
        { }

        private void BuildBoneMap()
        {
            boneMap.Clear();

            foreach (var t in skeletonRoot.GetComponentsInChildren<Transform>(true))
            {
                // TryAdd: при дубликатах имён берётся первая кость, остальные пропускаются

                boneMap.TryAdd(t.name, t);
                // if (!boneMap.TryAdd(t.name, t))
                //     Debug.LogWarning(
                //         $"{nameof(EquipmentCostumeRemapper)}: дубликат имени кости '{t.name}' в скелете персонажа", this);
            }
        }
    }
}