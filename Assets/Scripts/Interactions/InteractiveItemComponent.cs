using System;
using Arcatech.Actions;
using System.Collections.Generic;
using Arcatech.Units;
using UnityEngine;
using KBCore.Refs;

namespace Arcatech.Interactions
{
    [RequireComponent(typeof(BaseGameEntityComponent))]
    [RequireComponent(typeof(EntityMouseOverGlowComponent))]
    public class InteractiveItemComponent : ValidatedMonoBehaviour, IInteractive
    {

        [SerializeField] private bool _itemDisappearsWhenUsed;
        
        [Space,SerializeField, Self] private BaseGameEntityComponent baseComp;
        [SerializeField, Self] private EntityMouseOverGlowComponent entityMouseOver;
        [Space]
        [SerializeField] private List<InteractionHandlerBase> handlersOnThisItem;
        [SerializeField] private List<InteractionHandlerBase> handlers;
        
        public BaseGameEntityComponent GetBaseComponent => baseComp;
        private List<IKillableComponent> killableComponents;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            handlersOnThisItem = new  List<InteractionHandlerBase>(GetComponentsInChildren<InteractionHandlerBase>());
            killableComponents =  new  List<IKillableComponent>(GetComponentsInChildren<IKillableComponent>());
        }

        #region interaction

        [Space, Header("Condition checker")]
        //[SerializeField] SerializedDictionary<EventCondition, InteractionHandlerBase[]> _list;
        [SerializeField]
        protected InteractionCondition condition;
        
        public bool OnInteraction(IInteractor interactor)
        {
            var result = condition.CheckCondition(interactor, this);
            if (result)
            {
                foreach (var handler in handlers)
                {
                    handler.DoInteraction(interactor,this);
                }

                foreach (var handler in handlersOnThisItem)
                {
                    handler.DoInteraction(interactor,this);
                }

                if (_itemDisappearsWhenUsed)
                {
                    foreach (var killable in killableComponents)
                    {
                        killable.Killed = true;
                    }
                }
            }
            return result;
        }
        #endregion
    }
    
    
    
    
}