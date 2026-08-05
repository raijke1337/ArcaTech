using Arcatech.Items;
using Arcatech.Stats;
using Arcatech.Units;
using DG.Tweening;
using KBCore.Refs;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Arcatech.UI
{
    public class PlayerUnitPanel : ValidatedMonoBehaviour, IUnitInventoryView, IStatUpdatesViewer, IUnitCommandPerformer
    {
        [SerializeField, Child] protected PlayerBarUsablesIconsContainerManager usablesIcons;
        [SerializeField, Child] protected BarsContainersManager barsManager;
        [SerializeField, Child] protected OverchargeUIMain overcharge;
        [SerializeField, Self] protected RectTransform _rect;
        [SerializeField] protected float _shakeAtHealthDelta = 0.1f;
        [SerializeField] protected Image _dmgGlow;
        [SerializeField] Color glowColor = Color.red;
        
        /// <summary>
        /// not called in this component
        /// </summary>
        public event UnityAction ViewChangedInventory;

        BaseGameEntityComponent _player;
        UsablesCasterComponent _usablesCasterComponent;
        TailsOverchargeModule _tailsOverchargeModule;

        
        private void Start()
        {
            _player = GameObject.FindWithTag("Player").GetComponent<BaseGameEntityComponent>();
            if (_player != null)
            {
                var st = _player.GetComponent<EntityStatsComponent>();
                if (st != null)
                {
                    st.RegisterStatsViewer(barsManager);
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
                }
                else
                {
                    Debug.LogWarning("Player has no usablesCaster component");
                }
                _tailsOverchargeModule = _player.GetComponent<TailsOverchargeModule>();
                if (_tailsOverchargeModule != null && barsManager.TryGetResourceBar(ResourceStatType.Energy, out var b))
                {
                    overcharge.SetDataSource(_tailsOverchargeModule);
                }
                else
                {
                    Debug.LogWarning("Failed to setup overcharge plugin");
                }
                
                
                var inputs = _player.GetComponent<UnitInputsComponent>();
                inputs.RegisterCommandHandler(this);
            }
            else
            {
                Debug.LogWarning("No player in scene but player info panel is active, disabling");
                gameObject.SetActive(false);
            }
        }



        #region interface

        public void RefreshView(UnitInventoryModel model)
        {
            // reload icons in case items have changed
            if (!_usablesCasterComponent) return;
            usablesIcons.LoadIcons(_usablesCasterComponent.GetUsables);
        }
        public void HandleStatsUpdate(ResourceStatType stat, float statCurrent, float statMax, float statDelta, EntityStatsComponent.ExpendType changeType,
            BaseGameEntityComponent source)
        {
            if (stat != ResourceStatType.Health) return;

            if (statDelta >= 0 || Mathf.Abs(statDelta / statMax) < _shakeAtHealthDelta) return;

            _rect.DOShakePosition(0.2f, 5);
            if (_dmgGlow != null)
            {
                _dmgGlow.DOFade(1, 0.3f).OnComplete(() => _dmgGlow.DOFade(0, 0.3f));
            }
        }

        public void SetShieldValue(ResourceStatType stat, float currentValue)
        { // NYI
        }

        #endregion

        public void PrepareCommand(UnitActionType type)
        { }

        public void DoUnitCommand(UnitActionType type, bool wasSuccessful)
        {
            usablesIcons.HandlePlayerAction(type, wasSuccessful);
        }
    }
}
