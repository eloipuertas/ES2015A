﻿using UnityEngine;
using System.Collections;

public class MiniMapController : MonoBehaviour
{

    private Camera _camera;
    private Camera mainCam;
    
    // Minimap Rectangle
    GameObject quad;
    Rect rect_marker;
    Texture2D tex;
    private float main_zoom;
    private Vector3 act_pos;
    Vector3 cameraOffset;

    // Use this for initialization
    void Start()
    {
        this.GetComponent<AudioListener>().enabled = false;

        _camera = this.GetComponent<Camera>();
        _camera.orthographic = true;
        //Assign camera viewport
        _camera.rect = this.recalcViewport();

        mainCam = GameObject.Find("Main Camera").GetComponent<Camera>();

        initializeCameraPars();

        createMarker();

        // moves camera to show the whole map
        if (Terrain.activeTerrain)
        {
            float diagonal = Mathf.Sqrt(Mathf.Pow(Terrain.activeTerrain.terrainData.size.x, 2) + Mathf.Pow(Terrain.activeTerrain.terrainData.size.y, 2));
            _camera.transform.position = new Vector3(Terrain.activeTerrain.terrainData.size.x * 0.5f, Terrain.activeTerrain.terrainData.size.x, Terrain.activeTerrain.terrainData.size.z * 0.5f);
            _camera.transform.rotation = Quaternion.Euler(90f, 45f,0); 
            _camera.orthographicSize = diagonal * 0.75f; // a hack
            _camera.farClipPlane = Terrain.activeTerrain.terrainData.size.x * 1.5f;

        }

    }

    /// <summary>
    /// Here we draw the texture of the Marker.
    /// </summary>
    void OnGUI()
    {
        GUI.DrawTexture(rect_marker, tex);
    }

    private void initializeCameraPars()
    {
        main_zoom = mainCam.orthographicSize;
        act_pos = mainCam.transform.position;
        cameraOffset = new Vector3(252.8f, 0f, 252.8f);
    }

    /// <summary>
    /// This method initialize and sets up the Maerker.
    /// </summary>
    private void createMarker()
    {
        rect_marker = getCameraRect();
        tex = MinimapOverlays.CreateTextureMarker(Color.red);
    }

    /// <summary>
    /// Updates the position and the size of the marker.
    /// </summary>
    private void updateMarker()
    {
        if (main_zoom != mainCam.orthographicSize) // if the zoom has changed
        {
            float diff = (mainCam.orthographicSize - main_zoom) / 1.5f;
            rect_marker.width += diff; rect_marker.height += diff / 2.5f;
            rect_marker.center -= new Vector2(diff / 2, diff / (2 * 3));
            main_zoom = mainCam.orthographicSize;
        }
        if (!act_pos.Equals(mainCam.transform.position)) // if the camera has moved
        {
            Vector3 v = _camera.WorldToScreenPoint(mainCam.transform.position + cameraOffset); v.y = Screen.height - v.y;
            rect_marker.center = v;
            act_pos = mainCam.transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // GetMouseButton(0) if we want to move the camera by clicking & dragging around the minimap
        if (Input.GetMouseButtonDown(0)) 
        {
            RaycastHit hit;

            Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition);

            if (isInside(pos, _camera.rect)) {
                Vector3 ground = new Vector3();
                if (Physics.Raycast(_camera.ScreenToWorldPoint(Input.mousePosition), -Vector3.up, out hit)) 
                    ground = hit.point;          
                if(!ground.Equals(new Vector3()))
                    mainCam.GetComponent<CameraController>().lookAtPoint(ground);
                updateMarker();
            }
        }
        updateMarker();

    }

    /// <summary>
    /// Returns the Screen rectangle on the minimap that represents what we see in the main camera.
    /// </summary>
    /// <returns></returns>
    private Rect getCameraRect()
    {
        Vector3[] corners = new Vector3[2]
        {
            mainCam.ScreenToWorldPoint(new Vector3(0,0,mainCam.farClipPlane+70)),
            mainCam.ScreenToWorldPoint(new Vector3(Screen.width,Screen.height,mainCam.farClipPlane+70))
        };

        Debug.Log(mainCam.farClipPlane);
        //corners[0] += cameraOffset; corners[1] += cameraOffset; //  * 2.65f

        Vector3[] corners_minimap = new Vector3[2]
        {
            _camera.WorldToScreenPoint(corners[0]),
            _camera.WorldToScreenPoint(corners[1])
        };

        Rect r = new Rect();
        r.xMax = corners_minimap[1].x + 15;
        r.xMin = corners_minimap[0].x - 15;
        r.yMax = Screen.height - corners_minimap[1].y + 15;
        r.yMin = Screen.height - corners_minimap[0].y - 15;
        return r;
    }




    /// <summary>
    /// Checks if a coordinate is inside a certain rectangle
    /// </summary>
    /// <param name="coord"> Vector3 that represent the coordinate.</param>
    /// <param name="r">Rectangle that represents the box.</param>
    /// <returns></returns>
    private bool isInside(Vector3 coord, Rect r)
    {
        if (coord.x >= r.xMin && coord.x <= r.xMax)
        {
            return (coord.y >= r.yMin && coord.y <= r.yMax);
        }
        else { return false; }
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
