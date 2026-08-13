using System;
using UnityEngine;

namespace Arcatech.Units.Control
{
    [Serializable]
    public struct UnitCommand
    {
        public UnitActionType Type;

        // Направление команды.
        // Например, направление уклонения.
        public Vector3 Direction;

        // Конкретная цель.
        public BaseGameEntityComponent Target;

        // Точка прицеливания в глобальных координатах.
        // Используется, если Target == null.
        public Vector3 TargetPoint;

        public bool HasDirection =>
            Direction.sqrMagnitude > 0.0001f;

        public bool HasTarget =>
            Target != null;

        public bool HasTargetPoint =>
            TargetPoint.sqrMagnitude > 0.0001f;

        public UnitCommand(
            UnitActionType type,
            Vector3 direction = default,
            BaseGameEntityComponent target = null,
            Vector3 targetPoint = default)
        {
            Type = type;
            Direction = direction;
            Target = target;
            TargetPoint = targetPoint;
        }

        public Vector3 ResolveTargetPoint()
        {
            if (Target != null)
                return Target.transform.position;

            return TargetPoint;
        }

        public static UnitCommand None =>
            new(UnitActionType.None);

        public override string ToString()
        {
            return
                $"Type: {Type}, " +
                $"Direction: {Direction}, " +
                $"Target: {Target}, " +
                $"TargetPoint: {TargetPoint}";
        }
    }
}