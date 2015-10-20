using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Managers;

public class UserInput : MonoBehaviour
{
    private Player player;
    private UnitsManager uManager;

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

	CameraController camera;

    // Use this for initialization
    void Start()
    {
        player = GetComponent<Player>();
		selectionTexture = (Texture2D)Resources.Load("SelectionTexture");	
		camera = GameObject.Find("Main Camera").GetComponent ("CameraController") as CameraController;
        uManager = GetComponent<UnitsManager>();
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

    /// <summary>
    /// Checks if there has been activity with the mouse buttons
    /// </summary>
    private void MouseActivity()
    {


		// TODO : (Devel_c) Check positions with the HUD
        if (Input.GetMouseButtonDown (0)) {
			camera.disableManualControl();
			leftButtonIsDown = true;
			mouseButtonUpPoint = Input.mousePosition;    
			topLeft = GetScreenRaycastPoint(mouseButtonUpPoint);

		} else if (Input.GetMouseButtonUp (0)) {
			camera.enableManualControl();
			leftButtonIsDown = false;

			//Check if is a simple click or dragging if the range is not big enough
			if (IsSimpleClick (mouseButtonDownPoint, mouseButtonUpPoint))
			{
				LeftMouseClick ();
			} else {
				SelectUnitsInArea();
			}

		} else if (Input.GetMouseButtonDown (1)) {
			leftButtonIsDown = false;
			topLeft = GetScreenRaycastPoint(mouseButtonDownPoint);
			RightMouseClick ();
		}

		//if the left button is down and the mouse is moving, start dragging
		if(leftButtonIsDown)
		{
			mouseButtonDownPoint = Input.mousePosition;

			bottomRight = GetScreenRaycastPoint(mouseButtonDownPoint);
			bottomLeft  = GetScreenRaycastPoint(new Vector2( mouseButtonDownPoint.x+width(), mouseButtonDownPoint.y));
			topRight    = GetScreenRaycastPoint(new Vector2( mouseButtonDownPoint.x, mouseButtonDownPoint.y-height()));

			//SelectUnitsInArea();
		}
    }

	private Vector3 GetScreenRaycastPoint (Vector2 screenPosition)
	{
		Physics.Raycast(Camera.main.ScreenPointToRay(screenPosition), out hit, Mathf.Infinity);  
		return hit.point;
	}

	private void SelectUnitsInArea() {
		Vector3[] selectedArea = new Vector3[4];
        bool unitSelected = false;

		//set the array with the 4 points of the polygon
		selectedArea[0] = topLeft;
		selectedArea[1] = topRight;
		selectedArea[2] = bottomRight;
		selectedArea[3] = bottomLeft;

		foreach (GameObject unit in player.currentUnits) {
			Vector3 unitPosition = unit.transform.position;
			Selectable selectedObject = unit.GetComponent<Selectable>();
			if (AreaContainsObject(selectedArea, unitPosition)) {
				if (!player.SelectedObjects.Contains(selectedObject)) selectedObject.Select();
                
                // move units doesn't work if the code below is uncommented	
			} else {  
				if (player.SelectedObjects.Contains(selectedObject)) selectedObject.Deselect();
			}
		}

		Player.status currentAction = player.SelectedObjects.Count > 0 ? Player.status.SELECTED_UNTIS : Player.status.IDLE;
		player.setCurrently (currentAction);
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

    /// <summary>
    /// Controls the left mouse click events
    /// </summary>

    private void LeftMouseClick()
    {

		if (player.isCurrently(Player.status.IDLE)) 
        {
            GameObject hitObject = FindHitObject();
            if (hitObject)
            {

                Selectable selectedObject = hitObject.GetComponent<Selectable>();
                // We just be sure that is a selectable object
                if (selectedObject)
                {
                    selectedObject.SelectUnique();
                    player.setCurrently(Player.status.SELECTED_UNTIS);
                }
            }
        }
        else if (player.isCurrently(Player.status.PLACING_BUILDING) && !EventSystem.current.IsPointerOverGameObject())
        {   // HACK : (Hermetico)
            // Check if the player is placing the building but is not over game objetct. 
            // This is needed because just after clicking in a button to place the building, the onMouseUp event is triggered
            GetComponent<BuildingsManager>().placeBuilding();
        }
		else if (player.isCurrently(Player.status.SELECTED_UNTIS)) // There are people selected what should we do?
        {
            GameObject hitObject = FindHitObject();

            if (!hitObject) // out of bounds click
            {
                Debug.Log("The click is out of bounds?");
            }
            else if (hitObject.name == "Terrain") // if it is the terrain, let's go there
            {
                uManager.MoveTo(FindHitPoint());
            }
            else // let's find what it is
            {
                IGameEntity entity = hitObject.GetComponent<IGameEntity>();

                if (entity.info.race != player.race) // If it is another race, we'll attack, but if it's the same race?
                {
                    uManager.AttackTo(entity);
                }
                else // it is our own race, but we have units selected, we should deselect them first
                {
                    // TODO: (hermetico) deselect units first
                    //Vector3 hitPoint = FindHitPoint();

                    if (hitObject)
                    {

                        //maybe, if we are sure that the hit object is not a enemy race it will be
                        // our race, so that it will be selectable too. Prior to delete de check below
                        // double check that the reasoning is correct

                        Selectable selectedObject = hitObject.GetComponent<Selectable>();
                        // We just be sure that is a selectable object
                        if (selectedObject)
                        {
                            // the rest of the units will be deselected
                            selectedObject.SelectUnique();
                            player.setCurrently(Player.status.SELECTED_UNTIS);
                        }
                    }
                    //else if (hitPoint != this.invalidPosition)
                    //{
                    /* TODO check if click is not out of bounds ( perhaps the click is in the HUD ) */

                    // }
                    //else
                    //{
                    //TODO where is the hit???
                    //}
                }
            }
        }
    }

    /// <summary>
    /// Returns the object where the mouse hits
    /// </summary>
    /// <returns></returns>
    public GameObject FindHitObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) return hit.collider.gameObject;
        return null;
    }

    /// <summary>
    /// Returns the point where the mouse hits
    /// </summary>
    /// <returns></returns>
    public  Vector3 FindHitPoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) return hit.point;
        return this.invalidPosition;
    }


    /// <summary>
    /// Controls the right mouse click events
    /// </summary>
    private void RightMouseClick()
    {
        if (player.isCurrently(Player.status.PLACING_BUILDING))
        {
            GetComponent<BuildingsManager>().cancelPlacing();
        }
        else if (player.isCurrently(Player.status.SELECTED_UNTIS))
        {
            for (int i = player.SelectedObjects.Count - 1; i >= 0; i--)
            {
                Selectable selectedObject = (Selectable)player.SelectedObjects[i];
                selectedObject.Deselect();
            }
            player.setCurrently(Player.status.IDLE);
        }
    }
	private bool IsSimpleClick(Vector2 v1, Vector2 v2) 
	{
		if (Vector2.Distance (v1, v2) < mouseButtonReleaseRange) return true;
		return false;
	}
}