using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Arcatech.Cameras;
using Arcatech.Triggers;
using com.cyborgAssets.inspectorButtonPro;
using UnityEngine.Events;

namespace Arcatech.Levels
{
    public class LevelBlockComponent : MonoBehaviour, ITriggerNotificationReceiver
    {
        [Header("References")]
        private List<Renderer> _renderers = new(); 
        private List<Light> _lights = new();

        [SerializeField] private Transform boundsParent;
        [SerializeField, ReadOnlyText] private string boundsCount = "ERROR!";
        [SerializeField, ReadOnlyText] private string renderersCount = "0"; 
        [SerializeField, ReadOnlyText] private string lightsCount = "0"; 
        [Header("Settings")]
        [SerializeField] private int floor = 0; // 0 = базовый этаж, +1 выше, -1 ниже
        [SerializeField] private CamerasController.CameraViewType roomView;

        
        public int Floor => floor;
        private Material[] _originalMaterials;
        private RoomState _currentState = RoomState.Hidden;
        private bool _initialized = false; // теперь реально используется

        [SerializeField] private List<LevelBlockComponent> neighbors = new();
        public IReadOnlyList<LevelBlockComponent> Neighbors => neighbors;

        public RoomState CurrentState => _currentState;

        [SerializeField] private List<TriggerTrackerComponent> roomTriggers = new(); // инициализировано!
        private readonly HashSet<object> _entitiesInside = new();

        public UnityAction<LevelBlockComponent, bool> RoomHasPlayerEvent = delegate { };

        private void OnValidate()
        {
            AutoCollect();
        }

        private void OnEnable()
        {
            _originalMaterials = new Material[_renderers.Count];
            for (int i = 0; i < _renderers.Count; i++)
            {
                if (_renderers[i] != null)
                    _originalMaterials[i] = _renderers[i].sharedMaterial;
            }

            roomTriggers = boundsParent.GetComponentsInChildren<TriggerTrackerComponent>().ToList();
            boundsCount = roomTriggers.Count.ToString();
            foreach (var t in roomTriggers)
            {
                t.RegisterReceiver(this);
                t.Active = true;
            }
        }

        private void OnDisable()
        {
            if (roomTriggers == null || roomTriggers.Count == 0) return;
            foreach (var t in roomTriggers) t.UnregisterReceiver(this);
        }

        private void Start()
        {
            // to prevent race condition
            if (roomTriggers == null || roomTriggers.Count == 0) return;
            foreach (var t in roomTriggers) t.AreaCast(this);
        }

        public void SetState(RoomState newState,Material material)
        {
            // ключевой фикс: первый вызов всегда должен применяться,
            // даже если newState совпадает с дефолтным _currentState
            if (_initialized && _currentState == newState) return;

            _initialized = true;
            _currentState = newState;

            switch (newState)
            {
                case RoomState.Hidden:
                    // Полностью выключаем рендеринг и свет
                    foreach (var r in _renderers)
                    {
                        if (r != null)
                        {
                            r.enabled = true;
                            r.sharedMaterial = material;
                        }
                    }
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
                            r.sharedMaterial = material;
                        }
                    }

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

#if UNITY_EDITOR
         void AutoCollect()
        {
            _renderers.Clear();
            _lights.Clear();
            _renderers.AddRange(GetComponentsInChildren<Renderer>(true));
            _lights.AddRange(GetComponentsInChildren<Light>(true));
            Debug.Log($"{name} Collected {_renderers.Count} renderers and {_lights.Count} lights");
            renderersCount = _renderers.Count.ToString();
            lightsCount = _lights.Count.ToString();
            if (!boundsParent) return;
            boundsCount = "OK";
        }
#endif
        public void TriggerEntered(TriggerHitInfo triggerHitInfo)
        {
            if (!triggerHitInfo.TryGetEntityTarget(out var e) || !e.CompareTag("Player")) return;

            // Add() возвращает false, если сущность уже была в множестве —
            // дублирующий Enter (например, от AreaCast + реальной физики
            // на старте, или от нескольких триггеров одной комнаты) просто игнорируется.
            if (_entitiesInside.Add(e) && _entitiesInside.Count == 1)
            {
                RoomHasPlayerEvent.Invoke(this, true);
                CamerasController.Instance.SetView(roomView);
            }
        }

        public void TriggerExited(TriggerHitInfo triggerExitInfo)
        {
            if (!triggerExitInfo.TryGetEntityTarget(out var e) || !e.CompareTag("Player")) return;

            // Remove() возвращает false, если сущности не было в множестве —
            // лишний/повторный Exit тоже безопасно игнорируется.
            if (_entitiesInside.Remove(e) && _entitiesInside.Count == 0)
            {
                RoomHasPlayerEvent.Invoke(this, false);
            }
        }
    }
}