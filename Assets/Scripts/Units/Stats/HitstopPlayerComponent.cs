using System;
using Arcatech.Managers;
using Arcatech.Usables.Effects;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Stats
{
    [DisallowMultipleComponent]
    public class HitstopPlayerComponent : ValidatedMonoBehaviour, IStatReceiver
    {
        public bool Invulnerable { get; set; }
        public bool ApplyInstantDelta(StatDelta delta, BaseGameEntityComponent source, EffectKey key)
        {
            if (!DamagePipeline.HitStopEnabled) return false;
            if (source.CompareTag("Player") && delta.stat == ResourceStatType.Health && delta.amount < 0)
            {
                HitstopPlayer.Instance.PlayHitstop(0.3f,0.2f);
                return true;
            }
            return false;
        }
    }
}