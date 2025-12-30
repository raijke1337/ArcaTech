using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
namespace Arcatech
{
    [CreateAssetMenu(fileName = "tweenSO_sequence_", menuName = "Tweening/Sequence Preset")]
    public class SerializedDOTweenSequence : SerializedDOTweener
    {
        [Header("Sequence Settings")]
        public string sequenceName = "New Sequence";
        public UpdateType updateType = UpdateType.Normal;
        public bool ignoreTimeScale = false;
        public bool autoKill = true;
        public bool autoPlay = true;

        [Header("Loop Settings")]
        public int loops = 0;  // -1 for infinite
        public LoopType loopType = LoopType.Restart;

        [Header("Sequence Steps")]
        public List<SequenceStep> steps = new List<SequenceStep>();

        [Header("Global Callbacks")]
        public bool useOnComplete = false;
        public UnityEngine.Events.UnityEvent onSequenceComplete;
        public bool useOnStart = false;
        public UnityEngine.Events.UnityEvent onSequenceStart;

        protected override Tween Build(Transform defaultTarget)
        {
            Sequence sequence = DOTween.Sequence();

            // Configure sequence settings
            sequence.SetUpdate(updateType, ignoreTimeScale)
                    .SetAutoKill(autoKill)
                    .SetLoops(loops, loopType);

            // Add global callbacks
            if (useOnStart && onSequenceStart != null)
            {
                sequence.OnStart(() => onSequenceStart.Invoke());
            }

            if (useOnComplete && onSequenceComplete != null)
            {
                sequence.OnComplete(() => onSequenceComplete.Invoke());
            }

            // Build the sequence
            foreach (var step in steps)
            {
                ProcessStep(sequence, step, defaultTarget);
            }

            if (!autoPlay)
            {
                sequence.Pause();
            }

            return sequence;
        }

        private void ProcessStep(Sequence sequence, SequenceStep step, Transform defaultTarget)
        {
            // Determine target
            Transform target = defaultTarget;
            Tween t;
            //if (step.useTargetOverride && !string.IsNullOrEmpty(step.targetGameObjectName))
            //{
            //    GameObject foundObject = GameObject.Find(step.targetGameObjectName);
            //    if (foundObject != null)
            //    {
            //        target = foundObject.transform;
            //    }
            //}

            // Handle different step types
            switch (step.stepType)
            {
                case SequenceStep.StepType.AppendInterval:
                    sequence.AppendInterval(step.intervalDuration);
                    break;

                case SequenceStep.StepType.AppendCallback:
                    if (step.hasCallback && step.onStepComplete != null)
                    {
                        sequence.AppendCallback(() => step.onStepComplete.Invoke());
                    }
                    break;

                default:
                    t = step.Action.GetTween(target);
                    if (t != null)
                    {
                        AddTweenToSequence(sequence, t, step);
                    }
                    //if (step.tweenAction != SequenceStep.TweenAction.UsePreset)
                    //{
                    //    if (step.useCustomCurve)
                    //    {
                    //        tween.SetEase(step.customCurve);
                    //    }
                    //    else
                    //    {
                    //        tween.SetEase(step.easeType);
                    //    }

                    //    if (step.hasCallback && step.onStepComplete != null)
                    //    {
                    //        tween.OnComplete(() => step.onStepComplete.Invoke());
                    //    }
                    //}
                    break;
            }

        }


        private void AddTweenToSequence(Sequence sequence, Tween tween, SequenceStep step)
        {
            switch (step.stepType)
            {
                case SequenceStep.StepType.Append:
                    sequence.Append(tween);
                    break;

                case SequenceStep.StepType.Join:
                    sequence.Join(tween);
                    break;

                case SequenceStep.StepType.Insert:
                    sequence.Insert(step.insertTime, tween);
                    break;
            }
        }
    }
}