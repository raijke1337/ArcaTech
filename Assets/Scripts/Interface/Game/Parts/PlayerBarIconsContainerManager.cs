using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Arcatech.UI
{
    public class PlayerBarIconsContainerManager : MonoBehaviour
    {

        [SerializeField] private IconContainerUIScript _iconPrefab;
        [SerializeField, Space] private Transform _usablesP;

        private Dictionary<IUsable, IconContainerUIScript> _usablesD = new();

        public void IconUpdate(IUsable content)
        {
            if (_usablesD.ContainsKey(content))
            {
                _usablesD[content].AssignIcon(content);
            }
            else
            {
                _usablesD[content] = Instantiate(_iconPrefab, _usablesP);
                _usablesD[content].AssignIcon(content);
            }
        }

        public void HandlePlayerAction(UnitActionType action)
        { 
            var k = _usablesD.FirstOrDefault(t=>t.Key.UseActionType == action);
            k.Value.OnUse();
        }
    }
}