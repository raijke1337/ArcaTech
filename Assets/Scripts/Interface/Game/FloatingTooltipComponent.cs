using System;
using Arcatech.Interactions;
using Arcatech.Units.Control;
using TMPro;
using UnityEngine;

namespace Arcatech.UI
{
    public class FloatingTooltipComponent : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI TitleLabel;
        [SerializeField] private TextMeshProUGUI InteractiveLabel;
        private RectTransform _rectTransform;
        private void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            UpdatePosition();
        }
        
        void UpdatePosition()
        {
            Vector2 mousePosition = Input.mousePosition;
        
            // Add offset so tooltip doesn't overlap cursor
            Vector2 offset = new Vector2(30f, -30f);
            _rectTransform.position = mousePosition + offset;

           ClampToScreen();
        }
    
        void ClampToScreen()
        {
            Vector3[] corners = new Vector3[4];
            _rectTransform.GetWorldCorners(corners);
        
            Vector3 position = _rectTransform.position;
        
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
            
            _rectTransform.position = position;
        }

        public void Set(ITargetable tgt)
        {
            TitleLabel.text = tgt.GetInfo.Title;
        }
        
    }
}