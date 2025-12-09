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
        
        private InteractionContext ReadContext()
        {
            if (_context == null) _context = new InteractionContext(_base, interactionActionTransform);
            return _context;
        }
        public InteractionContext InteractionContext => ReadContext();

        public void RegisterInteractiveItem(IInteractive item)
        {
            ReadContext().CurrentInteractive = item;
            _itemLoaded = item.GetBaseComponent.GetName;
        }

        public void UnregisterInteractiveItem(IInteractive item)
        {
            if  (ReadContext().CurrentInteractive == item) _context.CurrentInteractive = null;
        }

        public bool CanDoUnitCommand(UnitActionType type, out string info)
        {
            info = "Interaction comp: ";
            switch (type)
            {
                case UnitActionType.Use:
                    info += $"{(ReadContext().CurrentInteractive == null ? "No item" : "Has item")}";
                    return ReadContext().CurrentInteractive != null;
                
                    default:
                    info += "default OK"; 
                        return true;
            }
        }

        public void PrepareCommand(UnitActionType type)
        {
            if (type == UnitActionType.Use)
                Debug.Log($"PrepareCommand {type}");
        }

        public bool DoUnitCommand(UnitActionType type, bool wasSuccessful)
        {
            if (type == UnitActionType.Use)
            {
                if (!wasSuccessful)
                {
                    Debug.Log($"DoUnitCommand {type} with result fail");
                    return false;
                }
                if (CanDoUnitCommand(type, out _))
                {
                    Debug.Log($"Trying interaction and updating result");
                    InteractionContext.UpdateInteractionResult(ReadContext().CurrentInteractive.TryInteraction(this));
                }

                return false;
            }

            return true;
        }

    }
}
