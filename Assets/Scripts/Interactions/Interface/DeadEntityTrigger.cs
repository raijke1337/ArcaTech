using System.Collections;
using System.Linq;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Interactions
{
    public class DeadEntityTrigger : InteractionTrigger
    {
        [SerializeField] private BaseGameEntityComponent[] entitiesToCheck;
        [SerializeField] private float updateFrequency = 1f;
        
        private Coroutine entitiesCheckRoutine;
        private IInteractor player;

        public override void TriggerEntered(TriggerHitInfo triggerHitInfo) { }

        public override void TriggerExited(TriggerHitInfo triggerExitInfo) { }

        private void Start()
        {
            var playerComponent = FindAnyObjectByType<PlayerComponent>();
            player = playerComponent?.GetComponent<InteractionComponent>();
            
            // Проверяем условие сразу при старте
            if (AreAllEntitiesDeadOrNull())
            {
                Trigger();
            }
            else
            {
                // Запускаем периодическую проверку
                entitiesCheckRoutine = StartCoroutine(Checking());
            }
        }

        /// <summary>
        /// Проверяет все сущности: dead или null?
        /// </summary>
        private bool AreAllEntitiesDeadOrNull()
        {
            if (entitiesToCheck == null || entitiesToCheck.Length == 0)
                return true;

            foreach (var entity in entitiesToCheck)
            {
                if (entity != null && entity.EntityAlive)
                {
                    return false;
                }
            }

            return true;
        }

        private IEnumerator Checking()
        {
            while (true)
            {
                if (AreAllEntitiesDeadOrNull())
                {
                    StopCoroutine(entitiesCheckRoutine);
                    Trigger();
                    yield break;
                }
                
                yield return new WaitForSeconds(updateFrequency);
            }
        }

        void Trigger()
        {
            Debug.Log("Triggered");
            HasTriggered = true;
            
            interactableComponent.StartInteraction(new InteractionContext()
            {
                Interactor = player,
                State = InteractionState.Starting,
                Target = interactableComponent.Entity,
            });
        }

        private void OnDestroy()
        {
            StopCoroutine(entitiesCheckRoutine);
        }
    }
}