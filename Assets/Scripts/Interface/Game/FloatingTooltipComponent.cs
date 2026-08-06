using Arcatech.Interactions;
using KBCore.Refs;
using SpankyBoy.JuiceUI.Free;
using TMPro;
using UnityEngine;

namespace Arcatech.UI
{
    [RequireComponent(typeof(PanelAnimator_Free))]
    public class FloatingTooltipComponent : ValidatedMonoBehaviour
    {

        [SerializeField, Self] private PanelAnimator_Free animator;
        [SerializeField] private TextMeshProUGUI TitleLabel;
        [SerializeField] private TextMeshProUGUI InteractiveLabel;
        [SerializeField, Self] RectTransform rectT;

        public PanelAnimator_Free PanelAnimator => animator;
        private void Update()
        {
            UpdatePosition();
        }
        
        void UpdatePosition()
        {
            Vector2 mousePosition = Input.mousePosition;
        
            // Add offset so tooltip doesn't overlap cursor
            Vector2 offset = new Vector2(30f, -30f);
            rectT.position = mousePosition + offset;

           ClampToScreen();
        }
    
        void ClampToScreen()
        {
            Vector3[] corners = new Vector3[4];
            rectT.GetWorldCorners(corners);
        
            Vector3 position = rectT.position;
        
            // Check right edge
            if (corners[2].x > Screen.width)
                position.x -= corners[2].x - Screen.width;
            
            // Check left edge
            if (corners[0].x < 0)
                position.x -= corners[0].x;
            
            // Check top edge
            if (corners[2].y > Screen.height)
                position.y -= corners[2].y - Screen.height;
            
            // Check bottom edge
            if (corners[0].y < 0)
                position.y -= corners[0].y;
            
            rectT.position = position;
        }

        public void Set(ITargetable tgt)
        {
            TitleLabel.text = tgt.GetInfo.Title;
        }
        
    }
}