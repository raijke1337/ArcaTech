using Arcatech.Level.Conditions;
using AYellowpaper.SerializedCollections;
using com.cyborgAssets.inspectorButtonPro;
using DG.Tweening;
using System;
using UnityEngine;

namespace Arcatech.Level
{
    [CreateAssetMenu(fileName = "new item moves and rotates behavior", menuName = "Level/Event Condition Behavior/Item translation")]
    public class ItemMovesBehavior : ConditionBehaviorStrategy
    {
        [SerializeField] ItemMovementPackage movement;
        public override IConditionControlledStrat Build(ConditionControlledItemComponent item)
        {
            return new ItemMovementStrat(item, movement);
        }
    }

    public class ItemMovementStrat : IConditionControlledStrat
    {
        Transform _transform;
        readonly ItemMovementPackage data;

        Tween movement;


        public ItemMovementStrat(ConditionControlledItemComponent comp, ItemMovementPackage data)
        {
            this.data = data;

            _transform = comp.transform;
            movement = _transform.DOLocalPath(data.path, data.movetime, data.pathType);
            movement.SetEase(data.ease).SetLoops(data.loops).Pause();
        }

        public void SetState(ConditionCheckResult newstate)
        {
            switch (newstate)
            {
                case ConditionCheckResult.Success:
                    movement.Play();
                    break;
                default:
                    Debug.Log($"NYI state {newstate} for {this}");
                    break;
            }
        }

    }

    [Serializable]
    public struct ItemMovementPackage
    {
        [SerializeField] public Vector3[] path;
        [SerializeField,Tooltip("-1 is infinite")] public int loops;
        [SerializeField] public float movetime;
        [SerializeField] public Ease ease;
        [SerializeField] public PathType pathType;
    }
}