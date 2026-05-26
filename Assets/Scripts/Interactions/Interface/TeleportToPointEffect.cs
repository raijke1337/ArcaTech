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
    }
}