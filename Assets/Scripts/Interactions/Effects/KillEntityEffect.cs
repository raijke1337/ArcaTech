using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Interactions
{
    public sealed class KillEntityEffect : InteractionEffect, IKillerComponent
    {
        [SerializeField] private InteractionState killState = InteractionState.Success;
        [SerializeField] private BaseGameEntityComponent[] entities;
        public override void Play(InteractionContext ctx)
        {
            if (ctx.State == killState)
            {
                foreach (var e in entities) e.SetKilled(this, true);
            }
        }

        public string KilledBy => $"Kill effect on {name}";
    }
}