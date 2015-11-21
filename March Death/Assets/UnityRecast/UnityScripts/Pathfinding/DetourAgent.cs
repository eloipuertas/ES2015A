using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Pathfinding
{
    public class DetourAgent : MonoBehaviour
    {
        public enum UpdateFlags
        {
        	DT_CROWD_ANTICIPATE_TURNS = 1,
        	DT_CROWD_OBSTACLE_AVOIDANCE = 2,
        	DT_CROWD_SEPARATION = 4,
        	DT_CROWD_OPTIMIZE_VIS = 8,			///< Use #dtPathCorridor::optimizePathVisibility() to optimize the agent path.
        	DT_CROWD_OPTIMIZE_TOPO = 16,		///< Use dtPathCorridor::optimizePathTopology() to optimize the agent path.
        };

        public enum ObstacleAvoidanceType
        {
            LOW = 0,
            MEDIUM,
            GOOD,
            HIGH
        }

        public enum CrowdAgentState
        {
            DT_CROWDAGENT_STATE_INVALID,        //< The agent is not in a valid state.
            DT_CROWDAGENT_STATE_WALKING,        //< The agent is traversing a normal navigation mesh polygon.
            DT_CROWDAGENT_STATE_OFFMESH,        //< The agent is traversing an off-mesh connection.
        };

        public enum MoveRequestState
        {
            DT_CROWDAGENT_TARGET_NONE = 0,
            DT_CROWDAGENT_TARGET_FAILED,
            DT_CROWDAGENT_TARGET_VALID,
            DT_CROWDAGENT_TARGET_REQUESTING,
            DT_CROWDAGENT_TARGET_WAITING_FOR_QUEUE,
            DT_CROWDAGENT_TARGET_WAITING_FOR_PATH,
            DT_CROWDAGENT_TARGET_VELOCITY,
        };

        private CrowdAgentParams ap = new CrowdAgentParams();
        public float Radius = 0.8f;
        public float Height = 2.0f;
        public float MaxSpeed = 2.0f;
        public float MaxAcceleration = 2.0f;
        [SerializeField] [EnumFlagsAttribute]
        public UpdateFlags Flags = UpdateFlags.DT_CROWD_ANTICIPATE_TURNS |
            UpdateFlags.DT_CROWD_OBSTACLE_AVOIDANCE | UpdateFlags.DT_CROWD_SEPARATION |
            UpdateFlags.DT_CROWD_OPTIMIZE_VIS | UpdateFlags.DT_CROWD_OPTIMIZE_TOPO;
        public ObstacleAvoidanceType AvoidanceType = ObstacleAvoidanceType.MEDIUM;
        public float SeparationWeight = 0.5f;

        private bool firstUpdate = true;

        private int idx = -1;
        private Vector3 targetPoint;
        private float _lastKnownDistance;

        public Vector3 Velocity { get; set; }
        public CrowdAgentState State { get; set; }
        public MoveRequestState TargetState { get; set; }
        public float LastKnownDistance { get { return _lastKnownDistance; } }
        public bool IsMoving
        {
            get
            {
                // TODO: Adaptative target distance
                _lastKnownDistance = (transform.position - targetPoint).sqrMagnitude;
                if (_lastKnownDistance > 50f)
                    return true;

                return Velocity.sqrMagnitude >= 0.1;
            }
        }

        public void AddToCrowd()
        {
            idx = DetourCrowd.Instance.AddAgent(this, ap);
        }

        public void RemoveFromCrowd()
        {
            DetourCrowd.Instance.RemoveAgent(idx);
            idx = -1;
        }

        public void UpdateParams()
        {
            ap.radius = Radius;
            ap.height = Height;
            ap.maxAcceleration = MaxAcceleration;
            ap.maxSpeed = MaxSpeed;
            ap.collisionQueryRange = ap.radius * 12.0f;
        	ap.pathOptimizationRange = ap.radius * 30.0f;
            ap.updateFlags = (byte)Flags;
            ap.obstacleAvoidanceType = (byte)AvoidanceType;
            ap.separationWeight = SeparationWeight;

            if (!firstUpdate)
            {
                DetourCrowd.Instance.UpdateAgentParemeters(idx, ap);
            }
            else
            {
                firstUpdate = false;
            }
        }

        public void Start()
        {
            UpdateParams();
            AddToCrowd();
        }

        public void OnDestroy()
        {
            RemoveFromCrowd();
        }

        public void MoveTo(Vector3 target)
        {
            targetPoint = target;
            DetourCrowd.Instance.MoveTarget(idx, target);
        }

        public void ResetPath()
        {
            DetourCrowd.Instance.ResetPath(idx);
        }
    }
}
