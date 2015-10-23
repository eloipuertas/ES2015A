using UnityEngine;
using Assets.Scripts.AI;

public class AIDebugSystem : MonoBehaviour {

    AIController controller { get; set; }
    bool showInfo { get; set; }
    public Rect windowRect = new Rect(20, 20, 200, 80);

    public string controllingAgent;
    public float confidence;

    private GUIStyle redFont;

    public static AIDebugSystem CreateComponent(GameObject parent, AIController controller)
    {
        AIDebugSystem AIDSys = parent.AddComponent<AIDebugSystem>();
        AIDSys.controller = controller;
        AIDSys.enabled = true;
        return AIDSys;
    }
    
    void Start()
    {
        redFont.normal.textColor = Color.red;
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
        GUI.Label(new Rect(10, 15, 100, 20), "Choosen Agent:");
        GUI.contentColor = Color.red;
        GUI.Label(new Rect(10, 30, 100, 20), controllingAgent);
        GUI.contentColor = Color.white;
        GUI.Label(new Rect(10, 45, 100, 20), "Confidence:");
        GUI.contentColor = Color.green;
        GUI.Label(new Rect(10, 60, 100, 20), confidence.ToString());
        GUI.DragWindow();
    }
}
