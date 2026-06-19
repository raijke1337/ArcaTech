using System;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Items
{
    public class EquipmentComponent : MonoBehaviour,IUsableComponent
    {
        [SerializeField] private Transform spawner;
        [SerializeField] private ParticleSystem particle;
        public Transform EffectSpawn => spawner;
        private EquipmentAnimator _equipmentAnimator;
        
        protected void OnEnable()
        {
            if (spawner == null)
            {
                Debug.LogWarning($"Spawner not set in {this}");
                spawner = transform;
            }
            TryGetComponent(out _equipmentAnimator);
            if (!particle) particle = GetComponentInChildren<ParticleSystem>();
            if (particle)
            {
                particle.Stop();
            }
        }


        public void OnChangeUsableState(StateMachineNotifyType notifyType)
        { 
            if (_equipmentAnimator) _equipmentAnimator.OnChangeUsableState(notifyType);
            switch (notifyType)
            {
                case StateMachineNotifyType.NoNotify:
                    break;
                case StateMachineNotifyType.Starting:
                    break;
                case StateMachineNotifyType.Use:
                    particle?.Play();
                    break;
                case StateMachineNotifyType.EndUse:
                    particle?.Stop();
                    break;
                case StateMachineNotifyType.Cancel:
                    particle?.Stop();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(notifyType), notifyType, null);
            }
        }
    }
}


