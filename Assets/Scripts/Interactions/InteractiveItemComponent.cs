using Arcatech.Actions;
using System.Collections.Generic;
using UnityEngine;
using KBCore.Refs;

namespace Arcatech.Interactions
{
    [RequireComponent(typeof(BaseGameEntityComponent))]
    public class InteractiveItemComponent : ValidatedMonoBehaviour, IInteractive
    {

        [SerializeField] private bool _itemDisappearsWhenUsed;
        
        [Space,SerializeField, Self] private BaseGameEntityComponent baseComp;
        [SerializeField] private List<InteractionHandlerBase> handlersOnThisItem;
        [SerializeField] private List<InteractionHandlerBase> handlers;

        public BaseGameEntityComponent GetBaseComponent => baseComp;

        protected override void OnValidate()
        {
            base.OnValidate();
            handlersOnThisItem = new  List<InteractionHandlerBase>(GetComponentsInChildren<InteractionHandlerBase>());
        }

        #region interaction

        [Space, Header("Condition checker")]
        //[SerializeField] SerializedDictionary<EventCondition, InteractionHandlerBase[]> _list;
        [SerializeField]
        protected InteractionCondition condition;
        
        public bool OnInteraction(IInteractor interactor,InteractionContext interactionContext)
        {
            var result = condition.CheckCondition(interactor, this, interactionContext);
            if (result)
            {
                foreach (var handler in handlers)
                {
                    handler.DoInteraction(interactor,this,interactionContext);
                }

                foreach (var handler in handlersOnThisItem)
                {
                    handler.DoInteraction(interactor,this,interactionContext);
                }
                if (_itemDisappearsWhenUsed) Destroy(gameObject,0.5f);
            }
            return result;
        }
        #endregion
    }
    
    
    
    
}