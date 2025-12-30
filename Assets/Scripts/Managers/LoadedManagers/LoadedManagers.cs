
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
        [SerializeField] CamerasController _camPrefab;
        [SerializeField] GameInterfaceManager _gameUIprefab;
    }
}