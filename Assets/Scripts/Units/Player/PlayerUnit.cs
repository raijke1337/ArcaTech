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
        //[SerializeField] int _armorBreakStam = 30;
        //[SerializeField] int _armorBreakEnergy = 30;

        [SerializeField, Child] protected Camera _faceCam;
        [SerializeField, Self] protected GroundDetection _ground;

        
        CostumesControllerComponent costumes;

        protected void ToggleCamera(bool value) { _faceCam.enabled = value; }

        
        protected override void Start()
        {
            ToggleCamera(true);
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