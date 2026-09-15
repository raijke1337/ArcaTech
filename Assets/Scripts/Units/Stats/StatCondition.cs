using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Arcatech.Stats
{
    [Serializable]
    public struct StatCondition
    {
        public ResourceStatType stat;
        public StatTarget target;       // Usually Current for threshold checks
        public bool usePercentOfMax;    // If true, compares normalized ratio (0..1) instead of absolute units
        public ConditionOp op;
        public float a;
        public float b;                 // Used when op == Between

        override public string ToString()
        {
            string statStr = $"{stat}({target})";
        
            // Форматируем числа: если это проценты, то умножаем 0..1 на 100.
            // Ограничиваем вывод двумя знаками после запятой ("0.##").
            string valA = usePercentOfMax ? $"{a * 100:0.##}%" : a.ToString("0.##");
            string valB = usePercentOfMax ? $"{b * 100:0.##}%" : b.ToString("0.##");

            // Преобразуем перечисление операторов в читаемый вид.
            // string-сравнение используется для совместимости с любыми названиями вашего enum ConditionOp
            switch (op.ToString())
            {
                case "Equal":
                case "EqualTo":
                    return $"{statStr} == {valA}";
                case "NotEqual":
                case "NotEqualTo":
                    return $"{statStr} != {valA}";
                case "Greater":
                case "GreaterThan":
                    return $"{statStr} > {valA}";
                case "GreaterOrEqual":
                case "GreaterThanOrEqual":
                    return $"{statStr} >= {valA}";
                case "Less":
                case "LessThan":
                    return $"{statStr} < {valA}";
                case "LessOrEqual":
                case "LessThanOrEqual":
                    return $"{statStr} <= {valA}";
                case "Between":
                    return $"{statStr} in [{valA}..{valB}]";
                default:
                    // Резервный вариант на случай специфических операторов
                    string extra = op.ToString().Contains("Between") ? $" and {valB}" : "";
                    return $"{statStr} {op} {valA}{extra}";
            }
        }
    }


    [Serializable]
    public struct ConditionGroup
    {
        [Tooltip("If true: all conditions must pass (AND). If false: any condition may pass (OR).")]
        public bool requireAll;

        [Tooltip("Optional inversion of the group result.")]
        public bool invert;

        public List<StatCondition> statConditions;

        public bool IsEmpty => statConditions == null || statConditions.Count == 0;

        override public string ToString()
        {
            if (IsEmpty)
                return "Empty Group";

            // Если условие всего одно
            if (statConditions.Count == 1)
            {
                string singleCond = statConditions[0].ToString();
                return invert ? $"NOT ({singleCond})" : singleCond;
            }

            // Если условий несколько, объединяем их через AND или OR
            string separator = requireAll ? " AND " : " OR ";
            string joined = string.Join(separator, statConditions.Select(c => c.ToString()));
        
            string result = $"({joined})";

            if (invert)
            {
                result = $"NOT {result}";
            }

            return result;
        }
    }
    
    public enum ConditionOp
    {
        Greater,
        GreaterOrEqual,
        Less,
        LessOrEqual,
        Between, // inclusive [a, b]
        Equal,
        NotEqual
    }
    
}