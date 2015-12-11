using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using Utils;

namespace Pathfinding
{
    public sealed class TileCache : IDisposable
    {
        #region DLL Import
        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool handleTileCacheBuild(ref Config cfg, ref ExtendedConfig ecfg, ref InputGeometry geom, ref IntPtr tileCache, ref IntPtr navMesh, ref IntPtr navQuery);

        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern void getTileCacheHeaders(ref TileCacheAsset.TileCacheSetHeader header, ref IntPtr tilesHeader, IntPtr tileCache, IntPtr navMesh);

        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool loadFromTileCacheHeaders(ref TileCacheAsset.TileCacheSetHeader header, TileCacheAsset.TileCacheTileHeader[] tilesHeader, byte[] data, ref IntPtr tileCache, ref IntPtr navMesh, ref IntPtr navQuery);

        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern void addConvexVolume(float[] verts, int nverts, float hmax, float hmin, int area);

        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern void addFlag(ushort area, ushort cost);

        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getTileCacheTile(IntPtr tileCache, int i);

        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint addObstacle(IntPtr tileCache, float[] pos, float[] verts, int nverts, int height);

        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern void removeObstacle(IntPtr tileCache, uint reference);

        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getObstacles(IntPtr tileCache, ref int nobstacles);

        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint addAreaFlags(IntPtr tileCache, IntPtr crowd, float[] center, float[] verts, int nverts, float height, ushort flags);

        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern void removeAreaFlags(IntPtr tileCache, uint reference);
        #endregion

        #region Private Attributes
        private IntPtr _tileCache = new IntPtr(0);
        private IntPtr _navMesh = new IntPtr(0);
        private IntPtr _navQuery = new IntPtr(0);
        #endregion

        #region Handles
        private HandleRef _tileCacheHandle;
        private HandleRef _navMeshHandle;
        private HandleRef _navQueryHandle;
        #endregion

        public TileCache(TileCacheAsset navmeshData, RecastConfig recastConfig)
        {
            Assert.IsTrue(Pathfinding.Detour.pointerSize() == IntPtr.Size);
            Pathfinding.Recast.DefaultConfig(Application.dataPath + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "Recast.log");

            recastConfig.SetupAreas();

            bool result = loadFromTileCacheHeaders(ref navmeshData.header, navmeshData.tilesHeader, navmeshData.tilesData, ref _tileCache, ref _navMesh, ref _navQuery);
            if (!result)
            {
                throw new ArgumentException("Invalid navmesh data");
            }
            
            _tileCacheHandle = new HandleRef(this, _tileCache);
            _navMeshHandle = new HandleRef(this, _navMesh);
            _navQueryHandle = new HandleRef(this, _navQuery);
        }

        // Provide access to 3rd party code
        public HandleRef TileCacheHandle
        {
            get { return _tileCacheHandle; }
        }
        public HandleRef NavMeshHandle
        {
            get { return _navMeshHandle; }
        }
        public HandleRef NavQueryHandle
        {
            get { return _navQueryHandle; }
        }

        // Dispose of the resource
        public void Dispose()
        {
            Cleanup();

            // Prevent the object from being placed on the
            // finalization queue
            System.GC.SuppressFinalize(this);
        }

        // Finalizer provided in case Dispose isn't called.
        // This is a fallback mechanism, but shouldn't be
        // relied upon (see previous discussion).
        public void OnDestroy()
        {
            Cleanup();
        }

        // Really dispose of the resource
        private void Cleanup()
        {
            //DeleteResource(Handle);

            // Don't permit the handle to be used again.
            _tileCacheHandle = new HandleRef(this, IntPtr.Zero);
            _navMeshHandle = new HandleRef(this, IntPtr.Zero);
            _navQueryHandle = new HandleRef(this, IntPtr.Zero);
        }

        public uint AddObstacle(DetourObstacle block)
        {
            Assert.IsTrue(_tileCacheHandle.Handle.ToInt64() != 0);

            Vector3[] blockVertices = block.Vertices();
            Vector3 pos = block.transform.position;

            float[] vertices =
            {
                blockVertices[0].x, blockVertices[0].y - 3.0f, blockVertices[0].z,
                blockVertices[1].x, blockVertices[1].y - 3.0f, blockVertices[1].z,
                blockVertices[2].x, blockVertices[2].y - 3.0f, blockVertices[2].z,
                blockVertices[3].x, blockVertices[3].y - 3.0f, blockVertices[3].z,
            };

            float[] position =
            {
                pos.x, pos.y - 3.0f, pos.z
            };

            return addObstacle(_tileCacheHandle.Handle, position, vertices, 4, (int)Mathf.Ceil(block.Size.y));
        }

        public void RemoveObstacle(uint reference)
        {
            Assert.IsTrue(_tileCacheHandle.Handle.ToInt64() != 0);
            removeObstacle(_tileCacheHandle.Handle, reference);
        }

        public void AddAreaFlags(DetourFlag flag)
        {
            Assert.IsTrue(_tileCacheHandle.Handle.ToInt64() != 0);
            Assert.IsTrue(DetourCrowd.Instance.CrowdHandle.Handle.ToInt64() != 0);

            Vector3[] flagVertices = flag.Vertices();

            float[] vertices =
            {
                flagVertices[0].x, flagVertices[0].y - 3.0f, flagVertices[0].z,
                flagVertices[1].x, flagVertices[1].y - 3.0f, flagVertices[1].z,
                flagVertices[2].x, flagVertices[2].y - 3.0f, flagVertices[2].z,
                flagVertices[3].x, flagVertices[3].y - 3.0f, flagVertices[3].z,
            };


            Debug.Log("Setting flags " + flag.Flags);
            flag.ID = addAreaFlags(_tileCacheHandle.Handle, DetourCrowd.Instance.CrowdHandle.Handle, flag.Center.ToFloat(), vertices, 4, flag.Size.y, flag.Flags);
        }

        public void RemoveAreaFlag(DetourFlag flag)
        {
            Assert.IsTrue(_tileCacheHandle.Handle.ToInt64() != 0);
            removeAreaFlags(_tileCacheHandle.Handle, flag.ID);
        }
    }
}
