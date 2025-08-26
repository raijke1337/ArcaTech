using Arcatech.Level.Conditions;
using Arcatech.Managers;
using Arcatech.Units;
using AYellowpaper.SerializedCollections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Items
{
    [CreateAssetMenu(fileName = "new entities are placed behavior", menuName = "Level/Event Condition Behavior/Place base entitites")]
    public class PlaceBaseEntityBehavior : ConditionBehaviorStrategy
    {
        [SerializeField] SerializedDictionary <ConditionCheckResult, EntitiesPlacementConfig[]> PlacedEntities;
        private void OnValidate()
        {
            Assert.IsNotNull(PlacedEntities);
        }
        public override IConditionControlledStrat Build(ConditionControlledItemComponent item)
        {
            return new PlaceEntititesStrat(PlacedEntities,item);
        }

    }

    public class PlaceEntititesStrat : IConditionControlledStrat
    {
        Dictionary<ConditionCheckResult, EntitiesPlacementConfig[]> _dict;
        Transform place;
        public PlaceEntititesStrat(Dictionary<ConditionCheckResult, EntitiesPlacementConfig[]> dict, ConditionControlledItemComponent i)
        {
            _dict = dict;
            place = i.transform;
        }

        public void SetState(ConditionCheckResult newstate)
        {
            foreach(var e in _dict[newstate])
            {
                var item = GameObject.Instantiate(e.entity, place.position + e.offset, place.rotation);
                UnitsManager.Instance.TryRegisterEntity(item);
            }
        }
    }
    [Serializable]
    public struct EntitiesPlacementConfig
    {
        public BaseEntityOLD entity;
        public Vector3 offset;
       // public bool randomizePlacement;
    }

}