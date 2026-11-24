using System;
using System.Collections;
using UnityEngine;

namespace Arcatech.Interactions
{
    public sealed class EffectsSequence : InteractionEffect
    {
        [Serializable]
        private sealed class Step
        {
            public InteractionEffect Effect = null;
            [Min(0f)] public float DelayBefore = 0f;
            [Min(0f)] public float DelayAfter = 0f;
        }

        [SerializeField] private Step[] steps = Array.Empty<Step>();

        private Coroutine _coroutine;
        private InteractionEffect _activeEffect;
        private bool _isPlaying;
        private int _runId;

        public override bool IsBlocking => true;
        public override bool IsBlockingComplete => !_isPlaying;

        public override void Play(InteractionContext ctx)
        {
            if (_isPlaying || !isActiveAndEnabled || ctx == null || steps.Length == 0)
                return;

            // The pipeline can advance to another state while a delayed step is pending.
            var sequenceContext = new InteractionContext
            {
                Interactor = ctx.Interactor,
                Target = ctx.Target,
                State = ctx.State
            };

            _isPlaying = true;
            int runId = ++_runId;
            var coroutine = StartCoroutine(PlaySteps(sequenceContext, runId));
            if (_isPlaying && runId == _runId)
                _coroutine = coroutine;
        }

        private IEnumerator PlaySteps(InteractionContext ctx, int runId)
        {
            try
            {
                foreach (var step in steps)
                {
                    var effect = step?.Effect;
                    if (effect == null || effect == this)
                        continue;

                    if (step.DelayBefore > 0f)
                        yield return new WaitForSeconds(step.DelayBefore);

                    if (effect == null)
                        continue;

                    _activeEffect = effect;
                    effect.Play(ctx);

                    // Play can synchronously cancel the sequence, for example by disabling its object.
                    if (runId != _runId)
                        yield break;

                    while (effect != null && effect.IsBlocking && !effect.IsBlockingComplete)
                        yield return null;

                    _activeEffect = null;
                    if (step.DelayAfter > 0f)
                        yield return new WaitForSeconds(step.DelayAfter);
                }
            }
            finally
            {
                if (runId == _runId)
                {
                    _isPlaying = false;
                    _coroutine = null;
                    _activeEffect = null;
                }
            }
        }

        public override void OnCancelled()
        {
            if (!_isPlaying)
                return;

            ++_runId;
            _isPlaying = false;
            var coroutine = _coroutine;
            _coroutine = null;
            var effect = _activeEffect;
            _activeEffect = null;

            if (coroutine != null)
                StopCoroutine(coroutine);

            if (effect != null)
                effect.OnCancelled();
        }

        private void OnDisable() => OnCancelled();
    }
}
