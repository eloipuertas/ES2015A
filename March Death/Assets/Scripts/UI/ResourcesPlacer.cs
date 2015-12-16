using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Storage;
using System.Collections.Generic;
using Utils;
using System;

public class ResourcesPlacer : Singleton<ResourcesPlacer>
{
    // attributes
    private readonly string[] txt_names = { "meat", "wood", "metal" };
    private readonly string[] txt_namesOther = { "food", "wood", "metal" };

    private List<Text> res_amounts, res_stats;

    /// <summary>
    /// Population text where number of units are displayed
    /// </summary>
    private Text pop;
    private Player _player;

    private Sprite up;
    private Sprite down;

    private List<Image> arrows;

    Dictionary<WorldResources.Type, int> resources;
    Dictionary<WorldResources.Type, Dictionary<IGameEntity, GrowthStatsPacket>> statistics;


    private ResourcesPlacer()
    {
        res_amounts = new List<Text>();
        res_stats = new List<Text>();
        arrows = new List<Image>();

        Setup();

        initializeResources();
        initializeStatistics();

        updateAmounts();
        updateStatistics();
        updatePopulation();
    }


    // Resources 

    private void initializeResources()
    {
        resources = new Dictionary<WorldResources.Type, int>()
        {
            { WorldResources.Type.FOOD ,  (int) _player.resources.getAmount(WorldResources.Type.FOOD) } ,
            { WorldResources.Type.WOOD ,  (int) _player.resources.getAmount(WorldResources.Type.WOOD) } ,
            { WorldResources.Type.METAL , (int) _player.resources.getAmount(WorldResources.Type.METAL) }
        };
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
        EntityAbilitiesController.ControlButtonsInteractability();

        for (int i = 0; i < txt_names.Length; i++)
        {
            res_amounts[i].text = "" + resources[(WorldResources.Type)i];
        }
    }

    public void updatePopulation()
    {
        pop.text = PopulationInfo.get.number_of_units.ToString();
    }

    public void updateStatistics()
    {
        float amount;

        for (int i = 0; i < txt_names.Length; i++)
        {
            amount = sumDict(statistics[(WorldResources.Type)i]);

            if (res_stats[i] != null)
            {
                res_stats[i].text = "" + Math.Abs(Math.Round(amount, 2));
                res_stats[i].color = amount >= 0 ? Color.green : Color.red;
                arrows[i] = GetArrow(arrows[i], amount);
                Debug.Log(i + " - " + arrows[i].name);
            }
        }

    }

    public bool enoughResources(EntityAbility info)
    {
        EntityResources res;

        if (info.targetType.Equals(EntityType.BUILDING))
            res = Info.get.of(info.targetRace, info.targetBuilding).resources;
        else
            res = Info.get.of(info.targetRace, info.targetUnit).resources;

        if (res.food <= resources[WorldResources.Type.FOOD] && res.wood <= resources[WorldResources.Type.WOOD] &&
            res.metal <= resources[WorldResources.Type.METAL])
            return true;
        else
            return false;
    }

    // Setup GameObjects

    /// <summary>
    /// Just initializations.
    /// </summary>
    private void Setup()
    {
        _player = GameObject.Find("GameController").GetComponent<Player>();

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