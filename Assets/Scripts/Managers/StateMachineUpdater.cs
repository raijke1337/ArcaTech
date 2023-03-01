using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StateMachineUpdater
{
    private List<StateMachine> _states = new List<StateMachine>();
    public void OnUpdate(float delta)
    {
        foreach (var state in _states)
        {
            state.UpdateInDelta(delta);
        }
    }

    public void AddUnit(NPCUnit u)
    {
        _states.Add(u.GetStateMachine);
    }
    public void RemoveUnit(NPCUnit u)
    {
        _states.Remove(u.GetStateMachine);
    }


    public StateMachineUpdater(List<StateMachine> states)
    {
        _states = states;
    }
    public StateMachineUpdater()
    {
        _states = new List<StateMachine>();
    }
}