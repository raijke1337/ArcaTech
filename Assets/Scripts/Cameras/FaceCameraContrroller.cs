using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Scenes.Cameras
{
    public class FaceCameraCOntrroller : MonoBehaviour
    {
        public FaceCameraTargetComponent Target;
        public float DesiredOffset;

        private void OnEnable()
        {
            Assert.IsNotNull(Target);

        }
        private void FixedUpdate()
        {
            transform.LookAt(Target.transform.position);
        }

        //private void OnDrawGizmos()
        //{
        //    Gizmos.color = Color.green;
        //    Gizmos.DrawWireCube(transform.position, Vector3.one);
        //    Gizmos.DrawLine(transform.position, Target.transform.position) ;
        //}
    }
}