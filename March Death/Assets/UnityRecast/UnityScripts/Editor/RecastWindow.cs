using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEditor;


#if UNITY_EDITOR

namespace Pathfinding
{
    class RecastWindow : EditorWindow
    {
        static TerrainData terrain;
        static Vector3 terrainPos;
        static Config config;
        static ExtendedConfig ecfg = new ExtendedConfig();

        static bool ConfigLoaded = false;
        static float EdgeLen;
        static float RegionMinSize;
        static float RegionMergeSize;
        static float DetailSampleDist;
        static float DetailSampleMaxError;

        static int TileCacheThreads;

        static bool LoadTerrain()
        {
            Terrain terrainObject = Selection.activeObject as Terrain;
            if (!terrainObject)
            {
                terrainObject = Terrain.activeTerrain;
            }

            if (terrainObject)
            {
                terrain = terrainObject.terrainData;
                terrainPos = terrainObject.transform.position;
            }

            return terrainObject;
        }

        static void LoadConfig()
        {
            if (!ConfigLoaded)
            {
                // Standard config
                IntPtr ptr = Recast.DefaultConfig(Application.dataPath + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "Recast.log");
                config = (Config)Marshal.PtrToStructure(ptr, typeof(Config));

                ecfg.AgentHeight = config.walkableHeight * config.ch;
                ecfg.AgentMaxClimb = config.walkableClimb * config.ch;
                ecfg.AgentRadius = config.walkableRadius * config.ch;
                EdgeLen = config.maxEdgeLen * config.cs;
                RegionMinSize = (float)Math.Sqrt(config.minRegionArea);
                RegionMergeSize = (float)Math.Sqrt(config.mergeRegionArea);
                DetailSampleDist = config.detailSampleDist / config.cs;
                DetailSampleMaxError = config.detailSampleMaxError / config.ch;
                ConfigLoaded = true;
            }
        }

        [MenuItem("Recast \\ Detour/Show window...")]
        static void Init()
        {
            LoadTerrain();
            LoadConfig();

            EditorWindow.GetWindow<RecastWindow>().Show();
        }

        void OnGUI()
        {
            titleContent = new GUIContent("Recast", AssetDatabase.LoadAssetAtPath<Texture>("Assets/Sprites/Gear.png"));

            if (!terrain)
            {
                if (!LoadTerrain())
                {
                    GUILayout.Label("No terrain found");
                    if (GUILayout.Button("Cancel"))
                    {
                        EditorWindow.GetWindow<RecastWindow>().Close();
                    }

                    return;
                }

                LoadConfig();
            }

            // CELL
            GUILayout.Label("Cell Settings", EditorStyles.boldLabel);
            config.cs = EditorGUILayout.FloatField("Cell Size", config.cs);
            config.ch = EditorGUILayout.FloatField("Cell Height", config.ch);
            config.tileSize = EditorGUILayout.IntField("Tile Size", config.tileSize);
            ecfg.MaxObstacles = EditorGUILayout.IntField("Max Obstacles", ecfg.MaxObstacles);


            // AGENT
            GUILayout.Label("Agent Settings", EditorStyles.boldLabel);
            ecfg.AgentHeight = EditorGUILayout.FloatField("Height", ecfg.AgentHeight);
            ecfg.AgentMaxClimb = EditorGUILayout.FloatField("Max Climb", ecfg.AgentMaxClimb);
            ecfg.AgentRadius = EditorGUILayout.FloatField("Radius", ecfg.AgentRadius);
            config.walkableSlopeAngle = EditorGUILayout.FloatField("Max Slope Angle", config.walkableSlopeAngle);

            // EDGE
            GUILayout.Label("Edge Settings", EditorStyles.boldLabel);
            EdgeLen = EditorGUILayout.FloatField("Max Len", EdgeLen);
            config.maxSimplificationError = EditorGUILayout.FloatField("Max Simplification Error", config.maxSimplificationError);
            RegionMinSize = EditorGUILayout.FloatField("Min Size", RegionMinSize);
            RegionMergeSize = EditorGUILayout.FloatField("Merge Size", RegionMergeSize);

            // QUALITY
            GUILayout.Label("Quality Settings", EditorStyles.boldLabel);
            config.maxVertsPerPoly = EditorGUILayout.IntField("Max verts per poly", config.maxVertsPerPoly);
            DetailSampleDist = EditorGUILayout.FloatField("Detail Sample List", DetailSampleDist);
            DetailSampleMaxError = EditorGUILayout.FloatField("Detail Sample Max Error", DetailSampleMaxError);

            EditorGUILayout.Space();
            GUILayout.Label("Dynamic Navmesh", EditorStyles.boldLabel);
            TileCacheThreads = EditorGUILayout.IntField("Max threads", TileCacheThreads);

            if (GUILayout.Button("Bake Recast Navmesh [TileCache]"))
            {
                BakeTileCache();
            }

            EditorGUILayout.Space();
            GUILayout.Label("Static Navmesh", EditorStyles.boldLabel);

            if (GUILayout.Button("Bake Recast Navmesh [Single Tile]"))
            {
                BakeSingleTile();
            }
        }

        void ProcessTerrain(ref float[] verts, ref int nverts, ref int[] tris, ref int ntris)
        {
            int w = terrain.heightmapWidth;
            int h = terrain.heightmapHeight;

            Vector3 meshScale = terrain.size;
            meshScale = new Vector3(meshScale.x / (w - 1) * 1, meshScale.y, meshScale.z / (h - 1) * 1);

            Vector2 uvScale = new Vector2(1.0f / (w - 1), 1.0f / (h - 1));
            float[,] tData = terrain.GetHeights(0, 0, w, h);

            nverts = w * h * 3;
            verts = new float[nverts];
            ntris = (w - 1) * (h - 1) * 2;
            tris = new int[ntris * 3];

            // Build vertices and UVs
            for (int z = 0; z < h; z++)
            {
                for (int x = 0; x < w; x++)
                {
                    Vector3 temp = Vector3.Scale(meshScale, new Vector3(z, tData[x, z], x)) + terrainPos;
                    verts[(z * w + x) * 3 + 0] = temp.x;
                    verts[(z * w + x) * 3 + 1] = temp.y;
                    verts[(z * w + x) * 3 + 2] = temp.z;
                }
            }

            int index = 0;
            // Build triangle indices: 3 indices into vertex array for each triangle
            for (int z = 0; z < h - 1; z++)
            {
                for (int x = 0; x < w - 1; x++)
                {
                    // For each grid cell output two triangles
                    tris[index++] = (z * w) + x + 1;
                    tris[index++] = ((z + 1) * w) + x;
                    tris[index++] = (z * w) + x;

                    tris[index++] = (z * w) + x + 1;
                    tris[index++] = ((z + 1) * w) + x + 1;
                    tris[index++] = ((z + 1) * w) + x;
                }
            }
        }

        void BakeTileCache()
        {
            UpdateProgress(0f);

            int nverts = 0;
            float[] verts = null;
            int ntris = 0;
            int[] tris = null;
            ProcessTerrain(ref verts, ref nverts, ref tris, ref ntris);

            UpdateProgress(0.25f);

            IntPtr vertsPtr = Marshal.AllocHGlobal(verts.Length * sizeof(float));
            Marshal.Copy(verts, 0, vertsPtr, verts.Length);

            IntPtr trisPtr = Marshal.AllocHGlobal(tris.Length * sizeof(int));
            Marshal.Copy(tris, 0, trisPtr, tris.Length);

            InputGeometry geom = new InputGeometry()
            {
                verts = vertsPtr,
                tris = trisPtr,
                nverts = nverts,
                ntris = ntris
            };

            UpdateProgress(0.5f);

            RecastConfig recastConfig = FindObjectOfType<RecastConfig>();
            Dictionary<string, ushort> areas = new Dictionary<string, ushort>();

            ushort k = 1;
            foreach (var layer in recastConfig.Layers)
            {
                areas.Add(layer.LayerID, k);
                TileCache.addFlag(k, 1);
                k *= 2;
            }

            DetourConvexVolume[] volumes = FindObjectsOfType<Pathfinding.DetourConvexVolume>();
            foreach (var volume in volumes)
            {
                TileCache.addConvexVolume(volume.floatNodes(), volume.nodes.Count, volume.maxY, volume.minY, areas[volume.AreaID]);
            }

            IntPtr tileCache = new IntPtr(0);
            IntPtr navMesh = new IntPtr(0);
            IntPtr navQuery = new IntPtr(0);
            TileCache.handleTileCacheBuild(ref config, ref ecfg, ref geom, ref tileCache, ref navMesh, ref navQuery);

            UpdateProgress(0.75f);

            // Create asset
            TileCacheAsset asset = CustomAssetUtility.CreateAssetWithoutSaving<TileCacheAsset>();
            IntPtr tilesHeader = new IntPtr(0);
            TileCache.getTileCacheHeaders(ref asset.header, ref tilesHeader, tileCache, navMesh);

            // Copy to asset
            asset.config = config;

            // Copy sizes
            int structSize = Marshal.SizeOf(typeof(TileCacheAsset.TileCacheTileHeader));
            asset.tilesHeader = new TileCacheAsset.TileCacheTileHeader[asset.header.numTiles];
            for (uint i = 0; i < asset.header.numTiles; ++i)
            {
                asset.tilesHeader[i] = (TileCacheAsset.TileCacheTileHeader)Marshal.PtrToStructure(new IntPtr(tilesHeader.ToInt64() + (structSize * i)), typeof(TileCacheAsset.TileCacheTileHeader));
            }

            // Copy data
            int dataSize = 0;
            int start = 0;
            for (uint i = 0; i < asset.header.numTiles; ++i)
            {
                dataSize += asset.tilesHeader[i].dataSize;
            }

            asset.tilesData = new byte[dataSize];

            for (uint i = 0; i < asset.header.numTiles; ++i)
            {
                IntPtr tilePtr = TileCache.getTileCacheTile(tileCache, (int)i);
                CompressedTile tile = (CompressedTile)Marshal.PtrToStructure(tilePtr, typeof(CompressedTile));

                asset.tilesHeader[i] = (TileCacheAsset.TileCacheTileHeader)Marshal.PtrToStructure(new IntPtr(tilesHeader.ToInt64() + (structSize * i)), typeof(TileCacheAsset.TileCacheTileHeader));

                if (asset.tilesHeader[i].dataSize > 0)
                {
                    Marshal.Copy(tile.data, asset.tilesData, start, asset.tilesHeader[i].dataSize);
                    start += asset.tilesHeader[i].dataSize;
                }
            }

            // Save asset
            CustomAssetUtility.SaveAsset<TileCacheAsset>(asset);

            // Close window
            UpdateProgress(0f);
            EditorUtility.ClearProgressBar();
        }

        void BakeSingleTile()
        {
            UpdateProgress(0f);

            int nverts = 0;
            float[] verts = null;
            int ntris = 0;
            int[] tris = null;
            ProcessTerrain(ref verts, ref nverts, ref tris, ref ntris);

            UpdateProgress(0.25f);

            config.walkableHeight = (int)Math.Ceiling(ecfg.AgentHeight / config.ch);
            config.walkableClimb = (int)Math.Floor(ecfg.AgentMaxClimb / config.ch);
            config.walkableRadius = (int)Math.Ceiling(ecfg.AgentRadius / config.cs);
            config.maxEdgeLen = (int)(EdgeLen / config.cs);
            config.minRegionArea = (int)Math.Pow(RegionMinSize, 2);
            config.mergeRegionArea = (int)Math.Pow(RegionMergeSize, 2);
            config.detailSampleDist = DetailSampleDist < 0.9f ? 0 : config.cs * DetailSampleDist;
            config.detailSampleMaxError = DetailSampleMaxError * config.ch;

            // Generate Recast
            Recast.handleBuild(ref config, verts, nverts, tris, ntris);

            // Fetch navmeshes
            IntPtr polyPtr = Recast.getPolyMesh();
            IntPtr detailPtr = Recast.getPolyMeshDetail();
            PolyMesh mesh = (PolyMesh)Marshal.PtrToStructure(polyPtr, typeof(PolyMesh));
            PolyMeshDetail detail = (PolyMeshDetail)Marshal.PtrToStructure(detailPtr, typeof(PolyMeshDetail));

            UpdateProgress(0.5f);

            // Create asset
            PolyMeshAsset asset = CustomAssetUtility.CreateAssetWithoutSaving<PolyMeshAsset>();

            // Save config
            asset.config = config;

            // Set poly data
            asset.PolyMesh.nverts = mesh.nverts;
            asset.PolyMesh.npolys = mesh.npolys;
            asset.PolyMesh.maxpolys = mesh.maxpolys;
            asset.PolyMesh.nvp = mesh.nvp;
            CopyArray<float>(mesh.bmin, ref asset.PolyMesh.bmin, 3);
            CopyArray<float>(mesh.bmax, ref asset.PolyMesh.bmax, 3);
            asset.PolyMesh.cs = mesh.cs;
            asset.PolyMesh.ch = mesh.ch;
            asset.PolyMesh.borderSize = mesh.borderSize;

            asset.PolyMesh.verts = new ushort[3 * mesh.nverts];
            CopyArray(mesh.verts, asset.PolyMesh.verts, 3 * mesh.nverts);

            asset.PolyMesh.polys = new ushort[mesh.maxpolys * 2 * mesh.nvp];
            CopyArray(mesh.polys, asset.PolyMesh.polys, mesh.maxpolys * 2 * mesh.nvp);

            asset.PolyMesh.regs = new ushort[mesh.maxpolys];
            CopyArray(mesh.regs, asset.PolyMesh.regs, mesh.maxpolys);

            asset.PolyMesh.flags = new ushort[mesh.npolys];
            CopyArray(mesh.flags, asset.PolyMesh.flags, mesh.npolys);

            asset.PolyMesh.areas = new byte[mesh.maxpolys];
            CopyArray(mesh.areas, asset.PolyMesh.areas, mesh.maxpolys);

            // Set detail data
            asset.PolyDetailMesh.nmeshes = detail.nmeshes;
            asset.PolyDetailMesh.nverts = detail.nverts;
            asset.PolyDetailMesh.ntris = detail.ntris;

            asset.PolyDetailMesh.meshes = new uint[4 * detail.nmeshes];
            CopyArray(detail.meshes, asset.PolyDetailMesh.meshes, 4 * detail.nmeshes);

            asset.PolyDetailMesh.verts = new float[3 * detail.nverts];
            CopyArray(detail.verts, asset.PolyDetailMesh.verts, 3 * detail.nverts);

            asset.PolyDetailMesh.tris = new byte[4 * detail.ntris];
            CopyArray(detail.tris, asset.PolyDetailMesh.tris, 4 * detail.ntris);

            // Save asset again
            UpdateProgress(0.75f);
            CustomAssetUtility.SaveAsset<PolyMeshAsset>(asset);

            // Close window
            UpdateProgress(0f);
            EditorUtility.ClearProgressBar();
        }

        private void CopyArray(IntPtr from, byte[] dest, int size)
        {
            if (size > 0)
            {
                if (from != null)
                {
                    Marshal.Copy(from, dest, 0, size);
                }
            }
        }

        private void CopyArray(IntPtr from, ushort[] dest, int size)
        {
            if (size > 0)
            {
                if (from != null)
                {
                    short[] tmp = new short[size];
                    Marshal.Copy(from, tmp, 0, size);

                    System.Buffer.BlockCopy(tmp, 0, dest, 0, size * sizeof(ushort));
                }
            }
        }

        private void CopyArray(IntPtr from, uint[] dest, int size)
        {
            if (size > 0)
            {
                if (from != null)
                {
                    int[] tmp = new int[size];
                    Marshal.Copy(from, tmp, 0, size);

                    System.Buffer.BlockCopy(tmp, 0, dest, 0, size * sizeof(uint));
                }
            }
        }

        private void CopyArray(IntPtr from, float[] dest, int size)
        {
            if (size > 0)
            {
                if (from != null)
                {
                    Marshal.Copy(from, dest, 0, size);
                }
            }
        }

        private void CopyArray<T>(T[] from, ref T[] dest, int size)
        {
            dest = new T[size];
            System.Buffer.BlockCopy(from, 0, dest, 0, size * Marshal.SizeOf(typeof(T)));
        }

        void UpdateProgress(float percentage)
        {
            EditorUtility.DisplayProgressBar("Baking navmesh", "This might take a while...", percentage);
        }
    }
}

#endif
