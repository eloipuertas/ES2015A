using UnityEngine;

[System.Serializable]
public class PolyMeshAsset : ScriptableObject
{
    [System.Serializable]
    public struct PolyMeshData
    {
        public ushort[] verts;
        public ushort[] polys;
        public ushort[] regs;
        public ushort[] flags;
        public byte[] areas;
        public int nverts;
        public int npolys;
        public int maxpolys;
        public int nvp;
        public float[] bmin;
        public float[] bmax;
        public float cs;
        public float ch;
        public int borderSize;
    }

    [System.Serializable]
    public struct PolyMeshDetailData
    {
        public uint[] meshes;
        public float[] verts;
        public byte[] tris;
        public int nmeshes;
        public int nverts;
        public int ntris;
    }

    public Pathfinding.Config config;
    public PolyMeshData PolyMesh;
    public PolyMeshDetailData PolyDetailMesh;
}
