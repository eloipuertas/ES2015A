using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;

public class PathDetour : Utils.Singleton<PathDetour>
{
    public IntPtr TileCache = new IntPtr(0);
    public IntPtr NavMesh = new IntPtr(0);
    public IntPtr NavQuery = new IntPtr(0);

    private PathDetour()
    {
    }

    public void Initialize(TileCacheAsset navmeshData)
    {
        // Is it already initialized?
        if (TileCache.ToInt32() == 0)
        {
            Pathfinding.Recast.DefaultConfig(Application.dataPath + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "Recast.log");

            bool result = Pathfinding.TileCache.loadFromTileCacheHeaders(ref navmeshData.header, navmeshData.tilesHeader, navmeshData.tilesData, ref TileCache, ref NavMesh, ref NavQuery);
            if (!result)
            {
                throw new ArgumentException("Invalid navmesh data");
            }
        }
    }

    public void AddObstacle(Pathfinding.DetourObstacle block)
    {
        Vector3[] blockVertices = block.Vertices();

        float[] vertices =
        {
            blockVertices[0].x, blockVertices[0].y, blockVertices[0].z,
            blockVertices[1].x, blockVertices[1].y, blockVertices[1].z,
            blockVertices[2].x, blockVertices[2].y, blockVertices[2].z,
            blockVertices[3].x, blockVertices[3].y, blockVertices[3].z,
        };

        float[] position =
        {
            block.transform.position.x, block.transform.position.y, block.transform.position.z
        };

        Pathfinding.TileCache.addObstacle(TileCache, position, vertices, 4, (int)Mathf.Ceil(block.Size.y));
    }
}
