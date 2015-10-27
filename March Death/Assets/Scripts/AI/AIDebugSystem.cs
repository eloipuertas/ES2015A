using UnityEngine;
using Assets.Scripts.AI;
using System.Collections.Generic;
using Assets.Scripts.AI.Agents;
using System;

public class AIDebugSystem : MonoBehaviour {

    AIController controller { get; set; }

    bool showInfo { get; set; }
    public Rect windowRect = new Rect(20, 20, 200, 80);

    private const int WINDOW_HEIGHT_OFFSET_TOLERANCE = 20;

    public string controllingAgent;
    public float confidence;
    private int nextLine = 0;
    private int lineHeight = 15;
    private int marginLeft = 10;
    private int textWidth = 100;
    private int textHeight = 20;

    Dictionary<string, float> agentsConfidence;
    Dictionary<int, Unit> registeredUnits;
    Dictionary<int, string> individualUnitInfo;

    public static AIDebugSystem CreateComponent(GameObject parent, AIController controller)
    {
        AIDebugSystem AIDSys = parent.AddComponent<AIDebugSystem>();
        AIDSys.controller = controller;
        AIDSys.enabled = true;
        return AIDSys;
    }
    
    void Start()
    {
        agentsConfidence = new Dictionary<string, float>();
        individualUnitInfo = new Dictionary<int, string>();
        registeredUnits = new Dictionary<int, Unit>();

        foreach(BaseAgent agent in controller.Micro.agents)
        {
            agentsConfidence.Add(agent.agentName, 0);
        }
    }

	void Update () {
        if (Input.GetKeyDown(KeyCode.F9))
        {
            showInfo = !showInfo;
            Debug.Log("AIDebugger:" + showInfo);
        }
	}

    void OnGUI()
    {
        if (!showInfo) return;
        windowRect = GUI.Window(0, windowRect, DoMyWindow, "AI Debug");
        showAIInfoOverUnits();
    }

    void DoMyWindow(int windowID)
    {
        resetLines();
        GUI.Label(new Rect(marginLeft, getNextLine(), textWidth, textHeight), "Choosen Agent:");
        GUI.contentColor = Color.red;
        GUI.Label(new Rect(marginLeft, getNextLine(), textWidth, textHeight), controllingAgent);
        GUI.contentColor = Color.white;
        GUI.Label(new Rect(marginLeft, getNextLine(), textWidth, textHeight), "Confidence:");
        GUI.contentColor = Color.green;
        GUI.Label(new Rect(marginLeft, getNextLine(), textWidth, textHeight), confidence.ToString());
        GUI.contentColor = Color.white;
        GUI.Label(new Rect(marginLeft, getNextLine(), textWidth, textHeight), "Other Agents Confidence:");
        showConfidences();
        GUI.DragWindow();
    }

    public void showConfidences()
    {
        foreach(KeyValuePair<string, float> agent in agentsConfidence)
        {
            //We dont want to display the same agent 2 times
            if (agent.Key.Equals(controllingAgent))
            {
                continue;
            }
            
            GUI.contentColor = Color.magenta;
            GUI.Label(new Rect(marginLeft, getNextLine(), textWidth, textHeight), agent.Key);
            GUI.contentColor = Color.yellow;
            GUI.Label(new Rect(marginLeft, getNextLine(), textWidth, textHeight), agent.Value.ToString());            
        }
    }

    public void showAIInfoOverUnits()
    {
        Debug.Log(registeredUnits.Count);
        foreach(KeyValuePair<int, Unit> u in registeredUnits)
        {
            try
            {
                Vector3 infoPosition = Camera.main.WorldToScreenPoint(u.Value.transform.position);
                GUI.Label(new Rect((infoPosition.x), (Screen.height - infoPosition.y), 100, 50), individualUnitInfo[u.Key]);
            }
            catch (Exception ex)
            {
                registeredUnits.Remove(u.Key);
            }
        }        
    }

    public void registerDebugInfoAboutUnit(Unit unit, string info)
    {
        if(registeredUnits.ContainsKey(unit.GetInstanceID()))
        {
            individualUnitInfo[unit.GetInstanceID()] = info;
        }
        else
        {
            registeredUnits.Add(unit.GetInstanceID(), unit);
            individualUnitInfo.Add(unit.GetInstanceID(), info);
        }
    }

    public void setAgentConfidence(string name, float conf)
    {
        agentsConfidence[name] = conf;
    }

    public void resetLines()
    {
        nextLine = 0;
    }

    public int getNextLine()
    {
        nextLine += lineHeight;

        //We need to ensure that we have enought space int the window
        if(nextLine + WINDOW_HEIGHT_OFFSET_TOLERANCE >= windowRect.height)
        {
            windowRect.height += lineHeight;
        }

        return nextLine;
    }
}
