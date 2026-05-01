using System;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Lewding
{
    /// <summary>
    /// this component initializes lewd context from a config. Placeholder for now..
    /// </summary>
    public class LewdnessComponent : MonoBehaviour, IStateAugmentor
    {
        [SerializeField] private LewdnessSettings cfg;
        private LewdnessContext ctx;
        public void Attach(IStateAugmentorReceiver machine)
        {
            machine.Context.InitEcchiContext(cfg);
            ctx = machine.Context.EcchiContext;
           // Debug.Log($"Ecchi OK");
        }

        public void Detach(IStateAugmentorReceiver machine)
        {
            
        }

        public void OnStateEntered(UnitState state, StateMachineContext context)
        {
            // todo maybe add a separation between regular states and lewd states and do the horny logic here instead of the player component
        }

        public void OnStateExited(UnitState state, StateMachineContext context)
        {
        }
    }
[Serializable]
    public struct LewdnessSettings
    {
        public float stageOnePercent;
        public float stageTwoPercent;
        public float drainPerSecPercent;
    }
}