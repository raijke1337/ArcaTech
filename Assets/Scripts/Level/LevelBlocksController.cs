using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Arcatech.Levels
{
    public class LevelBlocksController : MonoBehaviour
    {
        private List<LevelBlockComponent> blocks = new();
        [SerializeField] private bool showDebugs = false;
        private LevelBlockComponent _currentRoom;

        [SerializeField] private Material hiddenMaterial;
        [SerializeField] private Material inactiveMaterial;
        

        // Больше не влияет на видимость (см. комментарий в UpdateAllRooms),
        // но оставлен на будущее (миникарта, ачивки и т.п.)
        private readonly HashSet<LevelBlockComponent> _exploredRooms = new();

        private void Awake()
        {
            // Важно собрать блоки ДО OnEnable(), иначе подписка на события
            // может произойти на пустом/неполном списке.
            blocks = GetComponentsInChildren<LevelBlockComponent>(true).ToList();
        }

        private void OnEnable()
        {
            foreach (var b in blocks) b.RoomHasPlayerEvent += OnRoomHasPlayerEvent;
        }

        private void OnDisable()
        {
            foreach (var b in blocks) b.RoomHasPlayerEvent -= OnRoomHasPlayerEvent;
        }

        private void Start()
        {
            // Приводим все комнаты к корректному стартовому состоянию.
            // Если игрок ещё не задетектирован ни в одной комнате — всё скрыто,
            // а корректные Active/Inactive применятся сразу же после
            // AreaCast/TriggerEntered конкретного блока.
            UpdateAllRooms();
        }

        private void OnRoomHasPlayerEvent(LevelBlockComponent b, bool hasPlayer)
        {
            if (showDebugs) Debug.Log($"Room changed event {b}: {(hasPlayer ? "Player in" : "Player Out")}");

            if (!hasPlayer) return;
            if (_currentRoom == b) return; // Уже в этой комнате

            _currentRoom = b;
            _exploredRooms.Add(b);
            UpdateAllRooms(); // Пересчитываем весь уровень
        }

        private void UpdateAllRooms()
        {
            foreach (var block in blocks)
            {
                if (block == null) continue;

                if (_currentRoom != null && block == _currentRoom)
                {
                    block.SetState(RoomState.Active,null);
                    continue;
                }

                // Правило этажей: всё, что строго выше текущего этажа — всегда скрыто,
                // не важно, соседний блок или посещённый. Решает проблему перекрытия обзора
                // элементами уровня выше игрока.
                if (_currentRoom != null && block.Floor > _currentRoom.Floor)
                {
                    block.SetState(RoomState.Hidden,hiddenMaterial);
                    continue;
                }

                // Тот же этаж или ниже — стандартные правила
                if (_currentRoom != null
                    && _exploredRooms.Contains(block)
                    && AreNeighbors(_currentRoom, block))
                {
                    block.SetState(RoomState.Inactive,inactiveMaterial);
                }
                else
                {
                    block.SetState(RoomState.Hidden,hiddenMaterial);
                }
            }
        }

        // Соседство считаем двунаправленным, даже если в инспекторе
        // оно проставлено только с одной стороны.
        private static bool AreNeighbors(LevelBlockComponent a, LevelBlockComponent b)
        {
            return a.Neighbors.Contains(b) || b.Neighbors.Contains(a);
        }
    }
}