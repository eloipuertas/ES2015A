using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[ExecuteInEditMode]
class DetourStarter : MonoBehaviour
{
    public enum RenderMode { POLYS, DETAIL_POLYS, TILE_POLYS }

    public bool Render = false;
    public Material material;
    public PolyMeshAsset polymesh;
    public TileCacheAsset navmeshData;
    public RenderMode Mode;

    private DbgRenderMesh mesh = new DbgRenderMesh();
    
    public void OnEnable()
    {
        PathDetour.get.Initialize(navmeshData);

        if (Application.isPlaying)
        {
            Destroy(this);
        }
        else if (Render)
        {
            mesh.Clear();

            switch (Mode)
            {
                case RenderMode.POLYS:
                    RecastDebug.ShowRecastNavmesh(mesh, polymesh.PolyMesh, polymesh.config);
                    break;

                case RenderMode.DETAIL_POLYS:
                    RecastDebug.ShowRecastDetailMesh(mesh, polymesh.PolyDetailMesh);
                    break;

                case RenderMode.TILE_POLYS:
                    for (int i = 0; i < navmeshData.header.numTiles; ++i)
                        RecastDebug.ShowTilePolyDetails(mesh, PathDetour.get.NavMesh, i);
                    break;
            }

            RecastDebug.RenderObstacles(PathDetour.get.TileCache);

            mesh.CreateGameObjects("RecastRenderer", material);
            mesh.Rebuild();
        }
    }
}
