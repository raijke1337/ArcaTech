using Arcatech.Effects;
using Arcatech.Items;
using Arcatech.Managers;
using Arcatech.Skills;
using Arcatech.Triggers;
using Arcatech.Units.Inputs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Units
{
    public abstract class ControlInputsBase : ManagedControllerBase, ITakesTriggers
    {
        protected BaseUnit Unit;

        public abstract ReferenceUnitType GetUnitType();
        private bool _inputsLocked;
        [SerializeField] public bool LockInputs
        {
            get 
            { 
                return _inputsLocked; 
            }
            set 
            { 
                _inputsLocked = value; 
                if (_inputsLocked)
                {
                    Debug.Log($"Lock inputs in {Unit.GetFullName}");
                    MoveDirectionFromInputs = Vector3.zero;
                }
            }
        }// todo ?


        protected List<BaseController> _controllers = new List<BaseController>();

        public void SetUnit(BaseUnit u) => Unit = u;

        protected float lastDelta;

        [SerializeField] protected ItemEmpties Empties;
        public ItemEmpties GetEmpties => Empties;

        protected BaseStatsController _statsCtrl;
        protected WeaponController _weaponCtrl;
        protected DodgeController _dodgeCtrl; // checks timer, the actual dodge is a skill
        protected ShieldController _shieldCtrl;
        protected SkillsController _skillCtrl;
        protected ComboController _comboCtrl;
        protected StunsController _stunsCtrl;

        public DodgeController GetDodgeController => _dodgeCtrl;
        public ShieldController GetShieldController => _shieldCtrl;
        public BaseStatsController GetStatsController => _statsCtrl;
        public SkillsController GetSkillsController => _skillCtrl;
        public WeaponController GetWeaponController => _weaponCtrl;


        public event SimpleEventsHandler StaggerHappened;
        public event SimpleEventsHandler ShieldBreakHappened;
        protected void StunEventCallback() => StaggerHappened?.Invoke();
        protected virtual void ShieldBreakEventCallback() => ShieldBreakHappened?.Invoke();

        #region ai
        public StatValueContainer AssessStat(TriggerChangedValue stat)
        {
            switch (stat)
            {
                case TriggerChangedValue.Health:
                    return _statsCtrl.GetBaseStats[BaseStatType.Health];
                case TriggerChangedValue.Shield:
                    return _shieldCtrl.GetShieldStats[ShieldStatType.Shield];
                case TriggerChangedValue.Combo:
                    return _comboCtrl.GetComboContainer;
                default:
                    return null;
            }
        }
        #endregion

        #region items
        public void AssignDefaultItems(UnitInventoryComponent items)
        {
            items.InventoryUpdateEvent += CheckEquipsOnInventoryUpdate;
            var skillsItems = new List<EquipmentItem>();

            foreach (var item in items.GetCurrentEquips)
            {
                if (item.Skill != null)
                {
                    skillsItems.Add(item);
                }
                EquipmentItem removed = null;
                switch (item.ItemType)
                {
                    default:
                        Debug.Log($"assigned {item} on {Unit.name} controller but it is not implemented");
                        break;
                    case (EquipItemType.MeleeWeap):
                        _weaponCtrl.LoadItem(item, out removed);
                        break;
                    case (EquipItemType.RangedWeap):
                        _weaponCtrl.LoadItem(item, out removed);
                        break;
                    case (EquipItemType.Shield):
                        _shieldCtrl.LoadItem(item, out removed);
                        break;
                    case (EquipItemType.Booster):
                        _dodgeCtrl.LoadItem(item, out removed);
                        break;
                }
            }

            if (_skillCtrl != null) // null in scenes
            {
                foreach (var item in skillsItems)
                {
                    _skillCtrl.LoadItemSkill(item);
                }
            }            
        }

        private void CheckEquipsOnInventoryUpdate(UnitInventoryComponent inve)
        {
            if (GameManager.Instance.GetCurrentLevelData.LevelType == LevelType.Game) return; // something was picked up and no need to refresh invenotry
            // yikes

            Debug.Log($"Inventory update in {Unit}");

            _weaponCtrl.TryRemoveItem(EquipItemType.MeleeWeap, out _);
            _weaponCtrl.TryRemoveItem(EquipItemType.RangedWeap, out _);
            _dodgeCtrl.TryRemoveItem(EquipItemType.Booster, out _);
            _shieldCtrl.TryRemoveItem(EquipItemType.Shield, out _);
            // TODO // 


            foreach (EquipmentItem e in inve.GetCurrentEquips)
            {
                switch (e.ItemType)
                {
                    default:
                        Debug.Log($"No equipment logic for equipement {e}, type {e.ItemType}");
                        break;
                    case (EquipItemType.MeleeWeap):                        
                        _weaponCtrl.LoadItem(e, out _);
                        break;
                    case (EquipItemType.RangedWeap):
                        _weaponCtrl.LoadItem(e, out _);
                        break;
                    case (EquipItemType.Shield):
                        _shieldCtrl.LoadItem(e, out _);
                        break;
                    case (EquipItemType.Booster):
                        _dodgeCtrl.LoadItem(e, out _);
                        break;
                }
            }
        }


        #endregion


        #region scenes

        public Vector3 InputDirectionOverride = Vector3.zero;
        #endregion

        #region ManagedController
        public override void UpdateController(float delta)
        {
            if (LockInputs) return; 
            lastDelta = delta;
            foreach (BaseController ctrl in _controllers)
            {
                if (ctrl.IsReady)
                    ctrl.UpdateInDelta(delta);
            }
        }
        public override void StartController()
        {
#if UNITY_EDITOR
            if (GameManager.Instance == null) return;
#endif
            switch (GameManager.Instance.GetCurrentLevelData.LevelType)
            {
                case LevelType.Menu:
                    Initialize(false);
                    break;
                case LevelType.Scene:
                    Initialize(false);
                    if (this is InputsPlayer p)
                    {
                        MoveDirectionFromInputs = InputDirectionOverride; // placeholder moved into condition because it stopped npcs from working in scenes
                    }
                        break;
                case LevelType.Game:
                    Initialize(true);
                    break;
            }
        }
        public override void StopController()
        {
            foreach (var ctrl in _controllers)
            {
                if (ctrl.IsReady)
                {
                    ctrl.StopStatsComponent();

                    ControllerSubs(ctrl, false);
                }
            }
            if (_stunsCtrl != null) // also TODO issue in scenes
            {
                _stunsCtrl.StunHappenedEvent -= StunEventCallback;
            }
        }

        private void Initialize(bool full)
        {

            _statsCtrl = new BaseStatsController(Unit);
            _shieldCtrl = new ShieldController(Empties, Unit);
            _dodgeCtrl = new DodgeController(Empties, Unit);
            _weaponCtrl = new WeaponController(Empties, Unit);

            _controllers.Add(_statsCtrl);
            _controllers.Add(_shieldCtrl);
            _controllers.Add(_dodgeCtrl);
            _controllers.Add(_weaponCtrl);

            if (full)
            {
                _comboCtrl = new ComboController(Unit);
                _stunsCtrl = new StunsController(Unit);
                _skillCtrl = new SkillsController(Empties, Unit);

                _controllers.Add(_skillCtrl);
                _controllers.Add(_comboCtrl);
                _controllers.Add(_stunsCtrl);

                LockInputs = false;
                _stunsCtrl.StunHappenedEvent += StunEventCallback;
            }

            foreach (var ctrl in _controllers)
            {
                ctrl.SetupStatsComponent();
                ControllerSubs(ctrl, true);
            }
        }

        protected void ControllerSubs(BaseController ctrl, bool isStart)
        {
            if (isStart)
            {
                if (ctrl is ShieldController s)
                {
                    s.ShieldBrokenEvent += ShieldBreakEventCallback;
                }
                ctrl.BaseControllerEffectEvent += EffectEventCallback;
                ctrl.BaseControllerTriggerEvent += TriggerEventCallBack;
                ctrl.ProjectileWasSpawnedEvent += ProjectileEventCallBack;

            }
            else
            {
                ctrl.BaseControllerEffectEvent -= EffectEventCallback;
                ctrl.BaseControllerTriggerEvent -= TriggerEventCallBack;
                ctrl.ProjectileWasSpawnedEvent -= ProjectileEventCallBack;

                if (ctrl is ShieldController s)
                {
                    s.ShieldBrokenEvent -= ShieldBreakEventCallback;
                }
            }
        }

        private void ProjectileEventCallBack(ProjectileComponent prefab)
        {
            SpawnProjectileEvent?.Invoke(prefab);
        }
        #endregion

        #region effects manager


        public event EffectsManagerEvent EffectEventRequest;
        protected void EffectEventCallback(EffectRequestPackage pack)
        {
            EffectEventRequest?.Invoke(pack);
        }
        #endregion

        #region triggers

        public event TriggerEvent TriggerEventRequest;
        public void ApplyEffectToController(TriggeredEffect eff)
        {
            switch (eff.StatType)
            {
                case TriggerChangedValue.Health:
                    if (!_shieldCtrl.IsReady)
                    {
                        _statsCtrl.ApplyEffectToController(eff);
                    }
                    else
                    {
                        _statsCtrl.ApplyEffectToController(_shieldCtrl.ProcessHealthChange(eff));
                    }
                    break;
                case TriggerChangedValue.Shield:
                    _shieldCtrl.ApplyEffectToController(eff);
                    break;
                case TriggerChangedValue.Combo:
                    _comboCtrl.ApplyEffectToController(eff);
                    break;
                case TriggerChangedValue.MoveSpeed:
                    _statsCtrl.ApplyEffectToController(eff);
                    break;
                case TriggerChangedValue.TurnSpeed:
                    _statsCtrl.ApplyEffectToController(eff);
                    break;
                case TriggerChangedValue.Stagger:
                    _stunsCtrl.ApplyEffectToController(eff);
                    break;
            }
        }

        // this needs to be updated to get unique triggeredeffects from each use of item
        protected void TriggerEventCallBack(BaseUnit target, BaseUnit source, bool isEnter, TriggeredEffect cfg)
        {
            TriggerEventRequest?.Invoke(target, source, isEnter, cfg);
        }

        #endregion
        #region projectiles

        public event SimpleEventsHandler<ProjectileComponent> SpawnProjectileEvent;

        protected void SpawnProjectileCallBack(ProjectileComponent proj)
        {
            SpawnProjectileEvent?.Invoke(proj);
        }

        #endregion


        #region skill requests manager
        protected void SkillSpawnEventCallback(SkillProjectileComponent data)
        {

            SkillSpawnEvent?.Invoke(data, Unit, Empties.ItemPositions[data.GetProjectileSettings.SpawnPlace]);

        }

        public event SkillRequestedEvent SkillSpawnEvent;

        #endregion

        #region movement

        public void ToggleBusyControls_AnimationEvent(int state)
        {
            LockInputs = state != 0;
        }

        public ref Vector3 GetMoveDirection => ref MoveDirectionFromInputs;
        protected Vector3 MoveDirectionFromInputs;

        protected virtual void LerpRotateToTarget(Vector3 looktarget, float delta)
        {

            Vector3 relativePosition = looktarget - transform.position;
            Quaternion rotation = Quaternion.LookRotation(relativePosition, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation,
                delta * _statsCtrl.GetBaseStats[BaseStatType.TurnSpeed].GetCurrent);
        }
        public event SimpleEventsHandler JumpCalledEvent;
        protected virtual void DoJumpAction()
        {
            if (!LockInputs)
            JumpCalledEvent?.Invoke();
        }

        #endregion

        #region combat actions

        public event SimpleEventsHandler<CombatActionType> CombatActionSuccessEvent;
        private void CombatActionSuccessCallback(CombatActionType type) => CombatActionSuccessEvent?.Invoke(type);
        public bool IsInMeleeCombo = false;

        protected virtual void DoCombatAction (CombatActionType type)
        {
            bool canAct = true;
            

            if (LockInputs && !IsInMeleeCombo)
            {
                //Debug.Log($"{Unit} tried to do action {type} but has inputs locked");
                canAct = false;
            }
            switch (type)
            {
                case CombatActionType.Melee:
                    if (canAct || IsInMeleeCombo)
                    {
                        canAct = _weaponCtrl.OnWeaponUseSuccessCheck(EquipItemType.MeleeWeap);
                        if (canAct) CombatActionSuccessCallback(type);
                    }
                    break;
                case CombatActionType.Ranged:
                    if (canAct)
                    {
                        canAct = _weaponCtrl.OnWeaponUseSuccessCheck(EquipItemType.RangedWeap);
                        if (canAct) CombatActionSuccessCallback(type);
                    }
                    break;
                case CombatActionType.Dodge:
                    if (canAct && _skillCtrl.TryUseSkill(type, _comboCtrl, out var sk))
                    {
                        SkillSpawnEventCallback(sk);
                        CombatActionSuccessCallback(type);
                    }
                    break;
                case CombatActionType.MeleeSpecialQ:
                    if (canAct && _skillCtrl.TryUseSkill(type, _comboCtrl, out sk))
                    {
                        SkillSpawnEventCallback(sk);
                        CombatActionSuccessCallback(type);
                    }
                    break;
                case CombatActionType.RangedSpecialE:
                    if (_skillCtrl.TryUseSkill(type, _comboCtrl, out sk))
                    {
                        SkillSpawnEventCallback(sk);
                        CombatActionSuccessCallback(type);
                    }
                    break;
                case CombatActionType.ShieldSpecialR:
                    if (_skillCtrl.TryUseSkill(type, _comboCtrl, out sk))
                    {
                        SkillSpawnEventCallback(sk);
                        CombatActionSuccessCallback(type);
                    }
                    break;
            }
        }


        #endregion


        #region dodging

        public void StartDodgeMovement(BoosterSkillInstanceComponent bs)
        {
            Debug.Log($"Start dodge for unit: {Unit}");
            _dodgeCor = StartCoroutine(DodgingMovement(bs));
        }

        private Coroutine _dodgeCor;
        private IEnumerator DodgingMovement(BoosterSkillInstanceComponent bs)
        {
            var stats = bs.GetDodgeSettings;

            LockInputs = true;

            Vector3 start = transform.position;
            Vector3 end = start + GetMoveDirection * stats.Range;

            float p = 0f;
            while (p <= 1f)
            {
                p += Time.deltaTime * stats.Speed;
                transform.position = Vector3.Lerp(start, end, p);
                yield return null;
            }
            LockInputs = false;
            yield return null;
        }
        // stop the dodge like this

        protected void OnCollisionEnter(Collision collision)
        {
            if (_dodgeCor != null && !collision.gameObject.CompareTag("Ground"))
            {
                Debug.Log($"Collided with {collision.gameObject.name} with tag {collision.gameObject.tag}\n {Unit} dodge cancelled.");
                LockInputs = false;
                StopCoroutine(_dodgeCor);
                _dodgeCor = null;
            }
        }

        #endregion



    }

}