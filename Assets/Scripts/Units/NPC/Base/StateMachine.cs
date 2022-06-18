using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
[Serializable]
public class StateMachine : IStatsComponentForHandler
{
    #region handler
    public void UpdateInDelta(float deltaTime)
    {
        if (!aiActive) return;
        CurrentState.UpdateState(this);
        CurrentVelocity = NMAgent.velocity;
        TimeInState += deltaTime;
        if (wasSelectedUnitUpdated) OnUpdatedUnit();
    }
    public void SetupStatsComponent()
    {
        EyesEmpty = new GameObject().transform;
        EyesEmpty.position = NMAgent.transform.position;
        EyesEmpty.rotation = NMAgent.transform.rotation;
        EyesEmpty.parent = NMAgent.transform;
        EyesEmpty.localPosition += EyesEmpty.transform.up;
        EyesEmpty.localPosition += EyesEmpty.transform.forward;
    }
    #endregion

    [SerializeField] private bool aiActive = true;

    public State CurrentState { get; private set; }
    public State RemainState { get; private set; }
    public Vector3 CurrentVelocity { get; protected set; }
    public float TimeInState { get; private set; }
    public EnemyStats GetEnemyStats { get; private set; }
    public BaseUnit StateMachineUnit { get; }


    //public PlayerUnit FoundPlayer { get; private set; }
    //public NPCUnit FoundAlly{ get; private set; }


    // set by inputs , bool for potential checks
    private bool wasSelectedUnitUpdated;
    private BaseUnit _selectedUnit;
    public BaseUnit SelectedUnit
    {
        get { return _selectedUnit; }
        set
        {
            _selectedUnit = value;
            wasSelectedUnitUpdated = false;
        }
    }
    public PlayerUnit PlayerFound { get; private set; }
    private void OnUpdatedUnit() { wasSelectedUnitUpdated = true; }



    public Transform SelectedUnitTransform => _selectedUnit.transform;
    
    public event StateMachineEvent<CombatActionType> AgressiveActionRequestSM;
    public event StateMachineEvent<PlayerUnit> PlayerSpottedSM;
    public event StateMachineEvent CombatPreparationSM;

    public NavMeshAgent NMAgent { get; private set; }
    public Transform [] PatrolPoints;
    public int CurrentPatrolPointIndex = 0;

    public Transform EyesEmpty { get; private set; } // used for sphere casting to look

    #region setups
    public StateMachine(NavMeshAgent agent, EnemyStats stats, State init, State dummy, BaseUnit unit)
    {
        NMAgent = agent;
        GetEnemyStats = stats;
        CurrentState = init;
        RemainState = dummy;
        StateMachineUnit = unit;
    }
    public void SetAI(bool setting)
    {
        aiActive = setting;
        if (NMAgent == null) return;
        NMAgent.isStopped = !setting;
    }

    public void TransitionToState(State nextState)
    {
        if (nextState != RemainState)
        {
           CurrentState = nextState;
           OnExitState();
        }
    }
    private void OnExitState() => TimeInState = 0f;

    #endregion

    public void SetPatrolPoints(List<Transform> points)
    {
        PatrolPoints = new Transform[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            PatrolPoints[i] = points[i];
            //Debug.Log($"Set patrol point index {i}, {points[i]}");
        }
    }
    public void OnPatrolPointReached()
    {
        CurrentPatrolPointIndex++;
        if (CurrentPatrolPointIndex == PatrolPoints.Length) CurrentPatrolPointIndex = 0;
    }
    public bool CanSeePlayerCast()
    {
        Physics.SphereCast(EyesEmpty.position, GetEnemyStats.LookSpereCastRadius,EyesEmpty.forward, out var hit, GetEnemyStats.LookSpereCastRange);
        if (hit.collider == null) return false;
        var _coll = hit.collider;

        var result = _coll.CompareTag("Player");
        if (result)
        {
            PlayerSpottedSM?.Invoke(_coll.GetComponent<PlayerUnit>()) ;
        }
        return result;
    }
    public void OnAttackRequest(CombatActionType type) => AgressiveActionRequestSM?.Invoke(type);

    public void StartCombat (PlayerUnit player,bool isCombat)
    {
        SelectedUnit = player;        PlayerFound = player;
    }
    public void OnCombatInitiate() => CombatPreparationSM?.Invoke();
    public bool CheckIsInStoppingRange()
    {
        var result = Vector3.Distance(NMAgent.transform.position, NMAgent.destination) < NMAgent.stoppingDistance;       
        return result;
    }

}
