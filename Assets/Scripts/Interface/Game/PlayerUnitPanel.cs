using System;
using Arcatech.Items;
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
    public class PlayerUnitPanel : ValidatedMonoBehaviour, IUnitInventoryView, IStatUpdatesViewer
    {
        [SerializeField,Child] protected PlayerBarUsablesIconsContainerManager usablesIcons;
        [SerializeField, Child] protected BarsContainersManager _bars;
        [SerializeField, Self] protected RectTransform _rect;
        [SerializeField] protected float _shakeAtHealthDelta = 0.1f;
        [SerializeField] protected Image _dmgGlow;
        [SerializeField] Color glowColor = Color.red;
        /// <summary>
        /// not called in this component
        /// </summary>
        public event UnityAction ViewChangedInventory;
        PlayerUnit _player;
        UsablesCasterComponent _usablesCasterComponent;

        private void Start()
        {
            _player = FindAnyObjectByType<PlayerUnit>();
            if ( _player != null )
            {
                var st = _player.GetComponent<EntityStatsComponent>();
                if (st != null)
                {
                    st.RegisterStatsViewer(_bars);
                    st.RegisterStatsViewer(this);
                }
                else
                {
                    Debug.LogWarning("Player has no stats component, disabling");
                    gameObject.SetActive(false);
                }
                
                var inv = _player.GetComponent<EntityInventoryComponent>();
                if (inv != null)
                {
                    inv.SetModelView(this);

                }
                else
                {
                    Debug.LogWarning("Player has no inventory component");
                }


                _usablesCasterComponent = _player.GetComponent<UsablesCasterComponent>();
                if (_usablesCasterComponent != null)
                {
                    usablesIcons.LoadIcons(_usablesCasterComponent.GetUsables);
                    _usablesCasterComponent.ActionAnnounce += AnimateIcons;
                }
                else
                {
                    Debug.LogWarning("Player has no usablesCaster component");
                }
            }
            else
            {
                Debug.LogWarning("No player in scene but player info panel is active, disabling");
                gameObject.SetActive(false);
            }
        }


        private void OnDisable()
        {
            _usablesCasterComponent.ActionAnnounce -= AnimateIcons;
        }
        
        void AnimateIcons(UnitActionType type, bool success) => usablesIcons.HandlePlayerAction(type,success);

        #region interface

        public void RefreshView(UnitInventoryModel model)
        {
            // reload icons in case items have changed
            if (!_usablesCasterComponent) return;
            usablesIcons.LoadIcons(_usablesCasterComponent.GetUsables);
        }

        

        


        public void HandleStatsUpdate(ResourceStatType stat, float statCurrent, float statMax, float statDelta, object changeSource)
        {
            if (stat != ResourceStatType.Health) return;

            if (statDelta >= 0 || Mathf.Abs(statDelta / statMax) < _shakeAtHealthDelta) return;
            
            _rect.DOShakePosition(0.2f,5);
            if (_dmgGlow != null)
            {
                _dmgGlow.DOFade(1, 0.3f).OnComplete(()=>_dmgGlow.DOFade(0, 0.3f));
            }
        }
        
        #endregion
    }

}