using System;
using Storage;

using UnityEngine;

class CreateSawmill : Create
{    
    protected override BuildingTypes _type { get { return BuildingTypes.SAWMILL; } }

    public CreateSawmill(EntityAbility info, GameObject gameObject) :
        base(info, gameObject)
    {
    }
}
