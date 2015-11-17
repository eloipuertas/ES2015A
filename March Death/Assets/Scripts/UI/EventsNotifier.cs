using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Displays text on the screen regarding game events, such as when the user is
/// under attack.
/// </summary>
public class EventsNotifier : MonoBehaviour {

    private readonly string UNDER_ATTACK = "You're under attack!";

    // Creation messages
    private readonly string CIVILIAN_CREATED = "Civilian created.";
    private readonly string BARRACK_CREATED = "Barrack created.";
    private readonly string FARM_CREATED = "Farm created.";

    // Resource related messages
    private readonly string FOOD_LOW = "Your food supplies are low!";
    private readonly string WOOD_LOW = "Your amount of wood is getting low!";
    private readonly string METAL_LOW = "Your metal reserves are getting low!";
    private readonly string NO_FOOD = "You don't have any food left!";
    private readonly string NO_WOOD = "You don't have any wood left!";
    private readonly string NO_METAL = "You don't have any metal left!";
    private readonly string NOT_ENOUGH_FOOD = "You don't have enough food.";
    private readonly string NOT_ENOUGH_WOOD = "You don't have enough wood.";
    private readonly string NOT_ENOUGH_METAL = "You don't have enough metal.";

    // Messages to indicate the loss of a unit
    private readonly string HERO_DEAD = "Your hero is dead.";
    private readonly string CIVILIAN_DEAD = "You have lost a civilian.";
    private readonly string STRONGHOLD_LOST = "You have lost your stronghold.";

    // Messages to indicate the loss of a building
    private readonly string FARM_LOST = "You have lost a farm.";
    private readonly string MINE_LOST = "You have lost a mine.";
    private readonly string SAWMILL_LOST = "You have lost a sawmill.";
    private readonly string ARCHERY_LOST = "You have lost your archery building.";
    private readonly string BARRACK_LOST = "You have lost a barrack.";

    private const float TIME_TO_UPDATE = 5f;

    private float countdown;

    private GUIText text;
    private Queue<int> trimming;
    private System.Text.StringBuilder messages;

    // Use this for initialization
    void Start()
    {
        text = gameObject.GetComponent<GUIText>();
        trimming = new Queue<int>();
        messages = new System.Text.StringBuilder();
        countdown = TIME_TO_UPDATE;
    }
	
    // Update is called once per frame
    void Update()
    {
        countdown -= Time.deltaTime;
        if (countdown <= 0.0f)
        {
            messages.Remove(0, trimming.Dequeue() + 1);
            countdown = TIME_TO_UPDATE;
        }
        text.text = messages.ToString();
    }

    private void AppendMessage(string what)
    {
        trimming.Enqueue(what.Length);
        messages.AppendLine(what);
    }

    public void DisplayUnderAttack(Vector3 where)
    {
        AppendMessage(UNDER_ATTACK);
        // TODO Display information on the mini-map
    }

    public void DisplayBuildingCreated(Storage.BuildingTypes type)
    {
        switch (type)
        {
            case Storage.BuildingTypes.STRONGHOLD:
                break;
            case Storage.BuildingTypes.ARCHERY:
            case Storage.BuildingTypes.BARRACK:
            case Storage.BuildingTypes.FARM:
            case Storage.BuildingTypes.MINE:
            case Storage.BuildingTypes.SAWMILL:
                break;
        }
    }

    public void DisplayUnitCreated(Storage.UnitTypes type)
    {
        switch (type)
        {
            case Storage.UnitTypes.CIVIL:
                AppendMessage(CIVILIAN_CREATED);
                break;
            case Storage.UnitTypes.LIGHT:
            case Storage.UnitTypes.THROWN:
            case Storage.UnitTypes.CAVALRY:
            case Storage.UnitTypes.HEAVY:
                break;
        }
    }

    public void DisplayUnitDead(Storage.UnitTypes type)
    {
        switch (type)
        {
            case Storage.UnitTypes.CIVIL:
                AppendMessage(CIVILIAN_CREATED);
                break;
            case Storage.UnitTypes.LIGHT:
            case Storage.UnitTypes.THROWN:
            case Storage.UnitTypes.CAVALRY:
            case Storage.UnitTypes.HEAVY:
                break;
        }
    }
    public void DisplayBuildingDestroyed(Storage.BuildingTypes type)
    {
        switch (type)
        {
            case Storage.BuildingTypes.STRONGHOLD:
                AppendMessage(STRONGHOLD_LOST);
                break;
            case Storage.BuildingTypes.ARCHERY:
                AppendMessage(ARCHERY_LOST);
                break;
            case Storage.BuildingTypes.BARRACK:
                AppendMessage(BARRACK_LOST);
                break;
            case Storage.BuildingTypes.FARM:
                AppendMessage(FARM_LOST);
                break;
            case Storage.BuildingTypes.MINE:
                AppendMessage(MINE_LOST);
                break;
            case Storage.BuildingTypes.SAWMILL:
                AppendMessage(SAWMILL_LOST);
                break;
        }
    }

    public void DisplayResourceIsLow(WorldResources.Type type) {}
    public void DisplayNotEnoughResources(WorldResources.Type type) {}
    public void DisplayTroopCreated(string info) {}

    public void DisplayUnderAttack(GameObject obj)
    {
        DisplayUnderAttack(obj.transform.position);
    }

    public void DisplayBuildingDestroyed(GameObject obj)
    {
        IGameEntity entity = obj.GetComponent<IGameEntity>();
        DisplayBuildingDestroyed(((Storage.BuildingInfo) entity.info).type);
    }

    public void DisplayBuildingCreated(GameObject obj)
    {
        IGameEntity entity = obj.GetComponent<IGameEntity>();
        DisplayBuildingCreated(((Storage.BuildingInfo) entity.info).type);
    }

    public void DisplayUnitCreated(GameObject obj)
    {
        Unit entity = (Unit) obj.GetComponent<IGameEntity>();
        DisplayUnitCreated(entity.type);
    }

    public void DisplayUnitDead(GameObject obj)
    {
        Unit entity = (Unit) obj.GetComponent<IGameEntity>();
        DisplayUnitDead(entity.type);
    }
}
