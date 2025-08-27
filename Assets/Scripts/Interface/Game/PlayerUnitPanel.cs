using Arcatech.EventBus;
using Arcatech.Items;
using Arcatech.Stats;
using Arcatech.Texts;
using Arcatech.Units;
using KBCore.Refs;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.UI
{
    public class PlayerUnitPanel : PanelWithBarGeneric
    {
        [SerializeField,Child] protected IconContainersManager _icons;
        EventBinding<UpdateIconEvent> bindIcons;

        private void Awake()
        {
            bindIcons = new EventBinding<UpdateIconEvent>(RefreshIcon);
            EventBus<UpdateIconEvent>.Register(bindIcons);
        }

        public void OnInventoryUpdate (UnitInventoryControllerOLD inv)
        {

        }


        private void RefreshIcon(UpdateIconEvent obj)
        {
            if (obj.User is PlayerUnit)
            {
                _icons.IconUpdate(obj.Used);
            }
        }



        private void OnDisable()
        {
            EventBus<UpdateIconEvent>.Deregister(bindIcons);
        }

    }

}