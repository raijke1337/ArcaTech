using System;
using System.Collections.Generic;
using Arcatech.Effects;
using Arcatech.Units;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using Arcatech.EventBus;

namespace Arcatech.Items
{
    public class EquipmentParticles : MonoBehaviour, IEquipmentPart
    {
        [SerializeField] private SerializedDictionary<StateMachineNotifyType, ParticleSystem[]> effects;
        private StateMachineNotifyType current;
        public void TriggerState(StateMachineNotifyType notification)
        {
            if (effects.TryGetValue(current, out var ef))
            {
                foreach (var e in ef)
                {
                    e.Stop();
                }
            }
            if (effects.TryGetValue(notification, out var ne))
            {
                foreach (var e in ne)
                {
                    e.Play();
                }
            }
            
            current = notification;
        }

        private void Start()
        {
            foreach (var particle in GetComponentsInChildren<ParticleSystem>())
            {
                particle.Stop();
            }
        }
    }
}