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

        /// <summary>
        /// Private constructor, singleton access only
        /// <remarks>Use Info.get instead</remarks>
        /// </summary>
        private Info()
        {
            parseUnitFiles();
            parsePrefabs();
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
    }
}
