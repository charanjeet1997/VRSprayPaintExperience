using UnityEditor;
using UnityEngine;

namespace VRSprayPainting.EditorTools
{
    /// <summary>
    /// Builds paint-ready meshes. Unity's built-in Cube maps the full 0-1 UV square onto every face, so one paint
    /// texture shows on all six faces. This cube lays its faces out in a 3x2 atlas with gutters, giving each face
    /// its own area of the paint texture.
    /// </summary>
    public static class PaintableMeshBuilder
    {
        public const string CubePath = "Assets/VRSprayPainting/Models/PaintableCube.asset";
        private const float Gutter = 0.03f; // empty UV border per cell so spray near an edge doesn't land on the neighbour face

        [MenuItem("Tools/VR Spray Painting/Build Paintable Cube Mesh")]
        public static Mesh BuildCube()
        {
            // Face normal, up and right vectors; the face spans right x up around normal * 0.5.
            Vector3[] normals = { Vector3.forward, Vector3.back, Vector3.right, Vector3.left, Vector3.up, Vector3.down };
            Vector3[] ups = { Vector3.up, Vector3.up, Vector3.up, Vector3.up, Vector3.back, Vector3.forward };

            var vertices = new Vector3[24];
            var meshNormals = new Vector3[24];
            var uvs = new Vector2[24];
            var triangles = new int[36];
            const float cellW = 1f / 3f, cellH = 1f / 2f;

            for (int f = 0; f < 6; f++)
            {
                Vector3 n = normals[f], up = ups[f];
                Vector3 right = Vector3.Cross(n, up); // viewed from outside, right is to the viewer's right
                Vector3 c = n * 0.5f;

                Vector2 cellMin = new Vector2((f % 3) * cellW + Gutter, (f / 3) * cellH + Gutter);
                Vector2 cellSize = new Vector2(cellW - Gutter * 2f, cellH - Gutter * 2f);

                int v = f * 4;
                Vector2[] corners = { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
                for (int k = 0; k < 4; k++)
                {
                    Vector2 q = corners[k];
                    vertices[v + k] = c + right * (q.x - 0.5f) + up * (q.y - 0.5f);
                    meshNormals[v + k] = n;
                    uvs[v + k] = cellMin + Vector2.Scale(q, cellSize);
                }

                int t = f * 6;
                triangles[t] = v; triangles[t + 1] = v + 2; triangles[t + 2] = v + 1;
                triangles[t + 3] = v; triangles[t + 4] = v + 3; triangles[t + 5] = v + 2;
            }

            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(CubePath);
            bool create = mesh == null;
            if (create)
                mesh = new Mesh();
            mesh.Clear();
            mesh.name = "PaintableCube";
            mesh.vertices = vertices;
            mesh.normals = meshNormals;
            mesh.uv = uvs;
            mesh.uv2 = uvs; // the face atlas has gutters and no overlap, so it doubles as the lightmap layout
            mesh.triangles = triangles;
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();

            if (create)
                AssetDatabase.CreateAsset(mesh, CubePath);
            EditorUtility.SetDirty(mesh);
            AssetDatabase.SaveAssets();
            return mesh;
        }
    }
}
