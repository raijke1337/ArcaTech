using System;
using com.cyborgAssets.inspectorButtonPro;
using UnityEditor;
using UnityEngine;

public class MeshDebug : MonoBehaviour
{
    private void OnValidate()
    {
        var smr = Selection.activeGameObject.GetComponent<SkinnedMeshRenderer>();
        Debug.Log("sharedMesh: " + (smr ? smr.sharedMesh?.name : "none"));
        Debug.Log("bindposes: " + (smr?.sharedMesh?.bindposes?.Length ?? 0));
        Debug.Log("bones array length (API): " + (smr?.bones?.Length ?? 0));
    }
}
