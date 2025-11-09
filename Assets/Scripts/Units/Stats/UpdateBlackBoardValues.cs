using Unity.Behavior;
using Unity.Behavior.GraphFramework;
using UnityEngine;

namespace Arcatech.Stats
{
    [CreateAssetMenu(menuName = "Create UpdateBlackBoardValues", fileName = "Behaviour/UpdateBlackBoardValues", order = 0)]
    public class UpdateBlackBoardValues : SerializedOnEffectApplyStrategy
    {
        private BehaviorGraphAgent agent;
        
        public override IStatHandlingStrategy Deserialize(EntityStatsComponent comp)
        {
            if (comp.TryGetComponent(out agent))
            {
                return new UpdateBlackBoard(agent);
            }
            throw new System.NotImplementedException($"No agent on {comp}");
        }
    }

    public class UpdateBlackBoard : IStatHandlingStrategy
    {
        BehaviorGraphAgent agent;
        private SerializableGUID hpPercGUID;
        public UpdateBlackBoard(BehaviorGraphAgent agent)
        {
            this.agent = agent;
            if (!agent.GetVariableID("hpPercent", out hpPercGUID))
            {
                Debug.LogError($"No HP perc value in {agent}!");
            }
        }
        public void StatChanged(ResourceStatType type, float current, float max, float delta, object contributionSource)
        {
            if (type == ResourceStatType.Health)
            {
                agent.SetVariableValue(hpPercGUID, current/max);
            }
        }

        public void OnInit()
        {
        }

        public void OnCleanUp()
        {
        }
    }
}