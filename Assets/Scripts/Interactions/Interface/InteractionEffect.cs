using System;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Interactions
{
    public abstract class InteractionEffect : MonoBehaviour
    {
        public abstract void Play(InteractionContext ctx);
    }
}