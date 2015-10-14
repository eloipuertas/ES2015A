using UnityEngine;
using System.Collections;

public class MiniMapController : MonoBehaviour
{

    private Camera _camera;
    private Camera mainCam;
    
    // Minimap Rectangle
    GameObject quad;
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

        initializeMinimapRect();

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

    private void initializeMinimapRect()
    {
        main_zoom = mainCam.orthographicSize;
        act_pos = mainCam.transform.position;
        cameraOffset = new Vector3(-252.8f, 0f, -252.8f);
        createRectangle();
    }

    // Update is called once per frame
    /// <summary>
    /// 
    /// </summary>
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
                updateRect();
            }
        }
        updateRect();

    }

    /// <summary>
    /// Creates and sets up the rectangle to show the view on the minimap.
    /// </summary>
    private void createRectangle()
    {
        Vector3 v = mainCam.transform.position;

        quad = new GameObject();
        quad.AddComponent<MeshFilter>();
        quad.AddComponent<MeshRenderer>();
        MeshRenderer mr = quad.GetComponent<MeshRenderer>();
        Material m = new Material(Shader.Find("Transparent/Diffuse"));
        m.color = new Color(1f, 0f, 0f, 0.35f);
        mr.material = m;
        quad.transform.position = v - cameraOffset;
        quad.AddComponent<MeshRect>();
        quad.layer = 8;
    }

    /// <summary>
    /// Updates the parameters of the Rectangle of the minimap.
    /// </summary>
    private void updateRect()
    {
        if (main_zoom != mainCam.orthographicSize)
        {
            float diff = (mainCam.orthographicSize - main_zoom)/ 80;
            quad.transform.localScale += new Vector3(diff, diff, diff);
            main_zoom = mainCam.orthographicSize;
        }
        if (!act_pos.Equals(mainCam.transform.position))
        {
            Vector3 vDiff = mainCam.transform.position - act_pos;
            quad.transform.Translate(vDiff);
            act_pos = mainCam.transform.position;
        }
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
