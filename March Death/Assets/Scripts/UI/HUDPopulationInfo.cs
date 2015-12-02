using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class HUDPopulationInfo : MonoBehaviour
{
    public static bool  onShow; // to show if info is shown
    public static bool UnitsMarked; // units (true) or buildings (false) are displayed.

    private float REPAINT_TIME = 0.3f;
    private float _timer = 0.3f;

    private GUIText pop_info;
    GameObject text;
    private GameObject windowInfo;
    private GameObject container;
    private GameObject canvasUnits;
    private GameObject canvasBuildings;
    private static Canvas cUnits;
    private static Canvas cBuildings;
    private List<Text> building;
    private List<Text> unit;
    private List<Text> building_val;
    private List<Text> unit_val;

    private Font ArialFont; // Font to use with the population info panel

    private List<string> uKeys; // unit keys
    private List<string> bKeys;  // building keys

	// Use this for initialization
	void Start ()
    {
        onShow = false;
        UnitsMarked = true;

        Setup();   // initialize arrays and list containing keys and values

        initializeContainer();   // Initialize HUD container windowInfo

        InitCanvas();         // Setup for the canvas
        SetupUnitCanvas();
        SetupBuildingCanvas();

        cBuildings.enabled = false;
        cUnits.enabled = false;
    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyUp(KeyCode.U))
        {
            Toggle();
        }

    }

    /// <summary>
    /// Toggles the current info between units and building
    /// if thy are being shown.
    /// </summary>
    public static void Toggle()
    {
        if (onShow)
        {
            UnitsMarked ^= true;
            cUnits.enabled ^= true;
            cBuildings.enabled ^= true;
        }
    }

    /// <summary>
    /// Toggles the current info panel between being shown and not.
    /// </summary>
    public static void ToggleVision()
    {
        onShow ^= true;

        if (onShow)
        {
            cUnits.enabled = true;
            cBuildings.enabled = false;
            UnitsMarked = true;
        }
        else
        {
            if (UnitsMarked) cUnits.enabled = false;
            else cBuildings.enabled = false;
        }
    }

    void OnGUI()
    {
        if (Event.current.type.Equals(EventType.repaint))
        {
            if (onShow)
            {
                if (_timer > REPAINT_TIME)
                {
                    if (UnitsMarked) paintUnitsInCanvas();
                    else paintBuildingsInCanvas();

                    _timer = 0f;
                }

                _timer += Time.deltaTime;
            }
        }
    }


    private void Setup()
    {
        uKeys = PopulationInfo.get.GetUnitKeys();
        bKeys = PopulationInfo.get.GetBuildingKeys();

        unit = new List<Text>();
        building = new List<Text>();
        unit_val = new List<Text>();
        building_val = new List<Text>();
    }


    private void EnableDisableCanvas()
    {
        if (!UnitsMarked) cBuildings.enabled = onShow;
        else cUnits.enabled = onShow;
    }

    private void initializeContainer()
    {
        windowInfo = GameObject.Find("HUD/populationInfo");
        container = new GameObject("container");
        container.AddComponent<RectTransform>();
        
        container.transform.localPosition = Vector3.zero;
        container.AddComponent<Text>();
    }


    /*****************************************************
     *  Canvas Setup.
     ****************************************************/
    
    /// <summary>
    /// Initializes Canvas and sets them as parents of the main container.
    /// </summary>
    private void InitCanvas()
    {
        canvasUnits = new GameObject("canvasUnits");
        canvasUnits.layer = 5; // UI LAYER
        cUnits = canvasUnits.AddComponent<Canvas>();
        cUnits.GetComponent<RectTransform>().SetParent(windowInfo.GetComponent<RectTransform>(), false);
        cUnits.GetComponent<RectTransform>().sizeDelta = windowInfo.GetComponent<RectTransform>().sizeDelta;

        canvasBuildings = new GameObject("canvasBuildings");
        canvasBuildings.layer = 5; // UI LAYER
        cBuildings = canvasBuildings.AddComponent<Canvas>();
        cBuildings.GetComponent<RectTransform>().SetParent(windowInfo.GetComponent<RectTransform>(), false);
        cBuildings.GetComponent<RectTransform>().sizeDelta = windowInfo.GetComponent<RectTransform>().sizeDelta;

        ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
    }


    private void SetupUnitCanvas()
    {
        int maxY = (int)cUnits.GetComponent<RectTransform>().sizeDelta.y;
        int step = maxY / uKeys.Count - 4; // raul_hack

        for (int i = 0; i < uKeys.Count; i++)
        {
            GameObject newText = new GameObject(uKeys[i]);
            newText.layer = 5; // UI LAYER
            newText.AddComponent<Text>();
            newText.transform.Translate( 0, maxY/2 - i*step - 25, 0); // raul_hack
            newText.GetComponent<RectTransform>().SetParent(cUnits.GetComponent<RectTransform>(), false);
            unit.Add(newText.GetComponent<Text>());
            unit[i] = SetupText(unit[i] , uKeys[i], 8);
        }

        List<string> l = PopulationInfo.get.GetUnitValues();

        for (int i = 0; i < uKeys.Count; i++)
        {
            GameObject newText = new GameObject(uKeys[i]);
            newText.layer = 5; // UI LAYER
            newText.AddComponent<Text>();
            newText.transform.Translate(0, maxY / 2 - i * step - 45, 0); // raul_hack
            newText.GetComponent<RectTransform>().SetParent(cUnits.GetComponent<RectTransform>(), false);
            unit_val.Add(newText.GetComponent<Text>());
            unit_val[i] = SetupText(unit_val[i], l[i].ToString(), 15);
        }

    }

    /// <summary>
    /// Update values of units
    /// </summary>
    private void paintUnitsInCanvas()
    {
        List<string> l = PopulationInfo.get.GetUnitValues();

        for (int i = 0; i < uKeys.Count; i++)
        {
            unit_val[i].text = l[i].ToString();
        }
    }


    private void SetupBuildingCanvas()
    {
        int maxY = (int)cBuildings.GetComponent<RectTransform>().sizeDelta.y;
        int step = maxY / bKeys.Count - 4; // raul_hack

        for (int i = 0; i < bKeys.Count; i++)
        {
            GameObject newText = new GameObject(bKeys[i]);
            newText.layer = 5; // UI LAYER
            newText.AddComponent<Text>();
            newText.transform.Translate(0, maxY / 2 - i * step - 25, 0); // raul_hack
            newText.GetComponent<RectTransform>().SetParent(cBuildings.GetComponent<RectTransform>(), false);
            building.Add(newText.GetComponent<Text>());
            building[i] = SetupText(building[i], bKeys[i], 8);
        }

        List<string> l = PopulationInfo.get.GetBuildingValues();

        for (int i = 0; i < bKeys.Count; i++)
        {
            GameObject newText = new GameObject(bKeys[i]);
            newText.layer = 5; // UI LAYER
            newText.AddComponent<Text>();
            newText.transform.Translate(0, maxY / 2 - i * step - 40, 0); // raul_hack
            newText.GetComponent<RectTransform>().SetParent(cBuildings.GetComponent<RectTransform>(), false);
            building_val.Add(newText.GetComponent<Text>());
            building_val[i] = SetupText(building_val[i], l[i].ToString(), 15);
        }
    }

    /// <summary>
    /// Update values of buildings
    /// </summary>
    private void paintBuildingsInCanvas()
    {
        List<string> l = PopulationInfo.get.GetBuildingValues();

        for (int i = 0; i < l.Count; i++)
        {
            building_val[i].text = l[i].ToString();
        }
    }

    private Text SetupText(Text t, string text, int fontSize)
    {
        t.text = text;
        t.font = ArialFont;
        t.fontStyle = FontStyle.Normal;
        t.fontSize = fontSize;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;

        return t;
    }

}
