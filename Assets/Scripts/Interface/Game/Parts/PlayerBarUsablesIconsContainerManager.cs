using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Arcatech.UI
{
    public class PlayerBarUsablesIconsContainerManager : MonoBehaviour
    {

        [SerializeField] private IconContainerUIScript _iconPrefab;
        [SerializeField, Space] private Transform usablesParent;

        private Dictionary<UnitActionType, IconContainerUIScript> _usablesD;
        public void LoadIcons(Dictionary<UnitActionType,IUsable> usables)
        {
            if (usables.Count == 0) return;
            
            if (_usablesD == null)
            {
                //first use
                _usablesD = new Dictionary<UnitActionType, IconContainerUIScript>();
                foreach (var usable in usables)
                {
                    var icon = Instantiate(_iconPrefab,usablesParent);
                    _usablesD[usable.Key] = icon;
                    icon.AssignIcon(usable.Value);
                }
            }
            else
            {
                // do a check if some icons need to be hidden
                foreach (var loaded in _usablesD.Keys)
                {
                    if (usables.All(t => t.Key != loaded))
                    {
                        _usablesD[loaded].gameObject.SetActive(false);
                    }
                }
                // do a change of existing ones
                foreach (var usable in usables)
                {
                    // look if this action type already has an icon
                    if (_usablesD.TryGetValue(usable.Key, out var icon1))
                    {
                        icon1.gameObject.SetActive(true); // in case it was disabled earlier
                        icon1.AssignIcon(usable.Value);
                    }
                    else
                    {
                        var icon = Instantiate(_iconPrefab,usablesParent);
                        _usablesD[usable.Key] = icon;
                        icon.AssignIcon(usable.Value);
                    }
                }
            }

        }
        public void HandlePlayerAction(UnitActionType action,bool success)
        {
            if (_usablesD == null) return;
            
            var k = _usablesD.FirstOrDefault(t=>t.Key == action);
            if (k.Value != null)
            {
                k.Value.OnUse(success);
            }
        }
    }
}