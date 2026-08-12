using Arcatech.Audio;
using Arcatech.Items;
using Arcatech.Units.Control;
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
        


        public void PrepareCommand(UnitCommand command)
        {
        }

        public void DoUnitCommand(UnitCommand command, bool wasSuccessful)
        {
            if (!wasSuccessful || !soundDefinitions.TryGetValue(command.Type,out var d)) return;
            AudioEvents.Play(d,entity.EffectSpawn.position);
        }
    }
}