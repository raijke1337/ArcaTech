using Arcatech.Audio;
using Arcatech.Items;
using AYellowpaper.SerializedCollections;
using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Units
{
    [RequireComponent(typeof(BaseGameEntityComponent))]
    public class UnitSoundsComponent : ValidatedMonoBehaviour, IUnitCommandPerformer
    {
        [SerializeField, Self] private BaseGameEntityComponent entity;
        [SerializeField] private SerializedDictionary<UnitActionType, SoundDefinition> soundDefinitions;

        public void PrepareCommand(UnitActionType type)
        {
        }

        public void DoUnitCommand(UnitActionType type, bool wasSuccessful)
        {
            if (!wasSuccessful || !soundDefinitions.TryGetValue(type,out var d)) return;
            AudioEvents.Play(d,entity.EffectSpawn.position);
        }
    }
}