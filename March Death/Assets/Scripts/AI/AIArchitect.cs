using System;
using UnityEngine;
using Storage;
using System.Collections.Generic;

namespace Assets.Scripts.AI
{
    class AIArchitect
    {
        enum StructureType {
            STRONGHOLD,
            MILITARY_BUILDING,
            RESOURCE_BUILDING,
            TOWER,
            HORIZONTALL_WALL,
            VERTICALL_WALL,
            CORNER_WALL,
            DEFENCE_ZONE,
        }

        const String RELATIVE_PATH_TO_MAPS = "Data/AIBaseMaps/";

        Color stronghold = new Color(0.000f, 0.000f, 0.000f, 1.000f);
        Color militaryBuilding = new Color(0.000f, 0.000f, 1.000f, 1.000f);
        Color resourcesBuilding = new Color(1.000f, 1.000f, 0.000f, 1.000f);
        Color tower = new Color(1.000f, 1.000f, 1.000f, 1.000f);
        Color horizontallWall = new Color(0.502f, 0.502f, 0.502f, 1.000f);
        Color verticallWall = new Color(0.376f, 0.376f, 0.376f, 1.000f);
        Color cornerWall = new Color(0.188f, 0.188f, 0.188f, 1.000f);
        Color defenceZone = new Color(1.000f, 0.000f, 0.000f, 1.000f);
        Color emptySpace = new Color(0.000f, 1.000f, 0.000f, 1.000f);
        Color ignorePixel = new Color(1.000f, 0, 1.000f, 1.000f);
        AIController ai;
        string dificultyFolder;

        Dictionary<StructureType, List<Vector3>> avaliablePositions;
        ConstructionGrid constructionGrid;

        Vector3 basePosition;

        public List<BuildingTypes> buildingPrefs;

        public AIArchitect(AIController aiController)
        {
            avaliablePositions = new Dictionary<StructureType, List<Vector3>>();
            avaliablePositions.Add(StructureType.MILITARY_BUILDING, new List<Vector3>());
            avaliablePositions.Add(StructureType.RESOURCE_BUILDING, new List<Vector3>());
            avaliablePositions.Add(StructureType.TOWER, new List<Vector3>());
            avaliablePositions.Add(StructureType.HORIZONTALL_WALL, new List<Vector3>());
            avaliablePositions.Add(StructureType.VERTICALL_WALL, new List<Vector3>());
            avaliablePositions.Add(StructureType.CORNER_WALL, new List<Vector3>());
            avaliablePositions.Add(StructureType.DEFENCE_ZONE, new List<Vector3>());
            constructionGrid = GameObject.Find("GameController").GetComponent<ConstructionGrid>();
            ai = aiController;

            //HACK: Probably would be cool to find a way to get this dinamically
            if (ai.race == Storage.Races.ELVES)
            {
                basePosition = new Vector3(590f, 80.00262f, 792f);
            }
            else
            {
                basePosition = new Vector3(801.4f, 80.00262f, 753.6f);
            }

            constructionGrid.reservePositionForStronghold(basePosition);

            planifyBuildingsAccordingToDifficulty();

            readMapData();
        }


        /// <summary>
        /// Gets the lists of buildings that we are going to construct for every difficulty
        /// </summary>
        public void planifyBuildingsAccordingToDifficulty()
        {

            if (ai.DifficultyLvl == 1)
            {
                dificultyFolder = "Easy";
                buildingPrefs = new List<BuildingTypes>()
                {
                    BuildingTypes.FARM,
                    BuildingTypes.MINE,
                    BuildingTypes.SAWMILL,
                    BuildingTypes.ARCHERY,
                    BuildingTypes.BARRACK,
                    BuildingTypes.STABLE,
                };
            }

            if (ai.DifficultyLvl == 2)
            {
                dificultyFolder = "Medium";
                buildingPrefs  = new List<BuildingTypes>()
                {
                    BuildingTypes.FARM,
                    BuildingTypes.MINE,
                    BuildingTypes.SAWMILL,
                    BuildingTypes.ARCHERY,
                    BuildingTypes.BARRACK,
                    BuildingTypes.WATCHTOWER,
                    BuildingTypes.STABLE,
                    BuildingTypes.WATCHTOWER,
                    BuildingTypes.WATCHTOWER,
                    BuildingTypes.WATCHTOWER,
                    BuildingTypes.WATCHTOWER,
                    BuildingTypes.WATCHTOWER,
                    BuildingTypes.WATCHTOWER,
                    BuildingTypes.WATCHTOWER,
                };
            }

            if (ai.DifficultyLvl == 3)
            {
                dificultyFolder = "Hard";
                buildingPrefs = new List<BuildingTypes>()
                {
                    BuildingTypes.FARM,
                    BuildingTypes.MINE,
                    BuildingTypes.SAWMILL,
                    BuildingTypes.ARCHERY,
                    BuildingTypes.BARRACK,
                    BuildingTypes.WATCHTOWER,
                    BuildingTypes.STABLE,
                    BuildingTypes.WATCHTOWER,
                    BuildingTypes.WATCHTOWER,
                    BuildingTypes.WATCHTOWER,
                    BuildingTypes.WATCHTOWER,
                    BuildingTypes.WALL,
                    BuildingTypes.WALL,
                    BuildingTypes.WALL,
                };
            }


        }

        /// <summary>
        /// Reads a file containing the map (we can read any map inside the folders an chooses one 
        /// randomly)
        /// </summary>
        /// <param name="mapName"></param>
        public void readMapData()
        {
            Debug.Log(RELATIVE_PATH_TO_MAPS + dificultyFolder);
            Texture2D[] maps = Resources.LoadAll<Texture2D>(RELATIVE_PATH_TO_MAPS + dificultyFolder);
            Texture2D mapData = maps[UnityEngine.Random.Range(0, maps.Length)];
            parseMapData(mapData);
        }

        /// <summary>
        /// Reads a map and syncronizes it with the construction grid
        /// </summary>
        /// <param name="mapData"></param>
        public void parseMapData(Texture2D mapData)
        {
            Color[] pixels = mapData.GetPixels();

            Vector2 center = new Vector2(mapData.width / 2 - 1, mapData.height / 2 - 1);

            // Math Facts: 
            // The equation to find te position of something is
            // Offset = (i , j) - Center
            // centerPos + GridSize * Offset 

            Vector3 processingPos = Vector3.zero;
            Vector2 processingOffset = Vector2.zero;

            processingPos.y = basePosition.y;

            for(int i = 0; i < mapData.height; i++)
            {
                for(int j = 0; j < mapData.width; j++)
                {
                    Color pixel = pixels[j + i * mapData.width];
                    processingOffset = new Vector2(i, j) - center;
                    processingPos.x = basePosition.x + constructionGrid.getDimensions().x * processingOffset.x;
                    processingPos.z = basePosition.z + constructionGrid.getDimensions().y * processingOffset.y;
                    processingPos = constructionGrid.discretizeMapCoords(processingPos);

                    //In order to be more flexible we need to check if we can construct
                    if (!constructionGrid.isNewPositionAbleForConstrucction(processingPos))
                    {
                        continue;
                    }

                    if (CompareColors(pixel, stronghold))
                    {
                        Debug.Log("Stronghold");
                    }
                    else if (CompareColors(pixel, militaryBuilding))
                    {
                        avaliablePositions[StructureType.MILITARY_BUILDING].Add(processingPos);
                    }
                    else if (CompareColors(pixel, resourcesBuilding))
                    {
                        avaliablePositions[StructureType.RESOURCE_BUILDING].Add(processingPos);
                    }
                    else if (CompareColors(pixel, tower))
                    {
                        avaliablePositions[StructureType.TOWER].Add(processingPos);
                    }
                    else if (CompareColors(pixel, horizontallWall))
                    {
                        avaliablePositions[StructureType.HORIZONTALL_WALL].Add(processingPos);
                    }
                    else if (CompareColors(pixel, verticallWall))
                    {
                        avaliablePositions[StructureType.VERTICALL_WALL].Add(processingPos);
                    }
                    else if (CompareColors(pixel, cornerWall))
                    {
                        avaliablePositions[StructureType.CORNER_WALL].Add(processingPos);
                    }
                    else if (CompareColors(pixel, defenceZone))
                    {
                        avaliablePositions[StructureType.DEFENCE_ZONE].Add(processingPos);
                    }
                    else if (CompareColors(pixel, emptySpace))
                    {
                        continue;
                    }
                    else if (CompareColors(pixel, ignorePixel))
                    {
                        continue;
                    }
                    else
                    {
                        Debug.Log("Unknown Structure" + pixel.ToString());
                    }
                }
            }
            Debug.Log("Reading Complete");
        }

        /// <summary>
        /// I need to compare this way because unity optimizes comparison and sometimes is not working,
        ///  and i don't want to work with alpha channel
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public static  bool CompareColors(Color c1, Color c2)
        {
            if((c1.r == c2.r && c1.g == c2.g &&  c1.b == c2.b) || c1.ToString().Equals(c2.ToString()))
            {
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Translates type to structuretype
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Vector3 getPositionForBuildingType(BuildingTypes type)
        {

            StructureType buildingType = StructureType.RESOURCE_BUILDING;

            switch (type)
            {
                case BuildingTypes.FARM:
                    buildingType = StructureType.RESOURCE_BUILDING;
                    break;
                case BuildingTypes.MINE:
                    buildingType = StructureType.RESOURCE_BUILDING;
                    break;
                case BuildingTypes.SAWMILL:
                    buildingType = StructureType.RESOURCE_BUILDING;
                    break;
                case BuildingTypes.ARCHERY:
                    buildingType = StructureType.MILITARY_BUILDING;
                    break;
                case BuildingTypes.BARRACK:
                    buildingType = StructureType.MILITARY_BUILDING;
                    break;
                case BuildingTypes.STABLE:
                    buildingType = StructureType.MILITARY_BUILDING;
                    break;
                case BuildingTypes.WALL:
                    //TODO: Need to think about it
                    break;
                case BuildingTypes.WALLCORNER:
                    //TODO: Need to think about it
                    break;
                case BuildingTypes.WATCHTOWER:
                    buildingType = StructureType.TOWER;
                    break;
                default:
                    //In case that something new has entered return 0
                    return Vector3.zero;
            }
            return getPositionForStructureType(buildingType);
        }

        /// <summary>
        /// Gets the position of an structure on the AI map
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private Vector3 getPositionForStructureType(StructureType type)
        {
            List<Vector3> positionsForType = avaliablePositions[type];
            bool found = false;
            Vector3 requestedPosition = Vector3.zero;

            while (positionsForType.Count > 0 && !found)
            {
                // Recheck positions in order to know if player has constructed on this position
                if (constructionGrid.isNewPositionAbleForConstrucction(positionsForType[0]))
                {
                    requestedPosition = positionsForType[0];
                    found = true;
                }
                positionsForType.RemoveAt(0);
            }

            return requestedPosition;
        }

        /// <summary>
        /// Used to know what building construct on the next iteration
        /// </summary>
        public void constructNextBuilding()
        {
            Vector3 position = getPositionForBuildingType(buildingPrefs[0]);

            if (!ai.isAffordable(ai.race, buildingPrefs[0]))
            {
                return;
            }

            //Enque it if we cannot construct it in this position
            if (position.Equals(Vector3.zero))
            {
                buildingPrefs.Add(buildingPrefs[0]);
                buildingPrefs.RemoveAt(0);
            }
            else
            {
                ai.CreateBuilding(buildingPrefs[0], position, Quaternion.Euler(0, 0, 0));
                buildingPrefs.RemoveAt(0);
            }
        }
    }
}
