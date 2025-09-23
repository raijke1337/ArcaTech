using Arcatech.Actions;
using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Assertions;


namespace Arcatech.Triggers
{

    [RequireComponent(typeof(BaseGameEntityComponent))]
    public class ActionResultApplicatorTrigger : BaseTrigger,IKillableComponent
    {
        [Header("Action result applicator")]
        [SerializeField] protected TargetingType targetType;
        [Header("if 0, apply once. if >0, apply the results every f seconds")]
        [SerializeField, Range(0,3)] protected float ReapplyWhileActorInsideTimer = 0;
        [Space]
        [Space, SerializeField] protected SerializedActionResult[] ResultOnEntry;
        [SerializeField] protected bool DestroyOnEnter = false;
        [SerializeField] protected SerializedActionResult[] ResultOnExit;
        [SerializeField] protected bool DestroyOnExit = false;

        Timer reapplyTimer;
        [SerializeField,Self] BaseGameEntityComponent baseComp;

        private bool componentKilled = false;

        protected override void OnValidate()
        {
            base.OnValidate();
            Assert.IsFalse(targetType == TargetingType.None || targetType == TargetingType.OnlyUser,$"Incorrect targeting type set for {this}");
        }

        private void Update()
        {
            if (componentKilled) return;
            if (reapplyTimer != null && reapplyTimer.IsRunning)
            {
                reapplyTimer.Tick(Time.deltaTime);
                if (!reapplyTimer.IsRunning)
                {
                    RecheckCollisions();
                }
            }
        }

        void RecheckCollisions()
        {
            Collider.enabled = false;
            Collider.enabled = true;
        }
        protected override void OnTriggerEnter(Collider other)
        {
            if (componentKilled) return;
            Debug.Log(other.name);

            if (other.gameObject.TryGetComponent(out ActiveGameUnitComponent p))
            {

                if (reapplyTimer == null)
                {
                    reapplyTimer = new CountDownTimer(ReapplyWhileActorInsideTimer);                    
                }
                reapplyTimer.Start();

                switch (targetType)
                {
                    case TargetingType.AnyUnit:
                        ApplyResultsTo(p.GetMainEntity,ResultOnEntry);
                        break;
                    case TargetingType.AnyEnemy:
                        if (p.GetMainEntity.GetEntitySide != baseComp.GetEntitySide) ApplyResultsTo(p.GetMainEntity, ResultOnEntry);
                        break;
                    case TargetingType.AnyAlly:
                        if (p.GetMainEntity.GetEntitySide == baseComp.GetEntitySide) ApplyResultsTo(p.GetMainEntity, ResultOnEntry);
                        break;
                    default:
                        Debug.Log($"{p.name} entered {this} and nothing happened because of trigger settings");
                        break;
                }
                if (DestroyOnEnter)
                {
                    gameObject.SetActive(false);
                }
            }

        }


        protected override void OnTriggerExit(Collider other)
        {
            if (componentKilled) return;
            if (other.gameObject.TryGetComponent(out ActiveGameUnitComponent p))
            {
                switch (targetType)
                {
                    case TargetingType.AnyUnit:
                        ApplyResultsTo(p.GetMainEntity, ResultOnExit);
                        break;
                    case TargetingType.AnyEnemy:
                        if (p.GetMainEntity.GetEntitySide != baseComp.GetEntitySide) ApplyResultsTo(p.GetMainEntity,ResultOnExit);
                        break;
                    case TargetingType.AnyAlly:
                        if (p.GetMainEntity.GetEntitySide == baseComp.GetEntitySide) ApplyResultsTo(p.GetMainEntity, ResultOnExit);
                        break;
                    default:
                        Debug.Log($"{p.GetMainEntity.GetName} exited {this} and nothing happened because of trigger settings");
                        break;
                }
                if (DestroyOnExit)
                {
                    gameObject.SetActive(false);
                }
            }


        }


        void ApplyResultsTo(BaseGameEntityComponent p, SerializedActionResult[] results)
        {
            foreach (var action in results)
            {
                action.BuildActionResult().ProduceResult(null, p, transform);
            }
        }

        public void Kill() => componentKilled = true;
    }
}