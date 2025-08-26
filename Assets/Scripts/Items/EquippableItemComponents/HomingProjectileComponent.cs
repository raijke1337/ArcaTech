using Arcatech.Units;
using Cinemachine.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Items
{
    public class HomingProjectileComponent : ProjectileComponent
    {
        BaseEntityOLD target;

        float scanTimer = 0;
        float range;
        Collider[] scanResults;
        List<BaseEntityOLD> hitTarget = new();

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
                    if (col.TryGetComponent<BaseEntityOLD>(out var e) && e.Side != Owner.Side && !hitTarget.Contains(e))
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
        protected override void Col_SomethingHitEvent(Collider other)
        {
            base.Col_SomethingHitEvent(other);
            if (other.transform.Equals(target))
            {
                hitTarget.Add(target);
            }
        }

    }
}
