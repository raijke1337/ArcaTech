using System;
using UnityEngine;

namespace Arcatech.Interactions
{
   // [CreateAssetMenu(fileName = "New dummy condition",menuName = "Interactions/Condition/Dummy")]
    public class DummyInteractionCondition : InteractionCondition
    {
        [SerializeField] private bool _result;

        public override bool Check(InteractionContext ctx)
        {
            return _result; 
        }
    }
}