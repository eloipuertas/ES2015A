using System;
using Storage;

using UnityEngine;

class CreateMine : Create
{
    protected override BuildingTypes _type { get { return BuildingTypes.MINE; } }

    public CreateMine(EntityAbility info, GameObject gameObject) :
        base(info, gameObject)
    {
    }
}
