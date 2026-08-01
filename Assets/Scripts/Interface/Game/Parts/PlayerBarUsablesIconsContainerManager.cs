using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.UI
{
    public static class UIReferences
    {
        public static readonly IReadOnlyDictionary<UnitActionType, string> Hotkeys =
            new Dictionary<UnitActionType, string>
            {
                { UnitActionType.None, string.Empty },

                { UnitActionType.Melee, "LMB" },
                { UnitActionType.MeleeSkill, "Q" },

                { UnitActionType.Ranged, "RMB" },
                { UnitActionType.RangedSkill, "E" },

                { UnitActionType.ShieldSkill, "R" },
                { UnitActionType.DodgeSkill, "SHIFT" },
                { UnitActionType.Jump, "SPACE" },
                { UnitActionType.Use, "H" }
            };
    }

    public class PlayerBarUsablesIconsContainerManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private IconContainerUIScript iconPrefab;

        [SerializeField]
        private Transform usablesParent;

        private readonly Dictionary<UnitActionType, IconContainerUIScript> usablesIcons =
            new Dictionary<UnitActionType, IconContainerUIScript>();

        public void LoadIcons(Dictionary<UnitActionType, IUsable> usables)
        {
            
            if (usables == null)
            {
                HideAllIcons();
                return;
            }
            Debug.Log(
                $"[Usables UI] Actions received: {string.Join(", ", usables.Keys)}",
                this
            );
                
            /*
             * Скрываем иконки, для которых больше нет действия
             * в текущем наборе экипировки / инвентаря.
             *
             * Layout Group не учитывает inactive-объекты,
             * поэтому фон автоматически сузится.
             */
            foreach (var loadedIcon in usablesIcons)
            {
                bool actionStillExists = usables.ContainsKey(loadedIcon.Key);

                if (!actionStillExists && loadedIcon.Value != null)
                {
                    loadedIcon.Value.gameObject.SetActive(false);
                }
            }

            int iconIndex = 0;

            foreach (var usablePair in usables)
            {
                UnitActionType actionType = usablePair.Key;
                IUsable usable = usablePair.Value;

                Debug.Log(
                    $"[Usables UI] Processing: {actionType}; " +
                    $"usable null: {usable == null}; " +
                    $"runtime type: {(usable == null ? "null" : usable.GetType().Name)}",
                    this
                );

                if (actionType == UnitActionType.None)
                {
                    continue;
                }

                if (usable == null)
                {
                    Debug.LogWarning(
                        $"[Usables UI] Action {actionType} есть в Dictionary, но его IUsable = null. " +
                        $"Иконка не будет создана.",
                        this
                    );

                    continue;
                }

                IconContainerUIScript icon = GetOrCreateIcon(actionType);

                icon.gameObject.SetActive(true);

                icon.AssignIcon(usable)
                    .WithHotkey(GetHotkey(actionType));

                Debug.Log(
                    $"[Usables UI] Icon ready: {actionType}; " +
                    $"GameObject: {icon.gameObject.name}; " +
                    $"Active: {icon.gameObject.activeSelf}",
                    this
                );

                icon.transform.SetSiblingIndex(iconIndex);
                iconIndex++;
            }
        }

        public void HandlePlayerAction(UnitActionType action, bool success)
        {
            if (usablesIcons.TryGetValue(action, out IconContainerUIScript icon) &&
                icon != null &&
                icon.gameObject.activeInHierarchy)
            {
                icon.OnUse(success);
            }
        }

        private IconContainerUIScript GetOrCreateIcon(UnitActionType actionType)
        {
            if (usablesIcons.TryGetValue(actionType, out IconContainerUIScript existingIcon) &&
                existingIcon != null)
            {
                return existingIcon;
            }

            IconContainerUIScript newIcon = Instantiate(iconPrefab, usablesParent);

            newIcon.name = $"Usable Icon [{actionType}]";

            usablesIcons[actionType] = newIcon;

            return newIcon;
        }

        private string GetHotkey(UnitActionType actionType)
        {
            return UIReferences.Hotkeys.TryGetValue(actionType, out string hotkey)
                ? hotkey
                : string.Empty;
        }

        private void HideAllIcons()
        {
            foreach (var iconPair in usablesIcons)
            {
                if (iconPair.Value != null)
                {
                    iconPair.Value.gameObject.SetActive(false);
                }
            }
        }
    }
}