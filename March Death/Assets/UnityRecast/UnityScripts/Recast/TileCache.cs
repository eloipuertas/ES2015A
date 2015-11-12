using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pathfinding
{
    public static class TileCache
    {
        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool handleTileCacheBuild(ref Config cfg, ref ExtendedConfig ecfg, ref InputGeometry geom, ref IntPtr tileCache, ref IntPtr navMesh, ref IntPtr navQuery);

        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern void getTileCacheHeaders(ref TileCacheAsset.TileCacheSetHeader header, ref IntPtr tilesHeader, IntPtr tileCache, IntPtr navMesh);

        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool loadFromTileCacheHeaders(ref TileCacheAsset.TileCacheSetHeader header, TileCacheAsset.TileCacheTileHeader[] tilesHeader, byte[] data, ref IntPtr tileCache, ref IntPtr navMesh);

        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getTileCacheTile(IntPtr tileCache, int i);

        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern void addObstacle(IntPtr tileCache, float[] pos, float[] verts, int nverts, int height);
    }
}
