using UnityEngine;
using Utils;
using System.Collections;

public class ResourcesEvents : Singleton<ResourcesEvents>
{

    private ResourcesEvents()
    {

    }

    public void registerToEvents(IGameEntity entity)
    {
        if (entity.info.isResource)
        {
            Resource resource = (Resource)entity;
            resource.register(Resource.Actions.NEW_HARVEST, OnNewHarvest);
            resource.register(Resource.Actions.NEW_EXPLORER, OnNewExplorer);
        }
    }

    private void OnNewHarvest(System.Object obj)
    {
        PopulationInfo.get.AddWorker();
    }

    private void OnNewExplorer(System.Object obj)
    {
        PopulationInfo.get.RemoveWorker();
    }
}
