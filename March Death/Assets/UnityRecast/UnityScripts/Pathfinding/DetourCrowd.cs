using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

namespace Pathfinding
{
    public class DetourCrowd : MonoBehaviour
    {
		public PolyMeshAsset polymesh;
		public TileCacheAsset navmeshData;

        public int MaxAgents = 1024;
        public float AgentMaxRadius = 2;

        private IntPtr crowd = new IntPtr(0);
        private Dictionary<int, DetourAgent> agents = new Dictionary<int, DetourAgent>();

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
			PathDetour.get.Initialize(navmeshData);
			crowd = Detour.Crowd.createCrowd(MaxAgents, AgentMaxRadius, PathDetour.get.NavMesh);
            positions = new float[MaxAgents * 3];
            velocities = new float[MaxAgents * 3];
            targetStates = new byte[MaxAgents];
            states = new byte[MaxAgents];
        }

        public static float[] ToFloat(Vector3 p)
        {
            return new float[] { p.x, p.y, p.z };
        }

        public static Vector3 ToVector3(float[] p, int off)
        {
            return new Vector3(p[off + 0], p[off + 1], p[off + 2]);
        }

        public int AddAgent(DetourAgent agent, float radius, float height)
        {
            int idx = Detour.Crowd.addAgent(crowd, ToFloat(agent.transform.position), radius, height);
            if (idx != -1)
            {
                agents.Add(idx, agent);
            }

            return idx;
        }

        public void SetAgentParemeters(int idx, float maxAcceleration, float maxSpeed)
        {
            Detour.Crowd.updateAgent(crowd, idx, maxAcceleration, maxSpeed);
        }

        public void RemoveAgent(int idx)
        {
            Detour.Crowd.removeAgent(crowd, idx);
            agents.Remove(idx);
        }

        public void MoveTarget(int idx, Vector3 target)
        {
            Detour.Crowd.setMoveTarget(PathDetour.get.NavQuery, crowd, idx, ToFloat(target), false);
        }

        public void ResetPath(int idx)
        {
            Detour.Crowd.resetPath(crowd, idx);
        }

        public void Update()
        {
            Detour.Crowd.updateTick(PathDetour.get.TileCache, PathDetour.get.NavMesh, crowd, Time.deltaTime, positions, velocities, states, targetStates, ref numUpdated);

            foreach (KeyValuePair<int, DetourAgent> entry in agents)
            {
                DetourAgent agent = entry.Value;
                agent.Velocity = ToVector3(velocities, entry.Key * 3);
                agent.State = (DetourAgent.CrowdAgentState)states[entry.Key];
                agent.TargetState = (DetourAgent.MoveRequestState)targetStates[entry.Key];

                if (agent.Velocity.sqrMagnitude != 0)
                {
                    Vector3 newPosition = ToVector3(positions, entry.Key * 3);
                    Quaternion lookRotation = Quaternion.LookRotation(agent.Velocity);

                    agent.transform.position = newPosition;
                    agent.transform.rotation = lookRotation;
                }
            }
        }
    }
}
