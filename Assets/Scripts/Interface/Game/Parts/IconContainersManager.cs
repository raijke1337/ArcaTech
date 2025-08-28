using Arcatech.Items;
using Arcatech.Skills;
//using com.cyborgAssets.inspectorButtonPro;
using KBCore.Refs;
using System.Collections.Generic;
using UnityEngine;
namespace Arcatech.UI
{
    public class IconContainersManager : MonoBehaviour
    {

        [SerializeField] private IconContainerUIScript _iconPrefab;
        [SerializeField, Space] private Transform _usablesP;

        private Dictionary<IIconContent, IconContainerUIScript> _usablesD = new();

        public void IconUpdate(IIconContent content)
        {
            if (_usablesD.ContainsKey(content))
            {
                _usablesD[content].UpdateIcon(content);
            }

            else
            {
                _usablesD[content] = Instantiate(_iconPrefab, _usablesP);
                _usablesD[content].UpdateIcon(content);
            }

        }
    }
}