using System.Collections.Generic;
using Arcatech.Managers;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.UI
{
    public class LevelSelectMenuComp : MonoBehaviour
    {
        [SerializeField] RectTransform _content;
        [SerializeField] private LevelButtonComp _buttonPrefab;
        private List<LevelButtonComp> _buttons;

        private void Start()
        {
            CreateButtons();
        }
        public void CreateButtons()
        {
#if UNITY_EDITOR
            Assert.IsNotNull(_content);
            Assert.IsNotNull(_buttonPrefab);
#endif

            var lvs = DataManager.Instance.GetAvailableLevels;


            _buttons = new List<LevelButtonComp>();
            foreach (var lv in lvs)
            {
                if (lv.LevelType == LevelType.Game)
                {
                    var b = Instantiate(_buttonPrefab);
                    b.transform.SetParent(_content, false);
                    b.LevelData = lv;
                    _buttons.Add(b);
                }
            }
        }
    }

}