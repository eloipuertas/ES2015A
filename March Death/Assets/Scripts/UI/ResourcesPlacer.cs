using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Storage;
using System.Collections.Generic;
using Utils;
using System;

public class ResourcesPlacer
{
    // attributes
    private readonly string[] txt_names = { "meat", "wood", "metal" };
    private readonly string[] txt_namesOther = { "food", "wood", "metal" };

    private List<Text> res_amounts, res_stats;

    /// <summary>
    /// Population text where number of units are displayed
    /// </summary>
    private Text pop;
    private BasePlayer _owner;

    private Sprite up;
    private Sprite down;

    private List<Image> arrows;

    Dictionary<WorldResources.Type, int> resources;
    Dictionary<WorldResources.Type, Dictionary<IGameEntity, GrowthStatsPacket>> statistics;

    private static Dictionary<BasePlayer, ResourcesPlacer> _instances = new Dictionary<BasePlayer, ResourcesPlacer>();

    private ResourcesPlacer(BasePlayer owner)
    {
        _owner = owner;
        res_amounts = new List<Text>();
        res_stats = new List<Text>();
        arrows = new List<Image>();

        Setup();
    }

    public static ResourcesPlacer get(BasePlayer player)
    {
        if (!_instances.ContainsKey(player))
        {
            _instances.Add(player, new ResourcesPlacer(player));
        }

        return _instances[player];
    }


    // Resources 

    public void InitializeResources(uint wood, uint food, uint metal, uint gold)
    {
        resources = new Dictionary<WorldResources.Type, int>()
        {
            { WorldResources.Type.FOOD , (int) food } ,
            { WorldResources.Type.WOOD , (int) wood } ,
            { WorldResources.Type.METAL, (int) metal },
            { WorldResources.Type.GOLD,  (int) gold }
        };

        initializeStatistics();

        updateAmounts();
        updateStatistics();
        updatePopulation();
    }

    /// <summary>
    /// Collect an amount (amount) from a type (type).  
    /// </summary>
    /// <param name="type"></param>
    /// <param name="amount"></param>
    public void Collect(WorldResources.Type type, float amount)
    {
        resources[type] += (int)amount;
        updateAmounts();
    }

    /// <summary>
    /// Units consume a certain amount froma a certain WorldResources.Type
    /// </summary>
    /// <param name="type"> </param>
    /// <param name="amount"> </param>
    public void Consume(WorldResources.Type type, float amount)
    {
        resources[type] -= (int)((resources[type] - amount < 0) ? resources[type] : amount);
        updateAmounts();
    }

    /// <summary>
    /// Method to remove resources from the top HUD when you buy an ability.
    /// </summary>
    /// <param name="amounts">A dictionary where type represents the WorldResources.Type and the 
    /// float represents the amount of WorldResources.Type whe will extract.</param>
    public void Buy(Dictionary<WorldResources.Type, float> amounts)
    {
        foreach (KeyValuePair<WorldResources.Type, float> tuple in amounts)
            resources[tuple.Key] -= (int)((resources[tuple.Key] - tuple.Value < 0) ? 0 : tuple.Value);

        updateAmounts();
    }

    public float Amount(WorldResources.Type type)
    {
        return resources[type];
    }

    // Statistics 

    private void initializeStatistics()
    {
        statistics = new Dictionary<WorldResources.Type, Dictionary<IGameEntity, GrowthStatsPacket>>()
        {
            { WorldResources.Type.FOOD  , new Dictionary<IGameEntity, GrowthStatsPacket>() { } } ,
            { WorldResources.Type.WOOD  , new Dictionary<IGameEntity, GrowthStatsPacket>() { } } ,
            { WorldResources.Type.METAL , new Dictionary<IGameEntity, GrowthStatsPacket>() { } }
        };
    }

    /// <summary>
    /// Method to call when a unit or a resource is created or when a resource building 
    /// adds or removes a worker. 
    /// </summary>
    /// <param name="entity">Entity where their inner stats have changed.</param>
    /// <param name="packet">Package that represents an entity.</param>
    public void StatisticsChanged(IGameEntity entity, GrowthStatsPacket packet)
    {

        if (statistics[packet.type].ContainsKey(entity))
        {
            statistics[packet.type][entity] = packet;
        }
        else
        {
            statistics[packet.type].Add(entity, packet);
        }

        updateStatistics();
    }

    /// <summary>
    /// Removes entity from the main dictionary.
    /// </summary>
    /// <param name="type">Type of resource this entity creates/destroys.</param>
    /// <param name="entity">Entity to Destroy.</param>
    public void RemoveEntity(WorldResources.Type type, IGameEntity entity)
    {
        statistics[type].Remove(entity);
        updateStatistics();
    }

    // Others

    /// <summary>
    /// Returns the stat from a given dictionary.
    /// </summary>
    /// <param name="dict"></param>
    /// <returns></returns>
    private float sumDict(Dictionary<IGameEntity, GrowthStatsPacket> dict)
    {
        float sum = 0;
        float maxTime = 1;

        foreach (KeyValuePair<IGameEntity, GrowthStatsPacket> tuple in dict)
        {
            if (maxTime < tuple.Value.updateTime) maxTime = tuple.Value.updateTime;
        }
        foreach (KeyValuePair<IGameEntity, GrowthStatsPacket> tuple in dict)
        {
            sum += (maxTime / tuple.Value.updateTime) * tuple.Value.amount;
        }

        return sum / maxTime;
    }

    private Image GetArrow(Image image, float amount)
    {
        image.sprite = (amount >= 0f) ? up : down;
        image.color = (amount >= 0f) ? Color.green : Color.red;

        return image;
    }



    // Updating Texts

    public void updateAmounts()
    {
        if (_owner != BasePlayer.player)
        {
            return;
        }

        EntityAbilitiesController.get.ControlButtonsInteractability();

        for (int i = 0; i < txt_names.Length; i++)
        {
            res_amounts[i].text = "" + resources[(WorldResources.Type)i];
        }
    }

    public void updatePopulation()
    {
        if (_owner != BasePlayer.player)
        {
            return;
        }

        pop.text = PopulationInfo.get.number_of_units.ToString();
    }

    public void updateStatistics()
    {
        if (_owner != BasePlayer.player)
        {
            return;
        }

        float amount;

        for (int i = 0; i < txt_names.Length; i++)
        {
            amount = sumDict(statistics[(WorldResources.Type)i]);

            if (res_stats[i] != null)
            {
                res_stats[i].text = "" + Math.Abs(Math.Round(amount, 2));
                res_stats[i].color = amount >= 0 ? Color.green : Color.red;
                arrows[i] = GetArrow(arrows[i], amount);
            }
        }

    }

    public bool enoughResources(EntityAbility info)
    {
        if (info.targetType.Equals(EntityType.BUILDING))
            return enoughResources(Info.get.of(info.targetRace, info.targetBuilding).resources);

        return enoughResources(Info.get.of(info.targetRace, info.targetUnit).resources);
    }

    public bool enoughResources(EntityResources res)
    {        
        return enoughResources(WorldResources.Type.FOOD, res.food) &&
                enoughResources(WorldResources.Type.METAL, res.metal) &&
                enoughResources(WorldResources.Type.WOOD, res.wood);
    }

    public bool enoughResources(WorldResources.Type type, float amount)
    {
        return amount <= resources[type];
    }

    // Setup GameObjects

    /// <summary>
    /// Just initializations.
    /// </summary>
    private void Setup()
    {
        if (_owner != BasePlayer.player)
        {
            return;
        }

        for (int i = 0; i < txt_names.Length; i++)
        {
            GameObject obj;
            Text text;
            Image image;

            string _text = "HUD/resources/text_" + txt_names[i];
            string _stats = "HUD/resources/text_" + txt_names[i] + "_hour";
            string _arrow = "HUD/resources/flecha_" + txt_namesOther[i];

            obj = GameObject.Find(_text);
            if (!obj) throw new Exception("Object " + _text + " not found!");

            text = obj.GetComponent<Text>();
            if (!text) throw new Exception("Component " + _text + " not found!");

            res_amounts.Add(text);


            obj = GameObject.Find(_stats);
            if (!obj) throw new Exception("Object " + _stats + " not found!");

            text = obj.GetComponent<Text>();
            if (!text) throw new Exception("Component " + _stats + " not found!");

            res_stats.Add(text);


            obj = GameObject.Find(_arrow);
            if (!obj) throw new Exception("Object " + _arrow + " not found!");

            image = obj.GetComponent<Image>();
            if (!text) throw new Exception("Component " + _arrow + " not found!");

            arrows.Add(image);

        }

        GameObject go = GameObject.Find("HUD/resources/text_population");
        if (!go) throw new Exception("Object text_population not found!");


        pop = go.GetComponent<Text>();

        up = Resources.Load<Sprite>("Statistics_icons/flecha_verde");
        if (up == null) throw new NullReferenceException("Up icon not found!");

        down = Resources.Load<Sprite>("Statistics_icons/flecha_roja");
        if (down == null) throw new NullReferenceException("Down icon not found!");

        foreach (Text t in res_amounts)
        {
            t.color = Color.white;
            t.fontStyle = FontStyle.BoldAndItalic;
        }
    }
}
public class CollectableGood
{
    public IGameEntity entity;
    public Goods goods;
}