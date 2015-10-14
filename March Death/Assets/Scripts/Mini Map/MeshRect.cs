using UnityEngine;
using System.Collections;

public class MeshRect : MonoBehaviour {

	// Use this for initialization
	void Start () {

        createRectangle();

	}

    private void createRectangle()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        mf.mesh = mesh;

        float height = 500f;
        float width = 500f;

        // Vertices
        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(-width/2,0,-height/2),
            new Vector3(width/2, 0, -height/2),
            new Vector3(-width/2, 0, height/2),
             new Vector3(width/2, 0, height/2)
        };

        // Triangles
        int[] triangles = new int[6]
        {
            0,2,1,
            2,3,1
        };

        // Normals
        Vector3[] normals = new Vector3[4]
        {
            Vector3.up,
            Vector3.up,
            Vector3.up,
            Vector3.up
        };

        // uvs
        Vector2[] uvs = new Vector2[4]
        {
            new Vector2(0,0),
            new Vector2(1,0),
            new Vector2(0,1),
            new Vector2(1,1),
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uvs;

    }
}
