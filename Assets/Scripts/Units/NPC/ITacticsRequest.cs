using Arcatech.Units;
using System.Collections.Generic;
using UnityEngine.ProBuilder.MeshOperations;
namespace Arcatech
{
    public enum Comparer
    {
        Equal,
        NotEqual,
        Greater,
        Less
    } }
namespace Arcatech.AI
{
    public interface ITacticsRequest
    {
        public NPCUnit Process(List<NPCUnit> units);
    }

    public class TacticsRequestLowStatAllyAction : ITacticsRequest
    {
        readonly float _valuePercent;
        readonly BaseStatType _stat;
        readonly public UnitActionType ActionType;
        readonly Comparer _comparer;
        public TacticsRequestLowStatAllyAction(BaseStatType stat, Comparer c, float valuePercent, UnitActionType actionType)
        {
            _valuePercent = valuePercent;
            _stat = stat;
            ActionType = actionType;
            _comparer = c;
        }

        public NPCUnit Process(List<NPCUnit> units)
        {
            foreach (NPCUnit unit in units)
            {
                if (unit.GetDisplayValues.TryGetValue(_stat, out var cont))
                {
                    switch (_comparer)
                    {
                        case Comparer.Equal:
                            if (cont.GetPercent == _valuePercent) return unit;
                            break;
                        case Comparer.NotEqual:
                            if (cont.GetPercent != _valuePercent) return unit;
                            break;
                        case Comparer.Greater:
                            if (cont.GetPercent > _valuePercent) return unit;
                            break;
                        case Comparer.Less:
                            if (cont.GetPercent < _valuePercent) return unit;
                            break;
                    }
                }

            }
            return null;
        }

    }
}