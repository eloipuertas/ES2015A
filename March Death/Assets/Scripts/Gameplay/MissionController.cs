using UnityEngine;
using System.Collections.Generic;

public class MissionController : MonoBehaviour
{
    private Dictionary<Storage.BuildingTypes, uint[]> buildings;
    private Dictionary<Storage.UnitTypes, uint[]> units;
    private Dictionary<WorldResources.Type, uint[]> resources;

    private static Object resourceThreadLock = new Object();
    private static Object unitThreadLock = new Object();
    private static Object buildingThreadLock = new Object();

    public enum Winner { NONE, PLAYER, PC }

    private static Dictionary<Storage.UnitTypes, Winner[]> unitWinners;
    private static Dictionary<Storage.BuildingTypes, Winner[]> buildingWinners;

    private Winner owner;

    private static readonly int ACCUMULATE = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnResourceAmountChanged(WorldResources.Type type, uint newAmmount) {}

    public void OnResourceAdded(WorldResources.Type type, uint ammount)
    {
        uint[] missionTargets;
        if (resources.TryGetValue(type, out missionTargets))
        {
            lock (resourceThreadLock)
            {
                // Logic for the resource finding (accumulation) mission
                if (missionTargets[0] != 0)
                {
                    if (missionTargets[0] < ammount)
                    {
                        missionTargets[0] = 0;
                    }
                    else
                    {
                        missionTargets[0] -= ammount;
                    }
                }
                // TODO Logic for resource keeping
            }
        }
    }

    public void OnResourceSubtracted(WorldResources.Type type, uint ammount)
    {
        uint[] missionTargets;
        if (resources.TryGetValue(type, out missionTargets))
        {
            lock (resourceThreadLock)
            {
                // No importa en caso de acumular
                // TODO Logic for resource keeping
            }
        }
    }

    public void OnBuildingCreated(Storage.BuildingTypes type)
    {
        uint[] missionTargets;
        if (buildings.TryGetValue(type, out missionTargets))
        {
            lock (buildingThreadLock)
            {
                if (missionTargets[1] > 0)
                {
                    missionTargets[1]--;
                    if (missionTargets[1] == 0 && buildingWinners[type][1] == Winner.NONE)
                    {
                        buildingWinners[type][1] = owner;
                    }
                }
            }
        }
    }

    public void OnBuildingDestroyed(Storage.BuildingTypes type)
    {
        uint[] missionTargets;
        if (buildings.TryGetValue(type, out missionTargets))
        {
            lock (buildingThreadLock)
            {
                if (missionTargets[0] > 0)
                {
                    missionTargets[0]--;
                    if (missionTargets[0] == 0 && buildingWinners[type][0] == Winner.NONE)
                    {
                        buildingWinners[type][0] = owner;
                    }
                }
            }
        }
    }

    //public void OnBuildingDestroyed(Storage.BuildingTypes type, string name) {}

    public void OnUnitCreated(Storage.UnitTypes type)
    {
        uint[] missionTargets;
        if (units.TryGetValue(type, out missionTargets))
        {
            lock (unitThreadLock)
            {
                if (missionTargets[1] > 0)
                {
                    missionTargets[1]--;
                    if (missionTargets[1] == 0 && unitWinners[type][1] == Winner.NONE)
                    {
                        unitWinners[type][1] = owner;
                    }
                }
            }
        }
    }

    public void OnUnitKilled(Storage.UnitTypes type)
    {
        uint[] missionTargets;
        if (units.TryGetValue(type, out missionTargets))
        {
            lock (unitThreadLock)
            {
                if (missionTargets[0] > 0)
                {
                    missionTargets[0]--;
                    if (missionTargets[0] == 0 && unitWinners[type][0] == Winner.NONE)
                    {
                        unitWinners[type][0] = owner;
                    }
                }
            }
        }
    }

    //public void OnUnitKilled(Storage.UnitTypes type, string name) {}
}
