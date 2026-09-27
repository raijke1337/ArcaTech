using Arcatech.Audio;
using UnityEngine;

namespace Arcatech.Interactions
{
    public sealed class PlaySoundEffect : InteractionEffect
    {
        [SerializeField]SoundDefinition soundDefinition;
        public override void Play(InteractionContext ctx)
        {
            AudioCall.Play(soundDefinition, ctx.Interactor.Entity.transform.position);
        }
    }
}