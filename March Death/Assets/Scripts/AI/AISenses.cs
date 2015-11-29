using UnityEngine;
using System.Collections.Generic;
using Storage;

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

    /// <summary>
    /// Gets all the units of a certain race
    /// </summary>
    /// <param name="position"></param>
    /// <param name="radius"></param>
    /// <param name="race"></param>
    /// <returns></returns>
    public List<Unit> getUnitsOfRaceNearPosition(Vector3 position, float radius, Races race)
    {
        GameObject[] foundGameObjects = getObjectsNearPosition(position, radius);
        List<Unit> unitsOfRace = new List<Unit>();

        for (int i = 0; i < foundGameObjects.Length; i++)
        {
            GameObject obj = foundGameObjects[i];
            Unit objUnit = obj.GetComponent<Unit>();
            if(objUnit != null && objUnit.status != EntityStatus.DEAD && objUnit.race == race)
            {
                unitsOfRace.Add(objUnit);
            }
        }
        return unitsOfRace;
    }

    /// <summary>
    /// Gets all the buildings of a certain race
    /// </summary>
    /// <param name="position"></param>
    /// <param name="radius"></param>
    /// <param name="race"></param>
    /// <returns></returns>
    public List<IBuilding> getBuildingsOfRaceNearPosition(Vector3 position, float radius, Races race)
    {
        GameObject[] foundGameObjects = getObjectsNearPosition(position, radius);
        List<IBuilding> buldingsOfRace = new List<IBuilding>();

        for (int i = 0; i < foundGameObjects.Length; i++)
        {
            GameObject obj = foundGameObjects[i];
            IBuilding objBuilding = obj.GetComponent<IBuilding>();
            if (objBuilding != null && objBuilding.healthPercentage > 0f && objBuilding.getRace() == race)
            {
                buldingsOfRace.Add(objBuilding);
            }
        }
        return buldingsOfRace;
    }

    /// <summary>
    /// Gets all units of a certain race that are being revealed to the other race.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="radius"></param>
    /// <param name="race"></param>
    /// <returns></returns>
    public List<Unit> getVisibleUnitsOfRaceNearPosition(Vector3 position, float radius, Storage.Races race)
    {
        GameObject[] foundGameObjects = getObjectsNearPosition(position, radius);
        List<Unit> unitsOfRace = new List<Unit>();

        for (int i = 0; i < foundGameObjects.Length; i++)
        {
            GameObject obj = foundGameObjects[i];
            Unit objUnit = obj.GetComponent<Unit>();
            if (objUnit != null && objUnit.race == race && objUnit.status!=EntityStatus.DEAD && obj.GetComponent<FOWEntity>().IsRevealed)
            {
                unitsOfRace.Add(objUnit);
            }
        }
        return unitsOfRace;
    }
}
