using UnityEngine;
using System.Runtime.InteropServices;

[System.Serializable]
public class TileCacheAsset : ScriptableObject
{
    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    struct dtNavMeshParams
    {
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = 3)]
        float[] orig;                  ///< The world space origin of the navigation mesh's tile space. [(x, y, z)]
        float tileWidth;                ///< The width of each tile. (Along the x-axis.)
        float tileHeight;               ///< The height of each tile. (Along the z-axis.)
        int maxTiles;                   ///< The maximum number of tiles the navigation mesh can contain.
        int maxPolys;                   ///< The maximum number of polygons each tile can contain.
    };

    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    struct dtTileCacheParams
    {
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = 3)]
        float[] orig;
        float cs, ch;
        int width, height;
        float walkableHeight;
        float walkableRadius;
        float walkableClimb;
        float maxSimplificationError;
        int maxTiles;
        int maxObstacles;
    };

    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct TileCacheSetHeader
    {
        public int magic;
        public int version;
        public int numTiles;

        dtNavMeshParams meshParams;
        dtTileCacheParams cacheParams;
    }

    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct TileCacheTileHeader
    {
        public uint tileRef;
        public int dataSize;
    };

    public TileCacheSetHeader header;
    public TileCacheTileHeader[] tilesHeader;
}
