using System.Collections.Generic;
using com.cyborgAssets.inspectorButtonPro;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[RequireComponent(typeof(BoxCollider))]
public class SurfaceBlockBuilder : MonoBehaviour
{
    [Tooltip("Prefab of the 1×1 block used to cover the surfaces.")]
    [SerializeField]
    private GameObject blockPrefab;

    [Tooltip("Prefab used for horizontal border segments on top of the cube.")]
    [SerializeField]
    private GameObject topBorderPrefab;

    [Tooltip("Prefab used for the corner pieces of the top border.")]
    [SerializeField]
    private GameObject topCornerPrefab;

    [Tooltip("Enable the top border (border + corners) on the surface blocks.")]
    [SerializeField]
    private bool addTopBorder = true;

    [Tooltip("Automatically rebuild surface blocks whenever the object is edited.")]
    [SerializeField]
    private bool autoRebuildOnValidate = true;

    private readonly List<GameObject> spawnedBlocks = new();
    private BoxCollider cachedBoxCollider;
    private bool isRebuilding;

    private void Awake()
    {
        //RebuildSurfaceBlocks();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying && autoRebuildOnValidate)
        {
            RebuildSurfaceBlocks();
        }
    }

    [ProButton]
    [ContextMenu("Rebuild Surface Blocks")]
    public void RebuildSurfaceBlocks()
    {
        if (isRebuilding)
        {
            return;
        }

        isRebuilding = true;

        try
        {
            if (blockPrefab == null)
            {
                Debug.LogWarning($"[{nameof(SurfaceBlockBuilder)}] Please assign a 1×1 block prefab.", this);
                return;
            }

            cachedBoxCollider ??= GetComponent<BoxCollider>();
            if (cachedBoxCollider == null)
            {
                Debug.LogWarning($"[{nameof(SurfaceBlockBuilder)}] BoxCollider is required.", this);
                return;
            }

            Bounds originalBounds = cachedBoxCollider.bounds;
            if (originalBounds.size == Vector3.zero)
            {
                Debug.LogWarning($"[{nameof(SurfaceBlockBuilder)}] Collider bounds are invalid (zero size).", this);
                return;
            }

            AlignColliderToBounds(originalBounds);
            DisableMeshComponents();
            ClearSpawnedBlocks();
            SpawnSurfaceBlocks();
        }
        finally
        {
            isRebuilding = false;
        }
    }

    private void AlignColliderToBounds(Bounds bounds)
    {
        Vector3 worldCenter = bounds.center;
        transform.localScale = Vector3.one;

        cachedBoxCollider.size = bounds.size;
        cachedBoxCollider.center = transform.InverseTransformPoint(worldCenter);
    }

    private void SpawnSurfaceBlocks()
    {
        Vector3 localSize = cachedBoxCollider.size;
        Vector3 localMin = cachedBoxCollider.center - localSize * 0.5f;

        Vector3Int counts = new(
            Mathf.Max(1, Mathf.RoundToInt(localSize.x)),
            Mathf.Max(1, Mathf.RoundToInt(localSize.y)),
            Mathf.Max(1, Mathf.RoundToInt(localSize.z)));

        spawnedBlocks.Clear();

        for (int x = 0; x < counts.x; x++)
        {
            for (int y = 0; y < counts.y; y++)
            {
                for (int z = 0; z < counts.z; z++)
                {
                    bool isSurface =
                        x == 0 || x == counts.x - 1 ||
                        y == 0 || y == counts.y - 1 ||
                        z == 0 || z == counts.z - 1;

                    if (!isSurface)
                    {
                        continue;
                    }

                    Vector3 localPosition = localMin + new Vector3(x + 0.5f, y, z + 0.5f);
                    Vector3 worldPosition = transform.TransformPoint(localPosition);

                    GameObject block = Instantiate(blockPrefab, worldPosition, Quaternion.identity, transform);
                    block.transform.localRotation = Quaternion.identity;
                    block.transform.localScale = Vector3.one;
                    block.name = $"SurfaceBlock_{x}_{y}_{z}";
                    block.AddComponent<SurfaceBlockBuilderMarker>();

                    spawnedBlocks.Add(block);
                }
            }
        }

        if (addTopBorder)
        {
            Debug.Log("NYI, TODO");
           // SpawnTopBorder(localMin, counts);
        }

        Debug.Log($"[{nameof(SurfaceBlockBuilder)}] Spawned {spawnedBlocks.Count} surface blocks.", this);
    }

    private void SpawnTopBorder(Vector3 localMin, Vector3Int counts)
    {
        if (topBorderPrefab == null || topCornerPrefab == null)
        {
            Debug.LogWarning($"[{nameof(SurfaceBlockBuilder)}] Top border or corner prefab missing.", this);
            return;
        }

        float borderY = localMin.y + counts.y;

        for (int x = 0; x < counts.x; x++)
        {
            for (int z = 0; z < counts.z; z++)
            {
                bool isEdge =
                    x == 0 || x == counts.x - 1 ||
                    z == 0 || z == counts.z - 1;

                if (!isEdge)
                {
                    continue;
                }

                bool isCorner =
                    (x == 0 || x == counts.x - 1) &&
                    (z == 0 || z == counts.z - 1);

                GameObject prefab = isCorner ? topCornerPrefab : topBorderPrefab;

                Vector3 localPosition = new Vector3(
                    localMin.x + x + 0.5f,
                    borderY,
                    localMin.z + z + 0.5f);

                Vector3 worldPosition = transform.TransformPoint(localPosition);

                Quaternion rotation = GetBorderRotation(x, z, counts);

                GameObject borderPiece = Instantiate(prefab, worldPosition, rotation, transform);
                borderPiece.transform.localScale = Vector3.one;
                borderPiece.name = $"TopBorder_{(isCorner ? "Corner" : "Edge")}_{x}_{z}";
                borderPiece.AddComponent<SurfaceBlockBuilderMarker>();

                spawnedBlocks.Add(borderPiece);
            }
        }
    }

    private static Quaternion GetBorderRotation(int x, int z, Vector3Int counts)
    {
        bool alongX = z == 0 || z == counts.z - 1;
        bool alongZ = x == 0 || x == counts.x - 1;

        if (alongX && !alongZ)
        {
            // Edge parallel to X axis
            return Quaternion.identity;
        }

        if (alongZ && !alongX)
        {
            // Edge parallel to Z axis
            return Quaternion.Euler(0f, 90f, 0f);
        }

        // Corner pieces keep their default rotation
        return Quaternion.identity;
    }

    private void ClearSpawnedBlocks()
    {
        SurfaceBlockBuilderMarker[] markers = GetComponentsInChildren<SurfaceBlockBuilderMarker>(true);

        foreach (SurfaceBlockBuilderMarker marker in markers)
        {
            if (marker == null || marker.gameObject == gameObject)
            {
                continue;
            }

#if UNITY_EDITOR
            DestroyImmediate(marker.gameObject);
#else
            Destroy(marker.gameObject);
#endif
        }

        spawnedBlocks.Clear();
    }

    private void DisableMeshComponents()
    {
        if (TryGetComponent<MeshRenderer>(out MeshRenderer renderer))
        {
            renderer.enabled = false;
        }

        if (TryGetComponent<MeshFilter>(out MeshFilter filter))
        {
            filter.sharedMesh = null;
        }
    }
}

internal sealed class SurfaceBlockBuilderMarker : MonoBehaviour
{
}
