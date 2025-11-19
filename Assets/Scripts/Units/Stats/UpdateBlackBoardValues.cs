using Unity.Behavior;
using Unity.Behavior.GraphFramework;
using UnityEngine;

namespace Arcatech.Stats
{
    [CreateAssetMenu(menuName = "Create UpdateBlackBoardValues", fileName = "/Behaviour/UpdateBlackBoardValues", order = 0)]
    public class UpdateBlackBoardValues : StatChangeResponseStrat
    {
        BehaviorGraphAgent agent;
        private SerializableGUID hpPercGUID;
        public void UpdateBlackBoard(BehaviorGraphAgent agent)
        {
            this.agent = agent;
            if (!agent.GetVariableID("hpPercent", out hpPercGUID))
            {
                Debug.LogError($"No HP perc value in {agent}!");
            }
        }
        public override void OnStatChanged(ResourceStatType type, float current, float max, float delta, object contributionSource)
        {
            
            Debug.LogWarning("NYI");
            if (type == ResourceStatType.Health)
            {
                agent?.SetVariableValue(hpPercGUID, current/max);
            }
        }
    }

}