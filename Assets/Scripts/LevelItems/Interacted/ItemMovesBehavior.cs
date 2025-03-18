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
        [SerializeField,Tooltip("if set to true, object will go through all steps and then back")] bool looping;
        [SerializeField] ItemMovementPackage[] movementSteps;
        public override IConditionControlledStrat Build(ConditionControlledItemComponent item)
        {
            return new ItemMovementStrat(item, movementSteps, looping);
        }
    }

    public class ItemMovementStrat : IConditionControlledStrat
    {
        readonly bool loop;
        readonly ItemMovementPackage[] steps; //nyi
        int index;
        Rigidbody _rb;
        Transform _transform;
        bool doRigidTransform = false;

        Vector3 moveFrom;
        Vector3 rotateFrom;


        public ItemMovementStrat(ConditionControlledItemComponent comp, ItemMovementPackage[] steps, bool loop)
        {
            this.loop = loop;
            this.steps = steps;
            index = 0;
            if (comp.TryGetComponent<Rigidbody>(out var r))
            {
                _rb = r;
                doRigidTransform = true;
                moveFrom = _rb.transform.position;
                rotateFrom = _rb.transform.eulerAngles;
            }
            else
            {
                _transform = comp.transform;
                moveFrom = _transform.position;
                rotateFrom = _transform.eulerAngles;
            }

        }

        public void SetState(ConditionCheckResult newstate)
        {
            switch (newstate)
            {
                case ConditionCheckResult.Success:
                    if (doRigidTransform)
                    {
                        RigidTransform(steps[index].targetMovement, steps[index].targetRotation, steps[index].movetime);
                    }
                    break;
                case ConditionCheckResult.Fail:
                    if (doRigidTransform)
                    {
                        RigidTransform(moveFrom,rotateFrom, steps[index].movetime);
                    }
                    break;
            }
        }


        void RigidTransform(Vector3 moveTO, Vector3 rotateTo, float time, bool loop = false)
        {
            _rb.DOMove(moveTO,time,true);
            _rb.DORotate(rotateTo,time);
        }
        void JustTransform(Vector3 moveTO, Vector3 rotateTo, float time, bool loop = false)
        {
            _transform.DOMove(moveTO, time, true);
            _transform.DORotate(rotateTo, time);    
        }
    }

    [Serializable]
    public struct ItemMovementPackage
    {
        [SerializeField] public Vector3 targetMovement;
        [SerializeField] public Vector3 targetRotation;
        [SerializeField] public bool loop;
        [SerializeField] public float movetime;
        [SerializeField] public Ease ease;
    }
}