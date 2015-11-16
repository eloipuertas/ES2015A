using System;
﻿using UnityEngine;
using System.Collections.Generic;

using Utils;
using Newtonsoft.Json;

namespace Storage
{
    /// <summary>
    /// Info singleton class might be used to query information of a given
    /// unit race and type.
    /// It automatically parses all units on Assets/Units and stores it.
    /// </summary>
    sealed class Info : Singleton<Info>
    {
        private Dictionary<Tuple<Races, UnitTypes>, EntityInfo> unitStore = new Dictionary<Tuple<Races, UnitTypes>, EntityInfo>();
        private Dictionary<Tuple<Races, UnitTypes>, List<string>> unitPrefabs = new Dictionary<Tuple<Races, UnitTypes>, List<string>>();

        private Dictionary<Tuple<Races, BuildingTypes>, EntityInfo> buildingStore = new Dictionary<Tuple<Races, BuildingTypes>, EntityInfo>();
        private Dictionary<Tuple<Races, BuildingTypes>, List<string>> buildingPrefabs = new Dictionary<Tuple<Races, BuildingTypes>, List<string>>();

        /// <summary>
        /// Private constructor, singleton access only
        /// <remarks>Use Info.get instead</remarks>
        /// </summary>
        private Info()
        {
            parseJSONFiles<UnitInfo, UnitTypes>("Data/Units", unitStore, EntityType.UNIT);
            parseJSONFiles<ResourceInfo, BuildingTypes>("Data/Buildings/Resources", buildingStore, EntityType.BUILDING);
            parseJSONFiles<BarrackInfo, BuildingTypes>("Data/Buildings/Barracks", buildingStore, EntityType.BUILDING);
            parseJSONFiles<BarrackInfo, BuildingTypes>("Data/Buildings/Archery", buildingStore, EntityType.BUILDING);
            parseJSONFiles<BarrackInfo, BuildingTypes>("Data/Buildings/Stable", buildingStore, EntityType.BUILDING);
            

            parsePrefabs<Unit, UnitTypes>("Prefabs/Units", unitPrefabs);
            parsePrefabs<Resource, BuildingTypes>("Prefabs/Buildings/Resources", buildingPrefabs);
            parsePrefabs<Barrack, BuildingTypes>("Prefabs/Buildings/Barracks", buildingPrefabs);
            parsePrefabs<Barrack, BuildingTypes>("Prefabs/Buildings/Military", buildingPrefabs);
        }

        /// <summary>
        /// Parses all unit files on "Resources/Data/Units".
        /// <exception cref="System.FileLoadException">
        /// Thrown when a unit file is not valid or has already been added
        /// </exception>
        /// </summary>
        private void parseJSONFiles<JSONType, EnumType>(string folder, Dictionary<Tuple<Races, EnumType>, EntityInfo> store, EntityType entityType) where JSONType : EntityInfo where EnumType : struct, IConvertible
        {
            Debug.Log("Parsing " + typeof(JSONType));

            UnityEngine.Object[] assets = Resources.LoadAll(folder, typeof(TextAsset));
            foreach (UnityEngine.Object jsonObj in assets)
            {
                TextAsset json = jsonObj as TextAsset;
                Debug.Log("\tParsing " + json.name);

                try
                {
                    EntityInfo entityInfo = JsonConvert.DeserializeObject<JSONType>(json.text);
                    entityInfo.entityType = entityType;

                    foreach (EntityAbility ability in entityInfo.abilities)
                    {
                        ability.tooltip = ability.tooltip.FormatWith(entityInfo, @"\[\[", @"\]\]");
                        ability.tooltip = ability.tooltip.FormatWith(ability, @"\(\(", @"\)\)");
                    }

                    Tuple<Races, EnumType> key = new Tuple<Races, EnumType>(entityInfo.race, entityInfo.getType<EnumType>());

                    if (store.ContainsKey(key))
                    {
                        throw new System.IO.FileLoadException("Unit info '" + json.name + "' already exists");
                    }

                    store.Add(key, entityInfo);
                }
                catch (JsonException e)
                {
                    throw new System.IO.FileLoadException(typeof(JSONType) + " '" + json.name + "' is invalid\n\t" + e.Message);
                }
            }
        }

        /// <summary>
        /// Parses all prefabs on "Resources/Prefabs/Units".
        /// <exception cref="System.FileLoadException">
        /// Thrown when two prefabs define the same Race and UnitType
        /// </exception>
        /// </summary>
        private void parsePrefabs<ComponentType, EnumType>(string folder, Dictionary<Tuple<Races, EnumType>, List<string>> store) where EnumType : struct, IConvertible where ComponentType : IGameEntity
        {
            UnityEngine.Object[] assets = Resources.LoadAll(folder, typeof(GameObject));
            foreach (UnityEngine.Object asset in assets)
            {
                GameObject gameObject = asset as GameObject;
                ComponentType component = gameObject.GetComponent<ComponentType>();

                if (component != null)
                {
                    Tuple<Races, EnumType> key = new Tuple<Races, EnumType>(component.getRace(), component.getType<EnumType>());

                    if (!store.ContainsKey(key))
                    {
                        store.Add(key, new List<string>());
                    }
                    else
                    {
                        Debug.LogWarning("Intentional? Duplicated " + typeof(ComponentType) + " prefab ('" + component.getRace() + "', '" + component.getType<EnumType>() + "')");
                    }

                    store[key].Add(folder + "/" + gameObject.name);
                }
            }
        }

        /// <summary>
        /// Gathers information for a race and type.
        /// </summary>
        /// <param name="race">Race to look for</param>
        /// <param name="type">Type to look for</param>
        /// <exception cref="System.ArgumentException">Thrown when a race/type combination is not found</exception>
        /// <returns>The UnitInfo object of that race/type combination</returns>
        public UnitInfo of(Races race, UnitTypes type)
        {
            Tuple<Races, UnitTypes> key = new Tuple<Races, UnitTypes>(race, type);

            if (!unitStore.ContainsKey(key))
            {
                throw new System.ArgumentException("Race (" + race + ") and Type (" + type + ") does not exist");
            }

            return (UnitInfo)unitStore[key];
        }

        /// <summary>
        /// Gathers information for a race and type.
        /// </summary>
        /// <param name="race">Race to look for</param>
        /// <param name="type">Type to look for</param>
        /// <exception cref="System.ArgumentException">Thrown when a race/type combination is not found</exception>
        /// <returns>The UnitInfo object of that race/type combination</returns>
        public BuildingInfo of(Races race, BuildingTypes type)
        {
            Tuple<Races, BuildingTypes> key = new Tuple<Races, BuildingTypes>(race, type);

            if (!buildingStore.ContainsKey(key))
            {
                throw new System.ArgumentException("Race (" + race + ") and Type (" + type + ") does not exist");
            }

            return (BuildingInfo)buildingStore[key];
        }

        /// <sumary>
        /// Given a race and unit it will return its prefab route
        /// </sumary>
        /// <param name="race">Race of the Unit</param>
        /// <param name="type">Type of the Unit</param>
        /// <exception cref="System.ArgumentException">Thrown when a race/type combination is not found</exception>
        /// <returns>The prefab path</returns>
        private string getPrefab(Races race, UnitTypes type, int variant = 0)
        {
            Tuple<Races, UnitTypes> key = new Tuple<Races, UnitTypes>(race, type);

            if (!unitPrefabs.ContainsKey(key))
            {
                throw new System.ArgumentException("Unit prefab for ('" + race+ "', '" + type + "') not found");
            }

            if (variant < 0)
            {
                variant = Utils.D6.get.rollN(unitPrefabs[key].Count);
            }

            return unitPrefabs[key][variant];
        }

        /// <sumary>
        /// Given a race and type it will return its prefab route
        /// </sumary>
        /// <param name="race">Race of the Building</param>
        /// <param name="type">Type of the Building</param>
        /// <exception cref="System.ArgumentException">Thrown when a race/type combination is not found</exception>
        /// <returns>The prefab path</returns>
        private string getPrefab(Races race, BuildingTypes type, int variant = 0)
        {
            Tuple<Races, BuildingTypes> key = new Tuple<Races, BuildingTypes>(race, type);

            if (!buildingPrefabs.ContainsKey(key))
            {
                throw new System.ArgumentException("Resource prefab for ('" + race + "', '" + type + "') not found");
            }

            if (variant < 0)
            {
                variant = Utils.D6.get.rollN(buildingPrefabs[key].Count);
            }

            return buildingPrefabs[key][variant];
        }

        /// <summary>
        /// Creates a Unit of a given race and type from a prefab
        /// </summary>
        /// <param name="race">Race of the Unit</param>
        /// <param name="type">Type of the Unit</param>
        /// <returns>The created GameObject</returns>
        public GameObject createUnit(Races race, UnitTypes type, int variant = -1)
        {
            string prefab = getPrefab(race, type, variant);
            return UnityEngine.Object.Instantiate((GameObject)Resources.Load(prefab, typeof(GameObject)));
        }

        /// <summary>
        /// Creates a Building of a given race and type from a prefab
        /// </summary>
        /// <param name="race">Race of the Building</param>
        /// <param name="type">Type of the Building</param>
        /// <returns>The created GameObject</returns>
        public GameObject createBuilding(Races race, BuildingTypes type, int variant = -1)
        {
            string prefab = getPrefab(race, type, variant);
            return UnityEngine.Object.Instantiate((GameObject)Resources.Load(prefab, typeof(GameObject)));
        }

        /// <summary>
        /// Creates a Unit of a given race and type from a prefab in a certain position and rotation
        /// </summary>
        /// <param name="race">Race of the Unit</param>
        /// <param name="type">Type of the Unit</param>
        /// <param name="position">Unit position</param>
        /// <param name="rotation">Unit rotation</param>
        /// <returns>The created GameObject</returns>
        public GameObject createUnit(Races race, UnitTypes type, Vector3 position, Quaternion rotation, int variant = 0)
        {
            string prefab = getPrefab(race, type, variant);
            return UnityEngine.Object.Instantiate((GameObject)Resources.Load(prefab, typeof(GameObject)), position, rotation) as GameObject;
        }

        /// <summary>
        /// Creates a Building of a given race and type from a prefab in a certain position and rotation
        /// </summary>
        /// <param name="race">Race of the Building</param>
        /// <param name="type">Type of the Building</param>
        /// <param name="position">Building position</param>
        /// <param name="rotation">Building rotation</param>
        /// <returns>The created GameObject</returns>
        public GameObject createBuilding(Races race, BuildingTypes type, Vector3 position, Quaternion rotation, int variant = 0)
        {
            string prefab = getPrefab(race, type, variant);
            return UnityEngine.Object.Instantiate((GameObject)Resources.Load(prefab, typeof(GameObject)), position, rotation) as GameObject;
        }
    }
}
