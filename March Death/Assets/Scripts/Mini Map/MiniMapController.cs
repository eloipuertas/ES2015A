using UnityEngine;
using System.Collections;

public class MiniMapController : MonoBehaviour
{

    private Camera _camera;
    // Use this for initialization
    void Start()
    {

        this.GetComponent<AudioListener>().enabled = false;

        _camera = this.GetComponent<Camera>();
        _camera.orthographic = true;
        //Assign camera viewport
        _camera.rect = this.recalcViewport();

        // moves camera to show the whole map
        if (Terrain.activeTerrain)
        {
            _camera.transform.position = new Vector3(Terrain.activeTerrain.terrainData.size.x * 0.5f, Terrain.activeTerrain.terrainData.size.x, Terrain.activeTerrain.terrainData.size.z * 0.5f);
            _camera.orthographicSize = Terrain.activeTerrain.terrainData.size.x * 0.5f;
        }

    }

    // Update is called once per frame
    void Update()
    {

    }

    private Rect recalcViewport()
    {


        float viewPortPosX = 0.007f;
        float viewPortPosY = 0.007f;


        // The minimap size
        float viewPortW = (1f / (float)Screen.width) * ((float)Screen.width / 3.9701f);
        // the height will be the ratio of the hole for the map 140/201
        float viewPortH = (1f / (float)Screen.height) * (((float)Screen.width / 3.9701f) * (140f / 201f));

        //Assign camera viewport
        return new Rect(viewPortPosX, viewPortPosY, viewPortW, viewPortH);

    }
}
