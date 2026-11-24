using UnityEngine;

namespace Arcatech
{
    [AddComponentMenu("")]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class DestructibleFragment : MonoBehaviour
    {
        private Mesh _mesh;

        internal void SetMesh(Mesh mesh, Material[] materials)
        {
            _mesh = mesh;
            GetComponent<MeshFilter>().sharedMesh = mesh;
            GetComponent<MeshRenderer>().sharedMaterials = materials;
        }

        private void OnDestroy()
        {
            if (_mesh != null) Destroy(_mesh);
        }
    }
}
