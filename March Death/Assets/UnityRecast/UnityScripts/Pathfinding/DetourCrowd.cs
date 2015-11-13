using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Pathfinding
{
    public class DetourCrowd : MonoBehaviour
    {
        public int MaxAgents = 1024;
        public int AgentMaxRadius = 2;

        private IntPtr crowd = new IntPtr(0);
        private Dictionary<int, IGameEntity> agents = new Dictionary<int, IGameEntity>();

        private float[] positionHolder = null;
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
            crowd = Detour.Crowd.createCrowd(MaxAgents, AgentMaxRadius, PathDetour.get.NavMesh);
            positionHolder = new float[MaxAgents * 3];
        }

        public static float[] ToFloat(Vector3 p)
        {
            return new float[] { p.x, p.y, p.z };
        }

        public static Vector3 ToVector3(float[] p, int off)
        {
            return new Vector3(p[off + 0], p[off + 1], p[off + 2]);
        }

        public int AddAgent(IGameEntity entity)
        {
            // HACK: Height is hardcoded
            int idx = Detour.Crowd.addAgent(crowd, ToFloat(entity.getTransform().position), AgentMaxRadius, 2);
            if (idx != -1)
            {
                agents.Add(idx, entity);
            }

            return idx;
        }

        public void MoveTarget(int idx, Vector3 target)
        {
            Detour.Crowd.setMoveTarget(PathDetour.get.NavQuery, crowd, idx, ToFloat(target), false);
        }

        public void Update()
        {
            Detour.Crowd.updateTick(PathDetour.get.NavMesh, crowd, Time.deltaTime, positionHolder, ref numUpdated);

            foreach (KeyValuePair<int, IGameEntity> entry in agents)
            {
                Debug.Log(ToVector3(positionHolder, entry.Key * 3));
                entry.Value.getTransform().position = ToVector3(positionHolder, entry.Key * 3);
            }
        }
    }
}
