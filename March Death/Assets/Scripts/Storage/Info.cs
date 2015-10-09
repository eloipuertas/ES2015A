using UnityEngine;
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
        private Dictionary<Tuple<Races, UnitTypes>, UnitInfo> unitStore = new Dictionary<Tuple<Races, UnitTypes>, UnitInfo>();
        private Dictionary<Tuple<Races, UnitTypes>, string> unitPrefabs = new Dictionary<Tuple<Races, UnitTypes>, string>();

        private Dictionary<Tuple<Races, BuildingTypes>, BuildingInfo> buildingStore = new Dictionary<Tuple<Races, BuildingTypes>, BuildingInfo>();
        private Dictionary<Tuple<Races, BuildingTypes>, string> buildingPrefabs = new Dictionary<Tuple<Races, BuildingTypes>, string>();

        private Dictionary<Tuple<Races, ResourceTypes>, ResourceInfo> resourceStore = new Dictionary<Tuple<Races, ResourceTypes>, ResourceInfo>();
        private Dictionary<Tuple<Races, ResourceTypes>, string> resourcePrefabs = new Dictionary<Tuple<Races, ResourceTypes>, string>();

        /// <summary>
        /// Private constructor, singleton access only
        /// <remarks>Use Info.get instead</remarks>
        /// </summary>
        private Info()
        {
            parseUnitFiles();
            parseResourceFiles();

            parseBuildingFiles();
            parseBuildingPrefabs();

            parsePrefabs();
            parseResourcePrefabs();
        }
    
        /// <summary>
        /// Parses all unit files on "Resources/Data/Units".
        /// <exception cref="System.FileLoadException">
        /// Thrown when a unit file is not valid or has already been added
        /// </exception>
        /// </summary>
        private void parseUnitFiles()
        {
            Object[] assets = Resources.LoadAll("Data/Units", typeof(TextAsset));
            foreach (Object jsonObj in assets)
            {
                TextAsset json = jsonObj as TextAsset;

                try
                {
                    UnitInfo unitInfo = JsonConvert.DeserializeObject<UnitInfo>(json.text);
                    unitInfo.entityType = EntityType.UNIT;

                    Tuple<Races, UnitTypes> key = new Tuple<Races, UnitTypes>(unitInfo.race, unitInfo.type);

                    if (unitStore.ContainsKey(key))
                    {
                        throw new System.IO.FileLoadException("Unit info '" + json.name + "' already exists");
                    }

                    unitStore.Add(key, unitInfo);
                }
                catch (JsonException e)
                {
                    throw new System.IO.FileLoadException("Unit info '" + json.name + "' is invalid\n\t" + e.Message);
                }
            }
        }

        /// <summary>
        /// Parses all unit files on "Resources/Data/Units".
        /// <exception cref="System.FileLoadException">
        /// Thrown when a unit file is not valid or has already been added
        /// </exception>
        /// </summary>
        private void parseBuildingFiles()
        {
            Object[] assets = Resources.LoadAll("Data/Buildings", typeof(TextAsset));
            foreach (Object jsonObj in assets)
            {
                TextAsset json = jsonObj as TextAsset;

                try
                {
                    BuildingInfo buildingInfo = JsonConvert.DeserializeObject<BuildingInfo>(json.text);
                    buildingInfo.entityType = EntityType.BUILDING;

                    Tuple<Races, BuildingTypes> key = new Tuple<Races, BuildingTypes>(buildingInfo.race, buildingInfo.type);

                    if (buildingStore.ContainsKey(key))
                    {
                        throw new System.IO.FileLoadException("Unit info '" + json.name + "' already exists");
                    }

                    buildingStore.Add(key, buildingInfo);
                }
                catch (JsonException e)
                {
                    throw new System.IO.FileLoadException("Unit info '" + json.name + "' is invalid\n\t" + e.Message);
                }
            }
        }

        /// <summary>
        /// Parses all resource files on "Resources/Data/Resources".
        /// <exception cref="System.FileLoadException">
        /// Thrown when a resource file is not valid or has already been added
        /// </exception>
        /// </summary>
        private void parseResourceFiles()
        {
            Object[] assets = Resources.LoadAll("Data/Resources", typeof(TextAsset));
            foreach (Object jsonObj in assets)
            {
                TextAsset json = jsonObj as TextAsset;

                try
                {
                    ResourceInfo resourceInfo = JsonConvert.DeserializeObject<ResourceInfo>(json.text);
                    resourceInfo.entityType = EntityType.RESOURCE;

                    Tuple<Races, ResourceTypes> key = new Tuple<Races, ResourceTypes>(resourceInfo.race, resourceInfo.type);

                    if (resourceStore.ContainsKey(key))
                    {
                        throw new System.IO.FileLoadException("Resource info '" + json.name + "' already exists");
                    }

                    resourceStore.Add(key, resourceInfo);
                }
                catch (JsonException e)
                {
                    throw new System.IO.FileLoadException("Resource info '" + json.name + "' is invalid\n\t" + e.Message);
                }
            }
        }

        /// <summary>
        /// Parses all prefabs on "Resources/Prefabs/Units".
        /// <exception cref="System.FileLoadException">
        /// Thrown when two prefabs define the same Race and UnitType
        /// </exception>
        /// </summary>
        private void parsePrefabs()
        {
            Object[] assets = Resources.LoadAll("Prefabs/Units", typeof(GameObject));
            foreach (Object asset in assets)
            {
                GameObject gameObject = asset as GameObject;
                Unit unit = gameObject.GetComponent<Unit>();

                if (unit != null)
                {
                    Tuple<Races, UnitTypes> key = new Tuple<Races, UnitTypes>(unit.race, unit.type);

                    if (unitPrefabs.ContainsKey(key))
                    {
                        throw new System.IO.FileLoadException("Duplicated unit prefab ('" + unit.race+ "', '" + unit.type + "')");
                    }

                    unitPrefabs.Add(key, "Prefabs/Units/" + gameObject.name);
                }
            }
        }

        /// <summary>
        /// Parses all prefabs on "Resources/Prefabs/Units".
        /// <exception cref="System.FileLoadException">
        /// Thrown when two prefabs define the same Race and UnitType
        /// </exception>
        /// </summary>
        private void parseBuildingPrefabs()
        {
            Object[] assets = Resources.LoadAll("Prefabs/Buildings", typeof(GameObject));
            foreach (Object asset in assets)
            {
                GameObject gameObject = asset as GameObject;
                Building building = gameObject.GetComponent<Building>();

                if (building != null)
                {
                    Tuple<Races, BuildingTypes> key = new Tuple<Races, BuildingTypes>(building.race, building.type);

                    if (buildingPrefabs.ContainsKey(key))
                    {
                        throw new System.IO.FileLoadException("Duplicated unit prefab ('" + building.race + "', '" + building.type + "')");
                    }

                    buildingPrefabs.Add(key, "Prefabs/Units/" + gameObject.name);
                }
            }
        }


        /// <summary>
        /// Parses all prefabs on "Resources/Prefabs/Units".
        /// <exception cref="System.FileLoadException">
        /// Thrown when two prefabs define the same Race and UnitType
        /// </exception>
        /// </summary>
        private void parseResourcePrefabs()
        {
            Object[] assets = Resources.LoadAll("Prefabs/Resources", typeof(GameObject));
            foreach (Object asset in assets)
            {
                GameObject gameObject = asset as GameObject;
                Resource resource = gameObject.GetComponent<Resource>();

                if (resource != null)
                {
                    Tuple<Races, ResourceTypes> key = new Tuple<Races, ResourceTypes>(resource.race, resource.type);

                    if (resourcePrefabs.ContainsKey(key))
                    {
                        throw new System.IO.FileLoadException("Duplicated resource prefab ('" + resource.race + "', '" + resource.type + "')");
                    }

                    resourcePrefabs.Add(key, "Prefabs/Resources/" + gameObject.name);
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

            return unitStore[key];
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

            return buildingStore[key];
        }

        /// <summary>
        /// Gathers information for a race and type.
        /// </summary>
        /// <param name="race">Race to look for</param>
        /// <param name="type">Type to look for</param>
        /// <exception cref="System.ArgumentException">Thrown when a race/type combination is not found</exception>
        /// <returns>The ResourceInfo object of that race/type combination</returns>
        public ResourceInfo of(Races race, ResourceTypes type)
        {
            Tuple<Races, ResourceTypes> key = new Tuple<Races, ResourceTypes>(race, type);

            if (!resourceStore.ContainsKey(key))
            {
                throw new System.ArgumentException("Race (" + race + ") and Type (" + type + ") does not exist");
            }

            return resourceStore[key];
        }



        /// <sumary>
        /// Given a race and unit it will return its prefab route
        /// </sumary>
        /// <param name="race">Race of the Unit</param>
        /// <param name="type">Type of the Unit</param>
        /// <exception cref="System.ArgumentException">Thrown when a race/type combination is not found</exception>
        /// <returns>The prefab path</returns>
        private string getPrefab(Races race, UnitTypes type)
        {
            Tuple<Races, UnitTypes> key = new Tuple<Races, UnitTypes>(race, type);

            if (!unitPrefabs.ContainsKey(key))
            {
                throw new System.ArgumentException("Unit prefab for ('" + race+ "', '" + type + "') not found");
            }

            return unitPrefabs[key];
        }

        /// <sumary>
        /// Given a race and type it will return its prefab route
        /// </sumary>
        /// <param name="race">Race of the Unit</param>
        /// <param name="type">Type of the Unit</param>
        /// <exception cref="System.ArgumentException">Thrown when a race/type combination is not found</exception>
        /// <returns>The prefab path</returns>
        private string getPrefab(Races race, ResourceTypes type)
        {
            Tuple<Races, ResourceTypes> key = new Tuple<Races, ResourceTypes>(race, type);

            if (!resourcePrefabs.ContainsKey(key))
            {
                throw new System.ArgumentException("Resource prefab for ('" + race + "', '" + type + "') not found");
            }

            return resourcePrefabs[key];
        }

        /// <summary>
        /// Creates a Unit of a given race and type from a prefab
        /// </summary>
        /// <param name="race">Race of the Unit</param>
        /// <param name="type">Type of the Unit</param>
        /// <returns>The created GameObject</returns>
        public GameObject createUnit(Races race, UnitTypes type)
        {
            string prefab = getPrefab(race, type);
            return UnityEngine.Object.Instantiate((GameObject)Resources.Load(prefab, typeof(GameObject)));
        }

        /// <summary>
        /// Creates a Resource of a given race and type from a prefab
        /// </summary>
        /// <param name="race">Race of the Resource</param>
        /// <param name="type">Type of the Resource</param>
        /// <returns>The created GameObject</returns>
        public GameObject createResource(Races race, ResourceTypes type)
        {
            string prefab = getPrefab(race, type);
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
        public GameObject createUnit(Races race, UnitTypes type, Vector3 position, Quaternion rotation)
        {
            string prefab = getPrefab(race, type);
            return UnityEngine.Object.Instantiate((GameObject)Resources.Load(prefab, typeof(GameObject)), position, rotation) as GameObject;
        }

        /// <summary>
        /// Creates a Resource of a given race and type from a prefab in a certain position and rotation
        /// </summary>
        /// <param name="race">Race of the Resource</param>
        /// <param name="type">Type of the Resource</param>
        /// <param name="position">Resource position</param>
        /// <param name="rotation">Resource rotation</param>
        /// <returns>The created GameObject</returns>
        public GameObject createResource(Races race, ResourceTypes type, Vector3 position, Quaternion rotation)
        {
            string prefab = getPrefab(race, type);
            return UnityEngine.Object.Instantiate((GameObject)Resources.Load(prefab, typeof(GameObject)), position, rotation) as GameObject;
        }
    }
}
