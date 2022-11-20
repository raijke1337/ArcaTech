using Assets.Scripts.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Zenject;
using static UnityEngine.InputSystem.InputAction;

[RequireComponent(typeof(BaseUnit))]
public abstract class ControlInputsBase : MonoBehaviour, ITakesTriggers
{
    protected BaseUnit Unit; 
    protected StatsUpdatesHandler _handler;


    public void SetUnit(BaseUnit u) => Unit = u;
    public void SetHandler(StatsUpdatesHandler h) => _handler = h;
    public abstract UnitType GetUnitType();

    [SerializeField] public bool IsControlsBusy; // todo ?



    [SerializeField] protected ItemEmpties Empties;
    public ItemEmpties GetEmpties => Empties;

    [SerializeField] protected BaseStatsController _statsCtrl;
    [SerializeField] protected WeaponController _weaponCtrl;
    [SerializeField] protected DodgeController _dodgeCtrl;
    [SerializeField] protected ShieldController _shieldCtrl;
    [SerializeField] protected SkillsController _skillCtrl;
    [SerializeField] protected ComboController _comboCtrl;
    [SerializeField]protected StunsController _stunsCtrl;

    public DodgeController GetDodgeController => _dodgeCtrl;
    public ShieldController GetShieldController => _shieldCtrl;
    public BaseStatsController GetStatsController => _statsCtrl;
    public SkillsController GetSkillsController => _skillCtrl;
    public WeaponController GetWeaponController => _weaponCtrl;

    #region skills

    public void AddSkillString(string name,bool isAdd = true)
    {
        _skillCtrl.UpdateSkills(name, isAdd);
    }

    #endregion

    public event SimpleEventsHandler<CombatActionType> CombatActionSuccessEvent;
    public event SimpleEventsHandler StaggerHappened;
    protected void CombatActionSuccessCallback(CombatActionType type) => CombatActionSuccessEvent?.Invoke(type);
    protected void StunEventCallback() => StaggerHappened?.Invoke();



    protected List<IStatsComponentForHandler> _controllers = new List<IStatsComponentForHandler>();

    public virtual void InitControllers(string statsID)
    {
        // these three need a ping to properly register for updates       
        
        _statsCtrl = new BaseStatsController(statsID);
        _comboCtrl = new ComboController(Unit.GetID);
        _stunsCtrl = new StunsController(); // using default "default" for now 


        _shieldCtrl = new ShieldController(Empties);
        _dodgeCtrl = new DodgeController(Empties); 
        _weaponCtrl = new WeaponController(Empties);
        _skillCtrl = new SkillsController(Empties);


        _controllers.Add(_shieldCtrl);
        _controllers.Add(_dodgeCtrl);
        _controllers.Add(_weaponCtrl);
        _controllers.Add(_skillCtrl);
        _controllers.Add(_comboCtrl);
        _controllers.Add(_stunsCtrl);
        _controllers.Add(_statsCtrl);

        foreach (var ctrl in _controllers)
        {
            ctrl.ComponentChangedStateToEvent += RegisterController;
            if (ctrl.IsReady) ctrl.Ping();
        }
    }

    protected void RegisterController(bool isEnable, IStatsComponentForHandler cont)
    {        
        _handler.RegisterUnitForStatUpdates(cont, isEnable);
    }

    public virtual void BindControllers(bool isEnable)
    {
                //Debug.Log("Bind controllers " + Unit.GetID + " " + isEnable);
        IsControlsBusy = false;
        _weaponCtrl.Owner = Unit;
        _stunsCtrl.StunHappenedEvent += StunEventCallback;    
    }

    private void OnDisable()
    {
        BindControllers(false);
    }

    public void ToggleBusyControls_AnimationEvent(int state)
    {
        IsControlsBusy = state != 0;
    }   


    public void AddTriggeredEffect(TriggeredEffect eff)
    {
        switch (eff.StatType)
        {
            case TriggerChangedValue.Health:
                if (!_shieldCtrl.IsReady)
                {
                    _statsCtrl.AddTriggeredEffect(eff);
                }
                else
                {
                    _statsCtrl.AddTriggeredEffect(_shieldCtrl.ProcessHealthChange(eff));
                }
                break;
            case TriggerChangedValue.Shield:
                _shieldCtrl.AddTriggeredEffect(eff);
                break;
            case TriggerChangedValue.Combo:
                _comboCtrl.AddTriggeredEffect(eff);
                break;
            case TriggerChangedValue.MoveSpeed:
                _statsCtrl.AddTriggeredEffect(eff);
                break;
            case TriggerChangedValue.TurnSpeed:
                _statsCtrl.AddTriggeredEffect(eff);
                break;
            case TriggerChangedValue.Stagger:
                _stunsCtrl.AddTriggeredEffect(eff);
                break;
        }
    }

    #region movement



    public ref Vector3 MoveDirection => ref velocityVector;
    protected Vector3 velocityVector;

    protected void LerpRotateToTarget(Vector3 looktarget)
    {
        Vector3 relativePosition = looktarget - transform.position;
        Quaternion rotation = Quaternion.LookRotation(relativePosition, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation,
            Time.deltaTime * _statsCtrl.GetBaseStats[BaseStatType.TurnSpeed].GetCurrent);
    }

    public void PerformDodging()
    {
        _dodgeCor = StartCoroutine(DodgingMovement());
    }

    private Coroutine _dodgeCor;
    // stop the dodge like this

    private void OnCollisionEnter(Collision collision)
    {
        if (_dodgeCor != null && !collision.gameObject.CompareTag("Ground"))
        {
            IsControlsBusy = false;
            StopCoroutine(_dodgeCor);
        }
    }

    private IEnumerator DodgingMovement()
    {
        var stats = _dodgeCtrl.GetDodgeStats;
        IsControlsBusy = true;

        Vector3 start = transform.position;
        Vector3 end = start + MoveDirection * stats[DodgeStatType.Range].GetCurrent;

        float p = 0f;
        while (p <= 1f)
        {
            p += Time.deltaTime * stats[DodgeStatType.Speed].GetCurrent;
            transform.position = Vector3.Lerp(start, end, p);
            yield return null;
        }
        IsControlsBusy = false;
        yield return null;
    }


    #endregion

}

