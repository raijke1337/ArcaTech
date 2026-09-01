using UnityEngine;
using System.Collections.Generic;
using com.cyborgAssets.inspectorButtonPro;

namespace Arcatech.Levels
{
    public enum RoomState
    {
        Hidden,
        Inactive,
        Active,
        Explored
    }

    public class LevelBlockComponent : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private List<Renderer> _renderers = new();

        [SerializeField] private List<Light> _lights = new();
        [SerializeField] private Material _inactiveMaterial; // Назначить Arcatech/Room/InactiveScan

        [Header("Settings")] [SerializeField] private bool _isSecret = false;

        private Material[] _originalMaterials;
        private RoomState _currentState = RoomState.Hidden;
        private bool _initialized = false;

        public RoomState CurrentState => _currentState;
        public bool IsSecret => _isSecret;

        private void Awake()
        {
            // Кэшируем оригинальные материалы при старте
            _originalMaterials = new Material[_renderers.Count];
            for (int i = 0; i < _renderers.Count; i++)
            {
                if (_renderers[i] != null)
                    _originalMaterials[i] = _renderers[i].sharedMaterial;
            }

            _initialized = true;

            // Начинаем в скрытом состоянии
            SetState(RoomState.Hidden);
        }

        [ProButton]
        public void SetState(RoomState newState)
        {
            if (!_initialized) return;
            if (_currentState == newState) return;

            _currentState = newState;

            switch (newState)
            {
                case RoomState.Hidden:
                    // Полностью выключаем рендеринг и свет
                    foreach (var r in _renderers)
                        if (r != null)
                            r.enabled = false;
                    foreach (var l in _lights)
                        if (l != null)
                            l.enabled = false;
                    break;

                case RoomState.Inactive:
                    // Показываем голограмму/скан
                    foreach (var r in _renderers)
                    {
                        if (r != null)
                        {
                            r.enabled = true;
                            r.sharedMaterial = _inactiveMaterial;
                        }
                    }

                    // Свет выключен, но можно оставить слабый point light для атмосферы
                    foreach (var l in _lights)
                        if (l != null)
                            l.enabled = false;
                    break;

                case RoomState.Active:
                case RoomState.Explored:
                    // Возвращаем оригинальные материалы
                    for (int i = 0; i < _renderers.Count; i++)
                    {
                        if (_renderers[i] != null && _originalMaterials[i] != null)
                        {
                            _renderers[i].enabled = true;
                            _renderers[i].sharedMaterial = _originalMaterials[i];
                        }
                    }

                    foreach (var l in _lights)
                        if (l != null)
                            l.enabled = true;
                    break;
            }
        }

        // Вызывается из RoomVisibilityManager при смене состояния
        public void RevealSecret()
        {
            _isSecret = false;
            // После раскрытия секрета комната ведет себя как обычная
        }

#if UNITY_EDITOR
        [ProButton]
        public void AutoCollect()
        {
            _renderers.Clear();
            _lights.Clear();
            _renderers.AddRange(GetComponentsInChildren<Renderer>(true));
            _lights.AddRange(GetComponentsInChildren<Light>(true));
            Debug.Log($"Collected {_renderers.Count} renderers and {_lights.Count} lights");
        }
#endif
    }
}