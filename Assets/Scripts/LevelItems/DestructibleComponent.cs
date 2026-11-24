using System.Collections.Generic;
using Arcatech.SaveSystem;
using Arcatech.Stats;
using Arcatech.Units;
using Arcatech.Usables.Effects;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Arcatech
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider), typeof(EffectsReceiverComponent))]
    public sealed class DestructibleComponent : SavedProgressItemBase, IStatReceiver, IKillerComponent
    {
        [SerializeField, Min(1)] private int hitsToBreak = 3;
        [SerializeField, Min(0f)] private float minDamagePerHit = 13f;

        [Header("Fragments")]
        [SerializeField, Range(2, 24)] private int fragmentCount = 8;
        [SerializeField, Min(0.1f)] private float fragmentLifetime = 4f;
        [SerializeField, FormerlySerializedAs("fragmentMaterial")] private Material cutMaterial;

        [Header("Fragment Motion")]
        [Tooltip("Speed away from the center of the collider in meters per second.")]
        [SerializeField, Min(0f), FormerlySerializedAs("fragmentSpeed")] private float radialSpeed = 5f;
        [Tooltip("Speed along the incoming hit direction in meters per second.")]
        [SerializeField, Min(0f)] private float impactSpeed = 2.5f;
        [Tooltip("Upward speed in meters per second.")]
        [SerializeField, Min(0f)] private float liftSpeed = 3f;
        [Tooltip("Angular speed around a random axis in radians per second.")]
        [SerializeField, Min(0f)] private float spinSpeed = 9f;

        private BoxCollider _box;
        private EntityEffectController _effectController;
        private float? _hitDamage;
        private int _remainingHits;

        public bool Invulnerable
        {
            get => Entity.Invulnerable;
            set => Entity.Invulnerable = value;
        }

        public string KilledBy => nameof(DestructibleComponent);
        public override ProgressItemKind Kind => ProgressItemKind.Destructible;

        private void Awake()
        {
            _box = GetComponent<BoxCollider>();
            _effectController = GetComponent<EntityEffectController>();
            _remainingHits = Mathf.Max(1, hitsToBreak);
            Entity.AnnounceDead ??= new UnityEvent<BaseGameEntityComponent>();
            Entity.AnnounceDead.AddListener(OnEntityDeath);
        }

        private void OnDestroy()
        {
            if (Entity != null) Entity.AnnounceDead.RemoveListener(OnEntityDeath);
        }

        protected override void OnApplySaveState(ProgressItemState state, ILevelProgressContext ctx)
        {
            if (state == ProgressItemState.Completed) Destroy(gameObject);
        }

        // Only deltas applied while the weapon is processing this contact count toward a hit.
        internal float? BeginHit()
        {
            float? previousDamage = _hitDamage;
            _hitDamage = 0f;
            return previousDamage;
        }

        internal float EndHit(float? previousDamage)
        {
            float damage = _hitDamage.GetValueOrDefault();
            _hitDamage = previousDamage;
            return damage;
        }

        public bool ApplyInstantDelta(StatDelta delta, BaseGameEntityComponent source, EffectKey key)
        {
            if (!_hitDamage.HasValue || _remainingHits == 0 || !isActiveAndEnabled || Invulnerable ||
                delta.stat != ResourceStatType.Health || delta.target != StatTarget.Current || !(delta.amount < 0f))
                return false;

            _hitDamage -= delta.amount;
            return true;
        }

        internal void CompleteHit(float damage, TriggerHitInfo hit)
        {
            if (_remainingHits == 0 || !Entity.EntityAlive || !isActiveAndEnabled ||
                !(damage > 0f && damage >= minDamagePerHit)) return;

            if (--_remainingHits > 0) return;

            var direction = hit.TryGetEntityTarget(out var hitEntity) && hitEntity == Entity
                ? hit.ImpactDirection
                : _box.bounds.center - hit.Position;
            Shatter(direction);
            Entity.SetKilled(this, true);
        }

        private void OnEntityDeath(BaseGameEntityComponent entity)
        {
            if (entity.EntityAlive) return;

            if (ReadItemState != ProgressItemState.Completed) Announce(ProgressItemState.Completed);
            if (isActiveAndEnabled && _remainingHits > 0)
                Shatter(Vector3.zero);
        }

        private void Shatter(Vector3 impactDirection)
        {
            _remainingHits = 0;
            Vector3 scale = transform.lossyScale;
            scale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
            Vector3 size = Vector3.Scale(_box.size, scale);
            var center = transform.TransformPoint(_box.center);
            var rotation = transform.rotation;
            var fragmentMesh = Mathf.Min(size.x, size.y, size.z) > 0f
                ? new BoxFragmentMesh(transform, center, rotation, size, cutMaterial)
                : null;

            foreach (var renderer in GetComponentsInChildren<Renderer>())
                if (renderer is MeshRenderer or SkinnedMeshRenderer) renderer.enabled = false;
            foreach (var collider in GetComponentsInChildren<Collider>())
                collider.enabled = false;
            _effectController.SetKilled(this, true);

            if (fragmentMesh == null) return;
            var carry = impactDirection.normalized * impactSpeed + Vector3.up * liftSpeed;
            foreach (var bounds in SplitBox(size, Mathf.Clamp(fragmentCount, 2, 24)))
                SpawnFragment(bounds, center, rotation, fragmentMesh, carry);
        }

        private static List<Bounds> SplitBox(Vector3 size, int count)
        {
            var fragments = new List<Bounds>(count) { new Bounds(Vector3.zero, size) };
            while (fragments.Count < count)
            {
                int largest = 0;
                for (int i = 1; i < fragments.Count; i++)
                {
                    var candidate = fragments[i].size;
                    var current = fragments[largest].size;
                    if (candidate.x * candidate.y * candidate.z > current.x * current.y * current.z)
                        largest = i;
                }

                var bounds = fragments[largest];
                var lengths = bounds.size;
                int axis = lengths.x >= lengths.y ? 0 : 1;
                if (lengths.z > lengths[axis]) axis = 2;

                var min = bounds.min;
                var max = bounds.max;
                float cut = Mathf.Lerp(min[axis], max[axis], Random.Range(0.35f, 0.65f));
                var firstMax = max;
                firstMax[axis] = cut;
                var secondMin = min;
                secondMin[axis] = cut;

                var first = new Bounds();
                first.SetMinMax(min, firstMax);
                fragments[largest] = first;

                var second = new Bounds();
                second.SetMinMax(secondMin, max);
                fragments.Add(second);
            }

            return fragments;
        }

        private void SpawnFragment(Bounds bounds, Vector3 center, Quaternion rotation, BoxFragmentMesh geometry,
            Vector3 carryVelocity)
        {
            var mesh = geometry.Build(bounds, out var materials);
            var fragment = new GameObject($"{gameObject.name} Fragment");
            Destroy(fragment, fragmentLifetime);
            if (fragment.scene != gameObject.scene)
                SceneManager.MoveGameObjectToScene(fragment, gameObject.scene);

            var offset = rotation * bounds.center;
            fragment.transform.SetPositionAndRotation(center + offset, rotation);
            fragment.AddComponent<DestructibleFragment>().SetMesh(mesh, materials);
            fragment.AddComponent<BoxCollider>().size = bounds.size * 0.96f;

            var body = fragment.AddComponent<Rigidbody>();
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            body.linearVelocity = offset.normalized * radialSpeed + carryVelocity;
            body.maxAngularVelocity = Mathf.Max(body.maxAngularVelocity, spinSpeed);
            body.angularVelocity = Random.onUnitSphere * spinSpeed;
        }
    }
}
