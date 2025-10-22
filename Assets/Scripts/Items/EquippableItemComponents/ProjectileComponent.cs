using System;
using Arcatech.Actions;
using Arcatech.Skills;
using Arcatech.Triggers;
using Arcatech.Units;
using System.Linq;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Items
{
    [RequireComponent(typeof(TriggerTrackerComponent), typeof(BaseGameEntityComponent))]
    public class ProjectileComponent : ValidatedMonoBehaviour, IPausableComponent, ITriggerNotificationReceiver
    {
        
        
        public BaseGameEntityComponent Owner { get; set; }

        [SerializeField, Self] private BaseGameEntityComponent entity;
        
        [HideInInspector] public int RemainingHits;
        [HideInInspector] public float Lifetime;
        [HideInInspector] public float Speed;
        protected bool hasHitUnit = false;
        TriggerTrackerComponent col;
        bool isAoe = false; // bandaid but w/e

        BaseGameEntityComponent[] hits;
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
            hits = new BaseGameEntityComponent[RemainingHits];
        }
        private void Start()
        {
            col = GetComponent<TriggerTrackerComponent>();
            if (GetComponent<AreaOfEffectSphereScalerComponent>()) isAoe = true;
            col.RegisterReceiver(this);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.isStatic)
            {
                Debug.Log("Collision Enter static item, should destroy");
                RemainingHits = 0;
            }
        }

        public virtual void TriggerEntered(BaseGameEntityComponent enterComponent, BaseGameEntityComponent trigger)
        {
            
            switch (targetingType)
            {
                case TargetingType.OnlyUser:
                    if (enterComponent == Owner)
                    {
                        OnColliderSuccess(enterComponent);
                    }
                    break;
                case TargetingType.AnyUnit:
                    OnColliderSuccess(enterComponent);
                    break;
                case TargetingType.AnyEnemy:
                    if (enterComponent.GetEntitySide != Owner.GetEntitySide)
                        OnColliderSuccess(enterComponent);
                    break;
                case TargetingType.AnyAlly:
                    if (enterComponent.GetEntitySide == Owner.GetEntitySide)
                        OnColliderSuccess(enterComponent);
                    break;
            }
            
            if (isAoe) return;

            if (RemainingHits == 0)
            {
                Expiry();
                Destroy(gameObject);
            }
        }

        public void TriggerExited(BaseGameEntityComponent exitComponent, BaseGameEntityComponent trigger)
        {
            //NOOP
        }

        void OnColliderSuccess(BaseGameEntityComponent u)
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
            if (Paused) return;

            transform.position += Speed * Time.deltaTime * transform.forward;
            Lifetime -= Time.deltaTime;
            if (Lifetime < 0)
            {
                Expiry();
            }
        }

        private void Expiry()
        {
            if (ExpirationCollisionResult.Length > 0 && !hasHitUnit)
            {               
                foreach (var exp in ExpirationCollisionResult)
                {
                    exp.ProduceResult(Owner, null, transform);
                }
            }
            col.UnregisterReceiver(this);
            entity.Killed = true;
        }

        public bool Paused { get; set; } = false;

    }
}
