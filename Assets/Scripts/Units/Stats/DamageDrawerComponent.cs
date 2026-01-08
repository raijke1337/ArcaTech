using System.Collections;
using Arcatech.Managers;
using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Stats
{

    [RequireComponent(typeof(BaseGameEntityComponent))]
    public class DamageDrawerComponent : ValidatedMonoBehaviour, IAppliedEffectsTakerComponent<AppliedStatsDeltaEffect>, IPausableComponent
    {
        [SerializeField, Self] private BaseGameEntityComponent entity;

        [Header("DoT/HoT Display")]
        [SerializeField] private float dotsDisplayInterval = 0.5f;

        public bool Paused { get; set; }
        
        
        public bool ApplyEffect(AppliedStatsDeltaEffect effect, BaseGameEntityComponent source)
        {
            if (Paused) return false;

            // Instant deltas
            if (effect.instantDeltas != null && effect.instantDeltas.Count > 0)
            {
                foreach (var delta in effect.instantDeltas)
                {
                    if (delta.stat != ResourceStatType.Health) return true;

                    if (delta.amount < 0)
                    {
                        ShowNumber(Mathf.Abs(delta.amount), true, null);
                    }
                    else if (delta.amount > 0)
                    {
                        ShowNumber(delta.amount, false, null);
                    }
                }
            }

            // Periodic deltas (visualization only)
            if (effect.periodicDeltas != null && effect.periodicDeltas.Count > 0)
            {
                foreach (var periodicDelta in effect.periodicDeltas)
                {
                    if (periodicDelta.delta.stat!= ResourceStatType.Health) return true;
                    if (periodicDelta.intervalSeconds <= 0) continue;

                    bool isDamage = periodicDelta.delta.amount < 0;
                    float totalDuration = effect.infiniteDuration ? float.MaxValue : effect.durationSeconds;

                    if (totalDuration == float.MaxValue)
                    {
                        Debug.LogWarning("DamageDrawerComponent: Infinite duration effect - capping DoT visualization at 30s.");
                        totalDuration = 30f;
                    }

                    int numTicks = Mathf.FloorToInt(totalDuration / periodicDelta.intervalSeconds);
                    float totalAmount = Mathf.Abs(periodicDelta.delta.amount) * numTicks;

                    if (totalAmount > 0f)
                    {
                        StartCoroutine(DisplayPeriodicRoutine(
                            totalAmount,
                            totalDuration,
                            dotsDisplayInterval,
                            isDamage
                        ));
                    }
                }
            }
            
            return true;

        }

        private IEnumerator DisplayPeriodicRoutine(float totalAmount, float effectDuration, float displayInterval, bool isDamage)
        {
            float shownSoFar = 0f;
            float elapsed = 0f;

            while (elapsed < effectDuration && shownSoFar < totalAmount)
            {
                yield return new WaitForSeconds(displayInterval);
                elapsed += displayInterval;

                float targetByNow = (elapsed / effectDuration) * totalAmount;
                float chunk = targetByNow - shownSoFar;

                if (chunk > 0.1f)
                {
                    // Do not request any display when paused; still consume the chunk
                    ShowNumber(chunk, isDamage, Mathf.Max(0.6f, displayInterval * 1.5f));
                    shownSoFar += chunk;
                }
                else if (elapsed >= effectDuration - 0.01f && shownSoFar < totalAmount - 0.1f)
                {
                    chunk = totalAmount - shownSoFar;
                    if (chunk > 0.1f)
                    {
                        ShowNumber(chunk, isDamage, Mathf.Max(0.6f, displayInterval * 1.5f));
                        shownSoFar += chunk;
                    }
                }
            }
        }

        private void ShowNumber(float amount, bool isDamage, float? durationOverride)
        {
            if (Paused) return;
            if (amount <= 0f || entity == null || entity.EffectSpawn == null) return;

            if (GameInterfaceManager.Instance == null)
            {
                Debug.LogWarning("DamageDrawerComponent: GameInterfaceManager.Instance not available.");
                return;
            }

            GameInterfaceManager.Instance.ShowFloatingNumber(
                amount,
                entity.EffectSpawn.position,
                isDamage,
                durationOverride
            );
        }
    }
}