using ECM.Components;
using KBCore.Refs;
using UnityEngine;


namespace Arcatech.Units
{
    public class PlayerUnit : ActiveGameUnitComponent
    {
        [Space, Header("Player Unit")]

        [SerializeField, Child] protected Camera _faceCam;
       // [SerializeField, Self] protected GroundDetection _ground;

        [SerializeField] private bool stickToPlatforms = true;
        [SerializeField] private string platfromTag;
        
        
        [Space,Header("Jump!"),SerializeField] SerializedUnitState jumpState;

        
        protected override void Start()
        {
            _faceCam.enabled = true;
            base.Start();
        }


        private void OnCollisionEnter(Collision collision)
        {
            // if (!stickToPlatforms) return;
            // if (collision.gameObject.CompareTag(platfromTag))
            // {
            //     transform.parent = collision.transform;
            // }
        }

        private void OnCollisionExit(Collision other)
        {
            // if (!stickToPlatforms) return;
            // if (other.gameObject.CompareTag(platfromTag))
            // {
            //     transform.parent = null;
            // }
        }

        public void PlayerJump()
        {
            ForceUnitState(jumpState.DeserializeState(this, transform));
        }



    }

}