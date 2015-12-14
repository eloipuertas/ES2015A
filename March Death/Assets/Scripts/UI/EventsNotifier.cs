using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Displays text on the screen regarding game events, such as when the user is
/// under attack.
/// </summary>
public class EventsNotifier : MonoBehaviour {

    private readonly string UNDER_ATTACK = "You're under attack!";
    private readonly string ENEMY_ON_SIGHT = "Enemy on sight!";

    // Unit creation messages
    private readonly string CIVILIAN_CREATED = "Civilian created.";
    private readonly string LIGHT_ARMY_CREATED = "Light armor warrior ready.";
    private readonly string HEAVY_ARMY_CREATED = "Heavy armor warrior ready.";
    private readonly string CAVALRY_CREATED = "Horseman ready.";
    private readonly string SHOOTER_CREATED = "Shooter ready.";
    private readonly string GRYPHON_CREATED = "Gryphon ready for battle.";
    private readonly string ENT_CREATED = "Ent ready for battle.";
    private readonly string WAR_MACHINE_CREATED = "War machine ready.";
    private readonly string SIEGE_MACHINE_CREATED = "Siege machine ready.";

    // Building creation messages
    private readonly string BARRACK_CREATED = "Barrack created.";
    private readonly string FARM_CREATED = "Farm created.";
    private readonly string MINE_CREATED = "Mine created.";
    private readonly string SAWMILL_CREATED = "Sawmill created.";
    private readonly string SHOOTING_RANGE_CREATED = "Shooting range created.";
    private readonly string STABLE_CREATED = "Stable created.";
    private readonly string WATCHTOWER_CREATED = "Watchtower created.";
    // TODO Find a better way to display these two messages: creation of wall and wall corner.
    private readonly string WALL_CREATED = "Wall created.";
    private readonly string WALL_CORNER_CREATED = "Wall tower created.";
    private readonly string WALL_GATE_CREATED = "Gate created.";
    private readonly string GRYPHON_BUILDING_CREATED = "Gryphon's nest created.";
    private readonly string ENT_BUILDING_CREATED = "Ent forest created.";
    private readonly string WORKSHOP_CREATED = "Workshop created.";
    private readonly string ARTILLERY_BUILDING_CREATED = "Artillery building created.";
    private readonly string SPECIAL_UNITS_BUILDING_CREATED = "Special units building created.";

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
    private readonly string GRYPHON_DEAD = "You have lost a gryphon.";
    private readonly string ENT_DEAD = "You have lost an ent.";
    private readonly string WAR_MACHINE_DESTROYED = "You have lost a war machine.";
    private readonly string SIEGE_MACHINE_DESTROYED = "You have lost a siege machine.";

    // Messages to indicate the loss of a building
    private readonly string STRONGHOLD_LOST = "You have lost your stronghold.";
    private readonly string FARM_LOST = "You have lost a farm.";
    private readonly string MINE_LOST = "You have lost a mine.";
    private readonly string SAWMILL_LOST = "You have lost a sawmill.";
    private readonly string SHOOTING_RANGE_LOST = "You have lost a shooting range.";
    private readonly string BARRACK_LOST = "You have lost a barrack.";
    private readonly string STABLE_LOST = "You have lost a stable.";
    private readonly string WATCHTOWER_LOST = "You have lost a watchtower.";
    private readonly string WALL_LOST = "Your wall has been wrecked.";
    private readonly string WALL_GATE_LOST = "You have lost a gate.";
    private readonly string GRYPHON_BUILDING_LOST = "A gryphon's nest was destroyed.";
    private readonly string ENT_BUILDING_LOST = "An ent's forest has been destroyed.";
    private readonly string WORKSHOP_LOST = "You have lost a workshop.";
    private readonly string ARTILLERY_BUILDING_LOST = "You have lost an artillery building.";
    private readonly string SPECIAL_UNITS_BUILDING_LOST = "You have lost a special units building.";

    private const float TIME_TO_UPDATE = 5f;

    /// <summary>
    /// Time remaining for the next update for the entire messages.
    /// </summary>
    private float countdown;

    /// <summary>
    /// Indicates whether messages should be updated from time to time.
    /// </summary>
    private bool updateMessages;

    private const float UNDER_ATTACK_TIME = 5f;
    private Dictionary<IGameEntity, float> entityTimer;

    private const float ON_SIGHT_WAIT_TIME = 15f;
    private const int LIMIT_SIGHT_UNITS = 3; //in case we want to restrict the number of units in the dictionary in case of spam on melee attacks.
    private Dictionary<IGameEntity, float> onSightTimer;

    private const int MAX_LINES = 10;

    private GUIText text;
    private Queue<int> trimming;
    private System.Text.StringBuilder messages;

    private Camera mainCam;
    private Managers.SoundsManager sounds;

    void Awake()
    {
        mainCam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        text = GameObject.Find("ScreenMessages").GetComponent<GUIText>();
        sounds = GameObject.FindWithTag("GameController").GetComponent<Managers.SoundsManager>();
    }

    // Use this for initialization
    void Start()
    {
        trimming = new Queue<int>();
        messages = new System.Text.StringBuilder();
        countdown = TIME_TO_UPDATE;
        updateMessages = false;
        entityTimer = new Dictionary<IGameEntity, float>();
        onSightTimer = new Dictionary<IGameEntity, float>();
    }
	
    // Update is called once per frame
    void Update()
    {
        if (updateMessages)
        {
            countdown -= Time.deltaTime;
            while (trimming.Count > MAX_LINES)
            {
                messages.Remove(0, trimming.Dequeue());
            }
            if (countdown <= 0.0f)
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

    private void DisplayUnderAttack(GameObject target)
    {
        AppendMessage(UNDER_ATTACK);
        target.GetComponent<EntityMarker>().entityUnderAttack();
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
            case Storage.BuildingTypes.WATCHTOWER:
                AppendMessage(WATCHTOWER_CREATED);
                break;
            case Storage.BuildingTypes.WALL:
                AppendMessage(WALL_CREATED);
                break;
            case Storage.BuildingTypes.WALLCORNER:
                AppendMessage(WALL_CORNER_CREATED);
                break;
            case Storage.BuildingTypes.WALLGATE:
                AppendMessage(WALL_GATE_CREATED);
                break;
            case Storage.BuildingTypes.ARTILLERY:
                AppendMessage(ARTILLERY_BUILDING_CREATED);
                break;
            case Storage.BuildingTypes.ENT:
                AppendMessage(ENT_BUILDING_CREATED);
                break;
            case Storage.BuildingTypes.GRYPHON:
                AppendMessage(GRYPHON_BUILDING_CREATED);
                break;
            case Storage.BuildingTypes.SPECIAL:
                AppendMessage(SPECIAL_UNITS_BUILDING_CREATED);
                break;
            case Storage.BuildingTypes.WORKSHOP:
                AppendMessage(WORKSHOP_CREATED);
                break;
        }
        sounds.onBuildingCreated(type);
    }

    private void DisplayUnitCreated(Unit unit)
    {
        switch (unit.type)
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
            case Storage.UnitTypes.MACHINE:
                switch (unit.race)
                {
                    case Storage.Races.ELVES:
                        AppendMessage(WAR_MACHINE_CREATED);
                        break;
                    case Storage.Races.MEN:
                        AppendMessage(SIEGE_MACHINE_CREATED);
                        break;
                }
                break;
            case Storage.UnitTypes.SPECIAL:
                switch (unit.race)
                {
                    case Storage.Races.ELVES:
                        AppendMessage(ENT_CREATED);
                        break;
                    case Storage.Races.MEN:
                        AppendMessage(GRYPHON_CREATED);
                        break;
                }
                break;
        }
        sounds.onUnitCreated();
    }

    private void DisplayUnitDead(Unit unit)
    {
        switch (unit.type)
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
            case Storage.UnitTypes.MACHINE:
                switch (unit.race)
                {
                    case Storage.Races.ELVES:
                        AppendMessage(WAR_MACHINE_DESTROYED);
                        break;
                    case Storage.Races.MEN:
                        AppendMessage(SIEGE_MACHINE_DESTROYED);
                        break;
                }
                break;
            case Storage.UnitTypes.SPECIAL:
                switch (unit.race)
                {
                    case Storage.Races.ELVES:
                        AppendMessage(ENT_DEAD);
                        break;
                    case Storage.Races.MEN:
                        AppendMessage(GRYPHON_DEAD);
                        break;
                }
                break;
        }
        sounds.onUnitDead();
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
            case Storage.BuildingTypes.WATCHTOWER:
                AppendMessage(WATCHTOWER_LOST);
                break;
            case Storage.BuildingTypes.WALLCORNER:
            case Storage.BuildingTypes.WALL:
                AppendMessage(WALL_LOST);
                break;
            case Storage.BuildingTypes.WALLGATE:
                AppendMessage(WALL_GATE_LOST);
                break;
            case Storage.BuildingTypes.ARTILLERY:
                AppendMessage(ARTILLERY_BUILDING_LOST);
                break;
            case Storage.BuildingTypes.ENT:
                AppendMessage(ENT_BUILDING_LOST);
                break;
            case Storage.BuildingTypes.GRYPHON:
                AppendMessage(GRYPHON_BUILDING_LOST);
                break;
            case Storage.BuildingTypes.SPECIAL:
                AppendMessage(SPECIAL_UNITS_BUILDING_LOST);
                break;
            case Storage.BuildingTypes.WORKSHOP:
                AppendMessage(WORKSHOP_LOST);
                break;
        }
        sounds.onBuildingDestroyed();
    }

    public void DisplayResourceIsLow(WorldResources.Type type)
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

    public void DisplayResourceDepleted(WorldResources.Type type)
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
                    DisplayUnderAttack(g);
            }
        }
        else
        {
            entityTimer.Add(entity, Time.time);
            DisplayUnderAttack(g);
        }
    }

    public void DisplayBuildingDestroyed(System.Object obj)
    {
        GameObject g = (GameObject) obj;
        IGameEntity entity = g.GetComponent<IGameEntity>();
        entityTimer.Remove(entity);
        PopulationInfo.get.Remove(entity);
        DisplayBuildingDestroyed(((Storage.BuildingInfo) entity.info).type);
    }

    public void DisplayBuildingCreated(System.Object obj)
    {
        GameObject g = (GameObject) obj;
        IGameEntity entity = g.GetComponent<IGameEntity>();
        PopulationInfo.get.Add(entity);
        DisplayBuildingCreated(((Storage.BuildingInfo) entity.info).type);
    }

    public void DisplayUnitCreated(System.Object obj)
    {
        Unit entity = (Unit) obj;
	    PopulationInfo.get.Add(entity);
        DisplayUnitCreated(entity);
    }

    public void DisplayUnitDead(System.Object obj)
    {
        GameObject g = (GameObject) obj;
        Unit entity = (Unit) g.GetComponent<IGameEntity>();
        entityTimer.Remove(entity);
	    PopulationInfo.get.Remove(entity);
        DisplayUnitDead(entity);
    }

    public void DisplayEnemySpotted(GameObject go)
    {
        AppendMessage(ENEMY_ON_SIGHT);

        IGameEntity entity = go.GetComponent<IGameEntity>();
        if (onSightTimer.ContainsKey(entity))
        {
            if ((Time.time - onSightTimer[entity]) >= ON_SIGHT_WAIT_TIME)
            {
                go.GetComponent<EntityMarker>().entityOnSight();
                onSightTimer[entity] = Time.time;
            }
        }
        else
        {
            if (onSightTimer.Count < LIMIT_SIGHT_UNITS)
            {
                go.GetComponent<EntityMarker>().entityOnSight();
                onSightTimer.Add(entity, Time.time);
            }
        }

        
		
    }
}
