using Arcatech.Triggers;
using UnityEngine;
namespace Arcatech.Level
{
    public class PassiveConditionCheckerComponent : BaseTrigger
    {
        protected override void OnTriggerEnter(Collider other)
        {
            
        }
        protected override void OnTriggerExit(Collider other)
        {

        }
    }

    public enum ConditionCheckResult
    {
        OK,
        Fail    
    }

}