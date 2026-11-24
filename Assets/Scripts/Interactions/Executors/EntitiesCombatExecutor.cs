using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Interactions
{
    public class EntitiesCombatExecutor : InteractionExecutor
    {
        [Serializable]
        internal struct Spawn
        {
            public BaseGameEntityComponent prefab;
            public Transform SpawnTransform;
            public bool NeedKillToComplete;
        }
        [SerializeField] Spawn[] spawns;
        private HashSet<BaseGameEntityComponent> entitiesToKillForCompletion;
        private UnityAction<InteractionState> completedCallback;
        public override void Execute(InteractionContext ctx, UnityAction<InteractionState> onComplete)
        {
            entitiesToKillForCompletion?.Clear();
            
            completedCallback = onComplete;
            foreach (var spawn in spawns)
            {
                var entity = Instantiate(spawn.prefab, spawn.SpawnTransform.position, spawn.SpawnTransform.rotation);
                if (spawn.NeedKillToComplete)
                {
                    entitiesToKillForCompletion??= new();
                    entitiesToKillForCompletion.Add(entity);
                    entity.AnnounceDead.AddListener(OnSpawnedEntityDied);
                }
            }
            if (entitiesToKillForCompletion == null || entitiesToKillForCompletion.Count == 0) onComplete.Invoke(InteractionState.Success);
        }

        private void OnSpawnedEntityDied(BaseGameEntityComponent p)
        {
            entitiesToKillForCompletion.Remove(p);
            p.AnnounceDead.RemoveListener(OnSpawnedEntityDied);
            if (entitiesToKillForCompletion.Count == 0) completedCallback.Invoke(InteractionState.Success);
        }
    }
}