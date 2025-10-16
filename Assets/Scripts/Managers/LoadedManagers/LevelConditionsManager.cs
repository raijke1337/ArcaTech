using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Arcatech;
using Arcatech.Interactions;
using Arcatech.Managers;
using Arcatech.Units;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// despite being a lazy singleton
/// this should be instantiated on level load
/// </summary>

namespace Arcatech.Level
{
    public class LevelConditionsManager : GenericLazySingleton<LevelConditionsManager>
    {
        private PlayerUnit _p;
        private IInteractor _player;
        private List<ActiveGameUnitComponent> allEnemies;
        private List<InteractiveItemComponent> allCollectables;
        private List<Collider> allSecretZones = new List<Collider>(); // NYI

        private void Start()
        {
            allEnemies = FindObjectsByType<ActiveGameUnitComponent>(FindObjectsSortMode.None)
                .Where(t => t.CompareTag("Enemy")).ToList();
            allCollectables = FindObjectsOfType<InteractiveItemComponent>().Where(t => t.CompareTag("Collectable"))
                .ToList();

            _p = FindAnyObjectByType<PlayerUnit>();
            _player = _p.GetComponent<IInteractor>();
        }

        [Header("Interactive Conditions")]
        [Header("Placeholder for now. If unit is dead, returns true to interactive item component")]
        [SerializeField]
        private List<VerifiedUnitCondition> unitConditions;

        /// <summary>
        /// if a IInteractive has a "validate with manager" condition strategy, it will check here
        /// </summary>
        /// <param name="toValidate"></param>
        public bool VerifyActivation(IInteractive toValidate)
        {
            if (unitConditions == null || unitConditions.Count == 0) return false;
            
        }

        private void Update()
        {
            // handle conditions that dont have the "only on query"
            foreach (var c in unitConditions)
            {
                if (!c.CheckOnlyOnQuery)
                {
                    if (c.Target.Killed)
                    {
                        c.Item.OnInteraction(_player, InteractionContext.Create(_p, c.Item.transform,"auto"));
                    }
                }
            }
        }

    }
    
    [Serializable]
    public class VerifiedUnitCondition
    {
        [SerializeField] ActiveGameUnitComponent target;
        [SerializeField] InteractiveItemComponent item;
        [SerializeField] private bool checkOnlyOnQuery;

        public ActiveGameUnitComponent Target => target;
        public InteractiveItemComponent Item => item;
        public bool CheckOnlyOnQuery => checkOnlyOnQuery;

    }
}