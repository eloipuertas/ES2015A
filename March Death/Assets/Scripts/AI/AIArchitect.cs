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

        Dictionary<StructureType, List<Vector3>> avaliablePositions;
        ConstructionGrid constructionGrid;

        Vector3 basePosition;

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

            readMapData("hard_base");
        }


        /// <summary>
        /// Reads a file containing the map
        /// </summary>
        /// <param name="mapName"></param>
        public void readMapData(String mapName)
        {
            Texture2D mapData = Resources.Load(RELATIVE_PATH_TO_MAPS + mapName) as Texture2D;
            parseMapData(mapData);
        }

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

        private Vector3 getPositionForStructureType(StructureType type)
        {
            List<Vector3> positionsForType = avaliablePositions[type];
            int numPositions = positionsForType.Count;
            if(numPositions > 0)
            {
                Vector3 requestedPosition = positionsForType[0];
                positionsForType.RemoveAt(0);
                return requestedPosition;
            }
            return Vector3.zero;
        }
    }
}
