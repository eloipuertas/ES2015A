using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;

public class Detour : Utils.Singleton<Detour>
{
    public IntPtr TileCache = new IntPtr(0);
    public IntPtr NavMesh = new IntPtr(0);

    private Detour()
    {
    }

    public void Initialize(TileCacheAsset navmeshData)
    {
        Pathfinding.Recast.DefaultConfig(Application.dataPath + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "Recast.log");

        bool result = Pathfinding.TileCache.loadFromTileCacheHeaders(ref navmeshData.header, navmeshData.tilesHeader, navmeshData.tilesData, ref TileCache, ref NavMesh);
        if (!result)
        {
            throw new ArgumentException("Invalid navmesh data");
        }
    }
}
