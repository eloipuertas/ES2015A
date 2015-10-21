using System;
using Storage;

using UnityEngine;

class CreateFarm : Create
{
    protected override BuildingTypes _type { get { return BuildingTypes.FARM; } }

    public CreateFarm(EntityAbility info, GameObject gameObject) :
        base(info, gameObject)
    {
    }
}
