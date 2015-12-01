using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Utils;
using Storage;

public sealed class PopulationInfo : Singleton<PopulationInfo>
{
    // Dictionaries to keep track of units and buildings
    Dictionary<BuildingTypes, int> buildings;
    Dictionary<UnitTypes, int> units;

    public int number_of_units { get; private set; }
    public int number_of_buildings { get; private set; }

    private PopulationInfo()
    {
        buildings = new Dictionary<BuildingTypes, int>();
        units = new Dictionary<UnitTypes, int>();

        Setup();
    }

    

    private void addToBuilding(BuildingTypes type)
    {
        if (buildings.ContainsKey(type))
        {
            buildings[type] += 1;
        }
        else
        {
            buildings.Add(type, 1);
        }

        number_of_buildings += 1;
    }

    private void addToUnit(UnitTypes type)
    {
        if (units.ContainsKey(type))
        {
            units[type] += 1;
        }
        else
        {
            units.Add(type, 1);
        }

        number_of_units += 1;
    }

    private void removeToBuilding(BuildingTypes type)
    {
        if (buildings.ContainsKey(type))
        {
            buildings[type] -= 1;
        }
        else
        {
            buildings.Add(type, 0);
        }

        number_of_buildings -= 1;
    }

    private void removeToUnit(UnitTypes type)
    {
        if (units.ContainsKey(type))
        {
            units[type] -= 1;
        }
        else
        {
            units.Add(type, 0);
        }

        number_of_units -= 1;
    }

    /// <summary>
    /// Adds 1 from a unit or buiilding type
    /// </summary>
    /// <param name="entity"></param>
    public void Add(IGameEntity entity)
    {
        if (entity.info.isUnit)
        {
            addToUnit(((Unit)entity).type);
        }
        else
        {
            addToBuilding(((BuildingInfo)entity.info).type);
        }
    }

    /// <summary>
    /// Removes 1 from a unit or buiilding type
    /// </summary>
    /// <param name="entity"></param>
    public void Remove(IGameEntity entity)
    {
        if (entity.info.isUnit)
        {
            removeToUnit(((Unit)entity).type);
        }
        else
        {
            removeToBuilding(((BuildingInfo)entity.info).type);
        }
    }


    /*****************************************************************
     *          OTHER METHODS
     *******************************************************************/

    public override string ToString()
    {
        string str = "";

        str += "> Units \n";
        foreach (KeyValuePair<UnitTypes, int> entry in units)
        {
            str += entry.Key + ": " + entry.Value + "\n";
        }

        str += "\n> Buildings \n";
        foreach (KeyValuePair<BuildingTypes, int> entry in buildings)
        {
            str += entry.Key + ": " + entry.Value + "\n";
        }

        return str;
    }

    public List<string> GetUnitKeys()
    {
        List<string> strList = new List<string>();
         
        foreach (KeyValuePair<UnitTypes, int> entry in units)
        {
            strList.Add(entry.Key.ToString());
        }

        return strList;
    }

    public List<string> GetUnitValues()
    {
        List<string> strList = new List<string>();

        foreach (KeyValuePair<UnitTypes, int> entry in units)
        {
            strList.Add(entry.Value.ToString());
        }

        return strList;
    }

    public List<string> GetBuildingKeys()
    {
        List<string> strList = new List<string>();

        foreach (KeyValuePair<BuildingTypes, int> entry in buildings)
        {
            strList.Add(entry.Key.ToString());
        }

        return strList;
    }

    public List<string> GetBuildingValues()
    {
        List<string> strList = new List<string>();

        foreach (KeyValuePair<BuildingTypes, int> entry in buildings)
        {
            strList.Add(entry.Value.ToString());
        }

        return strList;
    }


    // INITIALIZE
    private void Setup()
    {
        foreach (BuildingTypes type in Enum.GetValues(typeof(BuildingTypes)) )
        {
            buildings.Add(type, 0);
        }

        foreach (UnitTypes type in Enum.GetValues(typeof(UnitTypes)))
        {
            units.Add(type, 0);
        }

        units[UnitTypes.HERO] += 1;
        buildings[BuildingTypes.STRONGHOLD] += 1;
        number_of_units = 1;
        number_of_buildings = 1;
    }
}
