using Arcatech.Actions;
using Arcatech.Level;
using Arcatech.Units;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
namespace Arcatech.Triggers
{


    public class ActionResultApplicatorTrigger : BaseTrigger
    {
        [Header("Action result applicator")]
        [SerializeField] Side applicatorSide;
        [SerializeField] protected TargetingType targetType;
        [Header("if 0, apply once. if >0, apply the results every f seconds")]
        [SerializeField, Range(0,3)] protected float ReapplyWhileActorInsideTimer = 0;
        [Space]
        [Space, SerializeField] protected SerializedActionResult[] ResultOnEntry;
        [SerializeField] protected bool DestroyOnEnter = false;
        [SerializeField] protected SerializedActionResult[] ResultOnExit;
        [SerializeField] protected bool DestroyOnExit = false;

        Timer reapplyTimer;

        private void OnValidate()
        {
            Assert.IsFalse(targetType == TargetingType.None || targetType == TargetingType.OnlyUser,$"Incorrect targeting type set for {this}");
        }
        private void Update()
        {
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

            if (other.gameObject.TryGetComponent(out BaseEntityOLD p))
            {

                if (reapplyTimer == null)
                {
                    reapplyTimer = new CountDownTimer(ReapplyWhileActorInsideTimer);                    
                }
                reapplyTimer.Start();

                switch (targetType)
                {
                    case TargetingType.AnyUnit:
                        ApplyResultsTo(p);
                        break;
                    case TargetingType.AnyEnemy:
                        if (p.Side != applicatorSide) ApplyResultsTo(p);
                        break;
                    case TargetingType.AnyAlly:
                        if (p.Side == applicatorSide) ApplyResultsTo(p);
                        break;
                    default:
                        Debug.Log($"{p.UnitName} entered {this} and nothing happened because of trigger settings");
                        break;
                }
            }
            if (DestroyOnEnter)
            {
                gameObject.SetActive(false);
            }
        }


        protected override void OnTriggerExit(Collider other)
        {
            if (other.gameObject.TryGetComponent(out BaseEntityOLD p))
            {
                switch (targetType)
                {
                    case TargetingType.AnyUnit:
                        ApplyResultsTo(p);
                        break;
                    case TargetingType.AnyEnemy:
                        if (p.Side != applicatorSide) ApplyResultsTo(p);
                        break;
                    case TargetingType.AnyAlly:
                        if (p.Side == applicatorSide) ApplyResultsTo(p);
                        break;
                    default:
                        Debug.Log($"{p.UnitName} exited {this} and nothing happened because of trigger settings");
                        break;
                }
            }

            if (DestroyOnExit)
            {
                gameObject.SetActive(false);
            }
        }


        protected void ApplyResultsTo(BaseEntityOLD p)
        {
            foreach (var action in ResultOnEntry)
            {
                action.BuildActionResult().ProduceResult(null, p, transform);
            }
        }
    }
}