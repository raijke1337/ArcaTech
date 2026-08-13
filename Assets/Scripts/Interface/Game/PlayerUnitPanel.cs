using System.Linq;
using Arcatech.Items;
using Arcatech.Managers;
using Arcatech.Stats;
using ArcaTech.UI;
using Arcatech.Units;
using Arcatech.Units.Control;
using KBCore.Refs;
using SpankyBoy.JuiceUI.Free;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.UI
{
    public class PlayerUnitPanel : ValidatedMonoBehaviour, IUnitInventoryView, IStatUpdatesViewer, IUnitCommandPerformer
    {
        [SerializeField, Child] protected PlayerBarUsablesIconsContainerManager usablesIcons;
        [SerializeField, Child] protected BarsContainersManager barsManager;
        [SerializeField, Child] protected OverchargeUIMain overcharge;
        [SerializeField,Self] PanelAnimator_Free panelAnimator;
        [Space, SerializeField, Range(0,200)] private int bigDamageThreshold = 25;
        /// <summary>
        /// not called in this component
        /// </summary>
        public event UnityAction ViewChangedInventory;
        BaseGameEntityComponent _player;
        private EntityStatsComponent st;
        UsablesCasterComponent _usablesCasterComponent;
        TailsOverchargeModule _tailsOverchargeModule;
        public void Show() => panelAnimator.Show();
        public void Hide() => panelAnimator.Hide();
        
        private void Start()
        {
            _player = GameObject.FindWithTag("Player").GetComponent<BaseGameEntityComponent>();
            if (_player != null)
            {
                st = _player.GetComponent<EntityStatsComponent>();
                if (st != null)
                {
                    st.RegisterStatsViewer(barsManager);
                    st.RegisterStatsViewer(this);
                }
                else
                {
                    Debug.LogWarning("Player has no stats component, disabling");
                    panelAnimator.Hide();
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
                SetupUsables();
                SetupOvercharge();
                
                var inputs = _player.GetComponent<UnitInputsComponent>();
                inputs?.RegisterCommandHandler(this);
            }
            else
            {
                Debug.LogWarning("No player in scene but player info panel is active, disabling");
                panelAnimator.Hide();
            }
        }


        void SetupUsables()
        {
            _usablesCasterComponent ??= _player.GetComponent<UsablesCasterComponent>();
    
            if (_usablesCasterComponent != null)
            {
                if (_usablesCasterComponent.GetUsables.Keys.
                    Intersect(UIReferences.ShownUsableTypes).
                    Any())
                {
                    usablesIcons.gameObject.SetActive(true);
                    usablesIcons.LoadIcons(_usablesCasterComponent.GetUsables);
                    usablesIcons.Animator.Show();
                    return;
                }
            }
    
            // Используем условие для предотвращения повторного вызова Hide
            if (usablesIcons.gameObject.activeSelf)
            {
                usablesIcons.Animator.Hide();
            }
        }

        void SetupOvercharge()
        {
            _tailsOverchargeModule ??= _player.GetComponent<TailsOverchargeModule>();
    
            if (_tailsOverchargeModule != null && 
                barsManager.TryGetResourceBar(ResourceStatType.Energy, out var b) &&
                st.TryGetMax(ResourceStatType.Energy, out var v) &&
                v > 0)
            {
                overcharge.gameObject.SetActive(true);
                overcharge.SetDataSource(_tailsOverchargeModule);
                overcharge.Animator.Show();
            }
            else
            {
                if (overcharge.gameObject.activeSelf)
                {
                    overcharge.Animator.Hide();
                }
            }
        }
        #region interface

        public void RefreshView(UnitInventoryModel model)
        {
            SetupUsables();
            SetupOvercharge();
        }
        public void HandleStatsUpdate(ResourceStatType stat, float statCurrent, float statMax, float statDelta, EntityStatsComponent.ExpendType changeType,
            BaseGameEntityComponent source)
        {
            if (stat == ResourceStatType.Health && statDelta < 0)
            {
                GameInterfaceManager.Instance.ShowGlitchEffect();
                if (Mathf.Abs(statDelta) > bigDamageThreshold)
                {
                    GlitchController.Instance.TriggerGlitch();
                }
                else
                {
                    GlitchController.Instance.TriggerGlitch(0.2f,0.3f);
                }
            }
        }

        public void SetShieldValue(ResourceStatType stat, float currentValue)
        { // NYI
        }

        #endregion

        public void PrepareCommand(UnitCommand command)
        {// noop
            
        }

        public void DoUnitCommand(UnitCommand command, bool wasSuccessful)
        {
            usablesIcons.HandlePlayerAction(command.Type, wasSuccessful);
        }
    }
}
