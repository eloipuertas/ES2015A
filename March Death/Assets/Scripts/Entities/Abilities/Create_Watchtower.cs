using System;
using Storage;

using UnityEngine;

class CreateWatchtower : Create
{
    protected override BuildingTypes _type { get { return BuildingTypes.WATCHTOWER; } }

    public CreateWatchtower(EntityAbility info, GameObject gameObject) :
        base(info, gameObject)
    {
    }
}