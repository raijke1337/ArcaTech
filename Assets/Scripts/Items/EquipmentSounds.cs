using Arcatech.Audio;
using Arcatech.Units;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Arcatech.Items
{
    public class EquipmentSounds : MonoBehaviour, IEquipmentPart
    { 
       [SerializeField] private SerializedDictionary<StateMachineNotifyType, SoundDefinition> sounds;
       private SoundHandle oldSound;
       public void TriggerState(StateMachineNotifyType notification)
        {
            if (oldSound.IsValid) AudioEvents.Stop(oldSound);
            if (sounds.TryGetValue(notification, out var sound)) AudioEvents.Play(sound,transform.position,transform,HandlePlayed);
        }

        private void HandlePlayed(SoundHandle obj)
        {
            oldSound = obj;
        }
    }
}