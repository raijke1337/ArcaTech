using Arcatech.EventBus;
using Arcatech.Items;
using Arcatech.Stat;
using Arcatech.Stats;
using Arcatech.Texts;
using Arcatech.Units;
using KBCore.Refs;
using System;
using UnityEngine;
using UnityEngine.Events;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace Arcatech.UI
{
    public class PlayerUnitPanel : ValidatedMonoBehaviour, IUnitActionsHandler, IUnitInventoryView
    {
        [SerializeField,Child] protected PlayerBarIconsContainerManager _icons;
        [SerializeField, Child] protected BarsContainersManager _bars;

        public event UnityAction<UnitInventoryViewReference> ViewChangedInventory;

        PlayerUnit _player;

        private void OnEnable()
        {
            _player = FindAnyObjectByType<PlayerUnit>();
            if ( _player != null )
            {
                var inv = _player.GetComponent<EntityInventoryComponent>();
                inv.SetModelView(this);
                _player.AssignActionsHandler(this);
                
            }
            else
            {
                Debug.LogWarning("No player in scene but player info panel is active");
            }
        }

        private void Start()
        {
            if (_player != null)
            {
                _bars.LinkStats(_player.GetComponent<EntityStatsComponent>());
            }
        }

        public bool TryHandleAction(UnitActionType type, EntityStatsComponent stats, out BaseUnitAction action)
        {
            action = null;
            _icons.HandlePlayerAction(type);

            return true;
        }

        public void RefreshView(UnitInventoryModel model)
        {
            foreach (var u in model.Handler.GetUsables.Values)
            {
                _icons.IconUpdate(u);
            }
        }



    }

}