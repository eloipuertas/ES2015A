using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Storage;


/// <summary>
/// Building base class. Extends actor (which in turn extends MonoBehaviour) to
/// handle basic API operations
/// </summary>
public class Building : GameEntity<Building.Actions>
{
    public enum Actions { DAMAGED, DESTROYED };

    public Building() { }

    /// <summary>
    /// Edit this on the Prefab to set Units of certain races/types
    /// </summary>
    public Races race = Races.MEN;
    public BuildingTypes type = BuildingTypes.STRONGHOLD;

    /// <summary>
    /// List of ability objects of this building
    /// </summary>
    protected List<IBuildingAbility> _abilities;

    /// <summary>
    /// Contains all static information of the Building.
    /// That means: max health, damage, defense, etc.
    /// </summary>
    protected BuildingInfo _info;
    protected BuildingAttributes _attributes;
    

    /// <summary>
    /// Iterates all abilities on the
    /// </summary>
    protected void setupAbilities()
    {
        foreach (BuildingAbility ability in _info.actions)
        {
            // Try to get class with this name
            string abilityName = ability.name.Replace(" ", "");

            try
            {
                var constructor = Type.GetType(abilityName).
                    GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(BuildingAbility), typeof(GameObject) }, null);
                if (constructor == null)
                {
                    // No such constructor, construct default
                    _actions.Add(new GenericBuildingAbility(ability));
                }
                else
                {
                    // Class found, use that!
                    _actions.Add((IBuildingAbility)constructor.Invoke(new object[2] { ability, gameObject }));
                }
            }
            catch (Exception)
            {
                // No such class, use the GenericAbility class
                _actions.Add(new GenericBuildingAbility(ability));
            }
        }
    }

    /// <summary>
    /// When a wound is received, this is called
    /// </summary>
    protected override void onReceiveDamage()
    {
        fire(Actions.DAMAGED);
    }

    /// <summary>
    /// When wounds reach its maximum, thus unit dies, this is called
    /// </summary>
    protected override void onFatalWounds()
    {
        fire(Actions.DESTROYED);
    }

    /// <summary>
    /// Object initialization
    /// </summary>
    public override void Start()
    {
        _status = EntityStatus.IDLE;
        _info = Info.get.of(race, type);

        // Call GameEntity start
        base.Start();
    }

    /// <summary>
    /// Called once a frame to update the object
    /// </summary>
    void Update()
    {
    }

    /// <summary>
    /// Called every fixed physics frame
    /// </summary>
    void FixedUpdate()
    {
    }
}
