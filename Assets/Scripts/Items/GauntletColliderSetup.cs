using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Items
{

    public class GauntletColliderSetup : MonoBehaviour
    {
        [System.Serializable]
        private struct BoneCollider
        {
            public string boneName;
            public Vector3 localPosition;
            public Vector3 localEuler;
            public Vector3 size; // box size
            public bool isCapsule;
            public float capsuleRadius;
            public float capsuleHeight;
            public int capsuleDirection; // 0 = X, 1 = Y, 2 = Z
        }

        [SerializeField] private BoneCollider[] colliders;
        [SerializeField] private string targetBoneName = "spine";

        private readonly List<Component> spawned = new();

        private void Start()
        {
            var targetRoot = LocateTargetSkeleton(targetBoneName);
            if (targetRoot == null)
            {
                Debug.LogWarning($"{name}: Could not find '{targetBoneName}' to bind against.");
                return;
            }

            AttachToSkeleton(targetRoot);
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
            if (gameObject.scene.IsValid())
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
        
        public void AttachToSkeleton(Transform skeletonRoot)
        {
            foreach (var entry in colliders)
            {
                var bone = FindBone(skeletonRoot, entry.boneName);
                if (bone == null)
                {
                    Debug.LogWarning($"{name}: Bone '{entry.boneName}' not found.");
                    continue;
                }

                var holder = new GameObject($"{entry.boneName}_GauntletCollider");
                holder.transform.SetParent(bone, false);
                holder.transform.localPosition = entry.localPosition;
                holder.transform.localRotation = Quaternion.Euler(entry.localEuler);
                holder.transform.localScale = Vector3.one;

                if (entry.isCapsule)
                {
                    var capsule = holder.AddComponent<CapsuleCollider>();
                    capsule.radius = entry.capsuleRadius;
                    capsule.height = entry.capsuleHeight;
                    capsule.direction = entry.capsuleDirection;
                    spawned.Add(capsule);
                }
                else
                {
                    var box = holder.AddComponent<BoxCollider>();
                    box.size = entry.size;
                    spawned.Add(box);
                }
            }
        }

        private static Transform FindBone(Transform root, string name)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == name)
                    return t;
            return null;
        }
    }
}