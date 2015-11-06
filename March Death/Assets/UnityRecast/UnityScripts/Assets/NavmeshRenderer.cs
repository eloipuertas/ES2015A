using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using UnityEngine;
using UnityEditor;

namespace Pathfinding
{
    [ExecuteInEditMode]
    class NavmeshRenderer : MonoBehaviour
    {
        public PolyMeshAsset navmesh = null;
        private Mesh mesh = null;
        private MeshRenderer mesh_renderer = null;

        private bool _rendering = true;
        public bool RenderOnEditor = true;
        public bool RenderOnPlay = false;

        void OnGui()
        {
        }

        void Start()
        {
        
        }

        void OnEnable()
        {
            mesh_renderer = GetComponent<MeshRenderer>();
            mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;
            mesh.name = gameObject.name + "Mesh";
            mesh.Clear();

            if ((Application.isPlaying && !RenderOnPlay) || navmesh == null)
            {
                return;
            }

            DrawDetailLower();
        }
        
        void DrawDetailLower()
        {
            int totalverts = Math.Min(navmesh.PolyDetailMesh.nverts, UInt16.MaxValue);
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            int i = 0;
            for (i = 0; i < totalverts; ++i)
            {
                vertices.Add(new Vector3(navmesh.PolyDetailMesh.verts[i * 3], navmesh.PolyDetailMesh.verts[i * 3 + 1], navmesh.PolyDetailMesh.verts[i * 3 + 2]));
                uv.Add(new Vector2(navmesh.PolyDetailMesh.verts[i * 3] / 8.0f, navmesh.PolyDetailMesh.verts[i * 3 + 2] / 8.0f));
            }

            List<int> triangles = new List<int>();

            uint tri_offset = 0;
            uint old_nverts = 0;
            for (uint p = 0; p < navmesh.PolyDetailMesh.nmeshes; ++p)
            {
                uint m = p * 4;
                uint bverts = navmesh.PolyDetailMesh.meshes[m + 0]; // `b' means "beginning of"!!
                uint nverts = navmesh.PolyDetailMesh.meshes[m + 1];
                uint btris = navmesh.PolyDetailMesh.meshes[m + 2];
                uint ntris = navmesh.PolyDetailMesh.meshes[m + 3];
                uint tris = btris * 4;

                tri_offset += old_nverts;

                for (int n = 0; n < ntris; ++n)
                {
                    for (int k = 0; k < 3; ++k)
                    {
                        int tri = (int)(navmesh.PolyDetailMesh.tris[tris + (n * 4 + k)] + tri_offset);
                        if (tri < totalverts)
                        {
                            triangles.Add(tri);
                        }
                    }
                }

                old_nverts = nverts;
            }
            
            triangles.RemoveRange(triangles.Count - triangles.Count % 3, triangles.Count % 3);

            /*
            vertices.RemoveRange(0, UInt16.MaxValue);
            uv.RemoveRange(0, UInt16.MaxValue);

            triangles = triangles.Where(x => x >= UInt16.MaxValue).ToList();
            triangles = triangles.Select(x => x - UInt16.MaxValue).ToList();
            triangles.RemoveRange(triangles.Count - triangles.Count % 3, triangles.Count % 3);
            */

            mesh.vertices = vertices.ToArray();
            mesh.uv = uv.ToArray();
            mesh.triangles = triangles.ToArray();

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            
            mesh_renderer.sharedMaterial.color = new Color(0, 0.8f, 0.89f, 0.5f);
        }

        void Update()
        {
            if ((!RenderOnEditor && _rendering) || (RenderOnEditor && !_rendering))
            {
                GetComponent<MeshRenderer>().enabled = RenderOnEditor;
                _rendering = RenderOnEditor;
            }
        }

    }
}
