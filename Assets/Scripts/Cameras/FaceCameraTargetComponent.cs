using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Scenes.Cameras
{
    public class FaceCameraTargetComponent : MonoBehaviour
    {
        public Transform TargetBone;
        [Range(0.01f,3f)]public float FollowSpeed;
        private Vector3 velo;

        private void OnEnable()
        {
            Assert.IsNotNull(TargetBone);
            transform.SetParent(null);
            
            transform.position = TargetBone.position;
            transform.forward = TargetBone.forward;
        }

        private void FixedUpdate()
        {
            transform.position = Vector3.SmoothDamp(transform.position,TargetBone.position, ref velo, FollowSpeed);
        }

        //private void OnDrawGizmos()
        //{
        //    Gizmos.color = Color.green;
        //    Gizmos.DrawWireSphere(TargetBone.position, 0.2f);
        //    Gizmos.color = Color.blue;
        //    Gizmos.DrawWireSphere(transform.position, 0.1f);
        //}
    }
}