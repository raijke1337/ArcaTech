using System;
using Arcatech.SaveSystem;
using UnityEngine;

namespace Arcatech.Interactions
{
    public class TeleportToPointEffect : InteractionEffect
    {
        [SerializeField] Transform point;
        public override void Play(InteractionContext ctx)
        {
            ctx.Interactor.Entity.transform.position = point.position;
        }

        public override void OnLoadLevelState(ProgressItemState stateToLoad)
        {
            
        }

        private void OnDrawGizmos()
        {
            if (point != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(point.position, 1f);
            }
        }
    }
}