using System;
using Storage;

using UnityEngine;

class CreateFarm : Create
{    
	protected BuildingTypes _type = BuildingTypes.FARM;

    public CreateFarm(EntityAbility info, GameObject gameObject) :
        base(info, gameObject)
    {
    }
}
