using Arcatech.Effects;
using Arcatech.Items;
using Arcatech.Managers;
using Arcatech.Skills;
using Arcatech.Triggers;
using Arcatech.Units.Stats;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Arcatech.Units
{
    [RequireComponent(typeof(ControlInputsBase))]

    public abstract class BaseUnit : MonoBehaviour, ITakesTriggers, IHasEffects
    {
        protected Animator _animator;
        protected Rigidbody _rigidbody;
        public Collider GetCollider { get; private set; }
        protected ControlInputsBase _controller;

        public Side Side;
        public BaseStatsConfig StatsConfig;
        public ItemEmpties GetEmpties => _controller.GetEmpties;

        public abstract ReferenceUnitType GetUnitType();
        public IReadOnlyDictionary<BaseStatType, StatValueContainer> GetStats => _controller.GetStatsController.GetBaseStats;

        public T GetInputs<T>() where T : ControlInputsBase => _controller as T;
        public ControlInputsBase GetInputs()
        {
            return _controller;
        }

        private bool _locked = false;
        public bool LockUnit
        {
            get
            {
                return _locked;
            }
            set
            {
                _locked = value;
                _controller.LockInputs = value;
                if (value)
                {
                    AnimateMovement(); // to reset the movement anim
                }
            }
        }

        public string GetFullName => _controller.GetStatsController.GetDisplayName;
        public bool IsUnitAlive { get; protected set; } = true;

        public event SimpleEventsHandler<BaseUnit> BaseUnitDiedEvent;
        public event SkillRequestedEvent SkillRequestFromInputsSuccessEvent;


        #region equipments
        public UnitInventoryComponent GetUnitInventory { get; protected set; }

        protected abstract void InitInventory();

        protected void CreateStartingEquipments(UnitInventoryComponent item) // equipment can't be changed mid-level so it's no problem here that this is run once
        {
            UpdateComponents();
            _controller.AssignDefaultItems(item);
        }
        public bool IsArmed
        {
            get
            {
                var list = new List<EquipmentItem>(GetUnitInventory.GetCurrentEquips);
                return list.Any(t => t.ItemType == EquipItemType.MeleeWeap) || list.Any(t => t.ItemType == EquipItemType.RangedWeap);
            }
        }

        #endregion

        #region managed
        public virtual void InitiateUnit() // this is run by unit manager
        {
            UpdateComponents();
            _controller.SetUnit(this);
            _controller.StartController();
            InitInventory();
            cachedForwardDir = transform.forward;

            switch (GameManager.Instance.GetCurrentLevelData.LevelType)
            {

                case LevelType.Scene:
                    _animator.SetLayerWeight(_animator.GetLayerIndex("Scene"), 100f);
                    break;
                case LevelType.Menu:
                    _animator.SetLayerWeight(_animator.GetLayerIndex("Scene"), 100f);
                    break;
                case LevelType.Game:
                    _animator.SetLayerWeight(3, 0); // to prevent warnings because only relevant to player ; might be TODO
                    ControllerEventsBinds(true);
                    rotationChecker = StartCoroutine(UpdateRotations());
                    break;
            }
            //Debug.Log($"Initiated {GetFullName}");
            LockUnit = false;
        }

        public virtual void DisableUnit()
        {
            ControllerEventsBinds(false);
            _controller.StopController();
            StopCoroutine(rotationChecker);
        }


        public virtual void RunUpdate(float delta)
        {
            if (_controller == null)
            {
                Debug.LogWarning($"Unit {this} was not initialized!");
                return;
            }

            if (LockUnit) return;

            AnimateMovement();
            if (GameManager.Instance.GetCurrentLevelData.LevelType != LevelType.Game) return;
            _controller.UpdateController(delta);
        }

        #endregion


        #region unit

        protected virtual void UpdateComponents()
        {
            if (GetCollider == null) GetCollider = GetComponent<Collider>();
            if (_animator == null) _animator = GetComponent<Animator>();
            if (_rigidbody == null) _rigidbody = GetComponent<Rigidbody>();
            if (_controller == null) _controller = GetComponent<ControlInputsBase>();
            if (_groundCollider == null) _groundCollider = GetComponent<GroundDetectorPlatformCollider>();
        }


        protected virtual void ControllerEventsBinds(bool isEnable)
        {
            if (isEnable)
            {
                _controller.CombatActionSuccessEvent += (t) => AnimateCombatActivity(t);
                _controller.StaggerHappened += AnimateStagger;
                _controller.SkillSpawnEvent += OnInputsCreateSkill;
                _controller.EffectEventRequest += EffectEventCallback;
                _controller.TriggerEventRequest += TriggerEventCallback;
                _controller.SpawnProjectileEvent += PlaceProjectileCallback;
                _controller.JumpCalledEvent += AnimateJump;
                _controller.ZeroHealthHappened += OnInputsReportDeath;
                _controller.DamageHappened += OnInputsReportDamage;

                if (_groundCollider != null)
                {
                    _groundCollider.PlatfromCollidedWithTagEvent += HandleJumpLanding;
                }
                        

            }
            else
            {

                _controller.CombatActionSuccessEvent -= (t) => AnimateCombatActivity(t);
                _controller.StaggerHappened -= AnimateStagger;
                _controller.SkillSpawnEvent -= OnInputsCreateSkill;
                _controller.EffectEventRequest -= EffectEventCallback;
                _controller.TriggerEventRequest -= TriggerEventCallback;
                _controller.SpawnProjectileEvent -= PlaceProjectileCallback;
                _controller.JumpCalledEvent -= AnimateJump;
                _controller.ZeroHealthHappened -= OnInputsReportDeath;
                _controller.DamageHappened -= OnInputsReportDamage;

                if (_groundCollider != null)
                {
                    _groundCollider.PlatfromCollidedWithTagEvent -= HandleJumpLanding;
                }

            }

        }

        private void OnInputsReportDamage(float arg)
        {
            HandleDamage(arg);
            if (_controller.DebugMessage)
            {
                Debug.Log($" {GetFullName} hp change {-arg}");
            }

        }

        protected void OnInputsCreateSkill(SkillProjectileComponent data, BaseUnit source, Transform where)
        {
            SkillRequestFromInputsSuccessEvent?.Invoke(data, source, where);
        }

        #endregion

        #region stats
        private void OnInputsReportDeath()
        {
            if (_controller.DebugMessage)
            {
                Debug.Log($"{GetFullName} died");
            }

            HandleDeath();
            IsUnitAlive = false;
            _animator.SetTrigger("Death");
            _controller.LockInputs = true;

            GetCollider.enabled = false;
            _rigidbody.useGravity = false;

            BaseUnitDiedEvent?.Invoke(this);

        }

        protected abstract void HandleDamage(float value);
        protected abstract void HandleDeath();


        public virtual void ApplyEffectToController(TriggeredEffect eff)
        {
            _controller.ApplyEffectToController(eff);
        }
        #endregion
        #region movement, animations
        //sets animator values 

        protected Vector3 cachedForwardDir;
        protected Vector3 crossProd;
        protected Coroutine rotationChecker;
        [SerializeField] protected float _rotationCheckerDelay;

        public float RotationValue { get => crossProd.y; }

        protected virtual void AnimateMovement()
        {
            Vector3 movement = _controller.GetMoveDirection;

            if (movement.x == 0f && movement.z == 0f)
            {
                _animator.SetBool("Moving", false);
                _animator.SetFloat("ForwardMove", 0f);
                _animator.SetFloat("SideMove", 0f);

                // rotate in place


                _animator.SetFloat("Rotation", crossProd.y);
            }
            else
            {
                _animator.SetFloat("Rotation",0);
                _animator.SetBool("Moving", true);
                CalcAnimVector(movement);
            }


        }
        protected IEnumerator UpdateRotations()
        {
            while (true)
            {
                crossProd = CalculateCrossProd();
                cachedForwardDir = transform.forward;
                if (_controller.DebugMessage)
                {
                    Debug.Log($"Current rotation value {crossProd.y}");
                }
                yield return new WaitForSeconds(_rotationCheckerDelay);
            }
            yield return null;
        }
        // virtual because playerunit uses aim point
        protected virtual Vector3  CalculateCrossProd()
        {
            return Vector3.Cross(cachedForwardDir, transform.forward);
        }


        [SerializeField] float _debugJumpForceMult;
        protected bool IsAirborne = false;
        [SerializeField] private GroundDetectorPlatformCollider _groundCollider;
        private void AnimateJump()
        {
            if (_controller.DebugMessage)
            {
                Debug.Log("Start jump");
            }
            _rigidbody.AddForce((_rigidbody.transform.forward + _rigidbody.transform.up) * _debugJumpForceMult, ForceMode.Impulse);
            IsAirborne = true;
            _animator.SetTrigger("JumpStart");
            _animator.SetBool("JumpAirborne",true);
        }

        private void HandleJumpLanding(string arg)
        {

            if (arg == "Ground" || arg == "SolidItem")
            {
                if (_controller.DebugMessage && IsAirborne)
                {
                    Debug.Log($"Land  jump on tag {arg}");
                }
                IsAirborne = false;
                _animator.SetTrigger("JumpEnd");
                _animator.SetBool("JumpAirborne", false);
            }
        }



        //  calculates the vector which is used to set values in animator
        protected void CalcAnimVector(Vector3 vector)
        {
            var playerFwd = transform.forward;
            var playerRght = transform.right;
            vector.y = 0;
            vector.Normalize();
            // Dot product of two vectors determines how much they are pointing in the same direction.
            // If the vectors are normalized (transform.forward and right are)
            // then the value will be between -1 and +1.
            var x = Vector3.Dot(playerRght, vector);
            var z = Vector3.Dot(playerFwd, vector);

            _animator.SetFloat("ForwardMove", z);
            _animator.SetFloat("SideMove", x);
        }

        protected virtual void AnimateCombatActivity(CombatActionType type)
        {
            if (GameManager.Instance.GetCurrentLevelData.LevelType != LevelType.Game) return;
            switch (type)
            {
                case CombatActionType.Melee:
                    _animator.SetTrigger("MeleeAttack");
                    break;
                case CombatActionType.Ranged:
                    _animator.SetTrigger("RangedAttack");
                    break;
                case CombatActionType.Dodge:
                    _animator.SetTrigger("Dodge");
                   // _controller.PerformDodging(); //old Dodge.
                    //SkillRequestCallBack(_controller.GetSkillsController.GetSkillDataByType(type), this);
                    break;
                case CombatActionType.MeleeSpecialQ:
                    _animator.SetTrigger("QSpecial");
                    //SkillRequestCallBack(_controller.GetSkillsController.GetSkillDataByType(type), this);
                    break;
                case CombatActionType.RangedSpecialE:
                    _animator.SetTrigger("ESpecial");
                    //SkillRequestCallBack(_controller.GetSkillsController.GetSkillIDByType(type), this);
                    break;
                case CombatActionType.ShieldSpecialR:
                    _animator.SetTrigger("RSpecial");
                    //SkillRequestCallBack(_controller.GetSkillsController.GetSkillIDByType(type), this);
                    break;
            }
        }

        public virtual void TriggerTogglingEvent_UE(float value)
        {    // 1 on start 0 on end
            if (_controller == null) return; //in case we have a scene

            bool result = value > 0;
            _controller.GetWeaponController.ToggleTriggersOnMelee(result);
        }

        protected virtual void AnimateStagger()
        {
            _animator.SetTrigger("TakeDamage");
           // Debug.Log($"{GetFullName} got stunned!");
        }


        #endregion



        #region trigger events
        public event TriggerEvent UnitTriggerRequestEvent;
        protected void TriggerEventCallback(BaseUnit tg, BaseUnit src, bool ent, TriggeredEffect cfg)
        {
            UnitTriggerRequestEvent?.Invoke(tg, src, ent, cfg);
        }

        #endregion


        #region effects

        public event EffectsManagerEvent BaseControllerEffectEvent;
        protected void EffectEventCallback(EffectRequestPackage pack) => BaseControllerEffectEvent?.Invoke(pack);
        #endregion


        #region projectiles
        public event SimpleEventsHandler<ProjectileComponent, BaseUnit> UnitPlacedProjectileEvent;
        protected void PlaceProjectileCallback(ProjectileComponent comp)
        {
            UnitPlacedProjectileEvent?.Invoke(comp, this);
        }
        #endregion

        


    }
}