using System;
using System.Linq;
using Arcatech.Items;
using Arcatech.Units;
using Arcatech.Units.Control;
using KBCore.Refs;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Arcatech.Interactions
{
    [RequireComponent(typeof(PlayerAimingComponent), typeof(BaseGameEntityComponent))]
    public class InteractionComponent : ValidatedMonoBehaviour, IInteractor, IUnitCommandValidator,
        IUnitCommandPerformer
    {
        [SerializeField, Self] private BaseGameEntityComponent _base;
        [SerializeField, ReadOnlyText] private string _itemLoaded;
        
        [SerializeField, Tooltip("effects spawn here")]
        private Transform interactionActionTransform;
        private InteractionContext _context;

        private void OnEnable()
        {
            _context = new InteractionContext(_base, interactionActionTransform);
        }
        public InteractionContext InteractionContext => _context;

        public void RegisterInteractiveItemInContext(IInteractive item)
        {
            _context.CurrentInteractive = item;
            _itemLoaded = item.GetBaseComponent.GetName;
        }

        public void UnregisterInteractiveItemFromContext(IInteractive item)
        {
            if  (_context.CurrentInteractive == item) _context.CurrentInteractive = null;
        }

        public bool CanDoUnitCommand(UnitActionType type, out string info)
        {
            info = "OK";
            switch (type)
            {
                case UnitActionType.Use:
                    info += $"{(_context.CurrentInteractive == null ? "No item" : "Has item")}";
                    return _context.CurrentInteractive != null;
            }

            return true;
        }

        public void PrepareCommand(UnitActionType type)
        {
          //  if (type == UnitActionType.Use)
             //   Debug.Log($"PrepareCommand {type}");
        }

        public void DoUnitCommand(UnitActionType type, bool wasSuccessful)
        {
            if (type == UnitActionType.Use)
            {
                if (CanDoUnitCommand(type, out _))
                {
                  //  Debug.Log($"Trying interaction and updating result");
                    InteractionContext.UpdateInteractionResult(_context.CurrentInteractive.TryInteraction(this));
                }
            }
        }

    }
}
