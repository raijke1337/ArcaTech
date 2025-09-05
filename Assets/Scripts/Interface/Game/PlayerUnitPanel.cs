using Arcatech.Items;
using Arcatech.Stat;
using Arcatech.Stats;
using Arcatech.Units;
using DG.Tweening;
using KBCore.Refs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Arcatech.UI
{
    public class PlayerUnitPanel : ValidatedMonoBehaviour, IUnitActionsHandler, IUnitInventoryView, IStatUpdatesHandler
    {
        [SerializeField,Child] protected PlayerBarIconsContainerManager _icons;
        [SerializeField, Child] protected BarsContainersManager _bars;
        [SerializeField, Self] protected RectTransform _rect;
        [SerializeField] protected float _shakeAtHealthDelta = 0.1f;
        [SerializeField] protected Image _dmgGlow;
        [SerializeField] Color glowColor = Color.red;
        /// <summary>
        /// not called in this component
        /// </summary>
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
                _dmgGlow.color = glowColor;
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
                var st = _player.GetComponent<EntityStatsComponent>();
                st.RegisterStatChangesHandler(_bars);
                st.RegisterStatChangesHandler(this);

            }
        }

        #region interface
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

        
        public void HandleStatsUpdate(IDictionary<BaseStatType, StatValueContainer> stats)
        {
            var hp = stats[BaseStatType.Health];
            if (hp.GetFrameDeltaValue < 0 )
                if(hp.GetFrameDeltaPercentAbs > _shakeAtHealthDelta)
            {
                _rect.DOShakePosition(0.2f,5);

                    if (_dmgGlow != null)
                    {

                        _dmgGlow.DOFade(1, 0.3f).OnComplete(()=>_dmgGlow.DOFade(0, 0.3f));
                    }
            }
        }
        #endregion
    }

}