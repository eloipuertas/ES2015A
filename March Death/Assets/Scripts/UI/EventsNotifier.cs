using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Displays text on the screen regarding game events, such as when the user is
/// under attack.
/// </summary>
public class EventsNotifier : MonoBehaviour {

    private readonly string UNDER_ATTACK = "You're under attack!";

    // Unit creation messages
    private readonly string CIVILIAN_CREATED = "Civilian created.";
    private readonly string LIGHT_ARMY_CREATED = "Light army warrior ready.";
    private readonly string HEAVY_ARMY_CREATED = "Heavy army warrior ready.";
    private readonly string CAVALRY_CREATED = "Cavalry warrior ready.";
    private readonly string ARCHER_CREATED = "Archer created.";

    // Building creation messages
    private readonly string BARRACK_CREATED = "Barrack created.";
    private readonly string FARM_CREATED = "Farm created.";
    private readonly string MINE_CREATED = "Mine created.";
    private readonly string SAWMILL_CREATED = "Sawmill created.";
    private readonly string ARCHERY_CREATED = "Archery building created.";
    private readonly string STABLE_CREATED = "Stable created.";

    // Resource related messages
    private readonly string FOOD_LOW = "Your food supplies are low!";
    private readonly string WOOD_LOW = "Your wood stock is getting low!";
    private readonly string METAL_LOW = "Your metal reserves are getting low!";
    private readonly string GOLD_LOW = "Your gold reserves are getting low!";
    private readonly string NO_FOOD = "You don't have any food left!";
    private readonly string NO_WOOD = "You don't have any wood left!";
    private readonly string NO_METAL = "You don't have any metal left!";
    private readonly string NO_GOLD = "You don't have any gold left!";
    private readonly string NOT_ENOUGH_FOOD = "You don't have enough food.";
    private readonly string NOT_ENOUGH_WOOD = "You don't have enough wood.";
    private readonly string NOT_ENOUGH_METAL = "You don't have enough metal.";
    private readonly string NOT_ENOUGH_GOLD = "You don't have enough gold.";

    // Messages to indicate the loss of a unit
    private readonly string HERO_DEAD = "Your hero is dead.";
    private readonly string CIVILIAN_DEAD = "You have lost a civilian.";

    // Messages to indicate the loss of a building
    private readonly string STRONGHOLD_LOST = "You have lost your stronghold.";
    private readonly string FARM_LOST = "You have lost a farm.";
    private readonly string MINE_LOST = "You have lost a mine.";
    private readonly string SAWMILL_LOST = "You have lost a sawmill.";
    private readonly string ARCHERY_LOST = "You have lost your archery building.";
    private readonly string BARRACK_LOST = "You have lost a barrack.";
    private readonly string STABLE_LOST = "Your stable is destroyed.";

    private const float TIME_TO_UPDATE = 5f;

    private float countdown;

    private GUIText text;
    private Queue<int> trimming;
    private System.Text.StringBuilder messages;

    void Awake()
    {
        text = GameObject.Find("ScreenMessages").GetComponent<GUIText>();
    }

    // Use this for initialization
    void Start()
    {
        trimming = new Queue<int>();
        messages = new System.Text.StringBuilder();
        countdown = float.NegativeInfinity;
    }
	
    // Update is called once per frame
    void Update()
    {
        if (countdown == float.NegativeInfinity) return;
        countdown -= Time.deltaTime;
        if (countdown <= 0.0f)
        {
            messages.Remove(0, trimming.Dequeue());
            countdown = TIME_TO_UPDATE;
        }
        text.text = messages.ToString();
    }

    private void AppendMessage(string what)
    {
        if (countdown == float.NegativeInfinity) countdown = TIME_TO_UPDATE;
        trimming.Enqueue(what.Length + 1);
        messages.Append(what);
        messages.Append("\n");
    }

    private void DisplayUnderAttack(Vector3 where)
    {
        AppendMessage(UNDER_ATTACK);
        // TODO Display information on the mini-map
    }

    private void DisplayBuildingCreated(Storage.BuildingTypes type)
    {
        switch (type)
        {
            case Storage.BuildingTypes.STRONGHOLD:
                break;
            case Storage.BuildingTypes.ARCHERY:
                AppendMessage(ARCHERY_CREATED);
                break;
            case Storage.BuildingTypes.STABLE:
                AppendMessage(STABLE_CREATED);
                break;
            case Storage.BuildingTypes.BARRACK:
                AppendMessage(BARRACK_CREATED);
                break;
            case Storage.BuildingTypes.FARM:
                AppendMessage(FARM_CREATED);
                break;
            case Storage.BuildingTypes.MINE:
                AppendMessage(MINE_CREATED);
                break;
            case Storage.BuildingTypes.SAWMILL:
                AppendMessage(SAWMILL_CREATED);
                break;
        }
    }

    private void DisplayUnitCreated(Storage.UnitTypes type)
    {
        switch (type)
        {
            case Storage.UnitTypes.CIVIL:
                AppendMessage(CIVILIAN_CREATED);
                break;
            case Storage.UnitTypes.LIGHT:
                AppendMessage(LIGHT_ARMY_CREATED);
                break;
            case Storage.UnitTypes.THROWN:
                AppendMessage(ARCHER_CREATED);
                break;
            case Storage.UnitTypes.CAVALRY:
                AppendMessage(CAVALRY_CREATED);
                break;
            case Storage.UnitTypes.HEAVY:
                AppendMessage(HEAVY_ARMY_CREATED);
                break;
        }
    }

    private void DisplayUnitDead(Storage.UnitTypes type)
    {
        switch (type)
        {
            case Storage.UnitTypes.CIVIL:
                AppendMessage(CIVILIAN_DEAD);
                break;
            case Storage.UnitTypes.LIGHT:
            case Storage.UnitTypes.THROWN:
            case Storage.UnitTypes.CAVALRY:
            case Storage.UnitTypes.HEAVY:
                break;
            case Storage.UnitTypes.HERO:
                AppendMessage(HERO_DEAD);
                break;
        }
    }

    private void DisplayBuildingDestroyed(Storage.BuildingTypes type)
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
            case Storage.BuildingTypes.STABLE:
                AppendMessage(STABLE_LOST);
                break;
        }
    }

    private void DisplayResourceIsLow(WorldResources.Type type)
    {
        switch (type)
        {
            case WorldResources.Type.FOOD:
                AppendMessage(FOOD_LOW);
                break;
            case WorldResources.Type.METAL:
                AppendMessage(METAL_LOW);
                break;
            case WorldResources.Type.WOOD:
                AppendMessage(WOOD_LOW);
                break;
            case WorldResources.Type.GOLD:
                AppendMessage(GOLD_LOW);
                break;
        }
    }

    private void DisplayNotEnoughResources(WorldResources.Type type)
    {
        switch (type)
        {
            case WorldResources.Type.FOOD:
                AppendMessage(NOT_ENOUGH_FOOD);
                break;
            case WorldResources.Type.METAL:
                AppendMessage(NOT_ENOUGH_METAL);
                break;
            case WorldResources.Type.WOOD:
                AppendMessage(NOT_ENOUGH_WOOD);
                break;
            case WorldResources.Type.GOLD:
                AppendMessage(NOT_ENOUGH_GOLD);
                break;
        }
    }

    private void DisplayResourceDepleted(WorldResources.Type type)
    {
        switch (type)
        {
            case WorldResources.Type.FOOD:
                AppendMessage(NO_FOOD);
                break;
            case WorldResources.Type.METAL:
                AppendMessage(NO_METAL);
                break;
            case WorldResources.Type.WOOD:
                AppendMessage(NO_WOOD);
                break;
            case WorldResources.Type.GOLD:
                AppendMessage(NO_GOLD);
                break;
        }
    }

    // TODO Display troop's messages
    private void DisplayTroopCreated(string info) {}

    public void DisplayUnderAttack(System.Object obj)
    {
        GameObject g = (GameObject) obj;
        DisplayUnderAttack(g.transform.position);
    }

    public void DisplayBuildingDestroyed(System.Object obj)
    {
        GameObject g = (GameObject) obj;
        IGameEntity entity = g.GetComponent<IGameEntity>();
        DisplayBuildingDestroyed(((Storage.BuildingInfo) entity.info).type);
    }

    public void DisplayBuildingCreated(System.Object obj)
    {
        GameObject g = (GameObject) obj;
        IGameEntity entity = g.GetComponent<IGameEntity>();
        DisplayBuildingCreated(((Storage.BuildingInfo) entity.info).type);
    }

    public void DisplayUnitCreated(System.Object obj)
    {
        GameObject g = (GameObject) obj;
        Unit entity = (Unit) g.GetComponent<IGameEntity>();
        DisplayUnitCreated(entity.type);
    }

    public void DisplayUnitDead(System.Object obj)
    {
        GameObject g = (GameObject) obj;
        Unit entity = (Unit) g.GetComponent<IGameEntity>();
        DisplayUnitDead(entity.type);
    }
}
