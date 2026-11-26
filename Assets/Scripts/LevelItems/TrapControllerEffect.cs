using System;
using System.Collections;
using Arcatech.Triggers;
using Arcatech.Units;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Arcatech.Interactions
{
    public class TrapControllerEffect : InteractionEffect, IPausableComponent
    {
        [SerializeField] private TriggerTrackerComponent activationArea;
        [SerializeField] private GameObject[] toggledObjects;

        [Space, Header("Timers"), SerializeField]
        [Tooltip("Repeats while the effect is enabled. Steps with a non-positive duration are skipped.")]
        private TrapSequenceStep[] steps = Array.Empty<TrapSequenceStep>();

        [SerializeField, Min(0f)]
        [Tooltip("Seconds to show the danger zone before a timed activation. Zero disables the warning.")]
        private float attackWarningDuration;

        [Space, Header("Controls"), SerializeField]
        [Tooltip("True starts the timer or enables the trap if there are no timed steps. False stops the timer and disables the trap.")]
        private SerializedDictionary<InteractionState, bool> responseStates = new();

        private IEnumerator _sequence;
        private TriggerTrackerComponent _warningArea;
        private bool _timerAllowed = true;
        private int _stateVersion;

        public bool Paused { get; set; }

        private void OnEnable()
        {
            if (_timerAllowed)
                TryStartSequence();
        }

        public override void Play(InteractionContext ctx)
        {
            if (ctx == null)
                return;

            if (responseStates == null || !responseStates.TryGetValue(ctx.State, out bool enableTrap))
            {
                Debug.LogWarning($"{nameof(TrapControllerEffect)} has no response configured for interaction state '{ctx.State}'.", this);
                return;
            }

            if (!isActiveAndEnabled)
                return;

            _timerAllowed = enableTrap;
            if (enableTrap && TryStartSequence())
                return;

            StopSequence();
            SetTrapActive(enableTrap);
        }

        public override void OnCancelled()
        {
            if (_sequence == null)
                return;

            _timerAllowed = false;
            StopTimedTrap();
        }

        private void OnDisable() => StopTimedTrap();

        private bool TryStartSequence()
        {
            if (_sequence != null)
                return true;

            if (steps == null || !Array.Exists(steps, step => step != null && step.duration > 0))
                return false;

            _sequence = PlaySteps(steps);
            StartCoroutine(_sequence);
            return true;
        }

        private IEnumerator PlaySteps(TrapSequenceStep[] sequenceSteps)
        {
            var sequence = _sequence;
            float warningDuration = activationArea != null && float.IsFinite(attackWarningDuration)
                ? Mathf.Max(0f, attackWarningDuration)
                : 0f;
            bool? previousStepEnabled = null;
            float timeUntilActivation = float.PositiveInfinity;
            while (sequence == _sequence)
            {
                bool hasTimedStep = false;
                for (int i = 0; i < sequenceSteps.Length; i++)
                {
                    var step = sequenceSteps[i];
                    if (step == null || step.duration <= 0)
                        continue;

                    while (Paused)
                    {
                        yield return null;
                        if (sequence != _sequence)
                            yield break;
                    }

                    hasTimedStep = true;
                    float remaining = step.duration;
                    bool stepEnabled = step.enabled;
                    if (stepEnabled)
                    {
                        if (previousStepEnabled == null && warningDuration > 0f)
                        {
                            SetTrapActive(false);
                            if (sequence != _sequence)
                                yield break;

                            yield return WaitForStep(warningDuration, 0f, sequence);
                            if (sequence != _sequence)
                                yield break;
                        }

                        HideAttackWarning();
                    }
                    else if (previousStepEnabled != false && warningDuration > 0f)
                    {
                        timeUntilActivation = GetTimeUntilActivation(sequenceSteps, i);
                        float extraTime = Mathf.Max(0f, warningDuration - timeUntilActivation);
                        remaining += extraTime;
                        timeUntilActivation += extraTime;
                    }

                    SetTrapActive(stepEnabled);
                    if (sequence != _sequence)
                        yield break;

                    float warningDelay = stepEnabled ? float.PositiveInfinity : timeUntilActivation - warningDuration;
                    yield return WaitForStep(remaining, warningDelay, sequence);
                    if (sequence != _sequence)
                        yield break;

                    if (!stepEnabled)
                        timeUntilActivation -= remaining;
                    previousStepEnabled = stepEnabled;
                }

                if (!hasTimedStep)
                {
                    _sequence = null;
                    HideAttackWarning();
                    SetTrapActive(false);
                    yield break;
                }
            }
        }

        private IEnumerator WaitForStep(float remaining, float warningDelay, IEnumerator sequence)
        {
            while (remaining > 0f && sequence == _sequence)
            {
                if (!Paused && warningDelay <= 0f && _warningArea == null && activationArea != null)
                {
                    _warningArea = activationArea;
                    _warningArea.ShowAttackWarning(immediate: true);
                }

                yield return null;
                if (!Paused)
                {
                    float elapsed = Time.deltaTime;
                    // A long frame must still leave the configured time to show the warning.
                    if (warningDelay > 0f && warningDelay < remaining)
                        elapsed = Mathf.Min(elapsed, warningDelay);

                    remaining -= elapsed;
                    warningDelay -= elapsed;
                }
            }
        }

        private static float GetTimeUntilActivation(TrapSequenceStep[] sequenceSteps, int startIndex)
        {
            float duration = 0f;
            for (int offset = 0; offset < sequenceSteps.Length; offset++)
            {
                var step = sequenceSteps[(startIndex + offset) % sequenceSteps.Length];
                if (step == null || step.duration <= 0)
                    continue;
                if (step.enabled)
                    return duration;

                duration += step.duration;
            }

            return float.PositiveInfinity;
        }

        private void HideAttackWarning()
        {
            if (_warningArea != null)
                _warningArea.HideAttackWarning(immediate: true);
            _warningArea = null;
        }

        private void StopTimedTrap()
        {
            if (_sequence == null)
                return;

            StopSequence();
            SetTrapActive(false);
        }

        private void StopSequence()
        {
            var sequence = _sequence;
            _sequence = null;
            if (sequence != null)
                StopCoroutine(sequence);
            HideAttackWarning();
        }

        private void SetTrapActive(bool active)
        {
            if (activationArea != null)
                activationArea.Active = active;
            if (active) activationArea.AreaCast();

            int stateVersion = ++_stateVersion;
            if (toggledObjects == null)
                return;

            foreach (var obj in toggledObjects)
            {
                // Keep the controller active so it can run the next step.
                if (obj == null || obj.activeSelf == active || transform.IsChildOf(obj.transform))
                    continue;

                obj.SetActive(active);
                // SetActive can invoke callbacks that change the trap state again.
                if (stateVersion != _stateVersion)
                    return;
            }
        }
    }

    [Serializable]
    internal class TrapSequenceStep
    {
        [Min(0), Tooltip("Duration in gameplay seconds.")] public int duration = 1;
        public bool enabled = true;
    }
}
