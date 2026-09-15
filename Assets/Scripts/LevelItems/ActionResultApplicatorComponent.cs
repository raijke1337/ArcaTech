using System.Collections.Generic;
using System.Linq;
using Arcatech.Actions;
using Arcatech.SaveSystem;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Arcatech.Interactions
{
    public class ActionResultApplicatorComponent : InteractionEffect
    {
        [SerializeField] private SerializedDictionary<InteractionState, SerializedActionResult[]> actions;
        private Dictionary<InteractionState, ActionResult[]> _dict;

        [SerializeField,Header("False will target the interaction target")] private bool targetUser = true;
        private void Awake()
        {
            _dict = actions.ToDictionary(
                pair=>pair.Key,
                pair=>pair.Value.Select(
                    item=>item.Deserialize()).ToArray());
        }

        public override void Play(InteractionContext ctx)
        {

            var finalTarget = targetUser ? ctx.Interactor.Entity : ctx.Target;
            
            foreach (var result in _dict[ctx.State])
            {
                result.ProduceResult(ctx.Target, finalTarget,
                    ctx.Interactor.Entity.EffectSpawn.position, ctx.Interactor.Entity.EffectSpawn.rotation);
            }
        }

    }
}