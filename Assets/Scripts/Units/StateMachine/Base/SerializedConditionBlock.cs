using System;
using System.Linq;
using UnityEngine;

namespace Arcatech.Units
{
    [Serializable]
    public class SerializedConditionBlock
    {
        [Tooltip("Логический оператор, применяемый ко всем условиям и дочерним блокам внутри этого блока.")]
        public ConditionBlockOperator Operator = ConditionBlockOperator.And;

        [Tooltip("Инвертировать ли итоговое значение блока после вычисления оператора.")]
        public bool NegateResult;

        [Tooltip("Листовые условия, входящие в блок.")]
        public SerializedStateTransitionCondition[] Conditions = Array.Empty<SerializedStateTransitionCondition>();

        [Tooltip("Вложенные блоки, позволяющие строить дерево логики.")]
        public SerializedConditionBlock[] NestedBlocks = Array.Empty<SerializedConditionBlock>();

        public bool Evaluate(StateMachineContext ctx)
        {
            var values = Conditions.Where(c => c != null)
                .Select(condition => condition.CanTransition(ctx))
                .Concat(NestedBlocks.Where(b => b != null).Select(block => block.Evaluate(ctx)))
                .ToArray();

            if (values.Length == 0) return true;

            bool result = Operator switch
            {
                ConditionBlockOperator.And   => values.All(v => v),
                ConditionBlockOperator.Or    => values.Any(v => v),
                ConditionBlockOperator.Nand  => !values.All(v => v),
                ConditionBlockOperator.Nor   => !values.Any(v => v),
                ConditionBlockOperator.Xor   => values.Count(v => v) % 2 == 1,
                ConditionBlockOperator.Xnor  => values.Count(v => v) % 2 == 0,
                _                            => values.All(v => v)
            };

            return NegateResult ? !result : result;
        }
    }
}