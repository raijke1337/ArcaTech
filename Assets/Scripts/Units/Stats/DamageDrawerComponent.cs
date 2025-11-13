using System;
using Arcatech.Units;
using CartoonFX;
using UnityEngine;

namespace Arcatech.Stats
{
    public class DamageDrawerComponent : MonoBehaviour, IEffectsTakerComponent
    {

        [SerializeField] private CFXR_ParticleText textPrefab;

        private CFXR_ParticleText instance;

        private void Start()
        {
            instance = Instantiate<CFXR_ParticleText>(textPrefab, transform);
            instance.gameObject.SetActive(true);
        }

        public void ApplyEffect(StatsEffect effect, BaseGameEntityComponent source)
        {
            instance.UpdateText(effect.instantDeltas[0].amount.ToString());
        }
    }
}