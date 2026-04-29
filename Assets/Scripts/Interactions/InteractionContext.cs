using System;
using Arcatech.Units;
using NUnit.Framework;
using UnityEngine;

namespace Arcatech.Interactions
{
    [Serializable]
    public class InteractionContext
    {
        public Transform ActionTransform { get; }
        public BaseGameEntityComponent EntityComponent { get; }
        public IInteractive CurrentInteractive { get; set; }

        private int _resultVersion;
        private int _consumedVersion;
        
        private bool _pendingResult;
        
        public void UpdateInteractionResult(bool success)
        {
            _pendingResult = success;
            _resultVersion++;
        }

        public bool HasInteractionResult(out bool success)
        {
            if (_resultVersion == _consumedVersion)
            {
                success = false;
                return false;
            }

            success = _pendingResult;
            return true;
        }

        public bool ConsumeInteractionResult(out bool success)
        {
            if (_resultVersion == _consumedVersion)
            {
                success = false;
                return false;
            }

            success = _pendingResult;
            _consumedVersion = _resultVersion;
            return true;
        }
        public InteractionContext (BaseGameEntityComponent comp, Transform actionTransform)
        {
            Assert.IsNotNull(comp);
            EntityComponent = comp;
            ActionTransform = actionTransform;
        }
    }
}