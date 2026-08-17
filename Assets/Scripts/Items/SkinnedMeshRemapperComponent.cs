using System.Collections.Generic;
using com.cyborgAssets.inspectorButtonPro;
using UnityEngine;

namespace Arcatech.Items
{

    [DisallowMultipleComponent]
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    public class CostumeBinder : MonoBehaviour
    {
        [Header("Target skeleton lookup")] [SerializeField]
        private string targetBoneName = "spine"; // must match character bone

        [SerializeField] private bool allowSceneWideSearch = true;

        [Header("Template armature (for prefab previews)")] [SerializeField]
        private Transform templateArmatureRoot; // drag e.g. "Costume template rig"

        [SerializeField] private bool destroyTemplateAfterBind = true;

        private static readonly Dictionary<EntityId, Dictionary<string, Transform>> SkeletonCache = new();
        private bool isBound;

        private void Reset()
        {
            if (templateArmatureRoot == null)
                templateArmatureRoot = FindTemplateArmatureInChildren();
        }

        private void OnEnable()
        {
            TryBind();
        }

        [ProButton]
        public void TryBind()
        {
            if (isBound)
                return;

            var smr = GetComponent<SkinnedMeshRenderer>();
            if (smr == null || smr.sharedMesh == null)
            {
                Debug.LogWarning($"{name}: SkinnedMeshRenderer missing.");
                return;
            }

            var targetRoot = LocateTargetSkeleton(targetBoneName);
            if (targetRoot == null)
            {
                Debug.LogWarning($"{name}: Could not find '{targetBoneName}' to bind against.");
                return;
            }

            RebindRenderer(smr, targetRoot);

            isBound = true;

            if (destroyTemplateAfterBind &&
                templateArmatureRoot != null &&
                templateArmatureRoot != targetRoot &&
                templateArmatureRoot.IsChildOf(transform))
            {
                Destroy(templateArmatureRoot.gameObject);
                templateArmatureRoot = null;
            }
        }

        private Transform LocateTargetSkeleton(string boneName)
        {
            // 1) Check our own hierarchy first (costume already parented under player)
            var candidate = FindChildByName(transform.root, boneName);
            if (candidate != null)
                return candidate;

            // 2) Walk up through parents in case the costume lives deeper in the player hierarchy
            var parent = transform.parent;
            while (parent != null)
            {
                var found = FindChildByName(parent, boneName);
                if (found != null)
                    return found;
                parent = parent.parent;
            }

            // 3) Optional scene-wide search (only if prefab is instantiated in a scene)
            if (allowSceneWideSearch && gameObject.scene.IsValid())
            {
                foreach (var rootGO in gameObject.scene.GetRootGameObjects())
                {
                    var found = FindChildByName(rootGO.transform, boneName);
                    if (found != null)
                        return found;
                }
            }

            return null;
        }

        private static Transform FindChildByName(Transform parent, string name)
        {
            if (parent == null) return null;
            if (parent.name == name) return parent;

            foreach (Transform child in parent)
            {
                var result = FindChildByName(child, name);
                if (result != null)
                    return result;
            }

            return null;
        }

        private void RebindRenderer(SkinnedMeshRenderer smr, Transform targetBone)
        {
            var skeletonRoot = targetBone.root;
            var map = GetOrBuildSkeletonMap(skeletonRoot);

            var bones = smr.bones;
            int replaced = 0;
            for (int i = 0; i < bones.Length; i++)
            {
                var source = bones[i];
                if (source != null && map.TryGetValue(source.name, out var replacement))
                {
                    bones[i] = replacement;
                    replaced++;
                }
                else if (source != null)
                {
                    Debug.LogWarning($"{name}: Bone '{source.name}' not found on '{skeletonRoot.name}'.");
                }
            }

            smr.bones = bones;

            if (smr.rootBone != null && map.TryGetValue(smr.rootBone.name, out var newRootBone))
                smr.rootBone = newRootBone;

            smr.updateWhenOffscreen = true;
            Debug.Log($"{name}: Bound {replaced}/{bones.Length} bones to '{skeletonRoot.name}'.");
        }

        private static Dictionary<string, Transform> GetOrBuildSkeletonMap(Transform skeletonRoot)
        {
            var id = skeletonRoot.GetEntityId();
            if (SkeletonCache.TryGetValue(id, out var cached))
                return cached;

            cached = new Dictionary<string, Transform>(128);
            foreach (var t in skeletonRoot.GetComponentsInChildren<Transform>(true))
                cached[t.name] = t;

            SkeletonCache[id] = cached;
            return cached;
        }

        private Transform FindTemplateArmatureInChildren()
        {
            foreach (Transform child in transform.GetComponentsInChildren<Transform>(true))
            {
                if (child == transform) continue;
                if (child.name.ToLower().Contains("armature") || child.name.ToLower().Contains("rig"))
                    return child;
            }

            return null;
        }
    }
}