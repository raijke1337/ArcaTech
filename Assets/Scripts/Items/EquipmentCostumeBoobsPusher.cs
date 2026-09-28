using System.Collections.Generic;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Items
{
    public class EquipmentCostumeBoobsPusher : MonoBehaviour, IEquipmentPart
    {
        [SerializeField] private string blendShapeName = "BreastsPushed";
        [SerializeField, Range(0f, 100f)] private float blendShapeValue = 100f;

        private struct SavedShape
        {
            public int Index;
            public float Weight;
        }

        private readonly Dictionary<SkinnedMeshRenderer, SavedShape> saved = new();

        public void OnEquip()
        {
            if (string.IsNullOrEmpty(blendShapeName))
            {
                Debug.LogWarning($"{nameof(EquipmentCostumeBoobsPusher)}: не задано имя blend shape", this);
                return;
            }

            var root = transform.parent;
            if (root == null)
            {
                Debug.LogWarning($"{nameof(EquipmentCostumeBoobsPusher)}: у объекта нет родителя", this);
                return;
            }

            foreach (var renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (renderer == null || renderer.sharedMesh == null)
                    continue;

                int index = renderer.sharedMesh.GetBlendShapeIndex(blendShapeName);
                if (index < 0)
                    continue; // у этого меша такого шейпа нет

                // Сохраняем оригинал только один раз, повторный OnEquip его не затрёт
                if (!saved.ContainsKey(renderer))
                {
                    saved[renderer] = new SavedShape
                    {
                        Index = index,
                        Weight = renderer.GetBlendShapeWeight(index)
                    };
                }

                renderer.SetBlendShapeWeight(index, blendShapeValue);
            }
        }

        public void OnUnequip()
        {
            foreach (var pair in saved)
            {
                var renderer = pair.Key;

                // Unity-null: рендерер мог быть уничтожен
                if (renderer == null || renderer.sharedMesh == null)
                    continue;

                // Защита на случай, если меш подменили и индекс больше не валиден
                if (pair.Value.Index >= renderer.sharedMesh.blendShapeCount)
                    continue;

                renderer.SetBlendShapeWeight(pair.Value.Index, pair.Value.Weight);
            }

            saved.Clear();
        }

        private void OnDestroy()
        {
            // Если костюм удалили, не вызвав OnUnequip, шейп не останется нажатым
            OnUnequip();
        }

        public void TriggerState(StateMachineNotifyType notification)
        { }
    }
}