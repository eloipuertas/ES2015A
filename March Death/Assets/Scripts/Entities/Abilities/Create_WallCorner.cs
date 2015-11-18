using System;
using Storage;

using UnityEngine;

class CreateWallCorner : Create
{
    protected override BuildingTypes _type { get { return BuildingTypes.WALLCORNER; } }

    public CreateWallCorner(EntityAbility info, GameObject gameObject) :
        base(info, gameObject)
    {
    }
}