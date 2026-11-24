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
        [SerializeField] private Light targetLight;
        [SerializeField] private SerializedDictionary<InteractionState, Color> colors;

        private void Awake()
        {
            targetLight = GetComponentInChildren<Light>();
            if (targetLight == null)
            {
                targetLight = gameObject.AddComponent<Light>();
            }
        }

        public override void Play(InteractionContext ctx)
        {
            if (colors.TryGetValue(ctx.State, out  var color))
            {
                targetLight.color = color;
            }
        }
    }
}