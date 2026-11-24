using AYellowpaper.SerializedCollections;
using Unity.U2D.Physics;
using UnityEngine;

namespace Arcatech.Interactions
{
    /// <summary>
    /// set gameobject active based on the input state 
    /// </summary>
    public sealed class SetActiveStateEffect : InteractionEffect
    {
        public GameObject target;
        public SerializedDictionary<InteractionState, bool> states;


        public override void Play(InteractionContext ctx)
        {
            if (states.TryGetValue(ctx.State, out var state))
            {
                target.SetActive(state);
            }
        }
    }
}