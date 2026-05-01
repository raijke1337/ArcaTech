using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Interactions
{
    /// <summary>
    /// this interface is for checking if interaction can be performed
    /// it uses th strategy pattern
    /// </summary>

    public abstract class InteractionCondition : MonoBehaviour
    {
        [SerializeField] private List<InteractionEffect> _denyEffects;

        public abstract bool Check(InteractionContext ctx);

        public void PlayDenyEffects(InteractionContext ctx)
        {
            foreach (var e in _denyEffects) e.Play(ctx);
        }
    }
}