using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Managers;
using Utils;

public partial class UserInput : MonoBehaviour
{
    public enum action { NONE, LEFT_CLICK, RIGHT_CLICK, DRAG }
    private action currentAction;

    private SelectionManager sManager { get { return BasePlayer.player.selection; } }
    private BuildingsManager bManager { get { return BasePlayer.player.buildings; } }
    private CursorManager cursor;

    //Should be better to create a constants class or structure
    public Vector3 invalidPosition { get { return new Vector3(-99999, -99999, -99999); } }

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

    public Vector3 LastTerrainPos { get; set; }

    //width and height of the selectionTexture
    private float width() { return mouseButtonCurrentPoint.x - mouseButtonDownPoint.x; }
    private float height() { return (Screen.height - mouseButtonCurrentPoint.y) - (Screen.height - mouseButtonDownPoint.y); }

    // minimap related
    private Camera minimapCamera;


    Texture2D selectionTexture;
    Texture2D cursorAttack;

    private RaycastHit hit = new RaycastHit();

    CameraController camera;

    public LayerMask TerrainLayerMask;

    Rect rectActions;
    Rect rectInformation;

    void Awake()
    {
        cursor = CursorManager.Instance;
        cursor.SetInputs(this);
    }

    // Use this for initialization
    void Start()
    {
        selectionTexture = (Texture2D)Resources.Load("SelectionTexture");

        cursorAttack = (Texture2D)Resources.Load("cursor_attack");
        camera = GameObject.Find("Main Camera").GetComponent("CameraController") as CameraController;
        minimapCamera = GameObject.Find("Minimap Camera").GetComponent<Camera>();

        //Get hud components rect
        RectTransform actions = GameObject.Find("actions").GetComponent<RectTransform>();
        RectTransform info = GameObject.Find("Information").GetComponent<RectTransform>();
        rectActions = new Rect(actions.position.x - actions.rect.position.x - actions.rect.width, actions.position.y - actions.rect.position.y - actions.rect.height, actions.rect.width, actions.rect.height);
        rectInformation = new Rect(info.position.x - info.rect.position.x - info.rect.width, info.position.y - info.rect.position.y - info.rect.height, info.rect.width, info.rect.height);

    }

    void OnGUI()
    {

        //Draw selection texture if mouse is dragging
        if (currentAction == action.DRAG)
        {
            Rect rect = new Rect(mouseButtonDownPoint.x, Screen.height - mouseButtonDownPoint.y, width(), height());
            GUI.DrawTexture(rect, selectionTexture, ScaleMode.StretchToFill, true);
        }

    }

    void Update()
    {
        CheckKeyboard();

        bool oldLeftMouseDown = leftButtonIsDown;
        action oldAction = currentAction;
        currentAction = GetMouseAction();

        // Initial drag
        if (oldAction != action.DRAG && currentAction == action.DRAG)
        {
            sManager.DragStart();
        }

        
        // New approach, checking if the click is over an event system object
        if(EventSystem.current.IsPointerOverGameObject())
        // FIXME: add HUD colliders
        //if (rectActions.Contains(Input.mousePosition) || rectInformation.Contains(Input.mousePosition) || minimapCamera.pixelRect.Contains(Input.mousePosition) )
        {
            currentAction = action.NONE;
            return;
        }
        if (currentAction == action.NONE)
        {
            //DO NOTHING
        }
        else if (currentAction == action.LEFT_CLICK)
        {
            LeftClick();
        }
        else if (currentAction == action.RIGHT_CLICK)
        {
            RightClick();
        }
        else if (currentAction == action.DRAG)
        {
            Drag();
        }

        // End drag
        if (oldAction == action.DRAG && currentAction == action.NONE && oldLeftMouseDown && !leftButtonIsDown)
        {
            sManager.DragEnd();
        }
    }



    private void LeftClick()
    {
        switch (BasePlayer.player.currently)
        {
            case Player.status.IDLE:
                Select();
                break;
            case Player.status.SELECTED_UNITS:
                //Deselect(); when clicking two times over the same unit, there are two selections and two sounds
                Select();
                break;
            case Player.status.PLACING_BUILDING:
                PlaceBuilding();
                break;

        }
    }

    private void RightClick()
    {
        if (BasePlayer.player.isCurrently(Player.status.IDLE))
        {
            //Do nothing
        }
        else if (BasePlayer.player.isCurrently(Player.status.SELECTED_UNITS) )
        {

            bool hasHit;
            RaycastHit hit = FindHit(out hasHit, Constants.Layers.HIT_MASK);
            if (!hasHit) // out of bounds click
            {
                Debug.Log("The click is out of bounds?");
            }
            else
            {
                GameObject gameObject = hit.collider.gameObject;
                int hitLayer = (gameObject ? gameObject.layer : 0);

                if (hitLayer == Constants.Layers.TERRAIN)
                {
                    // if resource is selected then we are setting the meeting point
                    if (sManager.IsBuilding)
                    {
                        IBuilding building = sManager.SelectedBuilding;
                        building.setMeetingPoint(hit.point);
                    }
                    else
                    {
                        sManager.MoveTo(hit.point); // if it is the terrain,and we are not building let's go there
                    }

                }
                else if (hitLayer == Constants.Layers.UNIT || hitLayer == Constants.Layers.BUILDING) // let's find what it is, check if is own unit or rival
                {
                    IGameEntity entity = gameObject.GetComponent<IGameEntity>();

                    if (entity.info.race != BasePlayer.player.race
                        && entity.status != EntityStatus.DEAD
                        && entity.status != EntityStatus.DESTROYED)
                    {
                        sManager.AttackTo(entity);
                    }
                    else if (entity.info.isResource
                            && (entity.status == EntityStatus.IDLE
                            || entity.status == EntityStatus.WORKING))
                    {
                        sManager.Enter(entity);
                    }
                }
            }
        }
        else if (BasePlayer.player.isCurrently(Player.status.PLACING_BUILDING))
        {
            bManager.cancelPlacing();
            BasePlayer.player.setCurrently(Player.status.SELECTED_UNITS);
        }
    }


    /// <summary>
    /// Performs operations while dragging mouse
    /// </summary>
    private void Drag()
    {
        switch (BasePlayer.player.currently)
        {
            case Player.status.IDLE:
            case Player.status.SELECTED_UNITS:
                SelectUnitsInArea();
                break;
        }
    }

    private void Select()
    {
        bool hasHit;
        RaycastHit hit = FindHit(out hasHit, Constants.Layers.SELECTABLE_MASK);

        if (hasHit)
        {
            IGameEntity selectedObject = hit.collider.gameObject.GetComponent<IGameEntity>();

            if (sManager.CanBeSelected(selectedObject))
            {
                Deselect();
                sManager.Select(selectedObject);
                BasePlayer.player.setCurrently(Player.status.SELECTED_UNITS);
            }
            else
            {
                Deselect();
                BasePlayer.player.setCurrently(Player.status.IDLE);
            }
        }
        else
        {
            Deselect();
            BasePlayer.player.setCurrently(Player.status.IDLE);
        }
    }

    private void Deselect()
    {
        //Deselect all
        sManager.DeselectCurrent();
        BasePlayer.player.setCurrently(Player.status.IDLE);
    }

    private void PlaceBuilding()
    {
        //Place building if position is correct
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (bManager.placeBuilding())
            {
                BasePlayer.player.setCurrently(Player.status.SELECTED_UNITS);
            }
            else
            {
                BasePlayer.player.setCurrently(Player.status.PLACING_BUILDING);
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
        if (Input.GetMouseButtonUp(0))
        {
            leftButtonIsDown = false;
            camera.enableManualControl();

            //Check if is a simple click or dragging if the range is not big enough
            if (IsSimpleClick(mouseButtonDownPoint, mouseButtonCurrentPoint) /*&& !EventSystem.current.IsPointerOverGameObject()*/)
            {
                return action.LEFT_CLICK;
            }
            else
            {
                return action.NONE;
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            leftButtonIsDown = true;
            mouseButtonDownPoint = mouseButtonCurrentPoint;
            topLeft = GetScreenRaycastPoint(mouseButtonDownPoint);
            camera.disableManualControl();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            leftButtonIsDown = false;
            return action.RIGHT_CLICK;
        }

        if (leftButtonIsDown && !IsSimpleClick(mouseButtonDownPoint, mouseButtonCurrentPoint))
        {
            bottomRight = GetScreenRaycastPoint(mouseButtonCurrentPoint);
            bottomLeft = GetScreenRaycastPoint(new Vector2(mouseButtonDownPoint.x + width(), mouseButtonDownPoint.y));
            topRight = GetScreenRaycastPoint(new Vector2(mouseButtonDownPoint.x, mouseButtonDownPoint.y - height()));
            return action.DRAG;
        }
        else
        {
            return action.NONE;
        }
    }

    private Vector3 GetScreenRaycastPoint(Vector2 screenPosition)
    {
        Physics.Raycast(Camera.main.ScreenPointToRay(screenPosition), out hit, Mathf.Infinity);
        return hit.point;
    }

    private void SelectUnitsInArea()
    {
        Vector3[] selectedArea = new Vector3[4];

        //set the array with the 4 points of the polygon
        selectedArea[0] = topLeft;
        selectedArea[1] = topRight;
        selectedArea[2] = bottomRight;
        selectedArea[3] = bottomLeft;

        Vector3 center = topLeft + (bottomRight - topLeft) / 2;
        float radius = Mathf.Max(Vector3.Distance(topRight, topLeft), Vector3.Distance(bottomRight, topRight));
        GameObject[] objects = Helpers.getObjectsNearPosition(center, radius);
        List<Unit> newInArea = new List<Unit>();

        foreach (GameObject gob in objects)
        {
            IGameEntity entity = gob.GetComponent<IGameEntity>();
            if (entity == null)
            {
                continue;
            }

            // Check if it is an unit and race
            if (entity.info.isBuilding || entity.info.race != BasePlayer.player.race)
            {
                continue;
            }

            //Check if is selectable
            if (AreaContainsObject(selectedArea, entity.getTransform().position))
            {
                newInArea.Add((Unit)entity);
            }
        }

        sManager.DragUpdate(newInArea);

        Player.status currentAction = (sManager.SelectedSquad != null && sManager.SelectedSquad.Units.Count > 0) ? Player.status.SELECTED_UNITS : Player.status.IDLE;
        BasePlayer.player.setCurrently(currentAction);
    }

    //math formula to know if a given point is inside an area
    private bool AreaContainsObject(Vector3[] area, Vector3 objectPosition)
    {
        bool inArea = false;
        int l = area.Length;
        int j = l - 1;

        for (int i = -1; ++i < l; j = i)
        {
            if (((area[i].z <= objectPosition.z && objectPosition.z < area[j].z) || (area[j].z <= objectPosition.z && objectPosition.z < area[i].z))
               && (objectPosition.x < (area[j].x - area[i].x) * (objectPosition.z - area[i].z) / (area[j].z - area[i].z) + area[i].x))
                inArea = !inArea;
        }
        return inArea;
    }

    /// <summary>
    /// Returns the object where the mouse hits
    /// </summary>
    /// <returns></returns>
    public RaycastHit FindHit(out bool hasHit, int mask)
    {
        Ray ray = Camera.main.ScreenPointToRay(mouseButtonCurrentPoint);
        RaycastHit hit;
        hasHit = Physics.Raycast(ray, out hit, 500f, mask);
        return hit;
    }

    private bool IsSimpleClick(Vector2 v1, Vector2 v2)
    {
        if (Vector2.Distance(v1, v2) < mouseButtonReleaseRange) return true;
        return false;
    }

    public action GetCurrentAction()
    {
        return currentAction;
    }
}
