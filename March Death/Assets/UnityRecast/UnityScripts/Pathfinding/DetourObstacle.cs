using UnityEngine;
using UnityEngine.Assertions;

namespace Pathfinding
{
    [ExecuteInEditMode]
    public class DetourObstacle : MonoBehaviour
    {
        public bool DebugOnEditor = true;
        public Vector3 Position = new Vector3(0, 0, 0);
        public Vector3 Size = new Vector3(1, 1, 1);
        public float CheckEverySeconds = 1.0f;

        private Vector3 Bottom_1;
        private Vector3 Bottom_2;
        private Vector3 Bottom_3;
        private Vector3 Bottom_4;

        private uint obstacleReference = 0;
        private float lastChecked = 0.0f;
        private Vector3 lastKnownPosition = new Vector3();
        private Quaternion lastKnownRotation = new Quaternion();
        private bool alreadyAdded = false;

        private void CalcVertices()
        {
            Vector3 size = transform.rotation * Size;

            Bottom_1 = transform.position + Position;
            Bottom_1.x -= size.x / 2;
            Bottom_1.z += size.z / 2;

            Bottom_2 = transform.position + Position;
            Bottom_2.x += size.x / 2;
            Bottom_2.z += size.z / 2;

            Bottom_3 = transform.position + Position;
            Bottom_3.x += size.x / 2;
            Bottom_3.z -= size.z / 2;

            Bottom_4 = transform.position + Position;
            Bottom_4.x -= size.x / 2;
            Bottom_4.z -= size.z / 2;
        }

        private void checkObstacleStatus()
        {
            if (Time.time - lastChecked >= CheckEverySeconds)
            {
                lastChecked = Time.time;

                if (!alreadyAdded || lastKnownPosition != transform.position ||
                    lastKnownRotation != transform.rotation)
                {
                    lastKnownPosition = transform.position;
                    lastKnownRotation = transform.rotation;
                    CalcVertices();

                    if (alreadyAdded)
                    {
                        PathDetour.get.RemoveObstacle(obstacleReference);
                    }

                    obstacleReference = PathDetour.get.AddObstacle(this);
                    alreadyAdded = true;
                }
            }
        }

        public Vector3[] Vertices()
        {
            return new Vector3[]
            {
                Bottom_1, Bottom_2, Bottom_3, Bottom_4
            };
        }

        public void Update()
        {
            checkObstacleStatus();

#if UNITY_EDITOR
            if (DebugOnEditor)
            {
                CalcVertices();

                // Create Top Vectors
                Vector3 Top_1 = Bottom_1 + new Vector3(0, Size.y, 0);
                Vector3 Top_2 = Bottom_2 + new Vector3(0, Size.y, 0);
                Vector3 Top_3 = Bottom_3 + new Vector3(0, Size.y, 0);
                Vector3 Top_4 = Bottom_4 + new Vector3(0, Size.y, 0);

                // Draw Bottom
                Debug.DrawLine(Bottom_1, Bottom_2, Color.magenta);
                Debug.DrawLine(Bottom_2, Bottom_3, Color.magenta);
                Debug.DrawLine(Bottom_3, Bottom_4, Color.magenta);
                Debug.DrawLine(Bottom_4, Bottom_1, Color.magenta);

                // Draw Upper
                Debug.DrawLine(Top_1, Top_2, Color.magenta);
                Debug.DrawLine(Top_2, Top_3, Color.magenta);
                Debug.DrawLine(Top_3, Top_4, Color.magenta);
                Debug.DrawLine(Top_4, Top_1, Color.magenta);

                // Draw Botom to Upper
                Debug.DrawLine(Bottom_1, Top_1, Color.magenta);
                Debug.DrawLine(Bottom_2, Top_2, Color.magenta);
                Debug.DrawLine(Bottom_3, Top_3, Color.magenta);
                Debug.DrawLine(Bottom_4, Top_4, Color.magenta);
            }
#endif
        }
    }
}
