using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class HUDPopulationInfo : MonoBehaviour
{
    public bool onShow; // to show if info is shown
    public bool UnitsMarked; // units (true) or buildings (false) are displayed.

    private float REPAINT_TIME = 0.3f;
    private float _timer = 0.3f;

    private GUIText pop_info;
    GameObject text;

    private List<string> uKeys; // unit keys
    private List<string> bKeys;  // building keys

	// Use this for initialization
	void Start ()
    {
        onShow = false;
        UnitsMarked = true;

        Setup();
        CreateText();
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyUp(KeyCode.U))
        {
            Toggle();
        }
        if (Input.GetKeyUp(KeyCode.C))
        {
            ToggleVision();
            EnableDisableText();
        }

    }

    /// <summary>
    /// Toggles the current info between units and building
    /// if thy are being shown.
    /// </summary>
    public void Toggle()
    {
        if (onShow)
        {
            UnitsMarked ^= true;
        }
    }

    public void ToggleVision()
    {
        onShow ^= true;
    }

    void OnGUI()
    {
        if (Event.current.type.Equals(EventType.repaint))
        {
            if (onShow)
            {
                if (_timer > REPAINT_TIME)
                {
                    if (UnitsMarked) paintUnits();
                    else paintBuildings();

                    _timer = 0f;
                }

                _timer += Time.deltaTime;
            }
        }
    }



    private void paintUnits()
    {
        List<string> l = PopulationInfo.get.GetUnitValues();
        string str = "";

        for (int i = 0; i < uKeys.Count; i++)
        {
            str += uKeys[i] + ": " + l[i] + "\n";
        }

        UpdateText(str);
    }

    private void paintBuildings()
    {
        List<string> l = PopulationInfo.get.GetBuildingValues();
        string str = "";

        for (int i = 0; i < bKeys.Count; i++)
        {
            str += bKeys[i] + ": " + l[i] + "\n";
        }

        UpdateText(str);
    }


    private void Setup()
    {
        uKeys = PopulationInfo.get.GetUnitKeys();
        bKeys = PopulationInfo.get.GetBuildingKeys();
    }

    private void UpdateText(string newText)
    {
        text.GetComponent<GUIText>().text = newText; 
    }

    private void EnableDisableText()
    {
        text.GetComponent<GUIText>().enabled = onShow;
    }

    private void CreateText()
    {
        text = new GameObject("Pop_Info");
        text.AddComponent<GUIText>();
        text.GetComponent<GUIText>().fontSize = 15;
        text.GetComponent<GUIText>().alignment = TextAlignment.Right;
        text.transform.position = new Vector3(0.80f,0.5f,0f);
    }
}
