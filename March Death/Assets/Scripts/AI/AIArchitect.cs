using System;
using UnityEngine;

namespace Assets.Scripts.AI
{
    class AIArchitect
    {

        Color stronghold = new Color(0.000f, 0.000f, 0.059f, 1.000f);
        Color militaryBuilding = new Color(0, 0, 0, 1.000f);
        Color resourcesBuilding = new Color(0, 0, 0, 1.000f);

        Color tower = new Color(0, 0, 0, 1.000f);
        Color horizontallWall = new Color(0, 0, 0, 1.000f);
        Color verticallWall = new Color(0, 0, 0, 1.000f);

        Color cornerWall = new Color(0, 0, 0, 1.000f);
        Color defenceZone = new Color(0, 0, 0, 1.000f);
        Color emptySpace = new Color(0, 0, 0, 1.000f);

        String currentBaseMap;

        AIController ai;

        public AIArchitect(AIController aiController)
        {
            readMap("pixels");
            ai = aiController;

        }

        /// <summary>
        /// Reads a file containing the map
        /// </summary>
        /// <param name="mapName"></param>
        public void readMap(String mapName)
        {

            Texture2D mapTexture = Resources.Load<Texture2D>("Data/AIBaseMaps/" + mapName);
            Texture2D tex = new Texture2D(9, 1, TextureFormat.BGRA32, false);
            byte[] fileData;

            if (mapTexture == null)
            {
                Debug.Log("AIArchitect: " + mapName + " could not be found");
                return;
            }

            Color[] pixels = mapTexture.GetPixels();

            for (int i = 0; i < mapTexture.height; i++)
            {
                for(int j = 0; j < mapTexture.width; j++)
                {
                    Console.WriteLine(mapTexture.GetPixel(i, j));
                }
            }

            Debug.Log("Finished Parsing Map");

        }
    }
}
