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
using Zenject;
using System.Threading.Tasks;

public class GameInterfaceManager : MonoBehaviour
{
    [SerializeField] private TargetUnitPanel _tgt;
    [SerializeField] private PlayerUnitPanel _player;
    [SerializeField] private MenuPanel _pause;

    private UnitsManager _unitsM;

    private async void Start()
    {
        await Task.Yield();

        _unitsM = FindObjectOfType<UnitsManager>();
        if (_player == null) return;
        _player.AssignItem(_unitsM.GetPlayerUnit,true);

        _unitsM.GetPlayerUnit.SetInfoPanel(_tgt);
        _unitsM.GetPlayerUnit.ToggleMenuEvent += GameInterfaceManager_ToggleMenuEvent;


        _pause.OnToggle(false);
    }

    private void GameInterfaceManager_ToggleMenuEvent(GameMenuType arg)
    {
        MenuPanel panel;
        switch (arg)
        {
            case GameMenuType.Pause:
                _pause.OnToggle(true);
                panel = _pause;
                _unitsM.MenuOpened(panel.gameObject.activeSelf);
                break;
        }        
    }



}


