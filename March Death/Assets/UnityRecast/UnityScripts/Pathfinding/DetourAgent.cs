using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Pathfinding
{
    [RequireComponent(typeof(IGameEntity))]
    public class DetourAgent : MonoBehaviour
    {
        public float Radius = 0.8f;
        public float Height = 2.0f;
        public float MaxSpeed = 2.0f;
        public float MaxAcceleration = 2.0f;

        private int idx = -1;
        private bool updateScheduled = false;

        private bool _isMoving;
        public bool IsMoving
        {
            get
            {
                return _isMoving;
            }
        }

        public void Start()
        {
            idx = DetourCrowd.Instance.AddAgent(GetComponent<IGameEntity>(), Radius, Height);
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
            _isMoving = true;
            DetourCrowd.Instance.MoveTarget(idx, target);
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
