using System;
using Arcatech.Units;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Lewding
{
    [RequireComponent(typeof(LewdnessComponent))]
    public class CatTouchingComponent : ValidatedMonoBehaviour, IStateAugmentor, ILewdComponent
    {
        [SerializeField, Self] private LewdnessComponent comp;
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

        private LewdnessContext _lctx;
        
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

        private void Start()
        {
            comp.Register(this);
        }

        private void OnTouch(TouchZoneType place)
        {
            if (_lctx!=null) _lctx.LastTouchCommand = place;
        }

        public void Attach(IStateAugmentorReceiver machine)
        {
            machine.AddTransition(_toHeadPat);
            machine.AddTransition(_toTouchChest);
            machine.AddTransition(_toTouchBottom);
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

        public void Initialize(LewdnessContext context)
        {
            _lctx =  context;
        }
    }

    public interface ILewdComponent
    {
        public void Initialize(LewdnessContext context);
    }
    
}