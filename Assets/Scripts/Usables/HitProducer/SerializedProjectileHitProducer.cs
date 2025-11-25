using System.Collections;
using System.Collections.Generic;
using Arcatech.Actions;
using Arcatech.Items;
using Arcatech.Items.Projectiles;
using Arcatech.Triggers;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Usables
{
    /// <summary>
    /// uses projectiles to report hits
    /// </summary>
    [CreateAssetMenu(fileName = "New Projectiles Hit Producer", menuName = "Usables/Hit Producer/Projectile")]
    public class SerializedProjectileHitProducer : SerializedHitProducer
    {
        [SerializeField] private SerializedProjectileConfiguration projectile;
        [SerializeField] private ShootingConfig projectileShootingConfig;

        public override IHitProducer Deserialize(BaseGameEntityComponent owner, EquipmentComponent item)
        {
            return new ProjectileHitProducer(owner, item,projectile,projectileShootingConfig);
        }
    }

    public class ProjectileHitProducer : IHitProducer
    {
        
        private SerializedProjectileConfiguration _projectile;
        private ShootingConfig _shooting;
        private EquipmentComponent _item;
        private BaseGameEntityComponent _owner;
        private Coroutine _shootingCor;

        List<ProjectileComponent> trackedProjectiles = new List<ProjectileComponent>();
        public ProjectileHitProducer(BaseGameEntityComponent owner, EquipmentComponent item,SerializedProjectileConfiguration proj, ShootingConfig shooting)
        {
            _projectile = proj;
            _shooting = shooting;
            _owner = owner;
        }
        
        IEnumerator ShootingCoroutine()
        {
        
            Vector3 place = _item.EffectSpawn.position;
            Quaternion rotation = _item.EffectSpawn.rotation; 

            if (_shooting.ShotDelay >0)
            {
                yield return new WaitForSeconds(_shooting.ShotDelay);
            }

            int done = 0;
            while (done < _shooting.Shots)
            {
                done++; 
                var p = (_projectile.ProduceProjectile(_owner, place, rotation));
                trackedProjectiles.Add(p);
                
                yield return new WaitForSeconds(_shooting.BetweenShotsDelay);
            }
            yield return null;
        }
        

        public void Initialize()
        {
            _shootingCor = _item.StartCoroutine(ShootingCoroutine());
        }

        public event UnityAction<TriggerHitInfo> Hit;

        public void TriggerEntered(TriggerHitInfo triggerHitInfo)
        {
            Hit?.Invoke(triggerHitInfo);
        }

        public void TriggerExited(BaseGameEntityComponent exitComponent, ITriggerNotificationProvider trigger)
        {
            
        }

        public void Cleanup()
        {
            
        }
    }
}