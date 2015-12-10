using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Utils;

namespace Pathfinding
{
    public sealed class DetourCrowd : MonoBehaviour, IDisposable
    {
        #region DLL Imports
        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr createCrowd(int maxAgents, float maxRadius, IntPtr navmesh);

        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setFilter(IntPtr crowd, int filter, ushort include, ushort exclude);

        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern int addAgent(IntPtr crowd, float[] p, ref CrowdAgentParams ap);

        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getAgent(IntPtr crowd, int idx);

        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern void updateAgent(IntPtr crowd, int idx, ref CrowdAgentParams ap);

        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern void removeAgent(IntPtr crowd, int idx);

        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setMoveTarget(IntPtr navquery, IntPtr crowd, int idx, float[] p, bool adjust, int filterIndex);

        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern void resetPath(IntPtr crowd, int idx);

        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern void updateTick(IntPtr tileCache, IntPtr nav, IntPtr crowd, float dt, float[] positions, float[] velocities, byte[] states, byte[] targetStates, bool[] partial, ref int nagents);

        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool isPointValid(IntPtr crowd, float[] targetPoint);

        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool randomPoint(IntPtr crowd, float[] targetPoint);

        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool randomPointInCircle(IntPtr crowd, float[] initialPoint, float maxRadius, float[] targetPoint);
        #endregion

        #region Unity Attributes
        public enum RenderMode { POLYS, DETAIL_POLYS, TILE_POLYS }

        public PolyMeshAsset polymesh;
        public TileCacheAsset navmeshData;

        public int MaxAgents = 1024;
        public float AgentMaxRadius = 2;
        #endregion

        #region Mesh Debugging
        public bool RenderInGame = false;
        public Material material;
        public RenderMode Mode;
        private DbgRenderMesh mesh = new DbgRenderMesh();
        #endregion

        #region Internal Library Memory Wrappers
        private float[] randomSample = null;
        private float[] positions = null;
        private float[] velocities = null;
        private byte[] targetStates = null;
        private byte[] states = null;
        private bool[] partial = null;
        private int numUpdated = 0;
        #endregion

        // Global access
        public static DetourCrowd Instance;

        // Use a HandleRef to avoid race conditions;
        // see the GC-Safe P/Invoke Code section
        private HandleRef _crowd;
        private TileCache _tileCache;

        private List<DetourAgent> agents = new List<DetourAgent>();
        
        public void OnEnable()
        {
            RecastConfig recastConfig = GameObject.FindObjectOfType<RecastConfig>();
            _tileCache = new TileCache(navmeshData, recastConfig);

            IntPtr h = createCrowd(MaxAgents, AgentMaxRadius, _tileCache.NavMeshHandle.Handle);
            _crowd = new HandleRef(this, h);
            
            ushort k = 0;
            foreach (var filter in recastConfig.Filters)
            {
                ushort include = 0;
                ushort exclude = 0;

                foreach (var incl in filter.Include)
                {
                    include |= recastConfig.Areas[incl.Name];
                }

                foreach (var excl in filter.Exclude)
                {
                    exclude |= recastConfig.Areas[excl.Name];
                }

                setFilter(_crowd.Handle, k, include, exclude);
                ++k;
            }

            randomSample = new float[3];
            positions = new float[MaxAgents * 3];
            velocities = new float[MaxAgents * 3];
            targetStates = new byte[MaxAgents];
            states = new byte[MaxAgents];
            partial = new bool[MaxAgents];
            
            Instance = this;

            if (RenderInGame)
            {
                mesh.Clear();

                switch (Mode)
                {
                    case RenderMode.POLYS:
                        Assert.IsTrue(polymesh != null);

                        RecastDebug.ShowRecastNavmesh(mesh, polymesh.PolyMesh, polymesh.config);
                        break;

                    case RenderMode.DETAIL_POLYS:
                        Assert.IsTrue(polymesh != null);

                        RecastDebug.ShowRecastDetailMesh(mesh, polymesh.PolyDetailMesh);
                        break;

                    case RenderMode.TILE_POLYS:
                        for (int i = 0; i < navmeshData.header.numTiles; ++i)
                            RecastDebug.ShowTilePolyDetails(mesh, _tileCache.NavMeshHandle.Handle, i);
                        break;
                }

                //RecastDebug.RenderObstacles(_tileCache.TileCacheHandle.Handle);

                mesh.CreateGameObjects("RecastRenderer", material);
                mesh.Rebuild();
            }
        }

        // Provide access to 3rd party code
        public HandleRef CrowdHandle
        {
            get { return _crowd; }
        }

        public TileCache TileCache
        {
            get { return _tileCache; }
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
            // Disable other components first
            DetourAgent[] agents = FindObjectsOfType<DetourAgent>();
            foreach (DetourAgent script in agents)
            {
                script.enabled = false;
            }

            DetourObstacle[] obstacles = FindObjectsOfType<DetourObstacle>();
            foreach (DetourObstacle script in obstacles)
            {
                script.enabled = false;
            }

            Cleanup();
        }

        // Really dispose of the resource
        private void Cleanup()
        {
            //DeleteResource(Handle);

            // Don't permit the handle to be used again.
            _crowd = new HandleRef(this, IntPtr.Zero);
            Instance = null;
        }

        public int AddAgent(DetourAgent agent, CrowdAgentParams ap)
        {
            Assert.IsTrue(_crowd.Handle.ToInt64() != 0);

            int idx = addAgent(_crowd.Handle, agent.transform.position.ToFloat(), ref ap);
            if (idx != -1)
            {
                agents.Add(agent);
            }

            return idx;
        }

        public void UpdateAgentParemeters(int idx, CrowdAgentParams ap)
        {
            Assert.IsTrue(_crowd.Handle.ToInt64() != 0);

            updateAgent(_crowd.Handle, idx, ref ap);
        }

        public void RemoveAgent(DetourAgent agent)
        {
            Assert.IsTrue(_crowd.Handle.ToInt64() != 0);

            removeAgent(_crowd.Handle, agent.ID);
            agents.Remove(agent);
        }

        public void MoveTarget(DetourAgent agent, Vector3 target)
        {
            Assert.IsTrue(_crowd.Handle.ToInt64() != 0);
            Assert.IsTrue(_tileCache.NavQueryHandle.Handle.ToInt64() != 0);

            setMoveTarget(_tileCache.NavQueryHandle.Handle, _crowd.Handle, agent.ID, target.ToFloat(), false, agent.FilterIndex);
        }

        public void ResetPath(int idx)
        {
            Assert.IsTrue(_crowd.Handle.ToInt64() != 0);

            resetPath(_crowd.Handle, idx);
        }

        public bool IsPointValid(Vector3 point)
        {
            return isPointValid(_crowd.Handle, point.ToFloat());
        }

        public bool RandomValidPoint(ref Vector3 dest)
        {
            Assert.IsTrue(_crowd.Handle.ToInt64() != 0);

            if (randomPoint(_crowd.Handle, randomSample))
            {
                dest = randomSample.ToVector3();
                return true;
            }

            return false;
        }

        public bool RandomValidPointInCircle(Vector3 cercleCenter, float maxRadius, ref Vector3 dest)
        {
            Assert.IsTrue(_crowd.Handle.ToInt64() != 0);

            if (randomPointInCircle(_crowd.Handle, cercleCenter.ToFloat(), maxRadius, randomSample))
            {
                dest = randomSample.ToVector3();
                return true;
            }

            return false;
        }

        public void Update()
        {
            Assert.IsTrue(_crowd.Handle.ToInt64() != 0);
            Assert.IsTrue(_tileCache.TileCacheHandle.Handle.ToInt64() != 0);
            Assert.IsTrue(_tileCache.NavMeshHandle.Handle.ToInt64() != 0);

            updateTick(_tileCache.TileCacheHandle.Handle, _tileCache.NavMeshHandle.Handle, _crowd.Handle, Time.deltaTime, positions, velocities, states, targetStates, partial, ref numUpdated);

            foreach (DetourAgent agent in agents)
            {
                agent.Velocity = velocities.ToVector3(agent.ID * 3);
                agent.State = (DetourAgent.CrowdAgentState)states[agent.ID];
                agent.TargetState = (DetourAgent.MoveRequestState)targetStates[agent.ID];
                agent.IsPathPartial = partial[agent.ID];

                Vector3 newPosition = positions.ToVector3(agent.ID * 3);
                agent.transform.position = newPosition;

                if (agent.Velocity.sqrMagnitude != 0)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(agent.Velocity);

                    agent.transform.rotation = lookRotation;
                }
            }
        }
    }
}
