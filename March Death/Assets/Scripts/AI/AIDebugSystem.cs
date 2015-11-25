using UnityEngine;
using Assets.Scripts.AI;
using System.Collections.Generic;
using Assets.Scripts.AI.Agents;
using System;

public class AIDebugSystem : MonoBehaviour {

	//Used to visualize spheres when our Aisenses asks for it
    public struct DebugSphere
    {
        public Vector3 center;
        public float radius;

        public DebugSphere(Vector3 cent, float rad)
        {
            center = cent;
            radius = rad;
        }
    }

    AIController controller { get; set; }

    bool showInfo { get; set; }
    public Rect commandingAgentWindowRect = new Rect(20, 20, 200, 80);
    public Rect statsWindowRect = new Rect(20 + 200 + 10, 20, 400, 100);
    public Rect resourcesWindowRect = new Rect(20 + 200 + 10 + 400 + 10, 20, 120, 105);

    private const int WINDOW_HEIGHT_OFFSET_TOLERANCE = 20;

    private int nextLine = 0;
    private int lineHeight = 15;
    private int marginLeft = 10;
    private int textWidth = 100;
    private int textHeight = 20;
    
    //This angent is who is controllin NOW the situation
    public string controllingAgent;
    public float confidence;

    public DebugSphere debugSphere;

    //Stores the actual confidence of each agent
    Dictionary<string, float> agentsConfidence = new Dictionary<string, float>();
    //Stores how much times an agent has been controlling the situation
    Dictionary<string, int> timesCalledAgents = new Dictionary<string, int>();
    //Stores the max register of confidence for each agent
    Dictionary<string, float> maxRegisteredValue = new Dictionary<string, float>();
    //Stores the min register of confidence for each agent
    Dictionary<string, float> minRegisteredValue = new Dictionary<string, float>();
    //Stores all the AI units
    Dictionary<int, Unit> registeredUnits = new Dictionary<int, Unit>();
    //Stores all the AI units info to be displayed on the terrain
    Dictionary<int, string> individualUnitInfo = new Dictionary<int, string>();

    int numAgents = 0;//How much agents does we have

    private long timesCalled = 0;//How much times has an agent been called

    /// <summary>
    /// Used to create an instance of the debugger with some start parameters
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="controller"></param>
    /// <returns></returns>
    public static AIDebugSystem CreateComponent(GameObject parent, AIController controller)
    {
        AIDebugSystem AIDSys = parent.AddComponent<AIDebugSystem>();
        AIDSys.controller = controller;
        AIDSys.enabled = true;
        AIDSys.debugSphere = new DebugSphere(Vector3.zero, 10);
        return AIDSys;
    }
    
    void Start()
    {
        // Adds all the agents and the initial values on every data structure that we have
        foreach(BaseAgent agent in controller.Micro.agents)
        {
            agentsConfidence.Add(agent.agentName, 0);
            timesCalledAgents.Add(agent.agentName, 0);
            maxRegisteredValue.Add(agent.agentName, -Mathf.Infinity);
            minRegisteredValue.Add(agent.agentName, Mathf.Infinity);
            numAgents++;
        }
    }

    /// <summary>
    /// Used to enable or disable the GUI visualitzation by pressing F9
    /// </summary>
	void Update () {
        if (Input.GetKeyDown(KeyCode.F9))
        {
            showInfo = !showInfo;
        }
    }

    /// <summary>
    /// Method to show all the info on the screen
    /// </summary>
    void OnGUI()
    {
        if (!showInfo) return;
        commandingAgentWindowRect = GUI.Window(0, commandingAgentWindowRect, controllingAgentWindow, "AI Debug");
        statsWindowRect = GUI.Window(1, statsWindowRect, statisticsWindow, "AI Stats");
        showAIInfoOverUnits();
        resourcesWindowRect = GUI.Window(2, resourcesWindowRect, showResources, "AI Resources");
    }

    void showResources(int windowID)
    {
        GUI.contentColor = Color.red;
        GUI.Label(new Rect(marginLeft, 20, textWidth, textHeight), "Food: " + controller.resources.getAmount(WorldResources.Type.FOOD).ToString());
        GUI.contentColor = Color.white;
        GUI.Label(new Rect(marginLeft, 40, textWidth, textHeight), "Wood: " + controller.resources.getAmount(WorldResources.Type.WOOD).ToString());
        GUI.contentColor = Color.yellow;
        GUI.Label(new Rect(marginLeft, 60, textWidth, textHeight), "Gold: " + controller.resources.getAmount(WorldResources.Type.GOLD).ToString());
        GUI.contentColor = Color.cyan;
        GUI.Label(new Rect(marginLeft, 80, textWidth, textHeight), "Metal: " + controller.resources.getAmount(WorldResources.Type.METAL).ToString());
        GUI.DragWindow();
    }

    /// <summary>
    /// A window which can be moved intended to contain the confidence values of our agents
    /// Very important to see what's going on inside the AI System.
    /// </summary>
    /// <param name="windowID"></param>
    void controllingAgentWindow(int windowID)
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

    /// <summary>
    /// The statistics window
    /// </summary>
    /// <param name="windowID"></param>
    void statisticsWindow(int windowID)
    {
        resetLines();
        GUI.Label(new Rect(marginLeft, getNextLine(), textWidth, textHeight), "Agent");
        GUI.Label(new Rect(marginLeft + textWidth, nextLine, textWidth, textHeight), "Usage %");
        GUI.Label(new Rect(marginLeft * 2 + textWidth * 2, nextLine, textWidth, textHeight), "Min Value");
        GUI.Label(new Rect(marginLeft * 3+ textWidth * 3, nextLine, textWidth, textHeight), "Max Value");
        showAgentsStats();
        GUI.DragWindow();
    }



    /// <summary>
    /// Used to display each agent stats on the Statistics window
    /// </summary>
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

    /// <summary>
    /// Used to ask the AI debugger which agent is commanding the AI right now
    /// </summary>
    /// <param name="angent"></param>
    /// <param name="value"></param>
    public void setControllingAgent(string angent, float value)
    {
        controllingAgent = angent;
        confidence = value;
        //Used to count how much this agent has been dominating and how much times agents has done their work
        this.timesCalledAgents[angent]++;
        timesCalled++;
    }

    /// <summary>
    /// Used to show the debug info under every unit of the map controlled by the AI
    /// </summary>
    public void showAIInfoOverUnits()
    {
        List<int> toRemove=null;
        foreach(KeyValuePair<int, Unit> u in registeredUnits)
        {
            try
            {
                Vector3 infoPosition = Camera.main.WorldToScreenPoint(u.Value.transform.position);
                GUI.Label(new Rect((infoPosition.x), (Screen.height - infoPosition.y), 100, 50), individualUnitInfo[u.Key]);
            }
            catch (Exception ex)
            {
                if(toRemove== null)
                    toRemove = new List<int>();
                toRemove.Add(u.Key);
            }
        }
        if (toRemove != null)
        {
            foreach (int key in toRemove)
                registeredUnits.Remove(key);
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
        if(nextLine + WINDOW_HEIGHT_OFFSET_TOLERANCE >= commandingAgentWindowRect.height)
        {
            commandingAgentWindowRect.height += lineHeight;
        }

        return nextLine;
    }

	/// <summary>
	/// Raises the draw gizmos event and draws the spheres on scene mode.
	/// </summary>
    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(debugSphere.center, debugSphere.radius);
    }
}
