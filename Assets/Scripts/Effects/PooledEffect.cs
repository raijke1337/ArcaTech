using System.Collections;
using Arcatech.Managers;
using CartoonFX;
using UnityEngine;

namespace Arcatech.Effects
{
    [DisallowMultipleComponent]
    public class PooledEffect : MonoBehaviour
    {
        internal EffectsManager owner;
        internal CFXR_Effect prefabKey;

        ParticleSystem[] systems;
        bool anyLooping;

        void Awake()
        {
            // Cache once
            systems = GetComponentsInChildren<ParticleSystem>(true);
            anyLooping = false;
            for (int i = 0; i < systems.Length; i++)
            {
                var main = systems[i].main;
                if (main.loop) anyLooping = true;

                // Ensure we get callbacks when systems stop
                var m = systems[i].main;
                m.stopAction = ParticleSystemStopAction.Callback;
            }
        }

        // Called by manager before playing
        internal void PrepareForPlay()
        {
            // Reset all particle systems cleanly
            for (int i = 0; i < systems.Length; i++)
            {
                systems[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                systems[i].Clear(true);
                // Optional (slightly more expensive): systems[i].Simulate(0f, true, true, true);
            }
        }

        internal void PlayNow()
        {
            for (int i = 0; i < systems.Length; i++)
                systems[i].Play(true);

            // If the effect is non-looping, we can auto-return when done
            StopAllCoroutines();
            StartCoroutine(ReturnWhenComplete());
        }

        IEnumerator ReturnWhenComplete()
        {
            // For looping effects, you’ll need to Stop() manually via handle, but we still
            // return when explicitly stopped or parent disabled.
            if (anyLooping)
                yield break;

            // Wait until all systems are dead
            bool alive;
            do
            {
                alive = false;
                for (int i = 0; i < systems.Length; i++)
                {
                    if (systems[i] != null && systems[i].IsAlive(true))
                    {
                        alive = true;
                        break;
                    }
                }
                if (alive) yield return null;
            } while (alive);

            ReturnToPool();
        }

        public void StopAndReturn()
        {
            for (int i = 0; i < systems.Length; i++)
                systems[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ReturnToPool();
        }

        void OnDisable()
        {
            // If something disables the object (scene unload, parent disabled), return it.
            if (owner != null && gameObject.activeInHierarchy == false)
            {
                ReturnToPool();
            }
        }

        void OnParticleSystemStopped()
        {
            // For non-looping effects, the coroutine handles return.
            // For looping, if all are stopped, we can return immediately.
            if (!anyLooping)
                return;

            bool anyAlive = false;
            for (int i = 0; i < systems.Length; i++)
            {
                if (systems[i] != null && systems[i].IsAlive(true))
                {
                    anyAlive = true;
                    break;
                }
            }
            if (!anyAlive)
                ReturnToPool();
        }

        void ReturnToPool()
        {
            if (owner != null)
                owner.Return(this);
        }
    }
    
}