using Arcatech.Items;
using Arcatech.UI;
using Arcatech.Units;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Managers
{
    public class EquipsLevelController : MonoBehaviour
    {
        #region UI buttons
        public void OnDone()
        {
            if (_player.IsArmed)
            {
                GameManager.Instance.OnFinishedEquips();
            }
            else
            {
                Debug.LogError("Can't start level without weapons equipped, TODO");

                //_weaponsMessage.SetActive(true);
            }

        }
        public void OnMain()
        {
            GameManager.Instance.OnReturnToMain();
        }

        #endregion



        private PlayerUnit _player;

        [SerializeField] private EquipmentsMenuContainerScript _menuContainer;

        private void OnValidate()
        {
            _menuContainer = GetComponentInChildren<EquipmentsMenuContainerScript>();
        }

        private void OnEnable()
        {
            Assert.IsNotNull(_menuContainer,"Add menu container component to a child");


            _player = FindObjectOfType<PlayerUnit>();
            if (_player == null)
            {
                Debug.LogError($"Player unit not found!");
            }
            _player.InitiateUnit();
            _menuContainer.InitialInventoryDisplay(_player.GetUnitInventory);

        }


    }
}