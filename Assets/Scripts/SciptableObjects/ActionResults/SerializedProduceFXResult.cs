using Arcatech.Units;
using CartoonFX;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Actions
{




    [CreateAssetMenu(fileName = "New instantiate particles result ", menuName = "Actions/Action Result/Produce particle effects")]
    public class SerializedProduceFXResult : SerializedActionResult
    {
        [SerializeField] CFXR_Effect[] Effects;
        [SerializeField] bool ParentParticles;
        public override IActionResult BuildActionResult()
        {
            return new ProduceFXResult(Effects, ParentParticles);
        }

        private void OnValidate()
        {
            Assert.IsNotNull(Effects);
            Assert.IsTrue(Effects.Length > 0);
        }

        public override string ToString()
        {
            return $"produce particles result total {Effects.Length}";
        }
    }

    public class ProduceFXResult : ActionResult
    {
        CFXR_Effect[] _effs;
        bool parent;
        public ProduceFXResult(CFXR_Effect[] effs, bool p)
        {
            _effs = effs;
            parent = p;
        }

        public override void ProduceResult(BaseEntityOLD user, BaseEntityOLD target, Transform place)
        {
            if (parent)
            {
                Transform par = null;

                if (target != null) par = target.transform;
                else par = place; 
                foreach (var effect in _effs)
                {
                    GameObject.Instantiate(effect, place.position, place.rotation, par);
                }
            }
            else
            {
                foreach (var effect in _effs)
                {
                    GameObject.Instantiate(effect, place.position, place.rotation);
                }
            }


        }
    }



}