using UnityEngine;
using Assets.Scripts.AI;
using System.Collections.Generic;
using Assets.Scripts.AI.Agents;
using System;

public class AIDebugSystem : MonoBehaviour {

    AIController controller { get; set; }

    bool showInfo { get; set; }
    public Rect windowRect = new Rect(20, 20, 200, 80);
    public Rect windowRect2 = new Rect(20 + 200 + 10, 20, 400, 90);

    private const int WINDOW_HEIGHT_OFFSET_TOLERANCE = 20;

    public string controllingAgent;
    public float confidence;
    private int nextLine = 0;
    private int lineHeight = 15;
    private int marginLeft = 10;
    private int textWidth = 100;
    private int textHeight = 20;

    Dictionary<string, float> agentsConfidence = new Dictionary<string, float>();
    Dictionary<string, int> timesCalledAgents = new Dictionary<String, int>();
    Dictionary<string, float> maxRegisteredValue = new Dictionary<String, float>();
    Dictionary<string, float> minRegisteredValue = new Dictionary<String, float>();
    Dictionary<int, Unit> registeredUnits = new Dictionary<int, Unit>();
    Dictionary<int, string> individualUnitInfo = new Dictionary<int, string>();

    int numAgents = 0;//How much agents does we have

    private long timesCalled = 0;//How much times has an agent been called

    public static AIDebugSystem CreateComponent(GameObject parent, AIController controller)
    {
        AIDebugSystem AIDSys = parent.AddComponent<AIDebugSystem>();
        AIDSys.controller = controller;
        AIDSys.enabled = true;
        return AIDSys;
    }
    
    void Start()
    {
        foreach(BaseAgent agent in controller.Micro.agents)
        {
            agentsConfidence.Add(agent.agentName, 0);
            timesCalledAgents.Add(agent.agentName, 0);
            maxRegisteredValue.Add(agent.agentName, -Mathf.Infinity);
            minRegisteredValue.Add(agent.agentName, Mathf.Infinity);
            numAgents++;
        }
    }

	void Update () {
        if (Input.GetKeyDown(KeyCode.F9))
        {
            showInfo = !showInfo;
        }
	}

    void OnGUI()
    {
        if (!showInfo) return;
        windowRect = GUI.Window(0, windowRect, DoMyWindow, "AI Debug");
        windowRect2 = GUI.Window(1, windowRect2, DoMyWindow2, "AI Stats");
        showAIInfoOverUnits();
    }

    /// <summary>
    /// A window which can be moved intended to contain the confidence values of our agents
    /// Very important to see what's going on inside the AI System.
    /// </summary>
    /// <param name="windowID"></param>
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

    void DoMyWindow2(int windowID)
    {
        resetLines();
        GUI.Label(new Rect(marginLeft, getNextLine(), textWidth, textHeight), "Agent");
        GUI.Label(new Rect(marginLeft + textWidth, nextLine, textWidth, textHeight), "Usage %");
        GUI.Label(new Rect(marginLeft * 2 + textWidth * 2, nextLine, textWidth, textHeight), "Min Value");
        GUI.Label(new Rect(marginLeft * 3+ textWidth * 3, nextLine, textWidth, textHeight), "Max Value");
        showAgentsStats();
        GUI.DragWindow();
    }

    void showAgentsStats()
    {
        foreach (KeyValuePair<string, int> agentstat in timesCalledAgents)
        {
            GUI.contentColor = Color.white;
            GUI.Label(new Rect(marginLeft, getNextLine(), textWidth, textHeight), agentstat.Key);
            GUI.contentColor = Color.yellow;
            GUI.Label(new Rect(marginLeft + textWidth, nextLine, textWidth, textHeight), Convert.ToString(Math.Round((double)agentstat.Value / (double)timesCalled * 100, 2)) + "%");
            GUI.contentColor = Color.red;
            GUI.Label(new Rect(marginLeft * 2 + textWidth * 2, nextLine, textWidth, textHeight), minRegisteredValue[agentstat.Key].ToString());
            GUI.contentColor = Color.green;
            GUI.Label(new Rect(marginLeft * 3 + textWidth * 3, nextLine, textWidth, textHeight), maxRegisteredValue[agentstat.Key].ToString());
        }
        
    }

    /// <summary>
    /// Used to show all the confidences of non winner agents to contrast them with the 
    /// winner Angent
    /// </summary>
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

    public void setControllingAgent(string angent, float value)
    {
        controllingAgent = angent;
        confidence = value;
        //Used to count how much this agent has been dominating and how mutch times agents has done their work
        this.timesCalledAgents[angent]++;
        timesCalled++;
    }

    /// <summary>
    /// Used to show the debug info under every unit of the map controlled by the AI
    /// </summary>
    public void showAIInfoOverUnits()
    {
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

    /// <summary>
    /// Used by the agents to explain what units are they controlling
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="info"></param>
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

    /// <summary>
    /// Used to register the confidence of an AI agent in a certain moment
    /// </summary>
    /// <param name="name"></param>
    /// <param name="conf"></param>
    public void setAgentConfidence(string name, float conf)
    {
        agentsConfidence[name] = conf;
        if(conf > maxRegisteredValue[name])
        {
            maxRegisteredValue[name] = conf;
        }

        if(conf < minRegisteredValue[name])
        {
            minRegisteredValue[name] = conf;
        }
    }

    /// <summary>
    /// Used to move the "GUI Cursor" to the first line of the window
    /// </summary>
    public void resetLines()
    {
        nextLine = 0;
    }

    /// <summary>
    /// Used to make a <br> or \n in the GUI.
    /// </summary>
    /// <returns></returns>
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
