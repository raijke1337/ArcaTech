using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Usables.Effects
{
    public class ModifierAggregator : MonoBehaviour, IModifierAggregator
    {
        private struct StackEntry
        {
            public EffectKey             key;
            public ModifierParam         param;
            public float                 multiplier;
            public ModifierStackCounting counting;
            public int                   maxStacks;
        }

        private readonly List<StackEntry> _stacks = new();

        /// <summary>
        /// Tries to add a stack. Returns false if the cap is already reached
        /// (caller should treat the effect as Rejected).
        /// </summary>
        public bool AddStack(ModifierParam       param,
                             EffectKey            key,
                             float                multiplier,
                             ModifierStackCounting counting,
                             int                  maxStacks)
        {
            int current = counting == ModifierStackCounting.PerSource
                ? CountStacks(param, key)                      // same effect + same source
                : CountStacksByEffectId(param, key.EffectId);  // same effect, any source

            if (maxStacks > 0 && current >= maxStacks)
                return false;   // cap reached — Reject

            _stacks.Add(new StackEntry
            {
                key        = key,
                param      = param,
                multiplier = multiplier,
                counting   = counting,
                maxStacks  = maxStacks
            });
            return true;
        }

        public void RemoveStacks(EffectKey key)
        {
            for (int i = _stacks.Count - 1; i >= 0; i--)
                if (_stacks[i].key.Equals(key))
                    _stacks.RemoveAt(i);
        }

        public float GetMultiplier(ModifierParam param)
        {
            float product = 1f;
            for (int i = 0; i < _stacks.Count; i++)
                if (_stacks[i].param == param)
                    product *= _stacks[i].multiplier;
            return product;
        }

        public int CountStacks(ModifierParam param, EffectKey key)
        {
            int n = 0;
            for (int i = 0; i < _stacks.Count; i++)
                if (_stacks[i].param == param && _stacks[i].key.Equals(key)) n++;
            return n;
        }

        public int CountStacksByEffectId(ModifierParam param, string effectId)
        {
            int n = 0;
            for (int i = 0; i < _stacks.Count; i++)
                if (_stacks[i].param == param &&
                    string.Equals(_stacks[i].key.EffectId, effectId,
                                  System.StringComparison.Ordinal)) n++;
            return n;
        }
    }
}