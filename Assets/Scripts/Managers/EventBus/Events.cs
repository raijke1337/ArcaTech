﻿using Arcatech.Items;
using Arcatech.Skills;
using Arcatech.Stats;
using Arcatech.Triggers;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.EventBus
{
    public interface IEvent { }
    public struct StatChangedEvent : IEvent
    {
        public BaseStatType StatType { get; }
        public StatValueContainer Container { get; }
        public StatChangedEvent (BaseStatType statType, StatValueContainer container)
        { 
        StatType = statType; Container = container;
        }
    }

    public struct DrawDamageEvent : IEvent
    {
        public BaseUnit Unit { get; }
        public float Damage { get; }
        public DrawDamageEvent (BaseUnit unit, float damage)
        {
            Unit = unit; this.Damage = damage;
        }
    }

    public struct StatsEffectTriggerEvent : IEvent
    {
        public StatsEffectTriggerEvent(DummyUnit target, DummyUnit source, bool isEnteringTrigger, Transform place, StatsEffect[] appliedEffects)
        {
            Target = target;
            Source = source;
            IsEnteringTrigger = isEnteringTrigger;
            AppliedEffects = appliedEffects;
            Place = place;
        }

        public DummyUnit Target { get; }
        public DummyUnit Source { get; }
        public bool IsEnteringTrigger { get; }
        public StatsEffect[] AppliedEffects { get; }
        public Transform Place { get; }
    }

    public struct SpawnSkillEvent : IEvent
    {
        public ISkill Skill;
        public DummyUnit Caster;
        public Transform Place;
        public SpawnSkillEvent(ISkill skill, DummyUnit caster, Transform place)
        {
            Skill = skill;
            Caster = caster;
            Place = place;
        }
    }


    public struct PlayerPauseEvent : IEvent
    {
        public bool Value { get; }
        public PlayerPauseEvent(bool v) => Value = v;
    }

    public struct IUsableUpdatedEvent : IEvent
    {
        public IUsableUpdatedEvent(IUsable used, DummyUnit user)
        {
            Used = used;
            User = user;
        }
        public IUsable Used { get; }
        public DummyUnit User { get; }

    }



}