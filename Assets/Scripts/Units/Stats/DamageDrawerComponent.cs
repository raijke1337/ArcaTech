using System.Collections; // Needed for Coroutines
using System.Collections.Generic;
using Arcatech.Units;
using CartoonFX;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Stats
{
    [RequireComponent(typeof(BaseGameEntityComponent))]
    public class DamageDrawerComponent : ValidatedMonoBehaviour, IEffectsTakerComponent
    {

        [SerializeField, Self] private BaseGameEntityComponent entity;
        [SerializeField] private CFXR_ParticleText textPrefab;
        [SerializeField] private int poolSize = 10; // Initial size of the object pool
        [SerializeField] private float damageOffsetMagnitude = 0.75f; // How far damage numbers can randomly spread
        [SerializeField] private float dotsDisplayInterval = 0.5f; // How often to display a "chunk" of DoT damage

        private Queue<CFXR_ParticleText> textPool = new Queue<CFXR_ParticleText>();

        
        private Camera mainCamera; // Cache the main camera

        private void Start()
        {
            InitializePool();
            // Cache Camera.main once.
            if (Camera.main == null)
            {
                Debug.LogError("No main camera found in the scene. Damage numbers might not face the viewer correctly.");
            }
            mainCamera = Camera.main; 
        }
        private void InitializePool()
        {
            for (int i = 0; i < poolSize; i++)
            {
                CreateNewPooledText();
            }
        }

        private CFXR_ParticleText CreateNewPooledText()
        {
            CFXR_ParticleText newInstance = Instantiate(textPrefab, transform);
            newInstance.isDynamic = true;
            newInstance.gameObject.SetActive(false); // Start inactive
            textPool.Enqueue(newInstance);
            return newInstance;
        }

        private CFXR_ParticleText GetPooledText()
        {
            if (textPool.Count > 0)
            {
                CFXR_ParticleText instance = textPool.Dequeue();
                instance.gameObject.SetActive(true); // Activate before use
                return instance;
            }
            else
            {
                // If the pool is empty, create a new one (expands the pool)
                Debug.LogWarning("Damage particle text pool exhausted. Expanding pool.");
                return CreateNewPooledText();
            }
        }

        private void ReturnPooledText(CFXR_ParticleText instance)
        {
            // Reset any relevant state before returning to pool
            instance.gameObject.SetActive(false); // Deactivate after use
            textPool.Enqueue(instance);
        }

        public void ApplyEffect(UsableEffect effect, BaseGameEntityComponent source)
        {
            // --- Handle Instant Deltas ---
            if (effect.instantDeltas != null && effect.instantDeltas.Count > 0)
            {
                foreach (var delta in effect.instantDeltas)
                {
                    // Only consider negative deltas as damage for display
                    if (delta.amount < 0)
                    {
                        ShowDamageNumber(Mathf.Abs(delta.amount),1f);
                    }
                }
            }

            // --- Handle Periodic Deltas (Damage Over Time) ---
            if (effect.periodicDeltas != null && effect.periodicDeltas.Count > 0)
            {
                // We only simulate the *visual* display here.
                // The actual application of damage to the entity's stats
                // still needs to be handled by the game logic (e.g., in a StatEffectManager).

                foreach (var periodicDelta in effect.periodicDeltas)
                {
                    // We are interested in "damage" here, so look for negative amounts
                    if (periodicDelta.delta.amount < 0 && periodicDelta.intervalSeconds > 0)
                    {
                        // Calculate total damage this periodic delta will deal over its duration
                        float totalDuration = effect.infiniteDuration ? float.MaxValue : effect.durationSeconds;

                        if (totalDuration == float.MaxValue)
                        {
                            Debug.LogWarning("Cannot calculate total damage for infinite duration effect. " +
                                             "Will simulate periodic damage for a limited time based on dotsDisplayInterval. " +
                                             "Consider caps or different handling for infinite DoTs.");
                            // For infinite duration, we need an arbitrary end point for visualization
                            // or just let the coroutine run indefinitely until stopped externally.
                            // For now, let's just make sure it doesn't try to divide by infinite ticks.
                            totalDuration = 30f; // Arbitrary cap for visualization if infinite
                        }


                        int numTicks = Mathf.FloorToInt(totalDuration / periodicDelta.intervalSeconds);
                        float totalPeriodicDamage = Mathf.Abs(periodicDelta.delta.amount) * numTicks;

                        // Start a coroutine to display this total periodic damage in chunks
                        if (totalPeriodicDamage > 0)
                        {
                            StartCoroutine(DisplayPeriodicDamageRoutine(
                                totalPeriodicDamage,
                                totalDuration, // Use the actual duration of the effect
                                dotsDisplayInterval,
                                periodicDelta.delta.amount < 0 // true if damage, false if healing
                            ));
                        }
                    }
                }
            }
        }

        // Coroutine to display periodic damage in chunks
        private IEnumerator DisplayPeriodicDamageRoutine(
            float totalDamageAmount,
            float effectDuration,
            float displayInterval,
            bool isDamage)
        {
            float damageAlreadyDisplayed = 0f;
            float timeElapsed = 0f;

            while (timeElapsed < effectDuration && damageAlreadyDisplayed < totalDamageAmount)
            {
                yield return new WaitForSeconds(displayInterval);
                timeElapsed += displayInterval;

                // Calculate how much damage should have been *visually* displayed by now
                // and how much to show in this chunk.
                float targetDamageToDisplay = (timeElapsed / effectDuration) * totalDamageAmount;
                float chunkDamage = targetDamageToDisplay - damageAlreadyDisplayed;

                // Ensure we don't display negative or tiny amounts due to float precision
                if (chunkDamage > 0.1f) // Display if the chunk is substantial
                {
                    ShowDamageNumber(chunkDamage, displayInterval * 2f, isDamage); // Give it a short lifecycle
                    damageAlreadyDisplayed += chunkDamage;
                }
                else if (timeElapsed >= effectDuration - 0.01f && damageAlreadyDisplayed < totalDamageAmount - 0.1f)
                {
                    // Handle any remaining tiny damage at the very end
                    chunkDamage = totalDamageAmount - damageAlreadyDisplayed;
                    if (chunkDamage > 0.1f)
                    {
                        ShowDamageNumber(chunkDamage, displayInterval * 2f, isDamage);
                        damageAlreadyDisplayed += chunkDamage;

                    }
                }
            }
        }


        private void ShowDamageNumber(float amount, float displayDuration, bool isDamage = true)
        {
            if (amount <= 0) return; // Don't show zero or negative damage numbers visually

            CFXR_ParticleText instance = GetPooledText();

            string displayString = isDamage ? amount.ToString("F0") : "+" + amount.ToString("F0");
            instance.UpdateText(displayString);

            // Add a random offset for better visual distinction
            Vector3 randomOffset = new Vector3(
                Random.Range(-damageOffsetMagnitude, damageOffsetMagnitude),
                Random.Range(0f, damageOffsetMagnitude * 2f), // Slightly more upwards bias
                0f
            );
            instance.transform.position = entity.EffectSpawn.position + randomOffset;

            Vector3 directionToCamera = (mainCamera.transform.position - instance.transform.position).normalized;
            instance.transform.position += directionToCamera; // Adjust 0.1f as needed

            ParticleSystem particles = instance.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); // Ensure it's reset
                particles.Play();
            }

            // Use a Coroutine to return the instance to the pool after its duration
            StartCoroutine(ReturnTextAfterDelay(instance, displayDuration));
        }

        private IEnumerator ReturnTextAfterDelay(CFXR_ParticleText instance, float delay)
        {
            yield return new WaitForSeconds(delay);
            ReturnPooledText(instance);
        }
    }
}