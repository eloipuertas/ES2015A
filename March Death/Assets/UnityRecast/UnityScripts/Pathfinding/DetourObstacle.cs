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

        private Vector3 Bottom_1;
        private Vector3 Bottom_2;
        private Vector3 Bottom_3;
        private Vector3 Bottom_4;

        private void CalcVertices()
        {
            Bottom_1 = transform.position + Position;
            Bottom_1.x -= Size.x / 2;
            Bottom_1.z += Size.z / 2;

            Bottom_2 = transform.position + Position;
            Bottom_2.x += Size.x / 2;
            Bottom_2.z += Size.z / 2;

            Bottom_3 = transform.position + Position;
            Bottom_3.x += Size.x / 2;
            Bottom_3.z -= Size.z / 2;

            Bottom_4 = transform.position + Position;
            Bottom_4.x -= Size.x / 2;
            Bottom_4.z -= Size.z / 2;
        }

        public void OnEnable()
        {
            CalcVertices();

            if (Application.isPlaying)
            {
                Assert.IsTrue(PathDetour.get.TileCache.ToInt32() != 0);
                PathDetour.get.AddObstacle(this);
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
