using System.Collections.Generic;
using System.Linq;
using Arcatech.Cameras;
using Arcatech.SaveSystem;
using Arcatech.Triggers;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Levels
{
    public class LevelBlockComponent : MonoBehaviour, ITriggerNotificationReceiver, ISavedProgressItem
    {
        [Header("References")]
        [SerializeField] private Transform boundsParent;
        [SerializeField] private Material fogMaterial;
        [SerializeField, Min(0.01f)] private float fogFadeDuration = 0.2f;
        [SerializeField] private CamerasController.CameraViewType roomView;

        [Header("Room")]
        [SerializeField] private List<TriggerTrackerComponent> roomTriggers = new();
        [SerializeField] private List<LevelBlockComponent> neighbors = new();

        public IReadOnlyList<LevelBlockComponent> Neighbors => neighbors;
        public UnityAction<LevelBlockComponent, bool> RoomHasPlayerEvent = delegate { };

        private readonly HashSet<object> _entitiesInside = new();
        private readonly List<GameObject> _fogVolumes = new();
        private bool _explored;
        private ProgressItemState _savedState;
        private bool _fadeOutStarted;
        private float _fadeOutTime;
        private static readonly int FadeAmountId = Shader.PropertyToID("_FadeAmount");
        private MaterialPropertyBlock _fogProperties;

        private static Mesh _inwardCubeMesh;

        private void OnEnable()
        {
            _fogProperties ??= new MaterialPropertyBlock();
            LevelProgressController.Instance.Register(this);

            if (boundsParent == null)
            {
                Debug.LogWarning($"{nameof(LevelBlockComponent)} on {name} has no bounds parent.", this);
                return;
            }

            if (fogMaterial == null)
                fogMaterial = Resources.Load<Material>("Material/ARCA_FogOfWar");

            roomTriggers = boundsParent.GetComponentsInChildren<TriggerTrackerComponent>(true).ToList();
            foreach (var trigger in roomTriggers)
            {
                trigger.RegisterReceiver(this);
                trigger.Active = true;

                if (trigger.TryGetComponent(out BoxCollider boxCollider))
                    CreateFogVolume(boxCollider);
                else
                    Debug.LogWarning($"Fog of war supports BoxCollider bounds only. Unsupported bounds: {trigger.name}.", trigger);
            }
        }

        private void Start()
        {
            foreach (var trigger in roomTriggers)
                trigger.AreaCast(this);
        }

        private void Update()
        {
            if (!_fadeOutStarted) return;

            _fadeOutTime += Time.deltaTime;
            var fade = Mathf.Clamp01(_fadeOutTime / fogFadeDuration);
            foreach (var volume in _fogVolumes)
            {
                if (volume == null || !volume.activeSelf) continue;
                var renderer = volume.GetComponent<MeshRenderer>();
                if (renderer == null) continue;
                renderer.GetPropertyBlock(_fogProperties);
                _fogProperties.SetFloat(FadeAmountId, fade);
                renderer.SetPropertyBlock(_fogProperties);
            }

            if (fade >= 1f)
            {
                _fadeOutStarted = false;
                foreach (var volume in _fogVolumes)
                    if (volume != null)
                        volume.SetActive(false);
            }
        }

        private void OnDisable()
        {
            LevelProgressController.TryGetInstance()?.Unregister(this);

            foreach (var trigger in roomTriggers)
                if (trigger != null)
                    trigger.UnregisterReceiver(this);

            foreach (var volume in _fogVolumes)
                if (volume != null)
                    Destroy(volume);
            _fogVolumes.Clear();
        }

        private void CreateFogVolume(BoxCollider bounds)
        {
            if (fogMaterial == null)
            {
                Debug.LogWarning("Fog material was not assigned and Resources/Material/ARCA_FogOfWar was not found.", this);
                return;
            }

            var volume = new GameObject($"Fog of War ({bounds.name})");
            volume.transform.SetParent(bounds.transform, false);
            volume.transform.localPosition = bounds.center;
            volume.transform.localRotation = Quaternion.identity;
            volume.transform.localScale = bounds.size;

            var filter = volume.AddComponent<MeshFilter>();
            filter.sharedMesh = InwardCubeMesh;

            var renderer = volume.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = fogMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            _fogVolumes.Add(volume);
            // renderer.GetPropertyBlock(_fogProperties); 
            // _fogProperties.SetFloat(FadeAmountId, 0f);
            // renderer.SetPropertyBlock(_fogProperties);
            if (_explored)
                volume.SetActive(false);
        }

        private static Mesh InwardCubeMesh
        {
            get
            {
                if (_inwardCubeMesh != null)
                    return _inwardCubeMesh;

                var temporary = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _inwardCubeMesh = Instantiate(temporary.GetComponent<MeshFilter>().sharedMesh);
                Destroy(temporary);

                // var triangles = _inwardCubeMesh.triangles;
                // for (var i = 0; i < triangles.Length; i += 3)
                //     (triangles[i + 1], triangles[i + 2]) = (triangles[i + 2], triangles[i + 1]);
                // _inwardCubeMesh.triangles = triangles;
                //
                // var normals = _inwardCubeMesh.normals;
                // for (var i = 0; i < normals.Length; i++)
                //     normals[i] = -normals[i];
                // _inwardCubeMesh.normals = normals;
                // _inwardCubeMesh.name = "Fog of War Inward Cube";
                 return _inwardCubeMesh;
            }
        }

        public void TriggerEntered(TriggerHitInfo triggerHitInfo)
        {
            if (!triggerHitInfo.TryGetEntityTarget(out var entity) || !entity.CompareTag("Player"))
                return;

            if (_entitiesInside.Add(entity) && _entitiesInside.Count == 1)
            {
                if (!_explored)
                {
                    _explored = true;
                    _fadeOutStarted = _fogVolumes.Count > 0;
                    _fadeOutTime = 0f;
                    if (!_fadeOutStarted) ApplyExploredState();
                    _savedState = ProgressItemState.Completed;
                    LevelProgressController.Instance.SavedItemAnnounce(this);
                }

                RoomHasPlayerEvent.Invoke(this, true);
                CamerasController.Instance.SetView(roomView);
            }
        }

        public void TriggerExited(TriggerHitInfo triggerExitInfo)
        {
            if (!triggerExitInfo.TryGetEntityTarget(out var entity) || !entity.CompareTag("Player"))
                return;

            if (_entitiesInside.Remove(entity) && _entitiesInside.Count == 0)
                RoomHasPlayerEvent.Invoke(this, false);
        }
        [SerializeField, HideInInspector] private string savedItemID;
        public string SavedItemID => string.IsNullOrWhiteSpace(savedItemID) ? name : savedItemID;
        public string Name => name;
        public ProgressItemKind Kind => ProgressItemKind.Room;
        public ProgressItemState ReadItemState => _savedState;
        public void ApplySaveState(ProgressItemState state, ILevelProgressContext ctx)
        {
            _savedState = state;
            if (state == ProgressItemState.Completed)
                ApplyExploredState();
            else
                ApplyUnexploredState();
        }

        private void ApplyExploredState()
        {
            _explored = true;
            _fadeOutStarted = false;
            foreach (var volume in _fogVolumes)
                if (volume != null)
                    volume.SetActive(false);
        }

        private void ApplyUnexploredState()
        {
            _explored = false;
            foreach (var volume in _fogVolumes)
                if (volume != null)
                    volume.SetActive(true);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(savedItemID))
                savedItemID = System.Guid.NewGuid().ToString("N");
        }
#endif
    }
}
