using Unity.Behavior;
using Unity.Behavior.GraphFramework;
using UnityEngine;

namespace Arcatech.Stats
{
    [CreateAssetMenu(menuName = "Create UpdateBlackBoardValues", fileName = "/Behaviour/UpdateBlackBoardValues", order = 0)]
    public class UpdateBlackBoardValuesSO : StatChangeResponseStrat
    {

        public override IOnStatChange Deserialize(EntityStatsComponent comp)
        {
            return new UpdateBBValues(comp);

        }
    }

    public class UpdateBBValues : IOnStatChange
    {        
        BehaviorGraphAgent agent;
        private SerializableGUID hpPercGUID;
        public UpdateBBValues(EntityStatsComponent stats)
        {            
            this.agent = stats.GetComponentInChildren<BehaviorGraphAgent>();
            if (this.agent == null)
            {
                Debug.LogError("UpdateBlackBoardValues could not find BehaviorGraphAgent");
                return;
            }
            if (!agent.GetVariableID("hpPercent", out hpPercGUID))
            {
                Debug.LogError($"No HP perc value in {agent}!");
            }
        }
        public void OnStatChanged(ResourceStatType type, float current, float max, float delta, object contributionSource)
        {
            
            if (type == ResourceStatType.Health)
            {
                agent?.SetVariableValue(hpPercGUID, current/max);
            }
        }
    }

}