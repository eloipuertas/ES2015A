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
        private int idx = -1;

        public void Start()
        {
            idx = DetourCrowd.Instance.AddAgent(GetComponent<IGameEntity>());
        }

        public void MoveTo(Vector3 target)
        {
            DetourCrowd.Instance.MoveTarget(idx, target);
        }
    }
}
