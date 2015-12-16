using UnityEngine;
using Utils;
using Storage;
using System.Collections.Generic;

public class ResourcesEvents : Singleton<ResourcesEvents>
{

    private ResourcesEvents() { }

    // REGISTER METHODS
    public void registerBuildingToEvents(IGameEntity entity)
    {
        if (entity.info.isResource)
        {
            Resource resource = (Resource)entity;
            resource.register(Resource.Actions.NEW_HARVEST, OnNewHarvest);
            resource.register(Resource.Actions.NEW_EXPLORER, OnNewExplorer);
            resource.register(Resource.Actions.COLLECTION, OnCollection);
            resource.register(Resource.Actions.CREATED, OnCreated);
            resource.register(Resource.Actions.EXTERMINATED, OnDestroyed);
        }
        else if (entity.info.isBarrack)
        {
            Barrack barrack = (Barrack)entity;
            barrack.register(Barrack.Actions.CREATED, OnCreated);
        }
    }

    public void registerUnitToEvents(IGameEntity entity)
    {
        if (entity.info.isUnit)
        {
            Unit unit = (Unit)entity;
            unit.register(Unit.Actions.EAT, OnConsumption);
            unit.register(Unit.Actions.CREATED, OnCreated);
            unit.register(Unit.Actions.EXTERMINATED, OnDestroyed);
        }
    }

    public void unregisterBuildingToEvents(IGameEntity entity)
    {
        if (entity.info.isResource)
        {
            Resource resource = (Resource)entity;
            resource.unregister(Resource.Actions.NEW_HARVEST, OnNewHarvest);
            resource.unregister(Resource.Actions.NEW_EXPLORER, OnNewExplorer);
            resource.unregister(Resource.Actions.COLLECTION, OnCollection);
            resource.unregister(Resource.Actions.CREATED, OnCreated);
            resource.unregister(Resource.Actions.EXTERMINATED, OnDestroyed);
        }
        else if (entity.info.isBarrack)
        {
            Barrack barrack = (Barrack)entity;
            barrack.unregister(Barrack.Actions.CREATED, OnCreated);
        }
    }

    public void unregisterUnitToEvents(IGameEntity entity)
    {
        if (entity.info.isUnit)
        {
            Unit unit = (Unit)entity;
            unit.unregister(Unit.Actions.EAT, OnConsumption);
            unit.unregister(Unit.Actions.CREATED, OnCreated);
            unit.unregister(Unit.Actions.EXTERMINATED, OnDestroyed);
        }
    }


    // EVENT METHODS

    private void OnNewHarvest(System.Object obj)
    {
        PopulationInfo.get.AddWorker();

        IGameEntity entity = (IGameEntity)obj;
        ResourcesPlacer.get(BasePlayer.getOwner(entity)).StatisticsChanged(entity, CreatePackageFromEntity(entity));
    }


    private void OnNewExplorer(System.Object obj)
    {
        PopulationInfo.get.RemoveWorker();

        IGameEntity entity = (IGameEntity)obj;
        ResourcesPlacer.get(BasePlayer.getOwner(entity)).StatisticsChanged(entity, CreatePackageFromEntity(entity));
    }


    private void OnCollection(System.Object obj)
    {
        CollectableGood collectable = (CollectableGood) obj;
        BasePlayer player = BasePlayer.getOwner(collectable.entity);
        ResourcesPlacer.get(player).Collect(collectable.goods.type, collectable.goods.amount);
    }


    private void OnConsumption(System.Object obj)
    {
        CollectableGood collectable = (CollectableGood)obj;
        BasePlayer player = BasePlayer.getOwner(collectable.entity);
        ResourcesPlacer.get(player).Collect(collectable.goods.type, collectable.goods.amount);
    }


    private void OnCreated(System.Object obj)
    {
        IGameEntity entity = (IGameEntity)obj;
        EntityResources res = entity.info.resources;

        Dictionary<WorldResources.Type, float> d = new Dictionary<WorldResources.Type, float>()
        {
            {  WorldResources.Type.FOOD , res.food },
            {  WorldResources.Type.WOOD , res.wood },
            {  WorldResources.Type.METAL , res.metal }
        };

        BasePlayer owner = BasePlayer.getOwner(entity);
        ResourcesPlacer placer = ResourcesPlacer.get(owner);

        bool isUnit = entity.doIfUnit(unit =>
        {
            if (unit.type != UnitTypes.HERO)
            {
                placer.Buy(d);
            }
        });

        if (!isUnit)
        {
            if (((BuildingInfo)entity.info).type != BuildingTypes.STRONGHOLD)
            {
                placer.Buy(d);
            }
        }

        placer.updatePopulation();

        if (entity.info.isResource || entity.info.isUnit)
        {
            placer.StatisticsChanged(entity, CreatePackageFromEntity(entity));
        }
    }


    private void OnDestroyed(System.Object obj)
    {
        IGameEntity entity = (IGameEntity)obj;
        BasePlayer owner = BasePlayer.getOwner(entity);
        ResourcesPlacer placer = ResourcesPlacer.get(owner);

        if (entity.info.isUnit)
        {
            Unit unit = (Unit)entity;
            placer.RemoveEntity(WorldResources.Type.FOOD, entity);
        }
        else if (((BuildingInfo)entity.info).type != BuildingTypes.STRONGHOLD)
        {
            Resource resource = (Resource)entity;
            placer.RemoveEntity(GetElementFromResource(resource), entity);
        }

        placer.updatePopulation();

    }


    // **************************************************************

    /// <summary>
    /// Creates a new package from a given entity.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    private GrowthStatsPacket CreatePackageFromEntity(IGameEntity entity)
    {
        GrowthStatsPacket packet = new GrowthStatsPacket();

        if (entity.info.isResource)
        {
            Resource r = (Resource)entity;

            packet = new GrowthStatsPacket( GetElementFromResource(r) , Mathf.Min(r.HUD_productionRate, r.HUD_currentProductionRate) ,
                                            r.info.resourceAttributes.updateInterval);
        }
        if (entity.info.isUnit)
        {
            Unit unit = (Unit)entity;
            packet = new GrowthStatsPacket(WorldResources.Type.FOOD, -unit.info.unitAttributes.foodConsumption, 1f);
        }

        return packet;
    }

    /// <summary>
    /// Returns the element the resource building grows.
    /// </summary>
    /// <param name="resource"></param>
    /// <returns></returns>
    private WorldResources.Type GetElementFromResource(Resource resource)
    {
        WorldResources.Type t;

        switch (resource.type) {
            case BuildingTypes.FARM:
                t = WorldResources.Type.FOOD;
                break;
            case BuildingTypes.SAWMILL:
                t = WorldResources.Type.WOOD;
                break;
            case BuildingTypes.MINE:
                t = WorldResources.Type.METAL;
                break;
            default:
                t = WorldResources.Type.FOOD;
                break;
        }

        return t;
    }


}

/// <summary>
///  Class to pack the information relative to Growth Statistics.
/// </summary>
public class GrowthStatsPacket
{
    private WorldResources.Type _type;
    private float _amount;
    private float _updateTime;

    public WorldResources.Type type { get { return _type; } }
    public float amount { get { return _amount; } }
    public float updateTime { get { return _updateTime; } }


    public GrowthStatsPacket() { }

    public GrowthStatsPacket(WorldResources.Type type, float amount , float updateTime)
    {
        _type = type;
        _amount = amount;
        _updateTime = updateTime;
    }

}
