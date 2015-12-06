using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class HUDPopulationInfo : MonoBehaviour
{
    public static bool  onShow; // to show if info is shown

    private float REPAINT_TIME = 0.3f;
    private float _timer = 0.3f;

    private GUIText pop_info;
    GameObject text;
    private GameObject windowInfo;
    private GameObject container;
    private GameObject canvasUnits;
    private static Canvas cUnits;
    private List<Image> unit;
    private List<Text> unit_val;

    private Font ArialFont; // Font to use with the population info panel

    private List<string> uKeys; // unit keys
    private List<string> bKeys;  // building keys

	// Use this for initialization
	void Start ()
    {
        onShow = false;

        Setup();   // initialize arrays and list containing keys and values

        initializeContainer();   // Initialize HUD container windowInfo

        InitCanvas();         // Setup for the canvas
        SetupUnitCanvas();

        cUnits.enabled = false;
    }
	
	// Update is called once per frame
	void Update ()
    {
        foreach (Sprite s in Resources.LoadAll<Sprite>("PopInfoIcons"))
        {
            Debug.Log(s.name);
        }
    }

    /// <summary>
    /// Toggles the current info panel between being shown and not.
    /// </summary>
    public static void ToggleVision()
    {
        onShow ^= true;

        cUnits.enabled = onShow;

    }

    void OnGUI()
    {
        if (Event.current.type.Equals(EventType.repaint))
        {
            if (onShow)
            {
                if (_timer > REPAINT_TIME)
                {
                    if(onShow) paintUnitsInCanvas();

                    _timer = 0f;
                }

                _timer += Time.deltaTime;
            }
        }
    }


    private void Setup()
    {
        uKeys = PopulationInfo.get.GetGeneralKeys();

        unit = new List<Image>();
        unit_val = new List<Text>();
    }


    private void EnableDisableCanvas()
    {
        cUnits.enabled = onShow;
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
            newText.AddComponent<Image>();
            newText.transform.Translate( 0, maxY/2 - i*step - 25, 0); // raul_hack
            newText.GetComponent<RectTransform>().sizeDelta = new Vector2(16,16);
            newText.GetComponent<RectTransform>().SetParent(cUnits.GetComponent<RectTransform>(), false);
            unit.Add(newText.GetComponent<Image>());
            unit[i] = SetupImage(unit[i] , uKeys[i]);
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
            unit_val[i] = SetupText(unit_val[i], l[i].ToString(), 13);
        }

    }

    /// <summary>
    /// Update values of units
    /// </summary>
    private void paintUnitsInCanvas()
    {
        List<string> l = PopulationInfo.get.GetGeneralValues();

        for (int i = 0; i < uKeys.Count; i++)
        {
            unit_val[i].text = l[i].ToString();
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

    private Image SetupImage(Image img, string text)
    {
        img.color = new Color(1f,1f,1f,0f);

        Sprite sprite = Resources.Load<Sprite>("PopInfoIcons/"+text);

        if (sprite == null) {
            Debug.LogError("Null sprite!");
        }

        img.sprite = sprite;
        img.color = new Color(1f, 1f, 1f, 1f);

        return img;
    }

}
