using UnityEngine;
namespace Arcatech.Level
{
    public class EditorFlagHIder : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            var mesh = gameObject.GetComponent<MeshFilter>();
            Destroy(mesh);
        }

    }
}