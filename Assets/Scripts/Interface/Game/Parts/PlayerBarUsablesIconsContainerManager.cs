using System;
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
        public void LoadIcons(IEnumerable<IUsable> usables)
        {
            if (_usablesD == null)
            {
                //first use
                _usablesD = new Dictionary<UnitActionType, IconContainerUIScript>();
                foreach (var usable in usables)
                {
                    var icon = Instantiate(_iconPrefab,usablesParent);
                    _usablesD[usable.UseActionType] = icon;
                    icon.AssignIcon(usable);
                }
            }
            else
            {
                // do a check if some icons need to be hidden
                foreach (var loaded in _usablesD.Keys)
                {
                    if (!usables.Any(t => t.UseActionType == loaded))
                    {
                        _usablesD[loaded].gameObject.SetActive(false);
                    }
                }
                // do a change of existing ones
                foreach (var usable in usables)
                {
                    // look if this action type already has an icon
                    if (_usablesD.ContainsKey(usable.UseActionType))
                    {
                        var icon = _usablesD[usable.UseActionType];
                        icon.gameObject.SetActive(true); // in case it was disabled earlier
                        icon.AssignIcon(usable);
                    }
                    else
                    {
                        var icon = Instantiate(_iconPrefab,usablesParent);
                        _usablesD[usable.UseActionType] = icon;
                        icon.AssignIcon(usable);
                    }
                }
            }

        }
        public void HandlePlayerAction(UnitActionType action,bool success)
        { 
            var k = _usablesD.FirstOrDefault(t=>t.Key == action);
            if (k.Value != null)
            {
                k.Value.OnUse(success);
            }
        }
    }
}