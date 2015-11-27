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
    private readonly string LIGHT_ARMY_CREATED = "Light armor warrior ready.";
    private readonly string HEAVY_ARMY_CREATED = "Heavy armor warrior ready.";
    private readonly string CAVALRY_CREATED = "Horseman ready.";
    private readonly string SHOOTER_CREATED = "Shooter ready.";

    // Building creation messages
    private readonly string BARRACK_CREATED = "Barrack created.";
    private readonly string FARM_CREATED = "Farm created.";
    private readonly string MINE_CREATED = "Mine created.";
    private readonly string SAWMILL_CREATED = "Sawmill created.";
    private readonly string SHOOTING_RANGE_CREATED = "Shooting range created.";
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
    private readonly string LIGHT_ARMY_DEAD = "You have lost a light armor soldier.";
    private readonly string HEAVY_ARMY_DEAD = "You have lost a heavy armor soldier.";
    private readonly string CAVALRY_DEAD = "You have lost a horseman.";
    private readonly string SHOOTER_DEAD = "You have lost a shooter.";

    // Messages to indicate the loss of a building
    private readonly string STRONGHOLD_LOST = "You have lost your stronghold.";
    private readonly string FARM_LOST = "You have lost a farm.";
    private readonly string MINE_LOST = "You have lost a mine.";
    private readonly string SAWMILL_LOST = "You have lost a sawmill.";
    private readonly string SHOOTING_RANGE_LOST = "You have lost a shooting range.";
    private readonly string BARRACK_LOST = "You have lost a barrack.";
    private readonly string STABLE_LOST = "You have lost a stable.";

    private const float TIME_TO_UPDATE = 5f;

    private float countdown;
    private bool updateMessages;

    private const float UNDER_ATTACK_TIME = 10;
    private Dictionary<IGameEntity, float> entityTimer;

    private const int MAX_LINES = 10;

    private GUIText text;
    private Queue<int> trimming;
    private System.Text.StringBuilder messages;

    private Camera mainCam;

    void Awake()
    {
        mainCam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        text = GameObject.Find("ScreenMessages").GetComponent<GUIText>();
    }

    // Use this for initialization
    void Start()
    {
        trimming = new Queue<int>();
        messages = new System.Text.StringBuilder();
        countdown = TIME_TO_UPDATE;
        updateMessages = false;
        entityTimer = new Dictionary<IGameEntity, float>();
    }
	
    // Update is called once per frame
    void Update()
    {
        if (updateMessages)
        {
            countdown -= Time.deltaTime;
            if (countdown <= 0.0f || trimming.Count > MAX_LINES)
            {
                messages.Remove(0, trimming.Dequeue());
                countdown = TIME_TO_UPDATE;
                updateMessages = trimming.Count != 0;
            }
            text.text = messages.ToString();
        }
    }

    private void AppendMessage(string what)
    {
        if (!updateMessages) updateMessages = true;
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
                AppendMessage(SHOOTING_RANGE_CREATED);
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
                AppendMessage(SHOOTER_CREATED);
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
                AppendMessage(LIGHT_ARMY_DEAD);
                break;
            case Storage.UnitTypes.THROWN:
                AppendMessage(SHOOTER_DEAD);
                break;
            case Storage.UnitTypes.CAVALRY:
                AppendMessage(CAVALRY_DEAD);
                break;
            case Storage.UnitTypes.HEAVY:
                AppendMessage(HEAVY_ARMY_DEAD);
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
                AppendMessage(SHOOTING_RANGE_LOST);
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

    public void DisplayNotEnoughResources(WorldResources.Type type)
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

    /// <summary>
    /// Returns <code>true</code> if the entity is under the camera.
    /// </summary>
    /// <returns><c>true</c>, if the entity is under the camera, <c>false</c> otherwise.</returns>
    /// <param name="entity">Entity.</param>
    private bool isEntityUnderCamera(IGameEntity entity)
    {
        Vector3 vp = mainCam.WorldToViewportPoint(entity.getTransform().position);
        return vp.x >= 0 && vp.y >= 0 && vp.x <= 1 && vp.y <= 1 && vp.z >= 0;
    }

    /// <summary>
    /// Displays a message warning the user that a game entity is under attack.
    /// 
    /// The message will only be displayed if the object has not been attacked before
    /// and if it is not visible by the user.
    /// </summary>
    /// <param name="obj">Object that represents the entity being attacked.</param>
    public void DisplayUnderAttack(System.Object obj)
    {
        GameObject g = (GameObject) obj;
        IGameEntity entity = g.GetComponent<IGameEntity>();
        if (entityTimer.ContainsKey(entity))
        {
            if (Time.time - entityTimer[entity] >= UNDER_ATTACK_TIME)
            {
                entityTimer[entity] = Time.time;
                if (!isEntityUnderCamera(entity))
                    DisplayUnderAttack(g.transform.position);
            }
        }
        else
        {
            entityTimer.Add(entity, Time.time);
            DisplayUnderAttack(g.transform.position);
        }
    }

    public void DisplayBuildingDestroyed(System.Object obj)
    {
        GameObject g = (GameObject) obj;
        IGameEntity entity = g.GetComponent<IGameEntity>();
        entityTimer.Remove(entity);
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
        Unit entity = (Unit) obj;
        DisplayUnitCreated(entity.type);
    }

    public void DisplayUnitDead(System.Object obj)
    {
        GameObject g = (GameObject) obj;
        Unit entity = (Unit) g.GetComponent<IGameEntity>();
        entityTimer.Remove(entity);
        DisplayUnitDead(entity.type);
    }
}
