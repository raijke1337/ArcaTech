using UnityEngine;

namespace Arcatech.Actions
{
    [CreateAssetMenu(fileName = "Interface result", menuName = "Actions/Action Result/Stats Interfaces/Apply to all")]
    public class KillAllKillablesSO : SerializedActionResult
    {
        public InterfaceType interfaceType;
        private bool applyToUser = true;
        public override ActionResult BuildActionResult()
        {
            return new KillAllKillables(interfaceType);
        }

        public enum InterfaceType
        {
            IKillable,
            IStunnable,
        }
    }
    
    public class KillAllKillables : ActionResult
    {
        KillAllKillablesSO.InterfaceType interfaceType;
        public KillAllKillables(KillAllKillablesSO.InterfaceType interfaceType)
        {
            this.interfaceType =  interfaceType;
        }
        public override bool ProduceResult(BaseGameEntityComponent user, BaseGameEntityComponent target, Vector3 place,
            Quaternion placeRot)
        {
            return true;
            // Nothing happens
        }
    }
    
    
    
}