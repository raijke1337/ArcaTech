using System;
using System.Collections.Generic;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Units
{
    public partial class EntityStateMachineComponent
    {
        public enum CommandRejectReason
        {
            None,
            PausedOrKilled,
            NoCurrentState,
            NoValidTransitionYet
        }

        public CommandRejectReason LastCommandRejectReason { get; private set; }
        
        
#if UNITY_EDITOR
        public StateMachineContext Context => _context;
        public UnitState CurrentUnitState => _currentState;
        public Animator Animator => animator;
        public int PendingPerformersCount => _pendingPerformers.Count;
        public IReadOnlyList<IStateAugmentor> ActiveAugmentors => _activeAugmentors;

        [Serializable]
        public struct CandidateInfo
        {
            public string Source;
            public string NextState;
            public int Priority;
            public float ExitNormalizedTime;
            public bool OverrideMinTime;
            public bool CanTransition;
        }

        readonly List<CandidateInfo> _debugCandidates = new();
        public IReadOnlyList<CandidateInfo> DebugCandidates => _debugCandidates;

        public void DebugRefreshCandidates()
        {
            RefreshCandidates();   // existing method
        }

        public void DebugClearPendingCommand()
        {
            CompletePendingCommand(false);
        }

 #endif
    }
}