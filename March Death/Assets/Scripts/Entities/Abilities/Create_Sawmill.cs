using System;
using Storage;

using UnityEngine;

class CreateSawmill : Create
{    
    protected BuildingTypes _type = BuildingTypes.SAWMILL;

    public CreateSawmill(EntityAbility info, GameObject gameObject) :
        base(info, gameObject)
    {
    }
}
