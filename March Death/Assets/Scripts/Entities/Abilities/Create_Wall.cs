using System;
using Storage;

using UnityEngine;

class CreateWall : Create
{
    protected override BuildingTypes _type { get { return BuildingTypes.WALL; } }

    public CreateWall(EntityAbility info, GameObject gameObject) :
        base(info, gameObject)
    {
    }
}