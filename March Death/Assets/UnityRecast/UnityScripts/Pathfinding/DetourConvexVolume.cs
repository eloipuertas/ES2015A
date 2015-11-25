using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Pathfinding
{
    [ExecuteInEditMode]
    public class DetourConvexVolume : MonoBehaviour
    {
        [Header("Basic Settings")]
        public string AreaID = "";
        public float maxY;
        public float minY;
        
        [Header("Advanced Settings")]
        public bool RenderOnEditor = true;
        public int MaxVertexConnections = 4;
        public List<Vector3> nodes = new List<Vector3>();

        private Quaternion meshRotation;
        private Vector3 meshScale;
        private Vector3 meshPosition;
        private Dictionary<Vector3, int> references = new Dictionary<Vector3, int>();

        public void OnEnable()
        {
            Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
            int[] tris = mesh.triangles;
            Vector3[] verts = mesh.vertices;

            nodes.Clear();
            meshRotation = transform.rotation;
            meshScale = transform.localScale;
            meshPosition = transform.position;

            for (int i = 0; i < tris.Length; i += 3)
            {
                addVertex(verts[tris[i + 0]]);
                addVertex(verts[tris[i + 1]]);
                addVertex(verts[tris[i + 2]]);                
            }

            foreach (KeyValuePair<Vector3, int> entry in references)
            {
                if (entry.Value <= MaxVertexConnections)
                {
                    nodes.Add(entry.Key);
                }
            }
        }

        public void Update()
        {
            if (nodes.Count > 0 && RenderOnEditor)
            {
                for (int i = 0; i < nodes.Count - 1; ++i)
                {
                    Debug.DrawLine(nodes[i], nodes[i + 1], Color.magenta);
                }

                Debug.DrawLine(nodes[nodes.Count - 1], nodes[0], Color.magenta);
            }
        }

        private void addVertex(Vector3 vertex)
        {
            vertex = (meshRotation * Vector3.Scale(meshScale, vertex)) + meshPosition;

            if (!references.ContainsKey(vertex))
            {
                references.Add(vertex, 1);
            }
            else
            {
                references[vertex] += 1;
            }
        }

        public float[] floatNodes()
        {
            float[] vertices = new float[nodes.Count * 3];

            for (int i = 0; i < nodes.Count; ++i)
            {
                vertices[i * 3 + 0] = nodes[i].x;
                vertices[i * 3 + 1] = nodes[i].y;
                vertices[i * 3 + 2] = nodes[i].z;
            }

            return vertices;
        }
    }
}
