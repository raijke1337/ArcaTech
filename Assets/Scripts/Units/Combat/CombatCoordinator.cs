using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Arcatech.Units.Combat
{
    /// <summary>
    /// Оркестратор боя вокруг одной цели (Теилс). Вешается на объект цели.
    ///
    /// 1. Токены атаки. Враг атакует, только когда держит токен. У категорий свои лимиты,
    ///    между выдачами пауза. Из заявок выигрывает враг с наибольшим счётом
    ///    (агрессия + близость). Токен возвращается нодой Release, при оглушении/смерти врага
    ///    и по таймауту-страховке.
    /// 2. Нокдаун цели. Атакующие токены отзываются и не выдаются. Через GrabDecisionDelay
    ///    токен захвата получает сильнейший по тиру из заявивших. После нокдауна фора RecoveryGrace.
    /// 3. Слоты. Два кольца вокруг цели: внутреннее для держателей токенов ближнего боя,
    ///    внешнее для ожидающих. Слоты привязаны к мировым углам, враг занимает ближайший
    ///    к своей стороне свободный слот, с гистерезисом.
    ///
    /// Враги регистрируются сами, когда их граф вызывает ноды Request Attack Token / Move To Engagement Slot.
    /// </summary>
    [DisallowMultipleComponent]
    public class CombatCoordinator : MonoBehaviour
    {
        [Serializable]
        public class Settings
        {
            [Header("Токены атаки")]
            [Min(0)] public int MeleeTokens = 2;
            [Min(0)] public int HeavyTokens = 1;
            [Min(0)] public int RangedTokens = 2;
            [Min(0)] public int GrabTokens = 1;

            [Tooltip("Пауза между выдачами атакующих токенов, с. Берётся случайно из диапазона.")]
            public Vector2 GrantInterval = new(0.3f, 0.6f);

            [Tooltip("Минимальная агрессия врага, чтобы получить атакующий токен (1 = полная шкала).")]
            public float AggressionThreshold = 1f;

            [Tooltip("Вес близости к цели в счёте претендента. Счёт = агрессия + вес × близость (0..1).")]
            public float ProximityWeight = 0.5f;

            [Tooltip("Страховка: токен возвращается сам через столько секунд. " +
                     "До начала атаки отсчёт идёт от выдачи, после — от начала атаки (Request вернул Success).")]
            public float MaxTokenHold = 5f;

            [Header("Нокдаун и захват")]
            [Tooltip("Сколько секунд после начала нокдауна собирать заявки на захват, чтобы выбрать сильнейшего.")]
            public float GrabDecisionDelay = 0.3f;

            [Tooltip("Фора после окончания нокдауна: атакующие токены не выдаются, с.")]
            public float RecoveryGrace = 1.5f;

            [Header("Слоты")]
            [Tooltip("Радиус кольца атакующих. Для врага берётся не больше 0.8 × его MeleeRange.")]
            public float InnerRadius = 1.6f;

            [Tooltip("Радиус кольца ожидающих.")]
            public float OuterRadius = 3.8f;

            [Tooltip("Желаемое расстояние между соседними слотами по дуге, м.")]
            public float SlotSpacing = 1.4f;

            [Tooltip("На сколько градусов другой слот должен быть ближе к стороне врага, чтобы тот сменил текущий.")]
            public float SlotHysteresisDeg = 30f;

            [Tooltip("Радиус поиска навмеша вокруг точки слота.")]
            public float NavSampleRadius = 1f;

            [Tooltip("Как часто пересчитывать слоты, с.")]
            public float SlotsUpdateInterval = 0.15f;
        }

        [SerializeField] private Settings settings = new();
        [SerializeField, Tooltip("Цель (Теилс). Если пусто, берётся BaseGameEntityComponent с этого объекта или родителя.")]
        private BaseGameEntityComponent owner;
        [SerializeField] private bool drawGizmos = true;

        public Settings Config => settings;
        public BaseGameEntityComponent Owner => owner;
        public Transform Center => owner != null ? owner.transform : transform;

        /// <summary>Цель сейчас в нокдауне.</summary>
        public bool TargetKnockedDown => _knockdown;

        /// <summary>Идёт фора после нокдауна.</summary>
        public bool InRecovery => Time.time < _graceUntil;

        private sealed class Grant
        {
            public AttackTokenCategory Category;
            public int Cost;
            public float Time;
            public bool Committed;
        }

        private struct Request
        {
            public CombatParticipant Participant;
            public AttackTokenCategory Category;
            public int Cost;
        }

        private readonly List<CombatParticipant> _participants = new();
        private readonly Dictionary<CombatParticipant, Grant> _grants = new();
        private readonly List<Request> _requests = new();

        // буферы, чтобы не аллоцировать каждый кадр
        private readonly List<CombatParticipant> _buffer = new();
        private readonly List<CombatParticipant> _inner = new();
        private readonly List<CombatParticipant> _outer = new();
        private readonly Dictionary<int, Vector3?> _centerOnMesh = new();
        private bool[] _taken = Array.Empty<bool>();
        private bool[] _tried = Array.Empty<bool>();
        private Vector3 _sortCenter;
        private Comparison<CombatParticipant> _byDistance;

        private float _nextGrantTime;
        private float _nextSlotsTime;
        private bool _knockdown;
        private float _knockdownStart;
        private float _graceUntil = -1f;

        /// <summary>Координатор цели. Ищется на самой цели и её родителях.</summary>
        public static CombatCoordinator FindFor(Transform target) =>
            target != null ? target.GetComponentInParent<CombatCoordinator>() : null;

        private void Awake()
        {
            if (owner == null) owner = GetComponentInParent<BaseGameEntityComponent>();
            _byDistance = CompareByDistance;
        }

        private void OnDisable()
        {
            for (int i = _participants.Count - 1; i >= 0; i--)
                if (_participants[i] != null) _participants[i].ForgetCoordinator(this);
            _participants.Clear();
            _grants.Clear();
            _requests.Clear();
        }

        #region участники

        internal void Register(CombatParticipant p)
        {
            if (p != null && !_participants.Contains(p)) _participants.Add(p);
        }

        internal void Unregister(CombatParticipant p, bool died)
        {
            if (!_participants.Remove(p)) return;
            CancelRequest(p);
            _grants.Remove(p);
            if (!died) return;
            foreach (var other in _participants)
                if (other != null) other.OnAllyDied();
        }

        public IReadOnlyList<CombatParticipant> Participants => _participants;

        #endregion

        #region токены

        public int Capacity(AttackTokenCategory category) => category switch
        {
            AttackTokenCategory.Melee => settings.MeleeTokens,
            AttackTokenCategory.Heavy => settings.HeavyTokens,
            AttackTokenCategory.Ranged => settings.RangedTokens,
            AttackTokenCategory.Grab => settings.GrabTokens,
            _ => 0,
        };

        public int Used(AttackTokenCategory category)
        {
            int used = 0;
            foreach (var g in _grants.Values)
                if (g.Category == category) used += g.Cost;
            return used;
        }

        public bool HasToken(CombatParticipant p, AttackTokenCategory category) =>
            p != null && _grants.TryGetValue(p, out var g) && g.Category == category;

        public bool HasAnyToken(CombatParticipant p) => p != null && _grants.ContainsKey(p);

        /// <summary>
        /// Враг подошёл и начинает атаку (Request Attack Token вернул Success).
        /// С этого момента он держит позицию, а таймер страховки отсчитывается заново.
        /// </summary>
        public void CommitToken(CombatParticipant p)
        {
            if (p == null || !_grants.TryGetValue(p, out var g)) return;
            g.Committed = true;
            g.Time = Time.time;
        }

        /// <summary>Враг держит токен и уже атакует.</summary>
        public bool IsCommitted(CombatParticipant p) =>
            p != null && _grants.TryGetValue(p, out var g) && g.Committed;

        public bool TryGetToken(CombatParticipant p, out AttackTokenCategory category)
        {
            if (p != null && _grants.TryGetValue(p, out var g))
            {
                category = g.Category;
                return true;
            }
            category = default;
            return false;
        }

        /// <summary>
        /// Поставить заявку. Заявка одна на участника: новая заменяет старую.
        /// Если участник держит токен другой категории, тот возвращается.
        /// Выдача происходит в Update координатора, не мгновенно.
        /// </summary>
        public void RequestToken(CombatParticipant p, AttackTokenCategory category, int cost)
        {
            if (p == null) return;
            Register(p);
            if (TryGetToken(p, out var held))
            {
                if (held == category) return;
                ReleaseToken(p, false);
            }

            var request = new Request { Participant = p, Category = category, Cost = Mathf.Max(1, cost) };
            for (int i = 0; i < _requests.Count; i++)
            {
                if (_requests[i].Participant != p) continue;
                _requests[i] = request;
                return;
            }
            _requests.Add(request);
        }

        public bool IsRequestPending(CombatParticipant p)
        {
            for (int i = 0; i < _requests.Count; i++)
                if (_requests[i].Participant == p) return true;
            return false;
        }

        public void CancelRequest(CombatParticipant p)
        {
            for (int i = _requests.Count - 1; i >= 0; i--)
                if (_requests[i].Participant == p) _requests.RemoveAt(i);
        }

        /// <param name="resetAggression">true: враг «потратил ход», агрессия падает до AggressionAfterAttack.</param>
        public void ReleaseToken(CombatParticipant p, bool resetAggression)
        {
            if (p == null || !_grants.Remove(p)) return;
            if (resetAggression) p.OnAttackFinished();
        }

        #endregion

        private void Update()
        {
            float now = Time.time;
            RemoveDestroyed();
            UpdateKnockdown(now);
            ExpireGrants(now);
            ProcessRequests(now);

            if (now >= _nextSlotsTime)
            {
                _nextSlotsTime = now + settings.SlotsUpdateInterval;
                UpdateSlots();
            }
        }

        private void RemoveDestroyed()
        {
            for (int i = _participants.Count - 1; i >= 0; i--)
            {
                var p = _participants[i];
                if (p != null) continue;
                _participants.RemoveAt(i);
            }

            _buffer.Clear();
            foreach (var key in _grants.Keys)
                if (key == null) _buffer.Add(key);
            foreach (var key in _buffer) _grants.Remove(key);

            for (int i = _requests.Count - 1; i >= 0; i--)
                if (_requests[i].Participant == null) _requests.RemoveAt(i);
        }

        private void UpdateKnockdown(float now)
        {
            bool down = owner != null && owner.EntityAlive && owner.Stunned;
            if (down == _knockdown) return;

            _knockdown = down;
            if (down)
            {
                _knockdownStart = now;
                RevokeAttackTokens();
            }
            else
            {
                _graceUntil = now + settings.RecoveryGrace;
            }
        }

        private void RevokeAttackTokens()
        {
            _buffer.Clear();
            foreach (var kv in _grants)
                if (kv.Value.Category != AttackTokenCategory.Grab) _buffer.Add(kv.Key);
            // агрессию не сбрасываем: эти враги не успели атаковать и пойдут первыми после форы
            foreach (var p in _buffer) _grants.Remove(p);
        }

        private void ExpireGrants(float now)
        {
            _buffer.Clear();
            foreach (var kv in _grants)
            {
                var p = kv.Key;
                if (!p.CanReceiveToken || now - kv.Value.Time > settings.MaxTokenHold) _buffer.Add(p);
            }
            foreach (var p in _buffer) ReleaseToken(p, true);
        }

        private void ProcessRequests(float now)
        {
            for (int i = _requests.Count - 1; i >= 0; i--)
                if (!_requests[i].Participant.CanReceiveToken) _requests.RemoveAt(i);

            if (_requests.Count == 0) return;
            if (owner != null && !owner.EntityAlive) return;

            if (_knockdown)
            {
                if (now - _knockdownStart >= settings.GrabDecisionDelay) TryGrantGrab(now);
                return;
            }

            if (now < _graceUntil) return;
            if (Used(AttackTokenCategory.Grab) > 0) return; // идёт захват: остальные не атакуют
            if (now < _nextGrantTime) return;

            if (TryGrantAttack(now))
                _nextGrantTime = now + UnityEngine.Random.Range(settings.GrantInterval.x, settings.GrantInterval.y);
        }

        private void TryGrantGrab(float now)
        {
            int capacity = Capacity(AttackTokenCategory.Grab);
            if (Used(AttackTokenCategory.Grab) >= capacity) return;

            Vector3 center = Center.position;
            int best = -1;
            UnitTier bestTier = UnitTier.Unassigned;
            float bestDist = float.MaxValue;

            for (int i = 0; i < _requests.Count; i++)
            {
                var r = _requests[i];
                if (r.Category != AttackTokenCategory.Grab) continue;

                var tier = r.Participant.Tier;
                float dist = (r.Participant.transform.position - center).sqrMagnitude;
                if (best >= 0 && (tier < bestTier || (tier == bestTier && dist >= bestDist))) continue;

                best = i;
                bestTier = tier;
                bestDist = dist;
            }

            if (best >= 0) GrantAt(best, 1, now);
        }

        private bool TryGrantAttack(float now)
        {
            Vector3 center = Center.position;
            float proximityRange = Mathf.Max(0.1f, settings.OuterRadius * 2f);
            int best = -1;
            int bestCost = 0;
            float bestScore = float.MinValue;

            for (int i = 0; i < _requests.Count; i++)
            {
                var r = _requests[i];
                if (r.Category == AttackTokenCategory.Grab) continue;

                int capacity = Capacity(r.Category);
                if (capacity <= 0) continue;

                // стоимость больше лимита (Центурион с Cost 2 при лимите 1) — берём весь пул, иначе заявка зависнет навсегда
                int cost = Mathf.Min(r.Cost, capacity);
                if (Used(r.Category) + cost > capacity) continue;

                var p = r.Participant;
                if (p.Aggression < settings.AggressionThreshold) continue;

                float dist = Vector3.Distance(p.transform.position, center);
                float score = p.Aggression + settings.ProximityWeight * (1f - Mathf.Clamp01(dist / proximityRange));
                if (score <= bestScore) continue;

                best = i;
                bestCost = cost;
                bestScore = score;
            }

            if (best < 0) return false;
            GrantAt(best, bestCost, now);
            return true;
        }

        private void GrantAt(int requestIndex, int cost, float now)
        {
            var r = _requests[requestIndex];
            _requests.RemoveAt(requestIndex);
            _grants[r.Participant] = new Grant { Category = r.Category, Cost = cost, Time = now };
        }

        #region слоты

        private void UpdateSlots()
        {
            Vector3 center = Center.position;
            _inner.Clear();
            _outer.Clear();
            _centerOnMesh.Clear();

            foreach (var p in _participants)
            {
                if (p == null) continue;
                if (!p.WantsSlot || !p.CanMoveToSlot)
                {
                    p.ClearSlot();
                    continue;
                }

                bool attacking = TryGetToken(p, out var category) && category != AttackTokenCategory.Ranged;
                (attacking ? _inner : _outer).Add(p);
            }

            AssignRing(_inner, 0, settings.InnerRadius, center);
            AssignRing(_outer, 1, settings.OuterRadius, center);
        }

        private void AssignRing(List<CombatParticipant> list, int ring, float radius, Vector3 center)
        {
            if (list.Count == 0) return;

            int count = Mathf.Max(4, Mathf.FloorToInt(2f * Mathf.PI * radius / Mathf.Max(0.1f, settings.SlotSpacing)));
            if (_taken.Length < count)
            {
                _taken = new bool[count];
                _tried = new bool[count];
            }
            Array.Clear(_taken, 0, count);

            _sortCenter = center;
            list.Sort(_byDistance); // ближние выбирают первыми

            float step = 360f / count;
            foreach (var p in list)
            {
                float r = ring == 0 ? Mathf.Min(radius, p.AttackRange * 0.8f) : radius;

                Vector3 to = p.transform.position - center;
                to.y = 0f;
                float desired = to.sqrMagnitude > 0.01f
                    ? Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg
                    : p.transform.eulerAngles.y + 180f;
                int previous = p.SlotRing == ring ? p.SlotIndex : -1;

                Array.Clear(_tried, 0, count);
                bool assigned = false;

                for (int attempt = 0; attempt < count && !assigned; attempt++)
                {
                    int bestSlot = -1;
                    float bestCost = float.MaxValue;
                    for (int i = 0; i < count; i++)
                    {
                        if (_taken[i] || _tried[i]) continue;
                        float cost = Mathf.Abs(Mathf.DeltaAngle(desired, i * step));
                        if (i == previous) cost -= settings.SlotHysteresisDeg;
                        if (cost >= bestCost) continue;
                        bestCost = cost;
                        bestSlot = i;
                    }

                    if (bestSlot < 0) break;
                    _tried[bestSlot] = true;

                    if (!TryProjectSlot(p, center, bestSlot * step, r, out var position)) continue;
                    _taken[bestSlot] = true;
                    p.SetSlot(ring, bestSlot, position);
                    assigned = true;
                }

                if (!assigned) p.ClearSlot();
            }
        }

        private bool TryProjectSlot(CombatParticipant p, Vector3 center, float angle, float radius, out Vector3 position)
        {
            Vector3 point = center + Quaternion.Euler(0f, angle, 0f) * Vector3.forward * radius;
            position = point;

            var agent = p.Agent;
            if (agent == null) return true;

            if (!agent.SampleOwnMesh(point, settings.NavSampleRadius, out var hit)) return false;
            position = hit.position;

            // стена между целью и слотом: ставим слот у стены, если он не слишком близко к цели
            var centerOnMesh = CenterOnMesh(agent, center);
            if (centerOnMesh.HasValue &&
                NavMesh.Raycast(centerOnMesh.Value, hit.position, out var wall, agent.QueryFilter()))
            {
                Vector3 flat = wall.position - center;
                flat.y = 0f;
                if (flat.sqrMagnitude < radius * radius * 0.25f) return false;
                position = wall.position;
            }
            return true;
        }

        private Vector3? CenterOnMesh(NavMeshAgent agent, Vector3 center)
        {
            if (_centerOnMesh.TryGetValue(agent.agentTypeID, out var cached)) return cached;
            Vector3? result = agent.SampleOwnMesh(center, 2f, out var hit) ? (Vector3?)hit.position : null;
            _centerOnMesh[agent.agentTypeID] = result;
            return result;
        }

        private int CompareByDistance(CombatParticipant a, CombatParticipant b) =>
            (a.transform.position - _sortCenter).sqrMagnitude.CompareTo((b.transform.position - _sortCenter).sqrMagnitude);

        #endregion

        #region gizmos

        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;
            Vector3 c = Center.position;

            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.6f);
            DrawCircle(c, settings.InnerRadius);
            Gizmos.color = new Color(1f, 1f, 1f, 0.35f);
            DrawCircle(c, settings.OuterRadius);

            if (!Application.isPlaying) return;

            foreach (var p in _participants)
            {
                if (p == null) continue;
                Gizmos.color = TryGetToken(p, out var category) ? CategoryColor(category) : Color.white;
                if (p.HasSlot)
                {
                    Gizmos.DrawWireSphere(p.SlotPosition, 0.25f);
                    Gizmos.DrawLine(p.transform.position, p.SlotPosition);
                }
#if UNITY_EDITOR
                string label = $"{p.Aggression:0.00}" + (HasAnyToken(p) ? $" [{category}]" : "");
                UnityEditor.Handles.Label(p.transform.position + Vector3.up * 2f, label);
#endif
            }

#if UNITY_EDITOR
            string state = _knockdown ? "НОКДАУН" : InRecovery ? "фора" : "";
            UnityEditor.Handles.Label(c + Vector3.up * 2.5f,
                $"M {Used(AttackTokenCategory.Melee)}/{settings.MeleeTokens}  " +
                $"H {Used(AttackTokenCategory.Heavy)}/{settings.HeavyTokens}  " +
                $"R {Used(AttackTokenCategory.Ranged)}/{settings.RangedTokens}  " +
                $"G {Used(AttackTokenCategory.Grab)}/{settings.GrabTokens}  " +
                $"заявок {_requests.Count}  {state}");
#endif
        }

        private static Color CategoryColor(AttackTokenCategory category) => category switch
        {
            AttackTokenCategory.Melee => Color.red,
            AttackTokenCategory.Heavy => Color.magenta,
            AttackTokenCategory.Ranged => Color.cyan,
            AttackTokenCategory.Grab => Color.yellow,
            _ => Color.white,
        };

        private static void DrawCircle(Vector3 center, float radius)
        {
            const int segments = 40;
            Vector3 prev = center + Vector3.forward * radius;
            for (int i = 1; i <= segments; i++)
            {
                float a = i * Mathf.PI * 2f / segments;
                Vector3 next = center + new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a)) * radius;
                Gizmos.DrawLine(prev, next);
                prev = next;
            }
        }

        #endregion
    }
}
