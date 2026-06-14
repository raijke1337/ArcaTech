using UnityEngine;

namespace Arcatech.Usables.Effects
{
    public sealed class EffectContext
    {
        public BaseGameEntityComponent Source { get; internal set; }
        public BaseGameEntityComponent Target { get; internal set; }
        public EffectsReceiverComponent TargetReceiver { get; internal set; } // cached per application
        public Vector3 Place { get; private set; }
        public Quaternion PlaceRotation { get; private set; }
        public int TickIndex { get; internal set; }
        public ActiveEffectInstance Instance { get; internal set; }

        public void SetTarget(EffectsReceiverComponent receiver, Vector3 place, Quaternion placeRot)
        {
            TargetReceiver = receiver;
            Target = receiver != null ? receiver.Owner : null;
            Place = place; PlaceRotation = placeRot;
            TickIndex = 0; Instance = null; Source = null;
        }
    }
}