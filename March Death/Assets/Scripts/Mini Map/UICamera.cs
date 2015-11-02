using UnityEngine;
using System.Collections;

public class UICamera : MonoBehaviour {

    private Camera _uiCam;
    private Camera _minCam;
    private RenderTexture rt;

    public int depth;

	void Start ()
    {
        depth = 20;
        _minCam = GameObject.Find("Minimap Camera").GetComponent<Camera>();
        _uiCam = gameObject.GetComponent<Camera>();
        rt = new RenderTexture(Screen.width, Screen.height, 3);
        _uiCam.targetTexture = rt;
	}
	
	void Update () {

    }

    void OnGUI() {

        if (Event.current.type.Equals(EventType.repaint))
        {
            GUI.depth = 3;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), rt);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _minCam.targetTexture);
        }
        
    }
}
