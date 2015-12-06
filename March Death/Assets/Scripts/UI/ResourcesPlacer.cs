using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Storage;
using System.Collections.Generic;
using Utils;
using System;

public class ResourcesPlacer : MonoBehaviour
{
    // attributes
    private const float UPDATE_STATS = 1.5f;
    private float _timer = 1.5f;

    private readonly string[] txt_names = { "meat", "wood", "metal" };
    private WorldResources.Type[] t = { WorldResources.Type.FOOD, WorldResources.Type.WOOD, WorldResources.Type.METAL };
    private List<Text> res_amounts;
    private List<Text> res_stats;
    private Text pop;

    private float[] _statistics = { 0f, 0f, 0f };

    void Start()
    {
        res_amounts = new List<Text>();
        res_stats = new List<Text>();
        GameObject gameInformationObject = GameObject.Find("GameInformationObject");

        for (int i = 0; i < txt_names.Length; i++)
        {

            GameObject obj;
            Text text;

            string _text = "HUD/resources/text_" + txt_names[i];
            string _stats = "HUD/resources/text_" + txt_names[i] + "_hour";


            obj = GameObject.Find(_text);
            if (!obj) throw new Exception("Object " + _text + " not found!");

            text = obj.GetComponent<Text>();
            if (!text) throw new Exception("Component " + _text + " not found!");

            res_amounts.Add(text);

            obj = GameObject.Find(_stats);
            if (!obj) throw new Exception("Object " + _text + " not found!");

            text = obj.GetComponent<Text>();
            if (!text) throw new Exception("Component " + _text + " not found!");

            res_stats.Add(text);
        }

        GameObject go = GameObject.Find("HUD/resources/text_population");
        if (!go) throw new Exception("Object text_population not found!");

        pop = go.GetComponent<Text>();

        setupText();
        updateAmounts();

        // Regiter for all unit created events
        Subscriber<Selectable.Actions, Selectable>.get.registerForAll(Selectable.Actions.CREATED, onUnitCreated);
    }

    void Update()
    {
        if (_timer >= UPDATE_STATS)
        {
            updateStatistics();
            updatePopulation();
            _timer = 0f;
        }
        else _timer += Time.deltaTime;

    }

    void OnDestroy()
    {
        Subscriber<Selectable.Actions, Selectable>.get.unregisterFromAll(Selectable.Actions.CREATED, onUnitCreated);
    }

    // PUBLIC METHODS

    /// <summary>
    /// Substracts the costs for the unit from the players deposit.
    /// </summary>
    /// <param name="entity"></param>
    public void updateUnitCreated(IGameEntity entity)
    {
        BasePlayer.getOwner(entity).resources.SubstractAmount(WorldResources.Type.FOOD, entity.info.resources.food);
        BasePlayer.getOwner(entity).resources.SubstractAmount(WorldResources.Type.WOOD, entity.info.resources.wood);
        BasePlayer.getOwner(entity).resources.SubstractAmount(WorldResources.Type.METAL, entity.info.resources.metal);

        // TODO: Simplify, BasePlayer.isFromPlayer(entity)
        if (BasePlayer.getOwner(entity) == BasePlayer.player)
        {
            updateAmounts();
        }
    }

    public void updateAmounts()
    {
        for (int i = 0; i < txt_names.Length; i++)
        {
            res_amounts[i].text = "" + BasePlayer.player.resources.getAmount(t[i]);
        }
    }

    public void updatePopulation()
    {
        pop.text = PopulationInfo.get.number_of_units.ToString();
    }

    public void updateStatistics()
    {
        for (int i = 0; i < txt_names.Length; i++)
        {
            res_stats[i].text = "" + System.Math.Round(_statistics[i], 2) + "/s";
            res_stats[i].color = _statistics[i] >= 0 ? Color.gray : Color.red;
        }
    }

    public void insufficientFundsColor(IGameEntity entity)
    {
        if (entity.info.resources.food > BasePlayer.player.resources.getAmount(WorldResources.Type.FOOD))
            res_amounts[0].color = Color.red;
        if (entity.info.resources.wood > BasePlayer.player.resources.getAmount(WorldResources.Type.WOOD))
            res_amounts[1].color = Color.red;
        if (entity.info.resources.metal > BasePlayer.player.resources.getAmount(WorldResources.Type.METAL))
            res_amounts[2].color = Color.red;
    }

    public void clearColor()
    {
        foreach (Text t in res_amounts)
            t.color = Color.white;
    }

    // PRIVATE METHODS

    /// <summary>
    /// Checks if the game object is a hero or a stronghold. (Improve)
    /// </summary>
    /// <param name="ige"></param>
    /// <returns></returns>
    private bool isStarter(IGameEntity ige)
    {
        if (ige.info.isBuilding)
        {
            // TODO: If more Stronghold(s) can ever be done, this will not work
            if (ige.info.isBarrack)
            {
                return ((Barrack)ige).type == BuildingTypes.STRONGHOLD;
            }

            return false;
        }
        else
        {
            // TODO: If more Hero(s) can ever be done, this will not work
            return ((Unit)ige).type != UnitTypes.HERO;
        }
    }

    private void setupText()
    {
        foreach (Text t in res_amounts)
        {
            t.color = Color.white;
            t.fontStyle = FontStyle.BoldAndItalic;
        }
    }


    // EVENTS

    void onUnitCreated(System.Object obj)
    {
        GameObject go = (GameObject)obj;

        if (go)
        {
            IGameEntity i_game = go.GetComponent<IGameEntity>();

            if (!isStarter(i_game))
            {
                updateUnitCreated(i_game);
            }
        }
    }


    public void onStatisticsUpdate(System.Object obj)
    {
        Statistics st = (Statistics)obj;

        switch (st._type)
        {
            case WorldResources.Type.FOOD:
                _statistics[0] += st.growth_speed;
                break;
            case WorldResources.Type.WOOD:
                _statistics[1] += st.growth_speed;
                break;
            case WorldResources.Type.METAL:
                _statistics[2] += st.growth_speed;
                break;
            default:
                break;
        }

    }

    public void onFoodConsumption(System.Object obj)
    {
        CollectableGood collectable = (CollectableGood)obj;
        Goods goods = collectable.goods;

        BasePlayer.getOwner(collectable.entity).resources.SubstractAmount(t[0], goods.amount); // t[0] is FOOD

        // TODO: Simplify, BasePlayer.isFromPlayer(entity)
        if (BasePlayer.getOwner(collectable.entity) == BasePlayer.player)
        {
            updateAmounts();
        }
    }

    public void onCollection(System.Object obj)
    {
        CollectableGood collectable = (CollectableGood)obj;
        Goods goods = collectable.goods;

        switch (goods.type)
        {
            case Goods.GoodsType.FOOD:
                BasePlayer.getOwner(collectable.entity).resources.AddAmount(t[0], goods.amount);
                break;
            case Goods.GoodsType.WOOD:
                BasePlayer.getOwner(collectable.entity).resources.AddAmount(t[1], goods.amount);
                break;
            case Goods.GoodsType.METAL:
                BasePlayer.getOwner(collectable.entity).resources.AddAmount(t[2], goods.amount);
                break;
            default:
                break;
        }

        // TODO: Simplify, BasePlayer.isFromPlayer(entity)
        if (BasePlayer.getOwner(collectable.entity) == BasePlayer.player)
        {
            updateAmounts();
        }
    }
}

public class CollectableGood
{
    public IGameEntity entity;
    public Goods goods;
}
