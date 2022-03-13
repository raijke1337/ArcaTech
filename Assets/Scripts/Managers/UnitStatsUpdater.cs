using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using static BaseCommandEffect;

public class UnitStatsUpdater
{
    private float _deltaTime;
    private LinkedList<NPCUnit> _unitsNPCs;


    private PlayerUnit _playerUnit;

    // initialized and stored in unitmanager
    public UnitStatsUpdater(LinkedList<NPCUnit> npcs , PlayerUnit player)
    {
        _playerUnit = player; _unitsNPCs = npcs;
    }


    // called every fixedupdate by npc manager and does the stat changes
    public void CalculateStatsOnUpdate(float deltatime)
    {
        _deltaTime = deltatime;

        UpdateAllStatCommands(_playerUnit.GetUnitState.GetCommandsAssistant.GetAllCurrentlyActiveCommands);


        RegenerateStatValues(_playerUnit.GetUnitState);
        foreach (var unit in _unitsNPCs)
        {
            UpdateAllStatCommands(unit.GetUnitState.GetCommandsAssistant.GetAllCurrentlyActiveCommands);
            RegenerateStatValues(unit.GetUnitState);
        }
    }

    // register for stat updating
    // todo
    public void RegisterUnitInScene(Unit unit, bool bind = true)
    {
        if (bind)
        {

        }
        else
        {

        }
    }    

    // update all stats considering their effects
    private void UpdateAllStatCommands(IReadOnlyCollection<BaseCommandEffect> effects)
    {
        foreach (var e in effects)
        {
            e.CurrentDuration -= _deltaTime;
            if (e.CurrentDuration <= 0f)
            {
                e.OnEnd();
            }
            else
            {
                e.OnUpdate(_deltaTime);
            }
        }
    }
    // regenerate stats
    private void RegenerateStatValues(UnitState state)
    {
        // todo use range here maybe?
        if (state == null) return;
        var cond = state.GetStatContainer(StatType.Health);
        var up = state.GetStatContainer(StatType.HealthRegen);
        state.CurrentHP = Mathf.Min(state.CurrentHP + up.GetCurrentValue * Time.deltaTime,cond.GetCurrentValue);

        cond = state.GetStatContainer(StatType.Shield);
        up = state.GetStatContainer(StatType.ShieldRegen);
        state.CurrentShield = Mathf.Min(state.CurrentShield + up.GetCurrentValue * Time.deltaTime, cond.GetCurrentValue);

        cond = state.GetStatContainer(StatType.Heat);
        up = state.GetStatContainer(StatType.HeatRegen);
        state.CurrentHeat = Mathf.Min(state.CurrentHeat + up.GetCurrentValue * Time.deltaTime, cond.GetCurrentValue);
    }


}

