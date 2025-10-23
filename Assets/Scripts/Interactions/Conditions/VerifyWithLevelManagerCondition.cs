using Arcatech.Level;
using UnityEngine;

namespace Arcatech.Interactions
{
    [CreateAssetMenu(fileName = "Verify with level manager condition",menuName = "Interactions/Condition/Verify with level mgr")]
    public class VerifyWithLevelManagerCondition : InteractionCondition
    {
        public override bool CheckCondition(IInteractor actor, IInteractive item)
        {
            return LevelConditionsManager.Instance.VerifyActivation(item);
        }
    }
}