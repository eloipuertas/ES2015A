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
        AIController ai;

        Dictionary<StructureType, List<Vector3>> avaliablePositions;

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

            readMapData("map_palete");
            ai = aiController;
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

            for(int i = 0; i < mapData.height; i++)
            {
                for(int j = 0; j < mapData.width; j++)
                {
                    Color pixel = pixels[j + i * mapData.width];
                    if (CompareColors(pixel, stronghold))
                    {
                        Debug.Log("Stronghold");
                    }
                    else if (CompareColors(pixel, militaryBuilding))
                    {
                        Debug.Log("militaryBuilding");
                    }
                    else if (CompareColors(pixel, resourcesBuilding))
                    {
                        Debug.Log("resourcesBuilding");
                    }
                    else if (CompareColors(pixel, tower))
                    {
                        Debug.Log("tower");
                    }
                    else if (CompareColors(pixel, horizontallWall))
                    {
                        Debug.Log("horizontallWall");
                    }
                    else if (CompareColors(pixel, verticallWall))
                    {
                        Debug.Log("verticallWall");
                    }
                    else if (CompareColors(pixel, cornerWall))
                    {
                        Debug.Log("cornerWall");
                    }
                    else if (CompareColors(pixel, defenceZone))
                    {
                        Debug.Log("defenceZone");
                    }
                    else if (CompareColors(pixel, emptySpace))
                    {
                        Debug.Log("emptySpace");
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
            if(positionsForType.Count > 0)
            {
                Vector3 requestedPosition = positionsForType[0];
                positionsForType.RemoveAt(0);
                return requestedPosition;
            }
            return Vector3.zero;
        }
    }
}
