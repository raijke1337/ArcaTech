using System;
using Arcatech.Interactions;
using ECM.Components;
using KBCore.Refs;
using UnityEngine;


namespace Arcatech.Units
{
    [RequireComponent(typeof(GroundDetection))]
    public class PlayerUnit : ActiveGameUnitComponent
    {
        [Space, Header("Player Unit")]

        [SerializeField, Child] protected Camera _faceCam;
        [SerializeField, Self] protected GroundDetection _ground;

        [SerializeField] private bool stickToPlatforms = true;
        [SerializeField] private string platfromTag;
        
        CostumesControllerComponent costumes;


        
        protected override void Start()
        {
            _faceCam.enabled = true;
            base.Start();
        }

        public override void ApplyForceResultToUnit(float speed, float distance)
        {
            base.ApplyForceResultToUnit(speed, distance);
            //_movement.DisableGroundingOnUnitImpulse(speed, distance);
        }
        protected override bool CanAct()
        {
            return _ground.isOnGround && _ground.isValidGround;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!stickToPlatforms) return;
            if (collision.gameObject.CompareTag(platfromTag))
            {
                transform.parent = collision.transform;
            }
        }

        private void OnCollisionExit(Collision other)
        {
            if (!stickToPlatforms) return;
            if (other.gameObject.CompareTag(platfromTag))
            {
                transform.parent = null;
            }
        }


        #region inventory

        //protected override UnitInventoryItemConfigsContainer SelectSerializedItemsConfig()
        //{

        //    if (DataManager.Instance.IsNewGame)
        //    {
        //        return new UnitInventoryItemConfigsContainer(defaultEquips);
        //    }
        //    else
        //    {
        //        return DataManager.Instance.GetPlayerSaveEquips;
        //    }

        //}
        #endregion

    }

}