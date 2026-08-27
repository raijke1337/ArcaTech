using Arcatech.Managers;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Arcatech.UI
{
    public class MenuPrefabControllerComp : MonoBehaviour
    {

        public void OnNew()
        {
            GameManager.Instance.OnStartNewGameButton();
        }
        public void OnGallery()
        {
            GameManager.Instance.OnGalleryButton();
        }
        public void OnQuitGame()
        {
            GameManager.Instance.OnExitButton();
        }

    }
}