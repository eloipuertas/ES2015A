using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Managers;

public class UserInput : MonoBehaviour
{
	private enum action {NONE, LEFT_CLICK, RIGHT_CLICK, DRAG}
	private action currentAction;
	
	private Player player;
    private SelectionManager sManager { get { return player.selection; } }
    private BuildingsManager bManager { get { return player.buildings; } }

    //Should be better to create a constants class or structure
    public Vector3 invalidPosition { get{ return new Vector3(-99999, -99999, -99999); } }
	
	//range in which a mouse down and mouse up event will be treated as "the same location" on the map.
	private int mouseButtonReleaseRange = 20;
	
	//boolean to know if the left mouse button is down
	private bool leftButtonIsDown = false;
	
	private Vector2 mouseButtonDownPoint;
	private Vector2 mouseButtonCurrentPoint;
	
	//position of the 4 corners on selected area
	private Vector3 topLeft;
	private Vector3 topRight;
	private Vector3 bottomLeft;
	private Vector3 bottomRight;
	
	//width and height of the selectionTexture
	private float width() { return mouseButtonCurrentPoint.x - mouseButtonDownPoint.x; }
	private float height() { return (Screen.height - mouseButtonCurrentPoint.y) - (Screen.height - mouseButtonDownPoint.y); }
	
	Texture2D selectionTexture;
	Texture2D cursorAttack;
	
	private RaycastHit hit = new RaycastHit();
	private Ray ray;
	
	CameraController camera;
	
	public LayerMask TerrainLayerMask;
	
	Rect rectActions;
	Rect rectInformation;
	
	// Use this for initialization
	void Start()
	{
        player = GetComponent<Player>();
		selectionTexture = (Texture2D)Resources.Load ("SelectionTexture");
		cursorAttack = (Texture2D)Resources.Load("cursor_attack");
		camera = GameObject.Find("Main Camera").GetComponent ("CameraController") as CameraController;
		
		//Get hud components rect
		RectTransform actions = GameObject.Find("actions").GetComponent<RectTransform>();
		RectTransform info = GameObject.Find("Information").GetComponent<RectTransform>();
		rectActions = new Rect (actions.position.x - actions.rect.position.x - actions.rect.width, actions.position.y - actions.rect.position.y - actions.rect.height, actions.rect.width, actions.rect.height);
		rectInformation = new Rect (info.position.x - info.rect.position.x - info.rect.width, info.position.y - info.rect.position.y - info.rect.height, info.rect.width, info.rect.height);
		
	}
	
	void OnGUI() {
		
		//Draw selection texture if mouse is dragging
		if (currentAction == action.DRAG) {
			Rect rect = new Rect (mouseButtonDownPoint.x, Screen.height - mouseButtonDownPoint.y, width (), height ());
			GUI.DrawTexture (rect, selectionTexture, ScaleMode.StretchToFill, true); 
		}
		
	}
	
	void Update()
	{
		currentAction = GetMouseAction ();
		// FIXME: add HUD colliders
		if (rectActions.Contains (Input.mousePosition) || rectInformation.Contains (Input.mousePosition)) {
			currentAction = action.NONE;
			return;
		}
		if (currentAction == action.NONE) {
			//DO NOTHING
		} else if (currentAction == action.LEFT_CLICK) {
			LeftClick ();
		} else if (currentAction == action.RIGHT_CLICK) {
			RightClick ();
		} else if (currentAction == action.DRAG) {
			Drag ();
		}
	}
	


	private void LeftClick() 
	{
        switch (player.currently)
        {
            case Player.status.IDLE:
                Select();
                break;
            case Player.status.SELECTED_UNITS:
                Deselect();
                Select();
                break;
            case Player.status.PLACING_BUILDING:
                PlaceBuilding();
                break;

        }

	}
	


	private void RightClick() 
	{
		if (player.isCurrently (Player.status.IDLE)) {
			//Do nothing
		} else if (player.isCurrently (Player.status.SELECTED_UNITS)) {
			
			GameObject hitObject = FindHitObject();
			if (!hitObject) // out of bounds click
			{
				Debug.Log("The click is out of bounds?");
			}
			else if (hitObject.name == "Terrain") // if it is the terrain, let's go there
			{
				sManager.MoveTo(FindHitPoint());
			} 
			else // let's find what it is, check if is own unit or rival
			{
				IGameEntity entity = hitObject.GetComponent<IGameEntity>();
				
				if (entity.info.race != player.race) // If it is another race, we'll attack, but if it's the same race?
				{
					sManager.AttackTo(entity);
				}
			}
		} else if (player.isCurrently (Player.status.PLACING_BUILDING))
		{
			bManager.cancelPlacing();
		}
	}
	

    /// <summary>
    /// Performs operations while dragging mouse
    /// </summary>
	private void Drag() {
        switch (player.currently)
        {
            case Player.status.IDLE:
            case Player.status.SELECTED_UNITS:
			    SelectUnitsInArea();
                break;
		}
	}
	
	private void Select() {
		
		//Select if exists unit
		GameObject hitObject = FindHitObject ();
		if (hitObject) {		
			Selectable selectedObject = hitObject.GetComponent<Selectable> ();
            
			// We just be sure that is a selectable object
			if (selectedObject) {
                sManager.SelectUnique(selectedObject);
                player.setCurrently (Player.status.SELECTED_UNITS);
			} 
		}
	}
	
	private void Deselect() {

        //Deselect all
        sManager.EmptySelection();
		player.setCurrently(Player.status.IDLE);
	}
	
	private void PlaceBuilding() {
		
		//Place building if position is correct
		if (!EventSystem.current.IsPointerOverGameObject()) {
			if (bManager.placeBuilding()) {
				player.setCurrently (Player.status.SELECTED_UNITS);
			} else {
				player.setCurrently (Player.status.PLACING_BUILDING);
			}
		}
	}
	
	/// <summary>
	/// Checks if there has been activity with the mouse buttons
	/// </summary>
	private action GetMouseAction()
	{
		// TODO : (Devel_c) Check positions with the HUD
		mouseButtonCurrentPoint = Input.mousePosition;
		if (Input.GetMouseButtonUp (0)) {
			leftButtonIsDown = false;
			camera.enableManualControl();
			
			//Check if is a simple click or dragging if the range is not big enough
			if (IsSimpleClick (mouseButtonDownPoint, mouseButtonCurrentPoint))
			{
				return action.LEFT_CLICK;
			} else {
				return action.NONE;
			}
		} else if (Input.GetMouseButtonDown (0)) {
			leftButtonIsDown = true;
			mouseButtonDownPoint = mouseButtonCurrentPoint;
			topLeft = GetScreenRaycastPoint(mouseButtonDownPoint);
			camera.disableManualControl();
		} else if (Input.GetMouseButtonDown (1)) {
			leftButtonIsDown = false;
			return action.RIGHT_CLICK;
		}
		
		if (leftButtonIsDown && !IsSimpleClick (mouseButtonDownPoint, mouseButtonCurrentPoint)) {
			bottomRight = GetScreenRaycastPoint (mouseButtonCurrentPoint);
			bottomLeft = GetScreenRaycastPoint (new Vector2 (mouseButtonDownPoint.x + width (), mouseButtonDownPoint.y));
			topRight = GetScreenRaycastPoint (new Vector2 (mouseButtonDownPoint.x, mouseButtonDownPoint.y - height ()));
			return action.DRAG;
		} else {
			return action.NONE;
		}
	}
	
	private Vector3 GetScreenRaycastPoint (Vector2 screenPosition)
	{
		Physics.Raycast(Camera.main.ScreenPointToRay(screenPosition), out hit, Mathf.Infinity);  
		return hit.point;
	}

	private void DeselectBuildings()
    {
        sManager.RemoveBuildinds();
	}

	private void SelectUnitsInArea() {
		DeselectBuildings();
		Vector3[] selectedArea = new Vector3[4];
		
		//set the array with the 4 points of the polygon
		selectedArea[0] = topLeft;
		selectedArea[1] = topRight;
		selectedArea[2] = bottomRight;
		selectedArea[3] = bottomLeft;
		
		GameObject unit;
		Vector3 unitPosition;
		Selectable selectedObject;
		foreach (IGameEntity entity in player.activeEntities) {
			if (entity == null) {
				continue;
			}

			//Check if is unit
			if (entity.info.isBuilding){
				continue;
			}

			//Check if is selectable
			unit = entity.getGameObject();
			selectedObject = unit.GetComponent<Selectable>();
			if (selectedObject == null) {
				continue;
			}

			unitPosition = unit.transform.position;
			if (AreaContainsObject(selectedArea, unitPosition)) {
                sManager.Select(selectedObject);
			} else {  
                sManager.Deselect(selectedObject);
            }
		}
		
		Player.status currentAction = player.SelectedObjects.Count > 0 ? Player.status.SELECTED_UNITS : Player.status.IDLE;
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
	/// Returns the point of the terrain where the mouse hits
	/// </summary>
	/// <returns></returns>
	public Vector3 FindTerrainHitPoint()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out hit, Mathf.Infinity, TerrainLayerMask))
			return hit.point;
		else
			return this.invalidPosition;
	}
	
	private bool IsSimpleClick(Vector2 v1, Vector2 v2) 
	{
		if (Vector2.Distance (v1, v2) < mouseButtonReleaseRange) return true;
		return false;
	}
}
