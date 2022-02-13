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

public class UnitStatsUpdater
{
    private float _deltaTime;
    private LinkedList<NPCUnit> _unitsNPCs;
    private PlayerUnit _playerUnit;

    // initialize in unitmanager
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
    public void RegisterUnitInScene(Unit unit, bool bind = true)
    {
        if (bind)
        {
            unit.GetUnitState.GetCommandsAssistant.OnEffectEventHandler += GetCommandsAssistant_OnEffectEventHandler;
        }
        else
        {
            unit.GetUnitState.GetCommandsAssistant.OnEffectEventHandler -= GetCommandsAssistant_OnEffectEventHandler;
        }
    }    
    //calls effect's start on application to unit
    private void GetCommandsAssistant_OnEffectEventHandler(BaseCommand arg)
    {
        if (!(arg is IStartCommand start)) return;
        start.OnStartCommand();
    }
    // update all stats considering their effects
    private void UpdateAllStatCommands(IReadOnlyCollection<BaseCommand> effects)
    {
        foreach (var e in effects)
        {
            e.CurrentDuration -= _deltaTime;
            if (e.CurrentDuration <= 0f)
            {
                var end = e as IEndCommand;
                if (end == null) continue;
                end.OnEndCommand();
            }
            else
            {
                var upd = e as IUpdateCommand;
                if (upd == null) continue;
                upd.OnUpdateCommand(_deltaTime);
            }
        }
    }
    // regenerate stats
    private void RegenerateStatValues(UnitState state)
    {
        if (state == null) return;

        var cond = state.GetStatValueForCalculations(StatType.Health);
        var regen = state.GetStatValueForCalculations(StatType.HealthRegen);
        state.CurrentHP = Mathf.Min(state.CurrentHP + regen.GetCurrentValue 
            * _deltaTime, cond.GetCurrentValue);
         cond = state.GetStatValueForCalculations(StatType.Shield);
         regen = state.GetStatValueForCalculations(StatType.ShieldRegen);
        state.CurrentHP = Mathf.Min(state.CurrentHP + regen.GetCurrentValue
            * _deltaTime, cond.GetCurrentValue);
         cond = state.GetStatValueForCalculations(StatType.Heat);
         regen = state.GetStatValueForCalculations(StatType.HeatRegen);
        state.CurrentHP = Mathf.Min(state.CurrentHP + regen.GetCurrentValue
            * _deltaTime, cond.GetCurrentValue);
    }
}
