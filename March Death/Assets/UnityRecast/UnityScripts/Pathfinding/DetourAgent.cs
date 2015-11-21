using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Pathfinding
{
    public class DetourAgent : MonoBehaviour
    {
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

        public float Radius = 0.8f;
        public float Height = 2.0f;
        public float MaxSpeed = 2.0f;
        public float MaxAcceleration = 2.0f;

        private int idx = -1;
        private bool updateScheduled = false;
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
            idx = DetourCrowd.Instance.AddAgent(this, Radius, Height);
        }

        public void RemoveFromCrowd()
        {
            DetourCrowd.Instance.RemoveAgent(idx);
            idx = -1;
        }

        public void Start()
        {
            AddToCrowd();
        }

        public void OnDestroy()
        {
            RemoveFromCrowd();
        }

        public void SetMaxSpeed(float maxSpeed)
        {
            MaxSpeed = maxSpeed;
            updateScheduled = true;
        }

        public void SetMaxAcceleration(float maxAcceleration)
        {
            MaxAcceleration = maxAcceleration;
            updateScheduled = true;
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

        public void Update()
        {
            if (updateScheduled)
            {
                DetourCrowd.Instance.SetAgentParemeters(idx, MaxAcceleration, MaxSpeed);
                updateScheduled = false;
            }
        }
    }
}
