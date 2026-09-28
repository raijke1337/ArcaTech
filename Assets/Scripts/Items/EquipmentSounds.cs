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
       [SerializeField] private SoundDefinition equipSound;
       [SerializeField] private SoundDefinition unequipSound;
       
       public void TriggerState(StateMachineNotifyType notification)
        {
            if (oldSound.IsValid) AudioEvents.Stop(oldSound);
            if (sounds.TryGetValue(notification, out var sound)) AudioEvents.Play(sound,transform.position,null,HandlePlayed);
        }

        private void HandlePlayed(SoundHandle obj)
        {
            oldSound = obj;
        }

        public void OnEquip()
        {
            if (!equipSound) return;
            AudioEvents.Play(equipSound,transform.position,null,HandlePlayed);
        }

        public void OnUnequip()
        {
            if (!unequipSound) return;
            AudioEvents.Play(unequipSound,transform.position,null,HandlePlayed);
        }
    }
}