using UnityEngine;
using Assets.Scripts.AI;

public class AIDebugSystem : MonoBehaviour {

    AIController controller { get; set; }
    bool showInfo { get; set; }

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
        if (!showInfo)
        {
            return;
        }

        GUI.Label(new Rect(10, 10, 100, 20), "Hello Debug!");
    }
}
