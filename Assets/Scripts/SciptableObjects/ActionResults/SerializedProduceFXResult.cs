using Arcatech.Effects;
using Arcatech.EventBus;
using CartoonFX;
using UnityEngine;

namespace Arcatech.Actions
{


    [CreateAssetMenu(fileName = "New instantiate particles result ", menuName = "Actions/Action Result/Produce particle effects")]
    public class SerializedProduceFXResult : SerializedActionResult
    {
        [SerializeField] CFXR_Effect Effects;
        [SerializeField] bool ParentParticles;
        public override ActionResult Deserialize()
        {
            return new ProduceFXResult(Effects, ParentParticles);
        }
        
        public override string ToString()
        {
            return $"produce particles result";
        }
    }

    public class ProduceFXResult : ActionResult
    {
        ParticlesEvent _event;
        private bool _p;
        public ProduceFXResult(CFXR_Effect effs, bool p)
        {
            _event = new ParticlesEvent(effs);
            _p = p;
        }

        public override bool ProduceResult(BaseGameEntityComponent user, BaseGameEntityComponent target, Vector3 place,
            Quaternion placeRot)
        {
            if (_p && !_event.Parent) _event.Parent = user.EffectSpawn.transform;
            _event.Place = place;
            EventBus<ParticlesEvent>.Raise(_event);
            return true;
        }
    }
}