using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class AutoRemapSkinnedMesh : MonoBehaviour
{
    [Tooltip("Assign the character's armature/root (at runtime) before calling RemapBones")]
    public Transform targetSkeletonRoot;

    [Tooltip("Optional: prefix/suffix to adapt naming differences")]
    public string namePrefix = "";
    public string nameSuffix = "";

    SkinnedMeshRenderer smr;

    void Awake()
    {
        smr = GetComponent<SkinnedMeshRenderer>();
        targetSkeletonRoot = GetComponentsInChildren<Transform>().First(t=>t.name == "spine");
        // If targetSkeletonRoot is set before Awake (e.g. equip code), auto-remap
        if (targetSkeletonRoot != null)
            RemapBones();
    }

    [ContextMenu("Remap Bones")]
    public void RemapBones()
    {
        if (smr == null) smr = GetComponent<SkinnedMeshRenderer>();
        if (smr == null || targetSkeletonRoot == null)
        {
            Debug.LogWarning("Cannot remap: missing SMR or targetSkeletonRoot on " + name, this);
            return;
        }

        // Build lookup of target bones by name
        var targets = targetSkeletonRoot.GetComponentsInChildren<Transform>(true);
        System.Collections.Generic.Dictionary<string, Transform> dict = new System.Collections.Generic.Dictionary<string, Transform>(targets.Length);
        for (int i = 0; i < targets.Length; ++i)
            dict[targets[i].name] = targets[i];

        var srcBones = smr.bones;
        Transform[] newBones = new Transform[srcBones.Length];
        for (int i = 0; i < srcBones.Length; ++i)
        {
            if (srcBones[i] == null) continue;
            string srcName = srcBones[i].name;
            string search = namePrefix + srcName + nameSuffix;
            if (dict.TryGetValue(search, out var found))
                newBones[i] = found;
            else
                Debug.LogWarning($"Bone '{srcName}' -> '{search}' not found under '{targetSkeletonRoot.name}'", this);
        }

        // Remap rootBone too
        if (smr.rootBone != null)
        {
            string rootName = smr.rootBone.name;
            string searchRoot = namePrefix + rootName + nameSuffix;
            if (dict.TryGetValue(searchRoot, out var rootFound))
                smr.rootBone = rootFound;
            else
                Debug.LogWarning($"Root bone '{rootName}' not found under '{targetSkeletonRoot.name}'", this);
        }

        smr.bones = newBones;

        // Helpful defaults for debugging
        smr.updateWhenOffscreen = true;
        if (smr.sharedMesh != null) smr.localBounds = smr.sharedMesh.bounds;
        Debug.Log("Remap complete for " + name, this);
    }
}