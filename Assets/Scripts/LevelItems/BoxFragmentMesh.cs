using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Arcatech
{
    internal sealed class BoxFragmentMesh
    {
        private readonly List<Surface> _surfaces = new();
        private readonly Bounds _box;
        private readonly Material _cutMaterial;

        internal BoxFragmentMesh(Transform root, Vector3 center, Quaternion rotation, Vector3 size, Material cutMaterial)
        {
            _box = new Bounds(Vector3.zero, size);
            _cutMaterial = cutMaterial != null ? cutMaterial :
                GraphicsSettings.currentRenderPipeline?.defaultMaterial ?? Resources.GetBuiltinResource<Material>("Default-Material.mat");

            var worldToBox = Matrix4x4.TRS(center, rotation, Vector3.one).inverse;
            foreach (var filter in root.GetComponentsInChildren<MeshFilter>())
            {
                if (filter.sharedMesh != null && filter.TryGetComponent<MeshRenderer>(out var renderer) && renderer.enabled)
                    CaptureSurface(filter, renderer.sharedMaterials, worldToBox * filter.transform.localToWorldMatrix);
            }
        }

        private void CaptureSurface(MeshFilter filter, Material[] materials, Matrix4x4 toBox)
        {
            var source = filter.sharedMesh;
#if UNITY_EDITOR
            using var meshData = UnityEditor.MeshUtility.AcquireReadOnlyMeshData(source);
#else
            if (!source.isReadable)
            {
                Debug.LogWarning($"Enable Read/Write on '{source.name}' to preserve its UVs on fragments.", filter);
                return;
            }
            using var meshData = Mesh.AcquireReadOnlyMeshData(source);
#endif
            var data = meshData[0];
            bool hasNormals = data.HasVertexAttribute(VertexAttribute.Normal);
            bool hasUvs = data.HasVertexAttribute(VertexAttribute.TexCoord0);
            bool hasColors = data.HasVertexAttribute(VertexAttribute.Color);
            using var positions = new NativeArray<Vector3>(data.vertexCount, Allocator.Temp);
            using var normals = new NativeArray<Vector3>(hasNormals ? data.vertexCount : 0, Allocator.Temp);
            using var uvs = new NativeArray<Vector2>(hasUvs ? data.vertexCount : 0, Allocator.Temp);
            using var colors = new NativeArray<Color>(hasColors ? data.vertexCount : 0, Allocator.Temp);
            data.GetVertices(positions);
            if (hasNormals) data.GetNormals(normals);
            if (hasUvs) data.GetUVs(0, uvs);
            if (hasColors) data.GetColors(colors);

            var normalToBox = toBox.inverse.transpose;
            var vertices = new Vertex[data.vertexCount];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Vertex(toBox.MultiplyPoint3x4(positions[i]),
                    hasNormals ? normalToBox.MultiplyVector(normals[i]).normalized : Vector3.zero,
                    hasUvs ? uvs[i] : Vector2.zero, hasColors ? colors[i] : Color.white);
            }

            bool mirrored = toBox.determinant < 0f;
            for (int subMesh = 0; subMesh < data.subMeshCount; subMesh++)
            {
                var descriptor = data.GetSubMesh(subMesh);
                if (descriptor.topology != MeshTopology.Triangles || descriptor.indexCount == 0) continue;

                using var sourceIndices = new NativeArray<int>(descriptor.indexCount, Allocator.Temp);
                data.GetIndices(sourceIndices, subMesh);
                var indices = sourceIndices.ToArray();
                if (mirrored)
                    for (int i = 0; i < indices.Length; i += 3)
                        (indices[i + 1], indices[i + 2]) = (indices[i + 2], indices[i + 1]);

                var material = subMesh < materials.Length && materials[subMesh] != null ? materials[subMesh] : _cutMaterial;
                _surfaces.Add(new Surface(vertices, indices, material, hasNormals));
            }
        }

        internal Mesh Build(Bounds fragment, out Material[] materials)
        {
            var geometry = new Geometry(fragment.center);
            var polygon = new List<Vertex>(9);
            var clipped = new List<Vertex>(9);
            var min = fragment.min;
            var max = fragment.max;
            var boxMin = _box.min;
            var boxMax = _box.max;
            // The outer faces can differ slightly from the box after transforming vertices.
            var tolerance = _box.size.magnitude * 0.00001f;
            var clipMin = min;
            var clipMax = max;
            for (int axis = 0; axis < 3; axis++)
            {
                if (Mathf.Approximately(min[axis], boxMin[axis])) clipMin[axis] -= tolerance;
                if (Mathf.Approximately(max[axis], boxMax[axis])) clipMax[axis] += tolerance;
            }

            foreach (var surface in _surfaces)
            {
                var indices = new List<int>();
                for (int i = 0; i < surface.Indices.Length; i += 3)
                {
                    var a = surface.Vertices[surface.Indices[i]];
                    var b = surface.Vertices[surface.Indices[i + 1]];
                    var c = surface.Vertices[surface.Indices[i + 2]];
                    if (!surface.HasNormals)
                    {
                        var normal = Vector3.Cross(b.Position - a.Position, c.Position - a.Position).normalized;
                        a = a.WithNormal(normal);
                        b = b.WithNormal(normal);
                        c = c.WithNormal(normal);
                    }

                    polygon.Clear();
                    polygon.Add(a);
                    polygon.Add(b);
                    polygon.Add(c);
                    for (int axis = 0; axis < 3 && polygon.Count >= 3; axis++)
                    {
                        Clip(polygon, clipped, axis, clipMin[axis], 1f);
                        Clip(clipped, polygon, axis, clipMax[axis], -1f);
                    }

                    geometry.AddPolygon(polygon, indices);
                }

                geometry.AddSubMesh(indices, surface.Material);
            }

            var capIndices = new List<int>();
            // Close the faces created by cuts through the collider volume.
            for (int axis = 0; axis < 3; axis++)
            {
                if (_surfaces.Count == 0 || !Mathf.Approximately(min[axis], boxMin[axis]))
                    AddCap(geometry, capIndices, polygon, min, max, axis, false);
                if (_surfaces.Count == 0 || !Mathf.Approximately(max[axis], boxMax[axis]))
                    AddCap(geometry, capIndices, polygon, min, max, axis, true);
            }

            geometry.AddSubMesh(capIndices, _cutMaterial);
            return geometry.Build(out materials);
        }

        private static void Clip(List<Vertex> input, List<Vertex> output, int axis, float plane, float inward)
        {
            output.Clear();
            if (input.Count < 3) return;

            var previous = input[input.Count - 1];
            float previousDistance = (previous.Position[axis] - plane) * inward;
            foreach (var current in input)
            {
                float distance = (current.Position[axis] - plane) * inward;
                if ((previousDistance < 0f && distance > 0f) || (previousDistance > 0f && distance < 0f))
                    output.Add(Vertex.Lerp(previous, current, previousDistance / (previousDistance - distance)));
                if (distance >= 0f) output.Add(current);

                previous = current;
                previousDistance = distance;
            }
        }

        private static void AddCap(Geometry geometry, List<int> indices, List<Vertex> polygon,
            Vector3 min, Vector3 max, int axis, bool positive)
        {
            int u = (axis + 1) % 3;
            int v = (axis + 2) % 3;
            var a = min;
            a[axis] = positive ? max[axis] : min[axis];
            var b = a;
            b[u] = max[u];
            var c = b;
            c[v] = max[v];
            var d = a;
            d[v] = max[v];
            if (!positive) (b, d) = (d, b);

            var normal = Vector3.zero;
            normal[axis] = positive ? 1f : -1f;
            polygon.Clear();
            polygon.Add(new Vertex(a, normal, new Vector2(a[u], a[v]), Color.white));
            polygon.Add(new Vertex(b, normal, new Vector2(b[u], b[v]), Color.white));
            polygon.Add(new Vertex(c, normal, new Vector2(c[u], c[v]), Color.white));
            polygon.Add(new Vertex(d, normal, new Vector2(d[u], d[v]), Color.white));
            geometry.AddPolygon(polygon, indices);
        }

        private readonly struct Vertex
        {
            public readonly Vector3 Position;
            public readonly Vector3 Normal;
            public readonly Vector2 Uv;
            public readonly Color Color;

            public Vertex(Vector3 position, Vector3 normal, Vector2 uv, Color color)
            {
                Position = position;
                Normal = normal;
                Uv = uv;
                Color = color;
            }

            public Vertex WithNormal(Vector3 normal) => new Vertex(Position, normal, Uv, Color);

            public static Vertex Lerp(Vertex a, Vertex b, float t) => new Vertex(
                Vector3.LerpUnclamped(a.Position, b.Position, t), Vector3.LerpUnclamped(a.Normal, b.Normal, t),
                Vector2.LerpUnclamped(a.Uv, b.Uv, t), Color.LerpUnclamped(a.Color, b.Color, t));
        }

        private readonly struct Surface
        {
            public readonly Vertex[] Vertices;
            public readonly int[] Indices;
            public readonly Material Material;
            public readonly bool HasNormals;

            public Surface(Vertex[] vertices, int[] indices, Material material, bool hasNormals)
            {
                Vertices = vertices;
                Indices = indices;
                Material = material;
                HasNormals = hasNormals;
            }
        }

        private sealed class Geometry
        {
            private readonly Vector3 _center;
            private readonly List<Vector3> _positions = new();
            private readonly List<Vector3> _normals = new();
            private readonly List<Vector2> _uvs = new();
            private readonly List<Color> _colors = new();
            private readonly List<List<int>> _subMeshes = new();
            private readonly List<Material> _materials = new();

            public Geometry(Vector3 center) => _center = center;

            public void AddPolygon(List<Vertex> polygon, List<int> indices)
            {
                if (polygon.Count < 3) return;

                int start = _positions.Count;
                foreach (var vertex in polygon)
                {
                    _positions.Add(vertex.Position - _center);
                    _normals.Add(vertex.Normal.normalized);
                    _uvs.Add(vertex.Uv);
                    _colors.Add(vertex.Color);
                }

                for (int i = 1; i < polygon.Count - 1; i++)
                {
                    indices.Add(start);
                    indices.Add(start + i);
                    indices.Add(start + i + 1);
                }
            }

            public void AddSubMesh(List<int> indices, Material material)
            {
                if (indices.Count == 0) return;
                _subMeshes.Add(indices);
                _materials.Add(material);
            }

            public Mesh Build(out Material[] materials)
            {
                var mesh = new Mesh { name = "Destructible Fragment" };
                if (_positions.Count > ushort.MaxValue) mesh.indexFormat = IndexFormat.UInt32;
                mesh.SetVertices(_positions);
                mesh.SetNormals(_normals);
                mesh.SetUVs(0, _uvs);
                mesh.SetColors(_colors);
                mesh.subMeshCount = _subMeshes.Count;
                for (int i = 0; i < _subMeshes.Count; i++) mesh.SetTriangles(_subMeshes[i], i, false);
                mesh.RecalculateBounds();
                mesh.RecalculateTangents();

                materials = _materials.ToArray();
                return mesh;
            }
        }
    }
}
