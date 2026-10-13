using System;
using System.Collections.Generic;
using Arcatech.Interactions;
using Arcatech.Units.Control;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace Arcatech.UI
{
    /// <summary>
    /// Решает, для какого ITargetTooltip показывать подсказку, и обновляет её каждый кадр.
    ///
    /// Клавиатура и мышь: свой рейкаст из камеры через курсор. Первый твёрдый коллайдер
    /// решает всё: если это не объект с подсказкой (стена, пол, враг) - подсказки нет.
    /// Чужие триггерные зоны пропускаются, поэтому "сквозь стену" подсказка не появится.
    ///
    /// Геймпад: курсора нет, поэтому показывается подсказка объекта, в зону которого вошла Теилс,
    /// над самим объектом.
    ///
    /// Кнопки в тексте ({Interact} и т.п.) пересобираются при смене устройства на лету.
    /// </summary>
    public class TargetTooltipController : MonoBehaviour
    {
        [SerializeField] private FloatingTooltipComponent view;
        [Tooltip("Иконки кнопок. Пусто - кнопки выводятся текстом: [F], [X], [Square].")]
        [SerializeField] private InputGlyphSet glyphs;

        [Header("Наведение мышью")]
        [Tooltip("Слои, которые участвуют в рейкасте: объекты с подсказками И всё, что может их перекрыть (стены, укрытия, пол).")]
        [SerializeField] private LayerMask raycastMask = Physics.DefaultRaycastLayers;
        [Tooltip("Слои триггеров, которые считаются зоной наведения (если у объекта нет твёрдого коллайдера). " +
                 "Остальные триггеры луч игнорирует.")]
        [SerializeField] private LayerMask hoverTriggerLayers = 0;
        [SerializeField, Min(1f)] private float maxDistance = 200f;
        [Tooltip("Не показывать подсказку, когда курсор над UI (пауза, окна, HUD).")]
        [SerializeField] private bool hideWhenPointerOverUI = true;

        [Header("Геймпад")]
        [SerializeField] private bool showInRangeOnGamepad = true;

        [Header("Отладка")]
        [Tooltip("Писать в консоль, что попало под луч и почему подсказка показана/скрыта.")]
        [SerializeField] private bool debugLog;

        private const int MaxHits = 16;
        private readonly RaycastHit[] _hits = new RaycastHit[MaxHits];
        private readonly List<ITargetTooltip> _inRange = new();
        private static readonly IComparer<RaycastHit> ByDistance =
            Comparer<RaycastHit>.Create((a, b) => a.distance.CompareTo(b.distance));

        private ITargetTooltip _current;
        private InputDeviceKind _shownKind;
        private InputDevice _shownDevice;
        private bool _isShown;
        private Camera _camera;
        private PointerEventData _uiPointer;
        private EventSystem _uiPointerOwner;
        private readonly List<RaycastResult> _uiResults = new();
        private string _lastDebug;

        /// <summary>Глобальный выключатель (галочка showTooltip в GameInterfaceManager).</summary>
        public bool TooltipsEnabled { get; set; } = true;

        /// <summary>Временно скрыть подсказки (итоги уровня, кат-сцены и т.п.).</summary>
        public bool Suppressed { get; set; }

        public void Bind(FloatingTooltipComponent tooltipView)
        {
            if (view == null) view = tooltipView;
            if (view != null && glyphs != null) view.SetSpriteAsset(glyphs.SpriteAsset);
        }

        /// <summary>Теилс вошла/вышла из зоны взаимодействия объекта.</summary>
        public void SetInRange(ITargetTooltip target, bool inRange)
        {
            if (target == null) return;
            _inRange.Remove(target);
            if (inRange) _inRange.Add(target);
        }

        /// <summary>Объект выключен или уничтожен - забываем и сразу прячем подсказку.</summary>
        public void Forget(ITargetTooltip target)
        {
            if (target == null) return;
            _inRange.Remove(target);
            if (ReferenceEquals(_current, target)) Apply(null, default, null, null);
        }

        private void Start()
        {
            if (view != null && glyphs != null) view.SetSpriteAsset(glyphs.SpriteAsset);
        }

        private void LateUpdate()
        {
            PlayerInputGateway gateway = PlayerInputGateway.Instance;
            InputDeviceKind kind = gateway != null ? gateway.ActiveDeviceKind : InputDeviceKind.KeyboardMouse;
            InputDevice device = gateway != null ? gateway.LastUsedDevice : null;

            ITargetTooltip next = null;
            if (TooltipsEnabled && !Suppressed && view != null)
            {
                next = kind == InputDeviceKind.Gamepad
                    ? (showInRangeOnGamepad ? PickInRange() : null)
                    : PickUnderPointer();
            }

            Apply(next, kind, device, gateway);
        }

        private void Apply(ITargetTooltip next, InputDeviceKind kind, InputDevice device, PlayerInputGateway gateway)
        {
            if (next == null)
            {
                if (_isShown && view != null) view.Hide();
                _isShown = false;
                _current = null;
                return;
            }

            bool targetChanged = !ReferenceEquals(next, _current);
            // Клавиатура <-> мышь не считается сменой; геймпад Xbox <-> DualSense - считается.
            bool deviceChanged = kind != _shownKind ||
                                 (kind == InputDeviceKind.Gamepad && device != _shownDevice);
            if (_isShown && !targetChanged && !deviceChanged) return;

            _current = next;
            _shownKind = kind;
            _shownDevice = device;

            string body = InputPromptFormatter.Format(next.TooltipBody,
                gateway != null ? gateway.Actions : null, kind, device, glyphs);
            view.SetContent(next.TooltipTitle, body);

            if (kind == InputDeviceKind.Gamepad) view.FollowWorld(next);
            else view.FollowPointer();

            // При смене только устройства текст меняется без повторной анимации появления.
            if (!_isShown || targetChanged)
            {
                view.Show();
                _isShown = true;
            }
        }

        private ITargetTooltip PickUnderPointer()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null) return null;

            Vector2 pointer = mouse.position.ReadValue();

            if (hideWhenPointerOverUI && IsPointerOverUI(pointer, out GameObject ui))
                return Trace("курсор над UI: " + ui.name);

            if (_camera == null) _camera = Camera.main;
            if (_camera == null) return Trace("нет Camera.main");

            Ray ray = _camera.ScreenPointToRay(pointer);
            int count = Physics.RaycastNonAlloc(ray, _hits, maxDistance, raycastMask | hoverTriggerLayers,
                QueryTriggerInteraction.Collide);
            if (count == 0) return Trace("луч ничего не задел");

            Array.Sort(_hits, 0, count, ByDistance);

            for (int i = 0; i < count; i++)
            {
                Collider col = _hits[i].collider;
                if (col == null) continue;

                bool isTrigger = col.isTrigger;

                // Триггерные зоны (зоны взаимодействия, чекпоинты и т.п.) не перекрывают и не наводятся,
                // кроме специально отмеченных слоёв.
                if (isTrigger && (hoverTriggerLayers.value & (1 << col.gameObject.layer)) == 0)
                    continue;

                ITargetTooltip target = col.GetComponentInParent<ITargetTooltip>();
                if (target != null && IsShowable(target))
                {
                    Trace("цель: " + col.name);
                    return target;
                }

                if (target != null && debugLog)
                    Trace($"{col.name}: подсказка есть, но CanShowTooltip = false (объект недоступен/занят/нет Description)");

                if (isTrigger) continue;

                // Первый твёрдый коллайдер без подсказки - стена/укрытие/пол. Дальше не смотрим.
                return Trace("перекрыто: " + col.name + " (слой " + LayerMask.LayerToName(col.gameObject.layer) + ")");
            }

            return Trace("под лучом нет объектов с подсказкой");
        }

        /// <summary>Курсор над UI. Учитываются только графические рейкастеры (Canvas):
        /// EventSystem.IsPointerOverGameObject() срабатывает и на 3D-объекты, если на камере
        /// есть PhysicsRaycaster, - тогда подсказка пряталась бы всегда.</summary>
        private bool IsPointerOverUI(Vector2 pointer, out GameObject uiObject)
        {
            uiObject = null;
            EventSystem es = EventSystem.current;
            if (es == null) return false;

            if (_uiPointer == null || _uiPointerOwner != es)
            {
                _uiPointer = new PointerEventData(es);
                _uiPointerOwner = es;
            }
            _uiPointer.position = pointer;
            _uiResults.Clear();
            es.RaycastAll(_uiPointer, _uiResults);

            foreach (RaycastResult r in _uiResults)
            {
                if (r.module is UnityEngine.UI.GraphicRaycaster && r.gameObject != null)
                {
                    uiObject = r.gameObject;
                    return true;
                }
            }

            return false;
        }

        private ITargetTooltip Trace(string message)
        {
            if (debugLog && message != _lastDebug)
            {
                _lastDebug = message;
                UnityEngine.Debug.Log("[Tooltip] " + message, this);
            }
            return null;
        }

        private ITargetTooltip PickInRange()
        {
            for (int i = _inRange.Count - 1; i >= 0; i--)
            {
                ITargetTooltip t = _inRange[i];
                if (IsAlive(t))
                {
                    if (t.CanShowTooltip) return t; // последний, в чью зону вошли
                }
                else
                {
                    _inRange.RemoveAt(i);
                }
            }

            return null;
        }

        private static bool IsShowable(ITargetTooltip t) => IsAlive(t) && t.CanShowTooltip;

        private static bool IsAlive(ITargetTooltip t)
        {
            if (t == null) return false;
            if (t is Object o && o == null) return false; // уничтожен
            if (t is Behaviour b && !b.isActiveAndEnabled) return false;
            return true;
        }

        private void OnDisable()
        {
            Apply(null, default, null, null);
        }
    }
}
