using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Arcatech.Interactions
{[RequireComponent(typeof(Animator))]
    public class AnimatorControlEffect : InteractionEffect
    {
        [SerializeField] SerializedDictionary<InteractionState,string> _states;
        
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public override void Play(InteractionContext ctx)
        {
            _animator.Play(_states[ctx.State]);
        }
    }
}