using System.Collections.Generic;
using Arcatech.Triggers;
using UnityEngine;

namespace Arcatech.Items
{
    public class HomingProjectileComponent : ProjectileComponent
    {
        // TODO move this to targeting strategy (new) because this is buggy AF.
        
        BaseGameEntityComponent target;

        float scanTimer = 0;
        float range;
        Collider[] scanResults;
        List<BaseGameEntityComponent> hitTarget = new();

        public HomingProjectileComponent WithHoming (float scanRange)
        {
            range = scanRange;
            scanResults = new Collider[20];
            return this;
        }
        protected override void Update()
        {
            scanTimer += Time.deltaTime;

            if (scanTimer > 0.5f) //TODO maybe
            {
                scanTimer = 0;
                if (target != null) return;

                Physics.OverlapSphereNonAlloc(transform.position, range, scanResults);
                foreach (Collider col in scanResults)
                {
                    if (col == null) return;
                    if (col.TryGetComponent<BaseGameEntityComponent>(out var e) && e.GetEntitySide!= Owner.GetEntitySide&& !hitTarget.Contains(e))
                    { 
                        target = e;                      
                        break;
                    }
                }
            }
            if (target != null)
            {
                transform.LookAt(new Vector3 (target.transform.position.x, transform.position.y, target.transform.position.z));
            }

            transform.position += Speed * Time.deltaTime * transform.forward;
            Lifetime -= Time.deltaTime;
            if (Lifetime < 0)
            {
                Destroy(gameObject);
            }
        }

        public override void TriggerEntered(BaseGameEntityComponent enterComponent, TriggerTrackerComponent trigger)
        {
            base.TriggerEntered(enterComponent, trigger);
            if (enterComponent.transform.Equals(target.transform))
            {
                hitTarget.Add(target);
            }
        }


    }
}
