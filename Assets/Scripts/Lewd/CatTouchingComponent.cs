using System;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Lewding
{
    public class CatTouchingComponent : MonoBehaviour, IStateAugmentor
    {
        private float _lewdness = 0f;
        [SerializeField] private readonly string animatorLewdnessParameter = "LewdnessStage";
        private int _paramIndex;
        [SerializeField] private SerializedStateTransition headPat;
        [SerializeField, Range(0, 1f)] private float lewdGainOnHeadPat = 0.25f; 
        [Space]
        [SerializeField] private SerializedStateTransition touchChest;
        [SerializeField, Range(0, 1f)] private float lewdGainOnTouchChest = 0.25f; 
        [Space]
        [SerializeField] private SerializedStateTransition touchBottom;
        [SerializeField, Range(0, 1f)] private float lewdGainOnTouchBottom = 0.25f; 

        [Space]
        [SerializeField] private TouchZone[]  touchZones;
        
        private StateTransition _toHeadPat;
        private StateTransition _toTouchChest;
        private StateTransition _toTouchBottom;

        private LewdnessContext _cachedContextReference;
        
        private void Awake()
        {
            _paramIndex = Animator.StringToHash(animatorLewdnessParameter);
            _toHeadPat = headPat.Build();
            _toTouchChest = touchChest.Build();
            _toTouchBottom = touchBottom.Build();
            if (touchZones != null)
                for (var i = 0; i < touchZones.Length; i++)
                {
                    touchZones[i].Touch += OnTouch;
                }
        }

        private void OnTouch(TouchZoneType place)
        {
            _cachedContextReference.LastTouchCommand = place;
        }

        public void Attach(IStateAugmentorReceiver machine)
        {
            machine.AddTransition(_toHeadPat);
            machine.AddTransition(_toTouchChest);
            machine.AddTransition(_toTouchBottom);
            if (machine.Context.EcchiContext != null) return;
            
            machine.Context.EcchiContext = new LewdnessContext
            {
                LewdStage = 1,
                LastTouchCommand = TouchZoneType.None
            };
            Debug.Log("Create lewdContext");
            _cachedContextReference ??= machine.Context.EcchiContext;
        }

        public void Detach(IStateAugmentorReceiver machine)
        {
            machine.RemoveTransition(_toHeadPat);
            machine.RemoveTransition(_toTouchChest);
            machine.RemoveTransition(_toTouchBottom);
        }

        public void OnStateEntered(UnitState state, StateMachineContext context)
        {

        }

        public void OnStateExited(UnitState state, StateMachineContext context)
        {
            if (state == _toHeadPat.NextState)
            {
                _lewdness +=  lewdGainOnHeadPat;
                _lewdness = Mathf.Clamp01(_lewdness);
            }
            if (state == _toTouchChest.NextState)
            {
                _lewdness +=  lewdGainOnTouchChest;
                _lewdness = Mathf.Clamp01(_lewdness);
            }
            if (state == _toTouchBottom.NextState)
            {
                _lewdness +=  lewdGainOnTouchBottom;
                _lewdness = Mathf.Clamp01(_lewdness);
            }

            if (context.EcchiContext != null)
            {
                if (_lewdness >= 0.5f)
                {
                    context.EcchiContext.LewdStage = 2;
                }
                if (_lewdness >= 1f)
                {
                    context.EcchiContext.LewdStage = 3;
                }
                context.Animator.SetFloat(_paramIndex,context.EcchiContext.LewdStage);
                context.EcchiContext.LastTouchCommand = TouchZoneType.None;
            }
            
            Debug.Log($"State exit, lewdness is {_lewdness}, state in cached context: {_cachedContextReference.LewdStage}");
        }
    }

    public class LewdnessContext
    {
        public int LewdStage { get; set; }
        public TouchZoneType LastTouchCommand { get; set; }

        public void ClearContext()
        {
            LastTouchCommand = TouchZoneType.None;
            LewdStage = 1;
        }
    }
}