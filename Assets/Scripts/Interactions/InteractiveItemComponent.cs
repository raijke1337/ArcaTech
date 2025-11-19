using System;
using Arcatech.Actions;
using System.Collections.Generic;
using Arcatech.Managers;
using Arcatech.Units;
using UnityEngine;
using KBCore.Refs;
using UnityEngine.EventSystems;

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

        [SerializeField] private string interactTooltipText = "Interact";

        [Space, Header("Condition checker")]
        [SerializeField]
        protected InteractionCondition condition;
        
        
        public bool TryInteraction(IInteractor interactor)
        {
            var result = condition.CheckCondition(interactor, this);
            
            foreach (var handler in handlers)
            {
                handler.DoInteraction(result,interactor,this);
            }

            foreach (var handler in handlersOnThisItem)
            {
                handler.DoInteraction(result,interactor,this);
            }

            
            if (result)
            {

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

        public string InteractionText => interactTooltipText;

        #endregion

        public void OnPointerEnter(PointerEventData eventData)
        {
            GameInterfaceManager.Instance?.NotifyTargetable(this,true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            GameInterfaceManager.Instance?.NotifyTargetable(this,false);
        }

        public Side Side => GetBaseComponent.GetEntitySide;
        public string TargetName =>  GetBaseComponent.GetName;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            if (handlers != null)
            {
                foreach (var handler in handlers)
                {
                    Gizmos.DrawLine(transform.position,handler.transform.position);
                }
            }

            if (handlersOnThisItem != null)
            {
                Gizmos.DrawWireCube(this.transform.position, this.transform.localScale);
            }
        }
    }
    
}