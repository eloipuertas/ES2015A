using UnityEngine;
using System.Runtime.InteropServices;

[System.Serializable]
public class TileCacheAsset : ScriptableObject
{
    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct dtNavMeshParams
    {
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = 3)]
        public float[] orig;                  ///< The world space origin of the navigation mesh's tile space. [(x, y, z)]
        public float tileWidth;                ///< The width of each tile. (Along the x-axis.)
        public float tileHeight;               ///< The height of each tile. (Along the z-axis.)
        public int maxTiles;                   ///< The maximum number of tiles the navigation mesh can contain.
        public int maxPolys;                   ///< The maximum number of polygons each tile can contain.
    };

    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct dtTileCacheParams
    {
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = 3)]
        public float[] orig;
        public float cs, ch;
        public int width, height;
        public float walkableHeight;
        public float walkableRadius;
        public float walkableClimb;
        public float maxSimplificationError;
        public int maxTiles;
        public int maxObstacles;
    };

    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct TileCacheSetHeader
    {
        public int magic;
        public int version;
        public int numTiles;

        public dtNavMeshParams meshParams;
        public dtTileCacheParams cacheParams;
    }

    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct TileCacheTileHeader
    {
        public uint tileRef;
        public int dataSize;
    };

    public Pathfinding.Config config;
    public TileCacheSetHeader header;
    public TileCacheTileHeader[] tilesHeader;
    public byte[] tilesData;
}
