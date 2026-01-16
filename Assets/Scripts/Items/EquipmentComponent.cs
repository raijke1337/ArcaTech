using System;
using Arcatech.Units;
using CartoonFX;
using Drakkar.GameUtils;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Items
{
    public class EquipmentComponent : MonoBehaviour,IStateMachineNotificationReceiver
    {
        [SerializeField] private Transform spawner;
        [SerializeField] private ParticleSystem useParticleEffect;
        public Transform EffectSpawn => spawner;
        private EquipmentAnimator _equipmentAnimator;
        private DrakkarTrail _trail;
        

        protected virtual void OnEnable()
        {
            if (spawner == null)
            {
                Debug.LogWarning($"Spawner not set in {this}");
                spawner = transform;
            }
            TryGetComponent(out _equipmentAnimator);
            TryGetComponent(out _trail);
            if (!useParticleEffect) return;
           // useParticleEffect.clearBehavior = CFXR_Effect.ClearBehavior.Disable;
           // useParticleEffect.enabled = false;
        }


        public void StateMachineNotification(StateMachineNotifyType notifyType)
        {
            switch (notifyType)
            {
                case StateMachineNotifyType.Starting:
                {
                    if (_trail) _trail.Begin();
                    break;
                }
                case StateMachineNotifyType.EndUse:
                {
                    if (_trail) _trail.End();
                    break;
                }
                case StateMachineNotifyType.Use:
                {
                    if (useParticleEffect)  useParticleEffect.Play(); 
                    break;
                }
            }
            if (_equipmentAnimator) _equipmentAnimator.StateMachineNotification(notifyType);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawLine(spawner.position, spawner.position+spawner.transform.forward);
            Gizmos.DrawWireSphere(spawner.position, 0.5f);
        }
    }
}


