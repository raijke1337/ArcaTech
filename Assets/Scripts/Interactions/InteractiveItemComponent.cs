using System;
using Arcatech.Actions;
using System.Collections.Generic;
using Arcatech.Managers;
using Arcatech.Units;
using UnityEngine;
using KBCore.Refs;
using NUnit.Framework;
using UnityEngine.EventSystems;

namespace Arcatech.Interactions
{
    [RequireComponent(typeof(BaseGameEntityComponent))]
    [RequireComponent(typeof(EntityMouseOverGlowComponent))]
    public class InteractiveItemComponent : ValidatedMonoBehaviour, IInteractive
    {

        [SerializeField] private bool _itemDisappearsWhenUsed;
        [SerializeField, UnityEngine.Range(0, 59f)] private float useCooldown;
        private float _cd = 0;
        [SerializeField] private Collider _useAreaCollider;
        
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
            Assert.IsNotNull(_useAreaCollider);
        }

        private void Start()
        {
            _useAreaCollider.isTrigger = true;
        }


        #region interaction

        [SerializeField] private string interactTooltipText = "Interact";

        [Space, Header("Condition checker")]
        [SerializeField]
        protected InteractionCondition condition;

        private void Update()
        {
            _cd = Mathf.Clamp(_cd-Time.deltaTime, 0, useCooldown);
        }

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
                else
                {
                    _cd =  useCooldown;
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
            if (_cd > 0f) Gizmos.color = Color.red;
            if (handlers != null)
            {
                foreach (var handler in handlers)
                {
                    Gizmos.DrawLine(transform.position,handler.transform.position);
                }
            }

            if (handlersOnThisItem != null)
            {
                var bounds = _useAreaCollider.bounds;
                Gizmos.DrawWireCube(_useAreaCollider.transform.position, bounds.size);
            }
        }
    }
    
}