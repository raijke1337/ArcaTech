using Arcatech.Interactions;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.SaveSystem
{
    [RequireComponent(typeof(InteractableComponent))]
    public class ActivatorProgress : SavedProgressItemBase
    {
        [SerializeField,Self]private InteractableComponent comp;
        public override void ApplySaveState(ProgressItemState state, ILevelProgressContext ctx)
        {
            comp.ForceState(state);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            comp.StateChangedEvent += HandleEvent;
        }

        override protected void OnDisable()
        {
            comp.StateChangedEvent -= HandleEvent;
        }

        private void HandleEvent(InteractionState state)
        {
            if (state == InteractionState.Success) Announce(ProgressItemState.Completed); 
        }
    }
}