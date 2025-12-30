using Arcatech.Actions;
using UnityEditorInternal;
using UnityEngine;

namespace Arcatech.Units
{
    [CreateAssetMenu(fileName = "transitionTo_",menuName = "States/State Transition")]
    public class SerializedStateTransition : ScriptableObject
    {
        public int Priority;
       // [Header("Command transitions have higher priority than non-command ones")]
      //  public bool IsCommandTransition;
        public SerializedStateTransitionCondition[] conditions = new SerializedStateTransitionCondition[0];
        public SerializedUnitState nextState;
        public SerializedActionResult[] onTransition = new SerializedActionResult[0];
        

        [Header("The transition will not be valid unless this time has passed in source state")]
        [Range(0f, 1f)]
        public float minTimeInSourceStateNormalized = 1f;
        [Header("Override source state's minimum required time")]
        public bool overrideMinTime = false;


        public StateTransition Build()
        {
            return new StateTransition(nextState.Build(), 
                onTransition, 
                minTimeInSourceStateNormalized, 
                conditions,Priority,
                overrideMinTime);
        }
    }
}