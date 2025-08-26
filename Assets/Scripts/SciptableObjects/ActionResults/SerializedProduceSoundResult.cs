using Arcatech.Effects;
using Arcatech.EventBus;
using Arcatech.Managers;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Actions
{
    [CreateAssetMenu(fileName = "New play sound result ", menuName = "Actions/Action Result/Produce sound effects")]
    public class SerializedProduceSoundResult : SerializedActionResult
    {
       
        [SerializeField] SoundClipData[] sounds;
        [SerializeField] bool RandomPitch = false;
        public override IActionResult BuildActionResult()
        {
            return new ProduceSoundResult(sounds,RandomPitch);
        }
    }


    public class ProduceSoundResult : ActionResult
    {
        readonly SoundClipData[] sounds;
        bool pitch;
        public ProduceSoundResult(SoundClipData[] d, bool pitch)
        {
            sounds = d;
            this.pitch = pitch;
        }

        public override void ProduceResult(BaseEntityOLD user, BaseEntityOLD target, Transform place)
        {
            foreach (var s in sounds)
            {
                EventBus<SoundClipRequest>.Raise(new SoundClipRequest(s,pitch,place.position));
            }
        }
    }




}