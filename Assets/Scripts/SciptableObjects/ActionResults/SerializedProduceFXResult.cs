using Arcatech.Effects;
using Arcatech.EventBus;
using CartoonFX;
using UnityEngine;

namespace Arcatech.Actions
{


    [CreateAssetMenu(fileName = "New instantiate particles result ", menuName = "Actions/Action Result/Produce particle effects")]
    public class SerializedProduceFXResult : SerializedActionResult
    {
        [SerializeField] CFXR_Effect effects;
        [SerializeField] bool parentParticles;
        public override ActionResult Deserialize()
        {
            return new ProduceFXResult(effects, parentParticles);
        }
        
        public override string ToString()
        {
            return $"produce particles result";
        }
    }

    public class ProduceFXResult : ActionResult
    {
        private CFXR_Effect effect;
        private bool isParented;
        public ProduceFXResult(CFXR_Effect effs, bool isParented)
        {
            this.isParented = isParented;
            effect = effs;
        }

        public override bool ProduceResult(BaseGameEntityComponent user, BaseGameEntityComponent target, Vector3 place,
            Quaternion placeRot)
        {
            ParticlesEvent e;
            if (isParented) e = new ParticlesEvent(effect, user.EffectSpawn);
            else e = new ParticlesEvent(effect,place,placeRot);
            EventBus<ParticlesEvent>.Raise(e);
            return true;
        }
    }
}