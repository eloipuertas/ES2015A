using System;
using UnityEngine;
using Storage;
using System.Collections.Generic;

namespace Assets.Scripts.AI
{
    public class AIArchitect
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

        public static bool TESTING = false;

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
        public float buildingAngle = 0f;

        Dictionary<StructureType, List<Vector3>> avaliablePositions;
        ConstructionGrid constructionGrid;

        Vector3 basePosition;

        public List<BuildingTypes> buildingPrefs;

        int buildingsPlaced;

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
            buildingsPlaced = 0;
            ai = aiController;

            //HACK: Probably would be cool to find a way to get this dinamically
            if (ai.race == Storage.Races.ELVES)
            {
                basePosition = new Vector3(283.7f, 80.00262f, 562.5f);
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
                    BuildingTypes.FARM,
                    BuildingTypes.MINE,
                    BuildingTypes.SAWMILL,
                    BuildingTypes.BARRACK,
                };
            }

            else if (ai.DifficultyLvl == 2)
            {
                dificultyFolder = "Medium";
                buildingPrefs  = new List<BuildingTypes>()
                {
                    BuildingTypes.FARM,
                    BuildingTypes.MINE,
                    BuildingTypes.SAWMILL,
                    BuildingTypes.ARCHERY,
                    BuildingTypes.FARM,
                    BuildingTypes.MINE,
                    BuildingTypes.SAWMILL,
                    BuildingTypes.BARRACK,
                    BuildingTypes.WATCHTOWER,
                    BuildingTypes.STABLE,

                };

                for(int i = 0; i < 20; i++)
                {
                    buildingPrefs.Add(BuildingTypes.WATCHTOWER);
                }
            }

            else
            {
                dificultyFolder = "Hard";
                buildingPrefs = new List<BuildingTypes>()
                {
                    BuildingTypes.FARM,
                    BuildingTypes.FARM,
                    BuildingTypes.MINE,
                    BuildingTypes.SAWMILL,
                    BuildingTypes.FARM,
                    BuildingTypes.ARCHERY,
                    BuildingTypes.BARRACK,
                    BuildingTypes.WATCHTOWER,
                    BuildingTypes.STABLE,
                };
                int elvesDiscounter = ai.race == Races.ELVES ? 5 : 0;
               
                for (int i = 0; i < 30 - elvesDiscounter; i++)
                {
                    buildingPrefs.Add(BuildingTypes.WATCHTOWER);
                }

                if(ai.race == Races.ELVES)
                {
                    for(int i = 0; i < 40; i++)
                    {
                        buildingPrefs.Add(BuildingTypes.WALL);
                    }

                    for(int i = 0; i < 6; i++)
                    {
                        buildingPrefs.Add(BuildingTypes.WALLCORNER);
                    }
                }
            }

            // Need this in case we want to be creative
            if (TESTING)
            {
                buildingPrefs = new List<BuildingTypes>();
                for (int i = 0; i < 25; i++)
                {
                    buildingPrefs.Add(BuildingTypes.WALL);
                }
                
                for(int i = 0; i < 10; i++)
                {
                    buildingPrefs.Add(BuildingTypes.WALLCORNER);
                }

                dificultyFolder = "Testing";
                return;
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


            Vector2 center = ai.race == Races.ELVES ? new Vector2(mapData.width / 2 + 0.5f, mapData.height / 2 - 0.5f) : new Vector2(mapData.width / 2 -0.5f, mapData.height / 2 - 1f);

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

            buildingAngle = 0;

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
                    if (avaliablePositions[StructureType.HORIZONTALL_WALL].Count > 0)
                    {
                        buildingType = StructureType.HORIZONTALL_WALL;
                        if (ai.race == Races.MEN)
                            buildingAngle = 90f;
                    }
                    else
                    {
                        buildingType = StructureType.VERTICALL_WALL;
                        if (ai.race == Races.ELVES)
                            buildingAngle = 90f;
                    }
                    break;
                case BuildingTypes.WALLCORNER:
                    buildingType = StructureType.CORNER_WALL;
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
                int pos = UnityEngine.Random.Range(0, positionsForType.Count);
                // Recheck positions in order to know if player has constructed on this position
                if (constructionGrid.isNewPositionAbleForConstrucction(positionsForType[pos]))
                {
                    requestedPosition = positionsForType[pos];
                    found = true;
                }
                positionsForType.RemoveAt(pos);
            }

            return requestedPosition;
        }

        /// <summary>
        /// Used to know what building construct on the next iteration
        /// </summary>
        public void constructNextBuilding()
        {

            if(buildingsPlaced == 0)
            {
                fixPositions();
            }

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
                if(buildingPrefs[0] == BuildingTypes.WALLCORNER)
                {
                    //If the corner hasn't 2 walls or has 3 or more i need to skip it
                    if (!getCornerRotation(position))
                    {
                        buildingPrefs.RemoveAt(0);
                        return;
                    }
                }
                ai.CreateBuilding(buildingPrefs[0], position, Quaternion.Euler(0, buildingAngle, 0), this);
                buildingPrefs.RemoveAt(0);
                buildingsPlaced++;
            }
        }

        bool getCornerRotation(Vector3 pos)
        {
            List<IBuilding> buildings = ai.race == Races.ELVES ? AISenses.getBuildingsOfRaceNearPosition(pos, 20, ai.race) : AISenses.getBuildingsOfRaceNearPosition(pos, 22, ai.race);
            Debug.Log(buildings.Count);
            List <GameObject> walls  = new List<GameObject>();
            foreach(IBuilding wall in buildings)
            {
                if(wall.getType<BuildingTypes>() == BuildingTypes.WALL && wall.getRace() == ai.race)
                {
                    walls.Add(wall.getGameObject());
                }
            }

            bool choosenAngle = false;

            if(walls.Count == 2)
            {
                for(int i = 0; i < walls.Count; i++)
                {
                    GameObject wall = walls[i];
                   
                    if (choosenAngle) return choosenAngle;
                    float angle = Mathf.Round(wall.transform.rotation.eulerAngles.y);
                    
                    if(angle == 90f)
                    {
                        if(ai.race == Races.ELVES)
                        {
                            if (wall.transform.position.x > pos.x)
                            {
                                if (wall.transform.position.z.Equals(pos.z))
                                {
                                    GameObject horWall = i == 0 ? walls[1] : walls[0];
                                    if (horWall.transform.position.z > pos.z)
                                    {
                                        buildingAngle = ai.race == Races.ELVES ? 0f : 0f; 
                                        choosenAngle = true;
                                    }
                                    else
                                    {
                                        buildingAngle = ai.race == Races.ELVES ? 90f : 90f;
                                        choosenAngle = true;
                                    }

                                }
                            }

                            else
                            {
                                if (wall.transform.position.z.Equals(pos.z))
                                {
                                    GameObject horWall = i == 0 ? walls[1] : walls[0];
                                    if (horWall.transform.position.z < pos.z)
                                    {
                                        buildingAngle = ai.race == Races.ELVES ? 180f : 180f; 
                                        choosenAngle = true;
                                    }
                                    else
                                    {
                                        buildingAngle = ai.race == Races.ELVES ? 270f : 270f;
                                        choosenAngle = true;
                                    }

                                }
                            }
                        }
                        else
                        {
                            if (wall.transform.position.z > pos.z)
                            {
                                if (wall.transform.position.x.Equals(pos.x))
                                {
                                    GameObject horWall = i == 0 ? walls[1] : walls[0];
                                    if (horWall.transform.position.x > pos.x)
                                    {
                                        buildingAngle =  0f; 
                                        choosenAngle = true;
                                    }
                                    else
                                    {
                                        buildingAngle =  90f; 
                                        choosenAngle = true;
                                    }

                                }
                            }

                            else
                            {
                                if (wall.transform.position.x.Equals(pos.x))
                                {
                                    GameObject horWall = i == 0 ? walls[1] : walls[0];
                                    if (horWall.transform.position.z < pos.z)
                                    {
                                        buildingAngle = 180f; 
                                        choosenAngle = true;
                                    }
                                    else
                                    {
                                        buildingAngle = 270f; 
                                        choosenAngle = true;
                                    }

                                }
                            }
                        }
                    }
                       
                }
            }

            return choosenAngle;
        }
        void fixPositions()
        {
            GameObject stronghold;
            string sName;
            sName = ai.race == Races.ELVES ? "Elf-Stronghold(Clone)" : "Human_Stronghold(Clone)";

            stronghold = GameObject.Find(sName);
            stronghold.transform.position = constructionGrid.discretizeMapCoords(stronghold.transform.position);
            
        }

        /// <summary>
        /// This this method in order to maintain the base, when someone destroys our buildings
        /// </summary>
        /// <param name="obj"></param>
        public void onDestroy(System.Object obj)
        {
            GameObject gob = (GameObject)obj;
            Vector3 position = gob.transform.position;
            float yRot = gob.transform.rotation.eulerAngles.y;
            BuildingTypes type = gob.GetComponent<IGameEntity>().getType<BuildingTypes>();
            //Add back the building to the queue in order to reconstruct it as soon as it is possible
            buildingPrefs.Add(type);
            constructionGrid.liberatePosition(constructionGrid.discretizeMapCoords(position));

            //We need to inform the architect that new positions are avaliable
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
                    if(Mathf.Round(yRot) == 90f)
                    {
                        buildingType = ai.race == Races.MEN ? StructureType.HORIZONTALL_WALL : StructureType.VERTICALL_WALL; 
                    }
                    else
                    {
                        buildingType = ai.race == Races.MEN ? StructureType.VERTICALL_WALL : StructureType.HORIZONTALL_WALL;
                    }
                    break;
                case BuildingTypes.WALLCORNER:
                    buildingType = StructureType.CORNER_WALL;
                    break;
                case BuildingTypes.WATCHTOWER:
                    buildingType = StructureType.TOWER;
                    break;
                default:
                    Debug.Log("AIArchitect: Unknown type maibe there are new buildings?");
                    break;
            }

            avaliablePositions[buildingType].Add(position);

        }
    }
}
