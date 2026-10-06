using System;
using System.Collections.Generic;
using Arcatech.Interactions;
using Arcatech.Managers;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Arcatech.Items
{
    public class LightControlEffect : InteractionEffect
    {
        [SerializeField] private Color startColor;
        [SerializeField] private SerializedDictionary<InteractionState, Color> colors;
        private Light light;

        private void Awake()
        {
            light = GetComponentInChildren<Light>();
            light.color =  startColor;
        }

        public override void Play(InteractionContext ctx)
        {
            if (colors.TryGetValue(ctx.State, out  var color))
            {
                light.color = color;
            }
        }
    }
}