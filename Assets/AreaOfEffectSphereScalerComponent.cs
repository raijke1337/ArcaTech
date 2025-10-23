using Arcatech.Items;
using Arcatech.Units;
using UnityEngine;
using UnityEngine.Assertions;
namespace Arcatech.Skills
{
    [RequireComponent(typeof(SphereCollider),typeof(ProjectileComponent))]
    public class AreaOfEffectSphereScalerComponent : MonoBehaviour, IPausableComponent
    {
        SphereCollider _col;
        ProjectileComponent _projectile;

        [SerializeField] float desiredRad;
        float startRad;
        float time;
        float progress;

        public bool Paused { get; set; } = false;
        // bool start = false;

        private void Start()
        {
            _projectile = GetComponent<ProjectileComponent>();
            _col = GetComponent<SphereCollider>();
            time = _projectile.Lifetime;
            _col.radius = 0.1f;
            startRad = _col.radius;
           
           //start = true;
        }
        

        private void Update()
        {
            if (Paused) return;

            progress += Time.deltaTime * (1/time);
            Mathf.Clamp01(progress);
            if (_col.radius > desiredRad) return;
            _col.radius = Mathf.Lerp(startRad, desiredRad, progress);
        }
        

        private void OnValidate()
        {
            Assert.IsFalse(desiredRad == 0);
        }
        private void OnDrawGizmos()
        {
            if (_col == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _col.radius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, desiredRad);
            
        }
    }
}