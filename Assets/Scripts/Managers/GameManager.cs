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
using Zenject;

public class GameManager : MonoInstaller
{

    [SerializeField] private CursorManager _cursors;

    public override void Start()
    {
        base.Start();
        var config = Extensions.GetAssetsFromPath<CursorsDictionary>(Constants.Configs.c_ManagerConfigsPath).First();

        _cursors = new CursorManager(config);
    }
    public override void InstallBindings()
    {
        Container.BindInstance(FindObjectOfType<PlayerUnit>()).AsSingle();
        Container.BindInstance(FindObjectOfType<StatsUpdatesHandler>()).AsSingle();
        Container.BindInstance(GetComponent<TriggersProjectilesManager>()).AsSingle();
        Container.BindInstance(GetComponent<UnitsManager>()).AsSingle();
    }




}

