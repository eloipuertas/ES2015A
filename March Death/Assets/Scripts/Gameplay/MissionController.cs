using UnityEngine;
using System.Collections.Generic;

public class MissionController : MonoBehaviour
{
    private static Object winnerTestLock = new Object();

    //public enum Winner { NONE = 0, PLAYER = 1, PC = 2 }

    private static Dictionary<Storage.UnitTypes, uint> destroyedUnitsWinners = new Dictionary<Storage.UnitTypes, uint>();
    private static Dictionary<Storage.BuildingTypes, uint> destroyedBuildingsWinners = new Dictionary<Storage.BuildingTypes, uint>();

    private Battle battle;

    private int missionsToComplete = 0;

	// Use this for initialization
    void Start()
    {
        //Dictionary<Storage.UnitTypes, uint> dUnits = new Dictionary<Storage.UnitTypes, uint>();
        //Dictionary<Storage.BuildingTypes, uint> dBuildings = new Dictionary<Storage.BuildingTypes, uint>();
        battle = GameObject.Find("GameInformationObject").GetComponent<GameInformation>().GetBattle();
        foreach (Battle.MissionDefinition mission in battle.GetMissions())
        {
            switch (mission.purpose)
            {
                case Battle.MissionType.DESTROY:
                    switch (mission.target)
                    {
                        case Storage.EntityType.UNIT:
                            destroyedUnitsWinners.Add(mission.targetType.unit, 0);
                            //dUnits.Add(mission.targetType.unit, mission.amount);
                            missionsToComplete++;
                            break;
                        case Storage.EntityType.BUILDING:
                            destroyedBuildingsWinners.Add(mission.targetType.building, 0);
                            //dBuildings.Add(mission.targetType.building, mission.amount);
                            missionsToComplete++;
                            break;
                    }
                    break;
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public Battle.MissionDefinition[] getMissionListArray()
    {
        List<Battle.MissionDefinition> missions = battle.GetMissions();
        return missions.ToArray();
    }

    /// <summary>
    /// Notifies the controller that the caller has completed the mission to kill a unit.
    /// </summary>
    /// <param name="type">Type of unit.</param>
    /// <param name="caller">ID of the player (either human or PC).</param>
    public void notifyUnitKilled(Storage.UnitTypes type, uint caller)
    {
        bool notify = false;
        uint winner;
        lock (winnerTestLock)
        {
            if (destroyedUnitsWinners.TryGetValue(type, out winner))
            {
                if (winner == 0)
                {
                    notify = true;
                    destroyedUnitsWinners[type] = caller;
                    missionsToComplete--;
                }
            }
        }
        if (notify)
        {
            // TODO for each player, indicate winner
        }
    }

    public void notifyBuildingDestroyed(Storage.BuildingTypes type, uint caller)
    {
        bool notify = false;
        uint winner;
        lock (winnerTestLock)
        {
            if (destroyedBuildingsWinners.TryGetValue(type, out winner))
            {
                if (winner == 0)
                {
                    notify = true;
                    destroyedBuildingsWinners[type] = caller;
                    missionsToComplete--;
                }
            }
        }
        if (notify)
        {
            // TODO for each player, indicate winner
        }
    }

    public void notifyUnitCreated(Storage.UnitTypes type, uint caller) {}
    public void notifyBuildingCreated(Storage.BuildingTypes type, uint caller) {}
}
