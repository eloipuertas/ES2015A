using UnityEngine;
using System.Collections;

public class UserInput : MonoBehaviour
{
    private Player player;

    //Should be better to create a constants class or structure
    private Vector3 invalidPosition = new Vector3(-99999, -99999, -99999);

	//range in which a mouse down and mouse up event will be treated as "the same location" on the map.
	private int mouseButtonReleaseRange = 20;

	//boolean to know if the left mouse button is down
	private bool leftButtonIsDown = false;

	//the mouse position when the user move it ("bottom right" corner of the rect if you start from left to right and from top to bottom)
	private Vector2 mouseButtonDownPoint;
	//the mouse position where the user first click ("top left" corner of the rect if you start from left to right and from top to bottom)
	private Vector2 mouseButtonUpPoint;

	//position of the 4 corners on selected area
	private Vector3 topLeft;
	private Vector3 topRight;
	private Vector3 bottomLeft;
	private Vector3 bottomRight;

	//width and height of the selectionTexture
	private float width() { return mouseButtonUpPoint.x - mouseButtonDownPoint.x; }
	private float height() { return (Screen.height - mouseButtonUpPoint.y) - (Screen.height - mouseButtonDownPoint.y); }

	Texture2D selectionTexture;

	private RaycastHit hit = new RaycastHit();

    // Use this for initialization
    void Start()
    {
        player = GetComponent<Player>();
		selectionTexture = (Texture2D)Resources.Load("SelectionTexture");
    }

	void OnGUI() {

		//draw selection texture if mouse is dragging
		if (leftButtonIsDown) {
			Rect rect = new Rect(mouseButtonDownPoint.x, Screen.height - mouseButtonDownPoint.y, width(), height());
			GUI.DrawTexture (rect, selectionTexture, ScaleMode.StretchToFill, true); 
		}
	}

    void Update()
    {
        MouseActivity();
    }

    private void MouseActivity()
    {

        if (Input.GetMouseButtonDown (0)) {
			leftButtonIsDown = true;
			mouseButtonUpPoint = Input.mousePosition;    
			topLeft = GetScreenRaycastPoint(mouseButtonUpPoint);
		} else if (Input.GetMouseButtonUp (0)) {
			leftButtonIsDown = false;

			//Check if is a simple click or dragging if the range is not big enough
			if (isSimpleClick (mouseButtonDownPoint, mouseButtonUpPoint)) LeftMouseClick ();

		} else if (Input.GetMouseButtonDown (1)) {
			leftButtonIsDown = false;
			topLeft = GetScreenRaycastPoint(mouseButtonDownPoint);
			RightMouseClick ();
		}

		//if the left button is down and the mouse is moving, start dragging
		if(leftButtonIsDown)
		{
			//actual position of the mouse
			mouseButtonDownPoint = Input.mousePosition;

			bottomRight = GetScreenRaycastPoint(mouseButtonDownPoint);
			bottomLeft  = GetScreenRaycastPoint(new Vector2( mouseButtonDownPoint.x+width(), mouseButtonDownPoint.y));
			topRight    = GetScreenRaycastPoint(new Vector2( mouseButtonDownPoint.x, mouseButtonDownPoint.y-height()));

			selectUnitsInArea();

		}
    }

	private Vector3 GetScreenRaycastPoint ( Vector2 screenPosition )
	{
		Physics.Raycast(Camera.main.ScreenPointToRay(screenPosition), out hit, Mathf.Infinity);  
		return hit.point;
	}

	private void selectUnitsInArea() {
		Vector3[] selectedArea = new Vector3[4];

		//set the array with the 4 points of the polygon
		selectedArea[0] = topLeft;
		selectedArea[1] = topRight;
		selectedArea[2] = bottomRight;
		selectedArea[3] = bottomLeft;

		foreach (GameObject unit in player.currentUnits) {
			Vector3 unitPosition = unit.transform.position;
			Selectable selectedObject = unit.GetComponent<Selectable>();
			if (AreaContainsObject(selectedArea, unitPosition)) {
				selectedObject.Select(player);
			} else {
				selectedObject.Deselect();
			}
		}
	}

	//math formula to know if a given point is inside an area
	private bool AreaContainsObject(Vector3[] area, Vector3 objectPosition) {
		bool inArea = false;
		int l = area.Length;
		int j = l - 1;

		for(int i = -1 ; ++i < l; j = i){      
			if(((area[i].z <= objectPosition.z &&  objectPosition.z < area[j].z) || (area[j].z <= objectPosition.z && objectPosition.z < area[i].z)) 
			   && (objectPosition.x < (area[j].x - area[i].x) * (objectPosition.z - area[i].z) / (area[j].z - area[i].z) + area[i].x))
				inArea = !inArea;
		}
		return inArea;
	}

    private void LeftMouseClick()
    {
        GameObject hitObject = FindHitObject();
        Vector3 hitPoint = FindHitPoint();

        if (hitObject)
        {

            Selectable selectedObject = hitObject.GetComponent<Selectable>();
            // We just be sure that is a selectable object
            if (selectedObject)
            {
                selectedObject.SelectUnique(player);
            }
        }
        else if (hitPoint != this.invalidPosition)
        {
            /* TODO check if click is not out of bounds ( perhaps the click is in the HUD ) */

        }
        else
        {
            //TODO where is the hit???
        }
    }

	private void RightMouseClick()
	{
		foreach (Selectable selectedObject in player.SelectedObjects) {
			selectedObject.Deselect();
		}
	}
		
	private bool isSimpleClick(Vector2 v1, Vector2 v2) 
	{
		if (Vector2.Distance (v1, v2) < mouseButtonReleaseRange) return true;
		return false;

	}

    private GameObject FindHitObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) return hit.collider.gameObject;
        return null;
    }

    private Vector3 FindHitPoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) return hit.point;
        return this.invalidPosition;
    }
}