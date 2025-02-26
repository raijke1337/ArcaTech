using Arcatech.EventBus;
using Arcatech.Items;
using Arcatech.Stats;
using Arcatech.Triggers;
using Arcatech.Units.Stats;
using DG.Tweening;
using ECM.Components;
using KBCore.Refs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.ProBuilder.MeshOperations;

namespace Arcatech.Units
{
    [RequireComponent(typeof(GroundDetection))]
    public class BaseEntity : ValidatedMonoBehaviour, IInteractible
    {
        protected const float zeroF = 0f;
        [Header("Entity")]
        [SerializeField] protected bool _showDebugs = false;
        [SerializeField] protected Side _unitSide;
        [SerializeField] protected BaseStatsConfig defaultStats;
        [SerializeField] protected float statsUpdateFrequency = 0.1f;
        protected UnitStatsController _stats;
        [Space, SerializeField] protected SerializedUnitAction ActionOnDamage;
        [SerializeField] protected SerializedUnitAction ActionOnDeath;
        [SerializeField] protected SerializedUnitAction ActionOnStun;
        [SerializeField, Tooltip("Place to spawn effects")] protected Transform _headT;

        [SerializeField, Self] protected Animator _animator;

        protected GroundDetection _ground;
        public bool UnitDebug => _showDebugs;
        public string UnitName { get => defaultStats.DisplayName; }
        [HideInInspector] public Side Side => _unitSide;

        #region managed

        protected override void OnValidate()
        {
            base.OnValidate();
            Assert.IsFalse(defaultStats==null);
            Assert.IsNotNull(_headT);
        }
        public virtual void StartControllerUnit() // this is run by unit manager
        {
            if (_showDebugs) Debug.Log($"Starting unit {this}");
            _ground = GetComponent<GroundDetection>();
            _stats = new UnitStatsController(defaultStats.InitialStats, this);
            _stats.StartController();
            statsUpdateTimer = new CountDownTimer(statsUpdateFrequency);
            statsUpdateTimer.Start();
        }

        public virtual void DisableUnit()
        {
            if (_showDebugs) Debug.Log($"Stopping unit {this}");
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
                if (_showDebugs) Debug.Log($"Entity paused: {value}");
                OnUnitPause(value);
                _paused = value;
            }
        }

        private bool _dead = false;
        public bool UnitDead
        {
            get { return _dead; }
            set
            {
                if (_showDebugs) Debug.Log($"Entity dead: {value}");
                UnitPaused = value;
                _dead = value;
            }
        }

        #endregion


        public virtual void RunUpdate(float delta)
        {
            if (UnitPaused || UnitDead) return;

            _stats.ControllerUpdate(delta);
            if (statsUpdateTimer!=null)
            {
                statsUpdateTimer?.Tick(delta);
                if (statsUpdateTimer.IsReady)
                {
                    UpdateStats();
                    statsUpdateTimer.Reset();
                    statsUpdateTimer.Start();
                }
            }

        }
        public virtual void RunFixedUpdate(float delta)
        {
            if (UnitPaused) return;

            _stats.FixedControllerUpdate(delta);
        }

        #endregion
        #region stats
        CountDownTimer statsUpdateTimer;
        public event UnityAction<BaseEntity> BaseEntityDeathEvent = delegate { };

        protected virtual void UpdateStats()
        {
            var stats = _stats.GetStatValues;
            foreach (var k in stats.Keys)
            {
                switch (k)
                {
                    case BaseStatType.Health:
                        if (stats[k].GetCurrent == 0) HandleDeath();
                        break;
                    case BaseStatType.Stamina:
                        if (stats[k].GetCurrent == 0) HandleStun();
                        break;
                    case BaseStatType.Energy:
                        break;
                }
            }
        }

        public virtual void ApplyEffect(StatsEffect eff, IEquippable shield = null)
        {
            if (_stats.CanApplyEffect(eff, out var curr, shield))
            {
                switch (eff.StatType)
                {
                    case BaseStatType.Health:
                        if (eff.InitialValue < 0)
                        {
                            EventBus<DrawDamageEvent>.Raise(new DrawDamageEvent(this, Mathf.Abs(eff.InitialValue)));
                            HandleDamage(Mathf.Abs(eff.InitialValue));
                        }
                        if (curr == 0) HandleDeath();
                        break;
                    case BaseStatType.Stamina:
                        if (curr == 0) HandleStun();
                        break;
                    case BaseStatType.Energy:
                        break;
                }
            }
            UpdateStats();

        }

        protected virtual void HandleDamage(float value)
        {
            if (_showDebugs) Debug.Log($"{UnitName} took dmg {value}");
            if (ActionOnDamage != null)
            {                
                ForceUnitAction(ActionOnDamage.ProduceAction(this, transform));
            }
        }

        protected virtual void HandleDeath()
        {
            
            if (ActionOnDeath != null)
            {
                ForceUnitAction(ActionOnDeath.ProduceAction(this,transform));
            }
            if(TryGetComponent<Collider>(out var c))
            {
                c.enabled = false;
            }

            UnitDead = true;
            BaseEntityDeathEvent.Invoke(this);
        }
        protected virtual void HandleStun()
        {
            if (_showDebugs) Debug.Log($"{UnitName} got stunned");
            if (ActionOnStun != null)
            {
                ForceUnitAction(ActionOnStun.ProduceAction(this, _headT));
            }
        }
        protected virtual void OnUnitPause(bool isPause)
        {
            Debug.Log($"Entity paused: {isPause} and nothing else happened because this is not overwritten");
        }


        #endregion

        #region actions
        public void ForceUnitAction(BaseUnitAction act)
        {
            if (UnitPaused || act == null) return;
            OnForceAction(act);
        }
        protected virtual void OnForceAction(BaseUnitAction act) 
        {
            act.StartAction();
        }

        Tweener force;
        public virtual void ApplyForceResultToUnit(float speed, float distance)
        {
            if (gameObject.TryGetComponent<Rigidbody>(out var rb))
            {
                Vector3 end = rb.transform.position + (rb.transform.forward * distance);
                force = rb.DOMove(end, Mathf.Abs(distance / speed), false);
                //Debug.Log($"Tried to apply impulse {distance} to {GetName} but it has no movement controller component, using dotween");
                //rb.AddForce(rb.transform.forward * distance * 5f,ForceMode.Impulse);
            }
            else
            {
                Debug.Log($"Tried to apply impulse {distance} to {UnitName} but it has no rigidbody");
            }
        }
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {
               //if (_showDebugs) Debug.Log("Boom");
               force?.Kill();
            }
        }


        #endregion


        #region itargetable

        public IReadOnlyDictionary<BaseStatType, StatValueContainer> GetDisplayValues => _stats.GetStatValues;


        #endregion

        #region iinteractible
        public void AcceptInteraction(IInteractible actor)
        {
            Debug.Log($"Tried to interact with {UnitName}");
        }
        public Vector3 Position => transform.position;
        #endregion
    }



}