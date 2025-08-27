using Arcatech.Items;
using Arcatech.Stat;
using Arcatech.Stats;
using Arcatech.Triggers;
using Arcatech.Units.Stats;
using DG.Tweening;
using KBCore.Refs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Units
{
    public class BaseEntityOLD : ValidatedMonoBehaviour, IInteractible
    {
        protected const float zeroF = 0f;
        #region depreciated

        [Header("Entity")]
        //[SerializeField] protected bool _showDebugs = false;//
        //[SerializeField] protected Side entitySide; //


        //[SerializeField] protected BaseStatsConfig defaultStats; //
        //[SerializeField] protected float statsUpdateFrequency = 0.1f; // 

       // protected UnitStatsControllerOLD _stats; // moved to separate component TODO all the stuff

        

        #endregion



        [SerializeField,Self] protected EntityStatsComponent _stats;
        [SerializeField,Self] protected BaseGameEntityComponent _gameEntity;


        public virtual void ApplyEffect(StatsEffect eff, IEquippable shield, out float current)
        {
            current = 0;
            if (UnitDead) return;
            _stats.ApplyStatsEffect(eff);

            //if (_stats.CanApplyEffect(eff, shield))
            //{
            //    current = _stats.GetStatValues[eff.StatType].GetCurrent;
            //}
            OnTimedStatsUpdate();
        }



        #region managed


        public Side Side => _gameEntity.GetEntitySide; //
        public bool UnitDebug => _gameEntity.ShowingDebugs;
        public string UnitName => _gameEntity.GetName;

        [SerializeField] protected Transform _headT;

        public virtual void StartControllerUnit() // this is run by unit manager
        {
            if (UnitDebug) Debug.Log($"Starting {UnitName}");
            // _stats = new UnitStatsControllerOLD(defaultStats.BuildBaseStats, this);
            //  _stats.StartController();

            _stats = GetComponent<EntityStatsComponent>();
            statsUpdateTimer = new CountDownTimer(0.5f); // placeholder TODO
            statsUpdateTimer.Start();
        }

        public virtual void DisableUnit()
        {
            if (UnitDebug) Debug.Log($"Stopping {UnitName}");
        }

        /// <summary>
        /// we have action lock - on actions
        /// unitpause - on game pause
        /// unitDead - on death
        /// </summary>

        #region locks
        private bool _paused = false;
        public bool UnitPaused
        {
            get
            {
                return _paused;
            }
            set
            {
                if (UnitDebug) Debug.Log($"{UnitName} paused: {value}");
                OnUnitPause(value);
                _paused = value;
            }
        }

        void OnUnitPause(bool pause)
        {
            Debug.Log($"Pause currently NYI, refactoring");
        }

        private bool _dead = false;
        public bool UnitDead
        {
            get { return _dead; }
            set
            {
                if (UnitDebug) Debug.Log($"{UnitName} dead: {value}");
                UnitPaused = value;
                _dead = value;
            }
        }


        #endregion


        public virtual void RunUpdate(float delta)
        {
            if (UnitPaused || UnitDead) return;

            // _stats.ControllerUpdate(delta);
            if (!_stats.DidInit) return;

            if (statsUpdateTimer!=null)
            {
                statsUpdateTimer?.Tick(delta);
                if (statsUpdateTimer.IsReady)
                {
                    OnTimedStatsUpdate();
                    statsUpdateTimer.Reset();
                    statsUpdateTimer.Start();
                }
            }
        }
        public virtual void RunFixedUpdate(float delta)
        {
            if (UnitPaused) return;

            //_stats.FixedControllerUpdate(delta);
        }

        #endregion
        #region stats
        CountDownTimer statsUpdateTimer;
        public event UnityAction<BaseEntityOLD> BaseEntityDeathEvent = delegate { };

        protected virtual void OnTimedStatsUpdate()
        {
            //if (_stats.GetAllStats[BaseStatType.Health].GetCurrent <= 0) DeathAction(); 
        }



        //protected virtual void DamageAction()
        //{            
        //    if (ActionOnDamage != null)
        //    {                
        //        ForceUnitAction(ActionOnDamage.ProduceAction(this, transform));
        //    }
        //}

        //protected virtual void DeathAction()
        //{
        //    if (ActionOnDeath != null)
        //    {
        //        ForceUnitAction(ActionOnDeath.ProduceAction(this,transform));
        //    }
        //    if(TryGetComponent<Collider>(out var c))
        //    {
        //        c.enabled = false;
        //    }
        //    UnitDead = true;
        //    BaseEntityDeathEvent.Invoke(this);
        //}
        //protected virtual void StunAction() { }

        //protected virtual void OnUnitPause(bool isPause)
        //{
        //    Debug.Log($"{UnitName} OnPause NYI");
        //}


        #endregion

        #region actions depreciated


        ///// <summary>
        ///// actions moved to ActiveGameUnitComponent 
        ///// </summary>
        ///// <param name="act"></param>

        //[Space, SerializeField] protected SerializedUnitAction ActionOnDamage;
        //[SerializeField] protected SerializedUnitAction ActionOnDeath;
        //[SerializeField, Tooltip("Place to spawn effects")] protected Transform _headT;

        //[SerializeField] protected Animator _animator;
        //public Animator AnimatorReference => _animator;
        //public void ForceUnitAction(BaseUnitAction act)
        //{
        //    if (UnitPaused || act == null) return;
        //    OnForceAction(act);
        //}
        //protected virtual void OnForceAction(BaseUnitAction act)
        //{
        //    act.StartAction();
        //}

        //Tweener force;
        //public virtual void ApplyForceResultToUnit(float speed, float distance)
        //{
        //    if (gameObject.TryGetComponent<Rigidbody>(out var rb))
        //    {
        //        Vector3 end = rb.transform.position + (rb.transform.forward * distance);
        //        force = rb.DOMove(end, Mathf.Abs(distance / speed), false);
        //    }
        //    else
        //    {
        //        Debug.Log($"Tried to apply impulse {distance} to {UnitName} but it has no rigidbody");
        //    }
        //}
        //private void OnCollisionEnter(Collision collision)
        //{
        //    if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        //    {
        //        //if (_showDebugs) Debug.Log("Boom");
        //        force?.Kill();
        //    }
        //}

        #endregion


        #region itargetable

        public IReadOnlyDictionary<BaseStatType, StatValueContainer> GetDisplayValues => _stats.GetAllStats;


        #endregion

        #region iinteractible
        public virtual void AcceptInteraction(IInteractible target)
        {
            Debug.Log($"{UnitName} tried to interact with {target.UnitName} ");
        }
        public Vector3 Position => transform.position;

        public bool Triggered
        {
            get
            {
                if (UnitDebug) Debug.Log($"checking if {UnitName} was triggered by something, returning false - NYI");
                return false;
            }
        }
        #endregion

    }



}