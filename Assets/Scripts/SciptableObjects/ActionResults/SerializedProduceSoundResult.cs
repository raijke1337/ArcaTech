using Arcatech.Audio;
using UnityEngine;

namespace Arcatech.Actions
{
    [CreateAssetMenu(fileName = "actionResult_PlaySound_", menuName = "Actions/Action Result/FX/Sound")]
    public class SerializedProduceSoundResult : SerializedActionResult
    {
        [Header ("Plays as a 'regular' sound. UI/Music/etc - NYI")]
        [SerializeField] SoundDefinition soundDefinition;

        public override ActionResult Deserialize()
        {
            return new ProduceSoundResult(soundDefinition);
        }
    }

    public class ProduceSoundResult : ActionResult
    {
        private SoundDefinition s;
        public ProduceSoundResult(SoundDefinition soundDefinition) => s = soundDefinition;
        public override bool ProduceResult(BaseGameEntityComponent user, BaseGameEntityComponent target, Vector3 place, Quaternion placeRot)
        {
            AudioEvents.Play(s, place);
            return true;
        }
    }
}