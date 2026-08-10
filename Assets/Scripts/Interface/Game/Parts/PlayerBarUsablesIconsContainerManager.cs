using System.Collections.Generic;
using System.Linq;
using KBCore.Refs;
using SpankyBoy.JuiceUI.Free;
using UnityEngine;

namespace Arcatech.UI
{
    [RequireComponent(typeof(PanelAnimator_Free))]
    public class PlayerBarUsablesIconsContainerManager : ValidatedMonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private IconContainerUIScript iconPrefab;

        [SerializeField]
        private Transform usablesParent;
        
        [SerializeField,Child] private PanelAnimator_Free panelAnimator;
        public PanelAnimator_Free Animator => panelAnimator;

        private readonly Dictionary<UnitActionType, IconContainerUIScript> usablesIcons =
            new Dictionary<UnitActionType, IconContainerUIScript>();

        public void LoadIcons(Dictionary<UnitActionType, IUsable> usables)
        {
            if (usables == null)
            {
                HideAllIcons();
                return;
            }
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
                if (!UIReferences.ShownUsableTypes.Contains(usablePair.Key)) continue;
                
                UnitActionType actionType = usablePair.Key;
                IUsable usable = usablePair.Value;

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


                //icon.transform.SetSiblingIndex(iconIndex);
                icon.transform.SetAsLastSibling();
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