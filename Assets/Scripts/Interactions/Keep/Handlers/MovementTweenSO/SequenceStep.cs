using UnityEngine;
namespace Arcatech
{
    [System.Serializable]
    public class SequenceStep
    {

        public enum StepType
        {
            Append,      // Play after previous tween
            Join,        // Play with previous tween
            Insert,      // Insert at specific time
            AppendInterval,  // Add delay
            AppendCallback   // Add callback
        }



        [Header("Step Configuration")]
        public StepType stepType = StepType.Append;
        public SerializedDOTweener Action;

        [Header("Insert Settings")]
        [Tooltip("Only used when StepType is Insert")]
        public float insertTime = 0f;

        [Header("Interval Settings")]
        [Tooltip("Only used when StepType is AppendInterval")]
        public float intervalDuration = 1f;

        [Header("Target Settings")]
        public bool useTargetOverride = false;
        public string targetGameObjectName;  // Find by name if specified


        [Header("Callback")]
        public bool hasCallback = false;
        public UnityEngine.Events.UnityEvent onStepComplete;
    }
}
