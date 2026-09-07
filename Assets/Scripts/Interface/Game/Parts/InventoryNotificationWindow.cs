using System;
using System.Collections;
using Arcatech.Items;
using Arcatech.Units;
using KBCore.Refs;
using SpankyBoy.JuiceUI.Free;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.UI
{
    [RequireComponent(typeof(PanelAnimator))]
    public class InventoryNotificationWindow : ValidatedMonoBehaviour
    {
        [SerializeField, Self] PanelAnimator animator;
        public PanelAnimator Animator => animator;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private float textDisplayTime = 2f;



        public void ShowText(InventoryChangeNotification info)
        
        {            if (info.ChangeType == InventoryChangeType.Initialization ||
                         info.ChangeType == InventoryChangeType.Equip) return; // equip handled by a different window
            text.text = String.Concat($"{info.ChangedItem.Description.Title} {Mathf.Abs(info.ChangedQuantity)}");
            StartCoroutine(Display());
        }
        private IEnumerator Display()
        {
            yield return new WaitForSeconds(textDisplayTime);
            animator.Hide();
            
        }
    }
}