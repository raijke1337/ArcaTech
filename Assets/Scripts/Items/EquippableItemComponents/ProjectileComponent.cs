using Arcatech.Actions;
using Arcatech.Skills;
using Arcatech.Triggers;
using Arcatech.Units;
using System.Linq;
using UnityEngine;

namespace Arcatech.Items
{


    [RequireComponent(typeof(WeaponTriggerComponent))]
    public class ProjectileComponent : MonoBehaviour
    {
        public BaseEntity Owner { get; set; }
        [HideInInspector] public int RemainingHits;
        [HideInInspector] public float Lifetime;
        [HideInInspector] public float Speed;
        protected bool hasHitUnit = false;
        WeaponTriggerComponent col;
        bool isAoe = false; // bandaid but w/e

        BaseEntity[] hits;
        int index = 0;
        TargetingType targetingType;

        //public SerializedEffectsCollection VFX;
       // EffectsCollection _fx;

        IActionResult[] UnitCollisionResult; // explode (place aoe projectile) or apply effects
        IActionResult[] ExpirationCollisionResult; // explode (place aoe projectile) or stop moving
        

        public void SetResult(SerializedActionResult[] cfg, SerializedActionResult[] exp, TargetingType t)
        {
            targetingType = t;
            UnitCollisionResult = new ActionResult[cfg.Length];
            for (int i = 0; i < UnitCollisionResult.Length; i++)
            {
                UnitCollisionResult[i] = cfg[i].BuildActionResult();
            }

            ExpirationCollisionResult = new ActionResult[exp.Length];
            for (int i = 0; i < ExpirationCollisionResult.Length; i++)
            {
                ExpirationCollisionResult[i] = exp[i].BuildActionResult();
            }
            hits = new BaseEntity[RemainingHits];
        }
        private void Start()
        {
            col = GetComponent<WeaponTriggerComponent>();
            if (GetComponent<AreaOfEffectSphereScalerComponent>()) isAoe = true;
            col.SomethingHitEvent += Col_SomethingHitEvent;
        }

        protected virtual void Col_SomethingHitEvent(Collider other)
        {
            if (Owner.UnitDebug) { Debug.Log($"{this} hit {other}"); }
            if (other.TryGetComponent<BaseEntity>(out var u))
            {
                switch (targetingType)
                {
                    case TargetingType.OnlyUser:
                        if (u == Owner)
                        {
                            OnColliderSuccess(u);
                        }
                        break;
                    case TargetingType.AnyUnit:
                        OnColliderSuccess(u);
                        break;
                    case TargetingType.AnyEnemy:
                        if (u.Side != Owner.Side)
                            OnColliderSuccess(u);
                        break;
                    case TargetingType.AnyAlly:
                        if (u.Side == Owner.Side)
                            OnColliderSuccess(u);
                        break;
                }
            }
            if (isAoe) return;

            if (other.gameObject.isStatic)
            {
                RemainingHits = 0;
            }

            if (RemainingHits == 0)
            {
                Expiry();
                Destroy(gameObject);
            }
        }
        void OnColliderSuccess(BaseEntity u)
        {
            if (!hits.Contains(u) && RemainingHits > 0) // mightr be slow 
            {
                hits[index] = u;
                index++;
                hasHitUnit = true;
                RemainingHits--;

                if (UnitCollisionResult.Length > 0)
                {
                    foreach (var uc in UnitCollisionResult)
                    {
                        uc.ProduceResult(Owner, u, transform);
                    }
                }
            }
        }

        protected virtual void Update()
        {
            transform.position += Speed * Time.deltaTime * transform.forward;
            Lifetime -= Time.deltaTime;
            if (Lifetime < 0)
            {
                Expiry();
                Destroy(gameObject);
            }
        }

        protected virtual void Expiry()
        {
            if (ExpirationCollisionResult.Length > 0 && !hasHitUnit)
            {               
                foreach (var exp in ExpirationCollisionResult)
                {
                    exp.ProduceResult(Owner, null, transform);
                }
            }
        }




    }
}
