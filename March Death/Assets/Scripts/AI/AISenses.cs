using UnityEngine;

public class AISenses : MonoBehaviour {
    
    /// <summary>
    /// Returns all the gameObjects in some radius
    /// </summary>
    /// <param name="position"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    public GameObject[] getObjectsNearPosition(Vector3 position, float radius)
    {
        Collider[] collidersNearUs = Physics.OverlapSphere(position, radius);
        GameObject[] objectsNearUs = new GameObject[collidersNearUs.Length];

        for(int i = 0; i < collidersNearUs.Length; i++)
        {
            objectsNearUs[i] = collidersNearUs[i].gameObject;
        }
        return objectsNearUs;
    }
}
