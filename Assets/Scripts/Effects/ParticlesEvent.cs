using Arcatech.EventBus;
using CartoonFX;
using UnityEngine;

namespace Arcatech.Effects
{
    public struct ParticlesEvent : IEvent
    {
        public ParticlesEvent(CFXR_Effect effect, Vector3 place, Transform parent = null)
        {
            Effect = effect;
            Parent = parent;
            Place = place;
        }

        public CFXR_Effect Effect { get; }
        public Transform Parent { get; }
        public Vector3 Place { get; }
    }
}