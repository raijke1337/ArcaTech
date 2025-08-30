using Arcatech.EventBus;
using Arcatech.Items;
using Arcatech.Managers;
using Arcatech.Triggers;
using Arcatech.Units.Inputs;
using ECM.Components;
using KBCore.Refs;
using UnityEngine;


namespace Arcatech.Units
{
    [RequireComponent(typeof(GroundDetection))]
    public class PlayerUnit : ActiveGameUnitComponent
    {
        [Space, Header("Player Unit")]
        //[SerializeField] int _armorBreakStam = 30;
        //[SerializeField] int _armorBreakEnergy = 30;

        [SerializeField, Child] protected Camera _faceCam;
        [SerializeField, Self] protected GroundDetection _ground;
        CostumesControllerComponent costumes;

        protected void ToggleCamera(bool value) { _faceCam.enabled = value; }

        
        protected override void Start()
        {
            ToggleCamera(true);
            base.Start();
        }

        public override void ApplyForceResultToUnit(float speed, float distance)
        {
            base.ApplyForceResultToUnit(speed, distance);
            //_movement.DisableGroundingOnUnitImpulse(speed, distance);
        }
        protected override bool CanAct()
        {
            return _ground.isOnGround && _ground.isValidGround;
        }

        #region inventory

        //protected override UnitInventoryItemConfigsContainer SelectSerializedItemsConfig()
        //{

        //    if (DataManager.Instance.IsNewGame)
        //    {
        //        return new UnitInventoryItemConfigsContainer(defaultEquips);
        //    }
        //    else
        //    {
        //        return DataManager.Instance.GetPlayerSaveEquips;
        //    }

        //}
        #endregion

        #region stats
        //protected override void OnTimedStatsUpdate()
        //{
        //    foreach(var k in _stats.GetAllStats.Keys)
        //    {
        //        EventBus<PlayerStatsChangedUIEvent>.Raise(new PlayerStatsChangedUIEvent(k, _stats.GetAllStats[k]));
        //    }
        //    base.OnTimedStatsUpdate();
        //}

        //protected override void DamageAction()
        //{
        //    if (UnitDebug) Debug.Log($"Armor break NYI");
        //    //if (_stats.TryGetStatValu(BaseStatType.Stamina).GetCurrent <= _armorBreakStam && _stats.TryGetStatValu(BaseStatType.Energy).GetCurrent <= _armorBreakEnergy)
        //    //{
        //    //    if (_showDebugs) Debug.Log($"Armor break!");
        //    //    costumes.OnBreak();
        //    //}
        //    base.DamageAction();
        //}

        //protected override void DeathAction()
        //{
        //    base.DeathAction();
        //    _movement.SetDesiredMoveDirection(Vector3.zero);
        //}

        #endregion
        #region pause
        //private void OnInputsPauseButton()
        //{
        //    if (UnitDead) return;
        //    else
        //    {
        //        EventBus<PauseToggleEvent>.Raise(new PauseToggleEvent(!UnitPaused));
        //    }
        //}
        //protected override void OnUnitPause(bool isPause)
        //{
        //    // also stop moving
        //    _movement.SetDesiredMoveDirection(Vector3.zero);
        //}


        #endregion

        //protected override void HandleInteractionAction(IInteractible i)
        //{
        //    i.AcceptInteraction(this);
        //}
        //public override void AcceptInteraction(IInteractible target)
        //{
        //    if (target is ItemSOContainerComponent containerComponent)
        //    {
        //        if (containerComponent.Content is EquipSO equip)
        //        {
        //            _inventory.TryEquipItem(equip);
        //        }
        //        else
        //        {

        //        }
        //    }
        //    base.AcceptInteraction(target);
        //}

    }

}