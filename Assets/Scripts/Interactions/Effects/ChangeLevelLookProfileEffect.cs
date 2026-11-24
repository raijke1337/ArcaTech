using Arcatech.Rendering;
using UnityEngine;

namespace Arcatech.Interactions
{
    public sealed class ChangeLevelLookProfileEffect : InteractionEffect
    {
        [SerializeField] private ArcaLookProfile targetProfile;
        [SerializeField, Min(0f)] private float transitionDuration = 1.5f;
        [SerializeField] private InteractionState onState = InteractionState.Success;

        public override bool ReplayOnLoad => true;

        public override void Play(InteractionContext ctx)
        {
            if (ctx == null || ctx.State != onState || targetProfile == null)
                return;

            ArcaWorldLook worldLook = ArcaWorldLook.Active;
            if (worldLook == null || worldLook.Profile == targetProfile)
                return;

            worldLook.SetProfile(targetProfile, transitionDuration);
        }
    }
}
