using System;
using System.Collections;
using Arcatech.Items;
using com.cyborgAssets.inspectorButtonPro;
using KBCore.Refs;
using SpankyBoy.JuiceUI.Free;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Arcatech.UI
{
    [RequireComponent(typeof(PanelAnimator))]
    public class EquipmentNotificationWindow : ValidatedMonoBehaviour
    {
        [SerializeField, Self] private PanelAnimator animator;
        public PanelAnimator Animator => animator;
        [SerializeField]TextMeshProUGUI title;
        [SerializeField]TextMeshProUGUI description;
        [SerializeField] private UsablePanel usablePanelPrefab;
        [SerializeField] private Transform usablePanelsParent;

        private UsablePanel[] usablePanels;

        [SerializeField] private float time = 2f;
        public void LoadItem(Item item)
        {
            if (usablePanels == null)
            {
                usablePanels = new  UsablePanel[3];
                for (int i = 0; i < usablePanels.Length; i++)
                {
                    usablePanels[i] = Instantiate(usablePanelPrefab, usablePanelsParent);
                }
            }
            title.text = item.Description.Title;
            description.text = item.Description.Text;
            
            foreach (var t in usablePanels)
            {
                t.gameObject.SetActive(false);
            }
            
            if (item is IUsablesSource usables)
            {
                int index = 0;
                foreach (var usable in usables.GetUsables.Values)
                {
                    usablePanels[index].gameObject.SetActive(true);
                    usablePanels[index].SetInformation(usable);
                    index++;
                    if (index >= usablePanels.Length) break;
                }
            }

            _hide = StartCoroutine(HideWindow(time));
        }
        private Coroutine _hide;

        private IEnumerator HideWindow(float time)
        {
            yield return new WaitForSeconds(time);
            _hide = null;
            Animator.Hide();
        }
    }
}