using System.Collections.Generic;
using System.Linq;
using Arcatech.EventBus;
using CartoonFX;
using UnityEngine;

namespace Arcatech.Effects
{
    public struct ParticlesEvent : IEvent
    {
        public ParticlesEvent(IEnumerable<CFXR_Effect> particles)
        {
            Effects = particles.ToArray();
            Parent =  null;
            Place = Vector3.zero;
        }

        public CFXR_Effect[] Effects  { get; }
        public Transform Parent { get; set; }
        public Vector3 Place { get; set; }
    }
}