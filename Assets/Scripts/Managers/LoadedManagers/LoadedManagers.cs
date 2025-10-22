
using System.Collections.Generic;
using Arcatech.Effects;
using Arcatech.EventBus;
using Arcatech.Scenes.Cameras;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Managers
{
    public class LoadedManagers : MonoBehaviour
    {
        void OnValidate()
        {
            Assert.IsNotNull(_camPrefab);
            Assert.IsNotNull(_gameUIprefab);
        }

        [SerializeField] IsoCameraController _camPrefab;
        [SerializeField] GameInterfaceManager _gameUIprefab;

        //List<IManagedController> _ctrls;

        //TriggersManager _triggers;
        //LevelManager _levelBlocks;
        //UnitsManager _units;
        //GameInterfaceManager _ui;
        //IsoCameraController _camera;


        private void Start()
        {
            /*_ctrls = new();
            EventBus<SoundClipRequest>.Raise(new SoundClipRequest(GameManager.Instance.GetCurrentLevelData.Music, false, transform.position));

            switch (GameManager.Instance.GetCurrentLevelData.LevelType)
            {
                case LevelType.Menu:
                    break;
                case LevelType.Scene:
                    break;
                case LevelType.Game:
                    //_ctrls.Add(GetComponent<LevelManager>());
                    //_ctrls.Add(GetComponent<PauseManager>());
                    //_ctrls.Add(GetComponent<TriggersManager>());                    
                    //_ctrls.Add(Instantiate(_gameUIprefab));
                    var cam = FindFirstObjectByType<IsoCameraController>();
                    if (cam == null)
                    {
                        _ctrls.Add(Instantiate(_camPrefab));
                    }
                    else
                    {
                        _ctrls.Add(cam);
                    }

                    foreach (var c in _ctrls)
                    {
                        c.StartController();
                    }
                    break;
            }            */
        }

        
    }
}