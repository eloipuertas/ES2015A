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
        public float AgentMaxRadius = 2;

        private IntPtr crowd = new IntPtr(0);
        private Dictionary<int, DetourAgent> agents = new Dictionary<int, DetourAgent>();

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

        public int AddAgent(IGameEntity entity, float radius, float height)
        {
            int idx = Detour.Crowd.addAgent(crowd, ToFloat(entity.getTransform().position), radius, height);
            if (idx != -1)
            {
                agents.Add(idx, entity.getGameObject().GetComponent<DetourAgent>());
            }

            return idx;
        }

        public void SetAgentParemeters(int idx, float maxAcceleration, float maxSpeed)
        {
            Detour.Crowd.updateAgent(crowd, idx, maxAcceleration, maxSpeed);
        }

        public void MoveTarget(int idx, Vector3 target)
        {
            Detour.Crowd.setMoveTarget(PathDetour.get.NavQuery, crowd, idx, ToFloat(target), false);
        }

        public void Update()
        {
            Detour.Crowd.updateTick(PathDetour.get.TileCache, PathDetour.get.NavMesh, crowd, Time.deltaTime, positionHolder, ref numUpdated);

            foreach (KeyValuePair<int, DetourAgent> entry in agents)
            {
                DetourAgent agent = entry.Value;

                if (agent.IsMoving)
                {
                    Transform entityTransform = entry.Value.transform;
                    Vector3 newPosition = ToVector3(positionHolder, entry.Key * 3);

                    Vector3 direction = (newPosition - entityTransform.position).normalized;
                    if (direction.sqrMagnitude != 0)
                    {
                        Quaternion lookRotation = Quaternion.LookRotation(direction);
                        lookRotation = Quaternion.Slerp(entityTransform.rotation, lookRotation, Time.deltaTime * 10f);
                        Vector3 eulerAngles = lookRotation.eulerAngles;
                        eulerAngles = new Vector3(0, eulerAngles.y, 0);

                        entityTransform.position = newPosition;
                        entityTransform.rotation = Quaternion.Euler(eulerAngles);
                    }
                }
            }
        }
    }
}
