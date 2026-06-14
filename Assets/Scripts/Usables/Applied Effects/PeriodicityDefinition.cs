using System;
using Arcatech.Usables.Effects;
using UnityEngine;

[Serializable]
public struct PeriodicityDefinition
{
    public PeriodicityKind kind;

    // OneShot
    public OneShotMoment oneShotMoment;

    // Repeating
    [Tooltip("Total applications over the duration. Integer > 0.")]
    public int ticks;
    public IntervalMode intervalMode;
    [Tooltip("Delay from effect application until ticking begins.")]
    public float offsetSeconds;
}