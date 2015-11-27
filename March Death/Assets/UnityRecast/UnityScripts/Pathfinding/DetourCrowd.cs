using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using System.IO;

namespace Pathfinding
{
    [RequireComponent(typeof(RecastConfig))]
    public class DetourCrowd : MonoBehaviour
    {
        public enum RenderMode { POLYS, DETAIL_POLYS, TILE_POLYS }

		public PolyMeshAsset polymesh;
		public TileCacheAsset navmeshData;

        public int MaxAgents = 1024;
        public float AgentMaxRadius = 2;

        public bool RenderInEditor = false;
        public Material material;
        public RenderMode Mode;
        private DbgRenderMesh mesh = new DbgRenderMesh();

        private IntPtr crowd = new IntPtr(0);
        private Dictionary<int, DetourAgent> agents = new Dictionary<int, DetourAgent>();

        private float[] randomSample = null;
        private float[] positions = null;
        private float[] velocities = null;
        private byte[] targetStates = null;
        private byte[] states = null;
        int numUpdated = 0;

        public static DetourCrowd Instance = null;

        public void Awake()
        {
            if (Instance != null)
            {
                throw new Exception("No more than one DetourCrowd might exist");
            }

            Instance = this;
        }

        public void OnEnable()
        {
            if (navmeshData != null)
            {

                RecastConfig recastConfig = FindObjectOfType<RecastConfig>();
                Dictionary<string, ushort> areas = new Dictionary<string, ushort>();

                PathDetour.get.Initialize(navmeshData, () =>
                {
                    ushort n = 1;
                    foreach (var layer in recastConfig.Layers)
                    {
                        areas.Add(layer.LayerID, n);
                        TileCache.addFlag(n, 1);
                        n *= 2;
                    }
                });
    			crowd = Detour.Crowd.createCrowd(MaxAgents, AgentMaxRadius, PathDetour.get.NavMesh);

                ushort k = 0;
                foreach (var filter in recastConfig.Filters)
                {
                    ushort include = 0;
                    ushort exclude = 0;

                    foreach (var incl in filter.Include)
                    {
                        include |= areas[incl.Name];
                    }

                    foreach (var excl in filter.Exclude)
                    {
                        exclude |= areas[excl.Name];
                    }
                    
                    Detour.Crowd.setFilter(crowd, k, include, exclude);
                    ++k;
                }

                randomSample = new float[3];
                positions = new float[MaxAgents * 3];
                velocities = new float[MaxAgents * 3];
                targetStates = new byte[MaxAgents];
                states = new byte[MaxAgents];

                if (!Application.isPlaying && RenderInEditor)
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
                                RecastDebug.ShowTilePolyDetails(mesh, PathDetour.get.NavMesh, i);
                            break;
                    }

                    RecastDebug.RenderObstacles(PathDetour.get.TileCache);

                    mesh.CreateGameObjects("RecastRenderer", material);
                    mesh.Rebuild();
                }
            }
        }

        public static float[] ToFloat(Vector3 p)
        {
            return new float[] { p.x, p.y, p.z };
        }

        public static Vector3 ToVector3(float[] p, int off = 0)
        {
            return new Vector3(p[off + 0], p[off + 1], p[off + 2]);
        }

        public int AddAgent(DetourAgent agent, CrowdAgentParams ap)
        {
            Assert.IsTrue(crowd.ToInt64() != 0);

            int idx = Detour.Crowd.addAgent(crowd, ToFloat(agent.transform.position), ref ap);
            if (idx != -1)
            {
                agents.Add(idx, agent);
            }

            return idx;
        }

        public void UpdateAgentParemeters(int idx, CrowdAgentParams ap)
        {
            Assert.IsTrue(crowd.ToInt64() != 0);

            Detour.Crowd.updateAgent(crowd, idx, ref ap);
        }

        public void RemoveAgent(int idx)
        {
            Assert.IsTrue(crowd.ToInt64() != 0);

            Detour.Crowd.removeAgent(crowd, idx);
            agents.Remove(idx);
        }

        public void MoveTarget(int idx, Vector3 target)
        {
            Assert.IsTrue(crowd.ToInt64() != 0);

            Detour.Crowd.setMoveTarget(PathDetour.get.NavQuery, crowd, idx, ToFloat(target), false);
        }

        public void ResetPath(int idx)
        {
            Assert.IsTrue(crowd.ToInt64() != 0);

            Detour.Crowd.resetPath(crowd, idx);
        }

        public bool RandomValidPoint(ref Vector3 dest)
        {
            if (Detour.Crowd.randomPoint(crowd, randomSample))
            {
                dest = ToVector3(randomSample);
                return true;
            }

            return false;
        }

        public bool RandomValidPointInCircle(Vector3 cercleCenter, float maxRadius, ref Vector3 dest)
        {
            if (Detour.Crowd.randomPointInCircle(crowd, ToFloat(cercleCenter), maxRadius, randomSample))
            {
                dest = ToVector3(randomSample);
                return true;
            }

            return false;
        }

        public void Update()
        {
            Assert.IsTrue(crowd.ToInt64() != 0);

            Detour.Crowd.updateTick(PathDetour.get.TileCache, PathDetour.get.NavMesh, crowd, Time.deltaTime, positions, velocities, states, targetStates, ref numUpdated);

            foreach (KeyValuePair<int, DetourAgent> entry in agents)
            {
                DetourAgent agent = entry.Value;
                agent.Velocity = ToVector3(velocities, entry.Key * 3);
                agent.State = (DetourAgent.CrowdAgentState)states[entry.Key];
                agent.TargetState = (DetourAgent.MoveRequestState)targetStates[entry.Key];

                Vector3 newPosition = ToVector3(positions, entry.Key * 3);
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
