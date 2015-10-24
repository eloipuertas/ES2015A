using UnityEngine;
using Assets.Scripts.AI;

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

    public static AIDebugSystem CreateComponent(GameObject parent, AIController controller)
    {
        AIDebugSystem AIDSys = parent.AddComponent<AIDebugSystem>();
        AIDSys.controller = controller;
        AIDSys.enabled = true;
        return AIDSys;
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
        GUI.DragWindow();
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
