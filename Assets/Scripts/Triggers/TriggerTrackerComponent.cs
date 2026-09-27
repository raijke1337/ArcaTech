using System;
using System.Collections.Generic;
using Arcatech.Units;
using DG.Tweening;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Rendering;

namespace Arcatech.Triggers
{
    [RequireComponent(typeof(Collider), typeof(Rigidbody))]
    public class TriggerTrackerComponent : ValidatedMonoBehaviour, ITriggerNotificationProvider
    {
        #region ITriggerNotificationProvider

        private readonly HashSet<ITriggerNotificationReceiver> _receivers = new();
        public bool Active { get; set; } = true;

        public void RegisterReceiver(ITriggerNotificationReceiver r)
        {
            _receivers.Add(r);
        }

        public void UnregisterReceiver(ITriggerNotificationReceiver r) => _receivers.Remove(r);

        #endregion

        [SerializeField, Self] private Collider triggerCollider;
        [SerializeField, Self] private Rigidbody cachedRigidbody;

        // Доступ для наследников (например, AdvancedTriggerTracker) без расширения
        // публичного API самого компонента.
        protected Collider TriggerCollider => triggerCollider;
        protected Rigidbody CachedRigidbody => cachedRigidbody;
        protected int ReceiverCount => _receivers.Count;

        // Дедупликация попаданий в пределах одного кадра: без этого AreaCast,
        // вызванный в момент, когда объект и так физически перекрывает триггер,
        // уведомил бы получателей о том же самом хите дважды (один раз от
        // обычного Unity OnTriggerEnter, второй раз вручную из AreaCast).
        private readonly HashSet<Collider> _notifiedThisFrame = new();
        private int _dedupeFrame = -1;

        private void Start()
        {
            triggerCollider.isTrigger = true;
            cachedRigidbody.isKinematic = true;

            triggerCollider.includeLayers = TriggerLayerUtility.HitMask;

            var r = GetComponentsInChildren<ITriggerNotificationReceiver>();
            foreach (var r2 in r) RegisterReceiver(r2);
            if (_receivers.Count == 0) Active = false;
        }

        protected bool CanNotify() =>
            Active && _receivers.Count > 0;


        public void AreaCast(ITriggerNotificationReceiver receiver)
        {
            if (triggerCollider is not BoxCollider boxCollider)
            {
                Debug.LogError($"{nameof(AreaCast)} requires a {nameof(BoxCollider)}.", this);
                return;
            }

            Transform boxTransform = boxCollider.transform;

            Vector3 worldCenter = boxTransform.TransformPoint(boxCollider.center);

            Vector3 halfExtents = Vector3.Scale(
                boxCollider.size * 0.5f,
                boxTransform.lossyScale);

            // Та же маска слоёв, что и у обычного физического триггера, а не
            // Physics.AllLayers — иначе AreaCast находил бы объекты, на которые
            // обычное срабатывание триггера никогда бы не среагировало, и они
            // приходили бы получателям с HitLayerKind.Unknown.
            Collider[] found = Physics.OverlapBox(
                worldCenter,
                halfExtents,
                boxTransform.rotation,
                TriggerLayerUtility.HitMask,
                QueryTriggerInteraction.Collide);

            foreach (Collider foundCollider in found)
            {
                if (foundCollider == triggerCollider)
                {
                    continue;
                }

                NotifyEnter(foundCollider);
            }
        }

        protected void OnTriggerEnter(Collider other)
        {
            NotifyEnter(other);
        }

        /// <summary>
        /// Общая точка входа для физического Unity-события OnTriggerEnter и для
        /// ручного <see cref="AreaCast"/>. Дедуплицирует по коллайдеру в пределах
        /// одного кадра — см. комментарий у <see cref="_notifiedThisFrame"/>.
        /// </summary>
        private void NotifyEnter(Collider other)
        {
            if (!CanNotify() || other == null || other.isTrigger) return;
            if (!TryClaimHitThisFrame(other)) return;

            var layerKind = ResolveLayerKind(other);
            var hitGeometry = CalculateHitGeometry(other);

            // Создаём копию, чтобы избежать изменений во время итерации
            var receiversCopy = new List<ITriggerNotificationReceiver>(_receivers);

            foreach (var receiver in receiversCopy)
            {
                receiver.TriggerEntered(new TriggerHitInfo(
                    this,
                    other,
                    hitGeometry.position,
                    hitGeometry.direction,
                    hitGeometry.normal,
                    layerKind,
                    Time.time));
            }

            OnHitRegistered(hitGeometry.position, hitGeometry.direction, hitGeometry.normal);
        }

        /// <returns>false, если этот коллайдер уже был уведомлён в текущем кадре.</returns>
        private bool TryClaimHitThisFrame(Collider other)
        {
            if (Time.frameCount != _dedupeFrame)
            {
                _notifiedThisFrame.Clear();
                _dedupeFrame = Time.frameCount;
            }

            return _notifiedThisFrame.Add(other);
        }

        /// <summary>
        /// К какому из настроенных слоёв (ValidHitsLayer/InvalidHitsLayer) относится
        /// объект, по которому попали. Делегирует в <see cref="TriggerLayerUtility"/>,
        /// чтобы классификация была одинаковой для всех источников попаданий в игре
        /// (в частности, для BeamWeaponComponent, который не наследуется от этого класса).
        /// </summary>
        protected HitLayerKind ResolveLayerKind(Collider other) => TriggerLayerUtility.Resolve(other);

        protected void OnTriggerExit(Collider other)
        {
            if (!CanNotify() || other.isTrigger) return;

            var layerKind = ResolveLayerKind(other);

            foreach (var receiver in _receivers)
            {
                receiver.TriggerExited(new TriggerHitInfo(
                    this,
                    other,
                    other.transform.position,
                    Vector3.up,
                    Vector3.up,
                    layerKind,
                    Time.time));
            }
        }

        private void OnDisable()
        {
            //_receivers.Clear();
            _attackWarningTween?.Kill();
            if (_attackWarningObject != null) _attackWarningObject.SetActive(false);
        }

        /// <summary>
        /// Упрощённый расчёт позиции/направления/нормали удара — без учёта скоростей
        /// риджидбоди и прочей физики. Точный расчёт (разрешение направления по
        /// относительной скорости и т.п.) вынесен в <see cref="AdvancedTriggerTracker"/>,
        /// который переопределяет этот метод.
        /// </summary>
        protected virtual (Vector3 position, Vector3 direction, Vector3 normal) CalculateHitGeometry(Collider other)
        {
            if (other == null)
                return (transform.position, Vector3.zero, Vector3.zero);

            var hitPosition = transform.position;
            var direction = transform.forward.sqrMagnitude > 0f ? transform.forward.normalized : Vector3.forward;

            return (hitPosition, direction, -direction);
        }

        /// <summary>
        /// Хук, вызываемый сразу после уведомления получателей о попадании.
        /// По умолчанию ничего не делает; используется, например, для отладочной
        /// визуализации в <see cref="AdvancedTriggerTracker"/>.
        /// </summary>
        protected virtual void OnHitRegistered(Vector3 position, Vector3 direction, Vector3 normal)
        {
        }

        public void OnChangeUsableState(StateMachineNotifyType notification)
        {
            switch (notification)
            {
                case StateMachineNotifyType.NoNotify:
                    break;
                case StateMachineNotifyType.Starting:
                    // show a red quad inside the lower bounds area (danger zone warning)
                    ShowAttackWarning();
                    break;
                case StateMachineNotifyType.Use:
                    // hide the quad
                    HideAttackWarning();
                    break;
                case StateMachineNotifyType.EndUse:
                    HideAttackWarning();
                    break;
                case StateMachineNotifyType.Cancel:
                    HideAttackWarning();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(notification), notification, null);
            }
        }

        #region warning

        private GameObject _attackWarningObject;
        private LineRenderer _attackWarningLine;
        private Material _attackWarningMaterial;
        private Tween _attackWarningTween;
        private float _warningAlpha;

        private static readonly int ColorID = Shader.PropertyToID("_Color");
        private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");

        [SerializeField, Tooltip("Толщина линии контура")]
        private float _warningLineWidth = 0.1f;

        [SerializeField, Tooltip("Смещение контура над полом (избегаем z-fighting)")]
        private float _warningFloorOffset = 0.02f;

        [SerializeField, Tooltip("Длительность резкого fade-in, сек")]
        private float _warningFadeInDuration = 0.12f;

        [SerializeField, Tooltip("Длительность fade-out, сек")]
        private float _warningFadeOutDuration = 0.25f;


        private void EnsureAttackWarningVisual()
        {
            if (_attackWarningObject != null) return;

            _attackWarningObject = new GameObject("AttackWarningVisual");
            _attackWarningObject.transform.SetParent(null);

            _attackWarningLine = _attackWarningObject.AddComponent<LineRenderer>();
            _attackWarningLine.useWorldSpace = true;
            _attackWarningLine.loop = true; // замыкаем контур в прямоугольник
            _attackWarningLine.positionCount = 4;
            _attackWarningLine.widthMultiplier = _warningLineWidth;
            _attackWarningLine.numCornerVertices = 2;
            _attackWarningLine.numCapVertices = 2;
            _attackWarningLine.shadowCastingMode = ShadowCastingMode.Off;
            _attackWarningLine.receiveShadows = false;

            // Sprites/Default нативно поддерживает startColor/endColor через vertex color —
            // удобно для fade без доп. манипуляций с материалом.
            var shader = Shader.Find("Sprites/Default")
                         ?? Shader.Find("Universal Render Pipeline/Unlit")
                         ?? Shader.Find("Unlit/Transparent")
                         ?? Shader.Find("Standard");

            _attackWarningMaterial = new Material(shader) { name = "AttackWarningMaterial_Instance" };
            _attackWarningLine.sharedMaterial = _attackWarningMaterial;

            ApplyWarningColor(new Color(1f, 0f, 0f, 0f));
            _attackWarningObject.SetActive(false);
        }
        private void ApplyWarningColor(Color color)
        {
            if (_attackWarningLine != null)
            {
                _attackWarningLine.startColor = color;
                _attackWarningLine.endColor = color;
            }

            // На случай, если шейдер не использует vertex color (URP Unlit и т.п.)
            if (_attackWarningMaterial != null)
            {
                if (_attackWarningMaterial.HasProperty(BaseColorID))
                    _attackWarningMaterial.SetColor(BaseColorID, color);
                if (_attackWarningMaterial.HasProperty(ColorID))
                    _attackWarningMaterial.SetColor(ColorID, color);
            }
        }

        private void UpdateAttackWarningTransform()
        {
            if (triggerCollider == null || _attackWarningLine == null) return;

            var bounds = triggerCollider.bounds;
            var floorY = bounds.min.y + _warningFloorOffset;
            var center = bounds.center;
            var halfX = bounds.extents.x;
            var halfZ = bounds.extents.z;

            _attackWarningLine.SetPosition(0, new Vector3(center.x - halfX, floorY, center.z - halfZ));
            _attackWarningLine.SetPosition(1, new Vector3(center.x + halfX, floorY, center.z - halfZ));
            _attackWarningLine.SetPosition(2, new Vector3(center.x + halfX, floorY, center.z + halfZ));
            _attackWarningLine.SetPosition(3, new Vector3(center.x - halfX, floorY, center.z + halfZ));
        }
        private void ShowAttackWarning()
        {
            EnsureAttackWarningVisual();
            UpdateAttackWarningTransform();
            _attackWarningObject.SetActive(true);

            _attackWarningTween?.Kill();
            _attackWarningTween = DOTween.To(
                    () => _warningAlpha,
                    SetWarningAlpha,
                    endValue: 1f,
                    duration: _warningFadeInDuration)
                .SetEase(Ease.OutExpo) // резкий, "вспыхивающий" вход
                .SetTarget(this)
                .SetLink(gameObject);
        }

        private void HideAttackWarning()
        {
            if (_attackWarningObject == null || !_attackWarningObject.activeSelf) return;

            _attackWarningTween?.Kill();
            _attackWarningTween = DOTween.To(
                    () => _warningAlpha,
                    SetWarningAlpha,
                    endValue: 0f,
                    duration: _warningFadeOutDuration)
                .SetEase(Ease.InQuad)
                .SetTarget(this)
                .SetLink(gameObject)
                .OnComplete(() => _attackWarningObject.SetActive(false));
        }

        private void SetWarningAlpha(float alpha)
        {
            _warningAlpha = alpha;
            ApplyWarningColor(new Color(1f, 0f, 0f, alpha));
        }
        private void Update()
        {
            if (_attackWarningObject != null && _attackWarningObject.activeSelf)
                UpdateAttackWarningTransform();
        }

        private void OnDestroy()
        {
            _receivers.Clear();
            _attackWarningTween?.Kill();
            if (_attackWarningMaterial != null) Destroy(_attackWarningMaterial);
            if (_attackWarningObject != null) Destroy(_attackWarningObject);
        }

        #endregion
    }
}