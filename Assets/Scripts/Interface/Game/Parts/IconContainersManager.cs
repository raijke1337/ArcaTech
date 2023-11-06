using Arcatech.Items;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Arcatech.UI
{
    public class IconContainersManager : ManagedControllerBase
    {
        private List<IconContainerUIScript> _icons;
        [SerializeField] private IconContainerUIScript _iconPrefab;

        public void TrackItemIcon(EquipmentItem item) // TODO: Proper tracking once items are refactored
        {
            var icon = Instantiate(_iconPrefab, transform);

            icon.Image.sprite = item.ItemIcon;
            icon.Text.text = "NYI";
            _icons.Add(icon);
        }


        public override void StartController()
        {
            _icons = new List<IconContainerUIScript>();
            if (_iconPrefab == null)
            {
                Debug.LogError($"Set icon prefab in {this}!");
            }
        }

        public override void UpdateController(float delta)
        {
            
        }

        public override void StopController()
        {
            
        }
    }
}