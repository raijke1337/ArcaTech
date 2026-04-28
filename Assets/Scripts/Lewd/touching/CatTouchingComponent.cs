using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Lewding
{
    public class CatTouchingComponent : MonoBehaviour, IStateAugmentor
    {
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
            if (context.EcchiContext == null)
            {
                Debug.LogWarning($"Ecchi context not initialized!");
                return;
            }
            
            if (state == _toHeadPat.NextState)
            {
                context.EcchiContext.ArousalPercent += lewdGainOnHeadPat;
            }

            if (state == _toTouchChest.NextState)
            {
                context.EcchiContext.ArousalPercent += lewdGainOnTouchChest;
            }

            if (state == _toTouchBottom.NextState)
            {
                context.EcchiContext.ArousalPercent += lewdGainOnTouchBottom;
            }
            context.EcchiContext.LastTouchCommand = TouchZoneType.None;
        }
    }
}