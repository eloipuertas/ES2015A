using UnityEngine;
using System.Collections;

public class MiniMapController : MonoBehaviour
{ 
    private Camera _camera;
    private Camera mainCam;

    private RenderTexture rt;
    
    // Minimap Rectangle
    Rect rect_marker;
    Texture2D tex;

    private float main_zoom;
    private Vector3 act_pos;
    private float aspect;

    Vector3 cameraOffset;
	Transform mapContainer;

    // Use this for initialization
    void Start()
    {
        this.GetComponent<AudioListener>().enabled = false;

		mapContainer = GameObject.Find ("HUD").transform.FindChild ("Map");

        _camera = this.GetComponent<Camera>();
        _camera.orthographic = true;
        
        //Assign camera viewport
        _camera.rect = this.recalcViewport();

        mainCam = GameObject.Find("Main Camera").GetComponent<Camera>();

        initializeCameraPars();

        // moves camera to show the whole map
        if (Terrain.activeTerrain)
        {
            float diagonal = Mathf.Sqrt(Mathf.Pow(Terrain.activeTerrain.terrainData.size.x, 2) + Mathf.Pow(Terrain.activeTerrain.terrainData.size.y, 2));
            _camera.transform.position = new Vector3(Terrain.activeTerrain.terrainData.size.x * 0.5f, Terrain.activeTerrain.terrainData.size.x * 0.6f,Terrain.activeTerrain.terrainData.size.z * 0.5f);
            _camera.transform.rotation = Quaternion.Euler(90f, 135f,0); 
            _camera.orthographicSize = diagonal * 0.95f; // a hack
            _camera.farClipPlane = Terrain.activeTerrain.terrainData.size.x * 1.5f;
            _camera.clearFlags = CameraClearFlags.Depth;
            instantiateMask();
        }

        createMarker();

        rt = new RenderTexture(Screen.width, Screen.height, 2);
        _camera.targetTexture = rt;
    }

    /// <summary>
    /// initialize the params of the camera.
    /// </summary>
    private void initializeCameraPars()
    {
        main_zoom = mainCam.orthographicSize;
        act_pos = mainCam.transform.position;
        aspect = mainCam.aspect;
    }

    /// <summary>
    /// Here we draw the texture of the Marker.
    /// </summary>
    void OnGUI()
    {
        GUI.depth = 2;
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), rt);
        GUI.DrawTexture(rect_marker, tex);
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
    /// Instantiates the mask which makes invisible part of the minimap viewport.
    /// </summary>
    private void instantiateMask()
    {
        GameObject mask = (GameObject)Resources.Load("minimap_plane");
        mask.transform.position = new Vector3(_camera.transform.position.x+0,
                                              _camera.transform.position.y-50, 
                                              _camera.transform.position.z+0);
        mask.transform.localScale = new Vector3(350,1,350);
        mask.GetComponent<MeshRenderer>().sharedMaterial.shader = Shader.Find("Masked/Mask");
        Instantiate(mask);
    }

    /// <summary>
    /// Updates the position and the size of the marker.
    /// </summary>
    private void updateMarker()
    {
        if (main_zoom != mainCam.orthographicSize) // if the zoom has changed
        {
            float diff = (mainCam.orthographicSize - main_zoom) / 6.0f; // to reduce the zoom vel increment value (hack)
            rect_marker.width += diff; rect_marker.height += diff;
            rect_marker.center -= new Vector2(diff/2, diff/2);
            main_zoom = mainCam.orthographicSize;
        }
        if (!act_pos.Equals(mainCam.transform.position)) // if the camera has moved
        {
            Vector3 v = _camera.WorldToScreenPoint(mainCam.transform.position - mainCam.GetComponent<CameraController>().getCameraOffset); v.y = Screen.height - v.y;
            rect_marker.center = v;
            act_pos = mainCam.transform.position;
        }
        if (mainCam.aspect != aspect) {
            recalcViewport();
            rt = new RenderTexture(Screen.width, Screen.height, 2);
            _camera.targetTexture = rt;
            aspect = mainCam.aspect;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))  // GetMouseButton(0) to remove dragging.
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
            mainCam.ScreenToWorldPoint(new Vector3(0,0,mainCam.farClipPlane)),
            mainCam.ScreenToWorldPoint(new Vector3(Screen.width,Screen.height,mainCam.farClipPlane))
        };

        Vector3[] corners_minimap = new Vector3[2]
        {
            _camera.WorldToScreenPoint(corners[0]),
            _camera.WorldToScreenPoint(corners[1])
        };

        Rect r = new Rect();

        r.xMax = corners_minimap[1].x + 5; // 10 10 12 12
        r.xMin = corners_minimap[0].x - 5;
        r.yMax = Screen.height - corners_minimap[1].y + 7;
        r.yMin = Screen.height - corners_minimap[0].y - 7;

        Vector3 v = _camera.WorldToScreenPoint(mainCam.transform.position - mainCam.GetComponent<CameraController>().getCameraOffset);
        v.y = Screen.height - v.y;
        r.center = v;
  
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
		GameInformation info = (GameInformation)GameObject.Find("GameInformationObject").GetComponent("GameInformation");
		float viewPortPosX, viewPortPosY;
		float viewPortW, viewPortH;
		
		switch (info.GetPlayerRace()) {
		case Storage.Races.MEN:
			viewPortPosX = 0.018f;
			viewPortPosY = 0.02f; // _prev = 0.007f
			
			// The minimap size
			viewPortW = (1f / (float)Screen.width) * ((float)Screen.width / 4.9701f);  // _prev = 3.9701f
			// the height will be the ratio of the hole for the map 140/201
			viewPortH = (1f / (float)Screen.height) * (((float)Screen.width / 3.6701f) * (140f / 201f));
			break;
		case Storage.Races.ELVES:
			viewPortPosX = 0.018f;
			viewPortPosY = 0.02f;
			
			// The minimap size
			viewPortW = (1f / (float)Screen.width) * ((float)Screen.width / 5.6701f);
			// the height will be the ratio of the hole for the map 140/201
			viewPortH = (1f / (float)Screen.height) * (((float)Screen.width / 4.0701f) * (140f / 201f));
			break;
		default:
			viewPortPosX = 0; viewPortPosY = 0;
			viewPortW = 0; viewPortH = 0;
			break;
		}
		//Assign camera viewport
		return new Rect(viewPortPosX, viewPortPosY, viewPortW, viewPortH);
		
	}
}
