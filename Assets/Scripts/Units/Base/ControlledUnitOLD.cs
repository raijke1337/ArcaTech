using Arcatech.Items;
using Arcatech.Managers;
using Arcatech.Triggers;
using Arcatech.Units.Stats;
using ECM.Components;
using KBCore.Refs;
using System;
using System.Collections;
using UnityEngine;
namespace Arcatech.Units
{
    [RequireComponent(typeof(ControlInputsBaseOLD), typeof(Rigidbody),typeof(GroundDetection))]
    public abstract class ControlledUnitOLD : ArmedUnitOLD, IInteractible
    {
        [Space, Header("Controlled Unit")]
         [SerializeField] protected MovementStatsConfig movementStats;
        [SerializeField, Self] protected Rigidbody _rb;
        [Self, SerializeField] protected ControlInputsBaseOLD _inputs;
        [Self, SerializeField] protected GroundDetection _ground;
        #region MANAGED
        public override void StartControllerUnit()
        {
            base.StartControllerUnit();
            if (GameManager.Instance.GetCurrentLevelData.LevelType == LevelType.Game)
            {
                UnitPaused = false;
                _inputs.StartController();
            }
            _inputs.UnitActionRequestedEvent += HandleUnitAction;
            _inputs.RequestInteraction += HandleInteractionAction;
            //if (_stats.TryGetStatValue(BaseStatType.Stamina, out var stam))
            //{
            //    stunEndStamina = Mathf.Clamp(stunEndStamina, stam.GetMin, stam.GetMax);
            //}
            //else
            //{
            //    stunEndStamina = 0;
            //}
           
        }

        private void HandleUnitAction(UnitActionType type)
        {
            Debug.Log($"PLACEHOLDER: handle unit actions not operational - refactoring");
        }

        public override void RunUpdate(float delta)
        {
            base.RunUpdate(delta);
            //if (_stunned)
            //{
            //    if (_stats.TryGetStatValue(BaseStatType.Stamina, out var s))
            //    {
            //        if (s.GetCurrent >= stunEndStamina && stunEndProgress == null)
            //        {
            //            stunEndProgress = StartCoroutine(StunCancelCoroutine());
            //        }
            //    }
            //    else return;
            //}

            _inputs.ControllerUpdate(delta);
        }
                public override void DisableUnit()
        {
            base.DisableUnit();
            _inputs.UnitActionRequestedEvent -= HandleUnitAction;
            _inputs.StopController();
        }
        #endregion

        /// <summary>
        /// Stuns will be implemented differently through the stats component check
        /// </summary>
        /// 
        //[Space,Header("Stuns")]

        //[SerializeField] protected SerializedUnitAction ActionOnStun;
        //[SerializeField, Range(0, 300)] protected float stunStartStamina = 0f;
        //[SerializeField, Range(0, 300)] protected float stunEndStamina = 30f;
        //[SerializeField, Range(0.01f, 1)] protected float stunEndGetUpTime = 0.5f;

        //Coroutine stunEndProgress;


        //protected bool _stunned = false;
        //public bool UnitStunned
        //{
        //    get => _stunned;
        //    protected set
        //    {
        //        _stunned = value;

        //        if (_showDebugs) Debug.Log($"Entity stunned: {value}");
        //    }
        //}

        //IEnumerator StunCancelCoroutine()
        //{
        //    yield return new WaitForSeconds(stunEndGetUpTime);
        //    _stunned = false;
        //    _animator.SetTrigger("StunEnd");
        //    ActionLock = false;
        //    stunEndProgress = null;
        //    yield return null;
        //}
        //protected override void StunAction()
        //{
        //    if (UnitStunned) return;
        //    OnForceAction(ActionOnStun.ProduceAction(this,transform));
        //    UnitStunned = true;
        //}

        //protected override void OnTimedStatsUpdate()
        //{
        //    if (_stats.TryGetStatValue(BaseStatType.Stamina, out var stam))
        //    {
        //        if (stam.GetCurrent <= stunStartStamina)
        //        {
        //            StunAction();
        //        }
                
        //    }
        //    base.OnTimedStatsUpdate();
        //}





        #region interaction

        public virtual void ReceiveInteraction(IInteractible interactible)
        {
            if (UnitDebug) Debug.Log($"NYI: {this} receives interaction from {interactible}");
        }

        protected abstract void HandleInteractionAction(IInteractible i);

        #endregion

    }
}