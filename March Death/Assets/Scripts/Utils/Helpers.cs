using UnityEngine;
using System.Collections.Generic;
using Storage;

public static class Helpers
{
    /// <summary>
    /// Returns all the gameObjects in some radius
    /// </summary>
    /// <param name="position"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    public static GameObject[] getObjectsNearPosition(Vector3 position, float radius)
    {
        Collider[] collidersNearUs = Physics.OverlapSphere(position, radius);
        GameObject[] objectsNearUs = new GameObject[collidersNearUs.Length];

        for (int i = 0; i < collidersNearUs.Length; i++)
        {
            objectsNearUs[i] = collidersNearUs[i].gameObject;
        }
        return objectsNearUs;
    }

    /// <summary>
    /// Returns all the gameObjects in some radius
    /// </summary>
    /// <param name="position"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    public static List<IGameEntity> getEntitiesNearPosition(Vector3 position, float radius)
    {
        GameObject[] foundGameObjects = getObjectsNearPosition(position, radius);
        List<IGameEntity> entities = new List<IGameEntity>();

        for (int i = 0; i < foundGameObjects.Length; i++)
        {
            GameObject obj = foundGameObjects[i];
            IGameEntity objEntity = obj.GetComponent<IGameEntity>();

            if (objEntity != null && objEntity.status != EntityStatus.DEAD && objEntity.status != EntityStatus.DESTROYED)
            {
                entities.Add(objEntity);
            }
        }
        return entities;
    }

    /// <summary>
    /// Gets all the units of a certain race
    /// </summary>
    /// <param name="position"></param>
    /// <param name="radius"></param>
    /// <param name="race"></param>
    /// <returns></returns>
    public static List<Unit> getUnitsOfRaceNearPosition(Vector3 position, float radius, Races race)
    {
        GameObject[] foundGameObjects = getObjectsNearPosition(position, radius);
        List<Unit> unitsOfRace = new List<Unit>();

        for (int i = 0; i < foundGameObjects.Length; i++)
        {
            GameObject obj = foundGameObjects[i];
            Unit objUnit = obj.GetComponent<Unit>();
            if (objUnit != null && objUnit.status != EntityStatus.DEAD && objUnit.race == race)
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
    public static List<IBuilding> getBuildingsOfRaceNearPosition(Vector3 position, float radius, Races race)
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
    public static List<Unit> getVisibleUnitsOfRaceNearPosition(Vector3 position, float radius, Storage.Races race)
    {
        GameObject[] foundGameObjects = getObjectsNearPosition(position, radius);
        List<Unit> unitsOfRace = new List<Unit>();

        for (int i = 0; i < foundGameObjects.Length; i++)
        {
            GameObject obj = foundGameObjects[i];
            Unit objUnit = obj.GetComponent<Unit>();
            if (objUnit != null && objUnit.race == race && objUnit.status != EntityStatus.DEAD && obj.GetComponent<FOWEntity>().IsRevealed)
            {
                unitsOfRace.Add(objUnit);
            }
        }
        return unitsOfRace;
    }

    public static List<Unit> getVisibleUnitsNotOfRaceNearPosition(Vector3 position, float radius, Storage.Races race)
    {
        GameObject[] foundGameObjects = getObjectsNearPosition(position, radius);
        List<Unit> unitsOfRace = new List<Unit>();

        for (int i = 0; i < foundGameObjects.Length; i++)
        {
            GameObject obj = foundGameObjects[i];
            Unit objUnit = obj.GetComponent<Unit>();
            FOWEntity fowEntity = obj.GetComponent<FOWEntity>();

            if (objUnit == null || fowEntity == null)
            {
                continue;
            }

            if (objUnit.race != race && objUnit.status != EntityStatus.DEAD && fowEntity.IsRevealed)
            {
                unitsOfRace.Add(objUnit);
            }
        }
        return unitsOfRace;
    }
}
