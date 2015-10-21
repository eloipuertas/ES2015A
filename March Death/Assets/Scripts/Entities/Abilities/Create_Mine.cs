using System;
using Storage;

using UnityEngine;

class CreateMine : Create
{    
    protected BuildingTypes _type = BuildingTypes.MINE;

    public CreateMine(EntityAbility info, GameObject gameObject) :
        base(info, gameObject)
    {
    }
}
