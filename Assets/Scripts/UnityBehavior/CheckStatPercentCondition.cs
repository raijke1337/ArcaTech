using System;
using Arcatech.Stats;
using Arcatech.Units;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Check stat percent", story: "[Stat] in [Wrapper] is [Operator] [Variable]", category: "Conditions/Game", id: "bc6df0fbd03b721383d9105e1291426d")]
public partial class CheckStatPercentCondition : Condition
{
    [SerializeReference] public BlackboardVariable<ResourceStatType> Stat;
    [SerializeReference] public BlackboardVariable<NPCBehaviorWrapper> Wrapper;
    [Comparison(comparisonType: ComparisonType.All)]
    [SerializeReference] public BlackboardVariable<ConditionOperator> Operator;
    [SerializeReference] public BlackboardVariable<float> Variable;

    public override bool IsTrue()
    {
        if (Variable == null)
        {
            return false;
        }
        var comparison = Wrapper.Value.GetStatPercent(Stat.Value);
        return ConditionUtils.Evaluate(comparison, Operator, Variable);
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
