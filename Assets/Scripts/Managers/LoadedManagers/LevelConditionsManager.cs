using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Arcatech;
using Arcatech.EventBus;
using Arcatech.Interactions;
using Arcatech.Managers;
using Arcatech.Units;
using KBCore.Refs;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;


namespace Arcatech.Level
{
    public class LevelConditionsManager : GenericLazySingleton<LevelConditionsManager>
    {
        private PlayerUnit _p;
        private IInteractor _player;
        private List<ActiveGameUnitComponent> allEnemies;
        private List<InteractiveItemComponent> allCollectables;
        private List<Collider> allSecretZones = new List<Collider>(); // NYI

        [SerializeField] private float eventsRefreshTimer = 1;
        [SerializeField] private List<LevelConditionHolder> trackedEvents;
        private Coroutine refreshConditionsCor;

        private EventBinding<PauseToggleEvent> pauseBind;
        
        private void Start()
        {

            allEnemies = FindObjectsByType<ActiveGameUnitComponent>(FindObjectsSortMode.None)
                .Where(t => t.CompareTag("Enemy")).ToList();
            allCollectables = FindObjectsOfType<InteractiveItemComponent>().Where(t => t.CompareTag("Collectable"))
                .ToList();

            _p = FindAnyObjectByType<PlayerUnit>();
            _player = _p.GetComponent<IInteractor>();

            refreshConditionsCor = StartCoroutine(CheckLevelEvents());

        }

        private float time = 0;
        private IEnumerator CheckLevelEvents()
        {
            while (true)
            {
                yield return new WaitForSeconds(eventsRefreshTimer);
        
                //Debug.Log("Refresh events");
                DoRefresh();
            }
        }

        void DoRefresh()
        {
            foreach (var e in trackedEvents)
            {
                var rule = e.Rule;
                foreach (var pair in e.Pair)
                {
                    if (pair.Completed) continue;
                    
                    var result = rule.CheckCondition(pair);
                    if (!result) continue;
                    
                    pair.MarkComplete();
                    
                    foreach (var item in pair.Items)
                    {
                        item.DoInteraction(_player,null);
                    }
                }
            }
        }
        private void OnDisable()
        {
            StopCoroutine(refreshConditionsCor);
        }
        
        private void OnDrawGizmos()
        {
            if (trackedEvents == null)
                return;

            foreach (var container in trackedEvents)
            {
                foreach (var pair in container.Pair)
                {
                    if (pair.Check == null || pair.Check.Count == 0 || pair.Check[0] == null)
                        continue;
                    
                    GameObject firstCheck = pair.Check[0];
                    
                    // Set color based on completion
                    Gizmos.color = pair.Completed ? Color.green : Color.cyan;
        
                    // Draw wire box around first Check object
                    Bounds bounds = new Bounds(firstCheck.transform.position, Vector3.one * 2f);
                    if (firstCheck.TryGetComponent<Renderer>(out Renderer renderer))
                    {
                        bounds = renderer.bounds;
                        bounds.Expand(1f);
                    }
                    
                    Gizmos.DrawWireCube(bounds.center, bounds.size);
        
                    // Draw lines to all Items
                    if (pair.Items != null)
                    {
                        Gizmos.color = pair.Completed ? Color.gray : Color.yellow;
                        Vector3 fromPos = firstCheck.transform.position;
            
                        foreach (var item in pair.Items)
                        {
                            if (item != null)
                            {
                                Gizmos.DrawLine(fromPos, item.transform.position);
                            }
                        }
                    }
                }
            }
        }
                
        /// <summary>
        /// if a IInteractive has a "validate with manager" condition strategy, it will check here
        /// </summary>
        /// <param name="toValidate"></param>
        public bool VerifyActivation(IInteractive toValidate)
        {
            Debug.Log($"Verifying Activation {toValidate} NYI");
            return false;
        }
    }
    [Serializable]
    public class LevelConditionHolder
    {
        public CheckedLevelEventCondition Rule;
        public List<LevelEventPairContainer> Pair;
    }
    
    [Serializable]
    public class LevelEventPairContainer
    {
       // [SerializeField] bool ActivateImmediately = true;
        public List<GameObject> Check;
        public List<InteractionHandlerBase> Items;
        public bool Completed { get; private set; }

        
        public void MarkComplete()
        {
            Completed = true;
        }
    }


    public abstract class CheckedLevelEventCondition : ScriptableObject
    {
        public abstract bool CheckCondition(LevelEventPairContainer pair);
    }
}