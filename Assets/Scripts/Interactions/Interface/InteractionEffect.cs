using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Interactions
{
    public abstract class InteractionEffect : ValidatedMonoBehaviour
    {
        public abstract void Play(InteractionContext ctx);
    }
}