using Arcatech.Actions;
using Arcatech.Level;
using Arcatech.Units;
using UnityEngine;
using UnityEngine.Assertions;
namespace Arcatech.Triggers
{


    public class BaseLevelEventTrigger : BaseTrigger, IConditionControlled
    {
        [Header("Base event trigger")]
        [SerializeField] protected TargetingType targetType;
        [SerializeField] protected bool DestroyOnExit = false;
        [SerializeField] protected bool DestroyOnEnter = false;
        [Space, SerializeField] protected SerializedActionResult[] ActionOnEntry;
        [SerializeField] protected SerializedActionResult[] ActionOnExit;


        private void OnValidate()
        {
            Assert.IsFalse(targetType == TargetingType.None);
        }

        protected override void OnTriggerEnter(Collider other)
        {

            if (other.gameObject.TryGetComponent(out BaseEntity p))
            {
                switch (targetType)
                {
                    case TargetingType.AnyUnit:
                        ApplyResults(p);
                        break;
                    case TargetingType.OnlyUser:
                        ApplyResults(p);
                        break;
                    case TargetingType.AnyEnemy:
                        if (p.Side == Side.EnemySide) ApplyResults(p);
                        break;
                    case TargetingType.AnyAlly:
                        if (p.Side == Side.PlayerSide) ApplyResults(p);
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
            if (other.gameObject.TryGetComponent(out BaseEntity p))
            {
                switch (targetType)
                {
                    case TargetingType.AnyUnit:
                        ApplyResults(p);
                        break;
                    case TargetingType.OnlyUser:
                        ApplyResults(p);
                        break;
                    case TargetingType.AnyEnemy:
                        if (p.Side == Side.EnemySide) ApplyResults(p);
                        break;
                    case TargetingType.AnyAlly:
                        if (p.Side == Side.PlayerSide) ApplyResults(p);
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


        protected void ApplyResults(BaseEntity p)
        {
            foreach (var action in ActionOnEntry)
            {
                action.BuildActionResult().ProduceResult(null, p, transform);
            }

            if (DestroyOnEnter)
            {
                gameObject.SetActive(false);
            }
        }

        public void SetState(bool newstate)
        {
            Debug.Log($"Switch state of {this}, TODO");
            gameObject.SetActive(!gameObject.activeSelf);
        }
    }
}