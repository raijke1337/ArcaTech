using Arcatech.Managers;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Arcatech.UI
{
    public class MenuPrefabControllerComp : MonoBehaviour
    {
        [SerializeField] private Button _lv;
        [SerializeField] private Button _gallery;

        private void Start()
        {
#if UNITY_EDITOR
            Assert.IsNotNull(_lv);
            Assert.IsNotNull(_gallery);
#endif
           // _lv.gameObject.SetActive(!DataManager.Instance.IsNewGame);
          //  _gallery.gameObject.SetActive(!DataManager.Instance.IsNewGame);
        }

        #region unity UI

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
        #endregion

    }
}