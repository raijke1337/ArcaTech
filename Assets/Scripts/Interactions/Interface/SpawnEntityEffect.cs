using System;
using UnityEngine;

namespace Arcatech.Interactions
{
    public class SpawnEntityEffect : InteractionEffect
    {
        [SerializeField] private BaseGameEntityComponent prefab;
        public override void Play(InteractionContext ctx)
        {
            if (ctx.State == InteractionState.Success)
            {
                Instantiate(prefab, transform.position, Quaternion.identity);
            }
        }

        private void OnDrawGizmos()
        {
            if (prefab != null)
            {
                Gizmos.DrawWireSphere(transform.position, 0.1f);
            }
        }
    }
}