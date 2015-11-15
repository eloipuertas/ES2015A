using UnityEngine;
using System.Collections.Generic;

public class MissionStatus
{
    private Dictionary<Storage.BuildingTypes, uint[]> buildings;
    private Dictionary<Storage.UnitTypes, uint[]> units;
    private Dictionary<WorldResources.Type, uint[]> resources;

    private uint owner;

    private MissionController controller;

    private static readonly int ACCUMULATE = 0;

    public MissionStatus(uint owner)
    {
        Battle.MissionDefinition[] list;
        controller = GameObject.FindWithTag("GameController").GetComponent<MissionController>();
        this.owner = owner;
        buildings = new Dictionary<Storage.BuildingTypes, uint[]>();
        units = new Dictionary<Storage.UnitTypes, uint[]>();
        //resources = new Dictionary<WorldResources.Type, uint[]>();
        GameInformation info = GameObject.Find("GameInformationObject").GetComponent<GameInformation>();
        list = info.GetBattle().GetMissions().ToArray();
        foreach (Battle.MissionDefinition mission in list)
        {
            switch (mission.target)
            {
                case Storage.EntityType.UNIT:
                    if (!units.ContainsKey(mission.targetType.unit))
                    {
                        units.Add(mission.targetType.unit, new uint[2]);
                    }
                    switch (mission.purpose)
                    {
                        case Battle.MissionType.DESTROY:
                            units[mission.targetType.unit][0] = mission.amount;
                            break;
                        case Battle.MissionType.NEW:
                            units[mission.targetType.unit][1] = mission.amount;
                            break;
                    }
                    break;
                case Storage.EntityType.BUILDING:
                    if (!buildings.ContainsKey(mission.targetType.building))
                    {
                        buildings.Add(mission.targetType.building, new uint[2]);
                    }
                    switch (mission.purpose)
                    {
                        case Battle.MissionType.DESTROY:
                            buildings[mission.targetType.building][0] = mission.amount;
                            break;
                        case Battle.MissionType.NEW:
                            buildings[mission.targetType.building][1] = mission.amount;
                            break;
                    }
                    break;
            }
        }
    }

    public void OnResourceAmountChanged(WorldResources.Type type, uint newAmmount) {}

    public void OnResourceAdded(WorldResources.Type type, uint ammount)
    {
        uint[] missionTargets;
        if (resources.TryGetValue(type, out missionTargets))
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
                if (missionTargets[0] == 0)
                {
                    // TODO Notify the controller
                }
            }
            // TODO Logic for resource keeping
        }
    }

    public void OnResourceSubtracted(WorldResources.Type type, uint ammount)
    {
        uint[] missionTargets;
        if (resources.TryGetValue(type, out missionTargets))
        {
            // No importa en caso de acumular
            // TODO Logic for resource keeping
        }
    }

    public void OnBuildingCreated(Storage.BuildingTypes type)
    {
        uint[] missionTargets;
        if (buildings.TryGetValue(type, out missionTargets))
        {
            if (missionTargets[1] > 0)
            {
                missionTargets[1]--;
                if (missionTargets[1] == 0)
                {
                    controller.notifyBuildingCreated(type, owner);
                }
            }
        }
    }

    public void OnBuildingDestroyed(Storage.BuildingTypes type)
    {
        uint[] missionTargets;
        if (buildings.TryGetValue(type, out missionTargets))
        {
            if (missionTargets[0] > 0)
            {
                missionTargets[0]--;
                if (missionTargets[0] == 0)
                {
                    controller.notifyBuildingDestroyed(type, owner);
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
            if (missionTargets[1] > 0)
            {
                missionTargets[1]--;
                if (missionTargets[1] == 0)
                {
                    controller.notifyUnitCreated(type, owner);
                }
            }
        }
    }

    public void OnUnitKilled(Storage.UnitTypes type)
    {
        uint[] missionTargets;
        if (units.TryGetValue(type, out missionTargets))
        {
            if (missionTargets[0] > 0)
            {
                missionTargets[0]--;
                if (missionTargets[0] == 0)
                {
                    controller.notifyUnitKilled(type, owner);
                }
            }
        }
    }

    //public void OnUnitKilled(Storage.UnitTypes type, string name) {}

    public bool isGameOver()
    {
        return controller.IsGameOver();
    }

    public bool hasWon(uint owner)
    {
        return controller.HasWon(owner);
    }
}
