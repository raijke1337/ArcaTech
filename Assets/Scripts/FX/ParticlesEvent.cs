using System.Collections.Generic;
using System.Linq;
using Arcatech.EventBus;
using CartoonFX;
using UnityEngine;

namespace Arcatech.Effects
{
    public struct ParticlesEvent : IEvent
    {
        public ParticlesEvent(CFXR_Effect particles)
        {
            Effect = particles;
            Parent =  null;
            Place = Vector3.zero;
        }

        public ParticlesEvent(CFXR_Effect particles, Vector3 place)
        {
            Effect = particles;
            Parent = null;
            Place = place;
        }

        public CFXR_Effect Effect  { get; }
        public Transform Parent { get; set; }
        public Vector3 Place { get; set; }
    }
}