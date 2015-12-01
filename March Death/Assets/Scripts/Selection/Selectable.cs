using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Storage;
using Utils;

public class Selectable : SubscribableActor<Selectable.Actions, Selectable>
{

    public enum Actions { CREATED, SELECTED, DESELECTED };

    private Player player;
    private Rect selectedRect = new Rect();
    private Texture2D selectedBox;
    public IGameEntity entity;
    public Collider _collider;
    private GameObject controller;
    private GameObject plane;
    private EntitySelection _unitSelection;

    public bool currentlySelected { get; set; }
    private float healthRatio = 1f;
    private float _lastHealth = 0f;
    private bool entityMoving = true;
    private bool _changedVisible = false;

    public Storage.Races race {
        get { return entity.info.race; }
    }
    
    public Selectable() { }

    //Pendiente
    //IGameEntity gameEntity;

    public override void Awake()
    {
        base.Awake();

        entity = GetComponent<IGameEntity>();
        currentlySelected = false;
        controller = GameObject.Find("GameController");
        player = controller.GetComponent<Player>();
        _collider = GetComponent<Collider>();
        selectedRect = SelectionOverlay.CalculateBox(_collider);
    }

    public override void Start()
    {
        base.Start();
        fire(Actions.CREATED, this.gameObject);
        if (entity.info.isBuilding == true)
        {
        	selectedBox = SelectionOverlay.CreateTexture(false);
        } else 
        {
        	bool ownUnit = entity.info.race == player.race;
        	selectedBox = SelectionOverlay.CreateTexture(ownUnit);
        }

        plane = SelectionOverlay.getPlane(gameObject, selectedBox);

        entity.doIfUnit(unit =>
        {
            unit.register(Unit.Actions.DIED, onUnitDied);
        });

        // only apply for units
        if (entity.info.isUnit) RetrieveLightSelection();
        else _unitSelection = null;
    }

    public override void Update() { }

    protected virtual void LateUpdate()
    {
        if (!(currentlySelected ^ _changedVisible))
        {
            if (currentlySelected) plane = SelectionOverlay.getPlane(gameObject, selectedBox);
            else Destroy(plane, 0f); _lastHealth = 100f;

            _changedVisible = !currentlySelected;
        }

        if (currentlySelected)
        {
            selectedRect = SelectionOverlay.CalculateBox(_collider);
            // only updates the texture if there's been a change in the healthy -> improves quite a lot the results in the profiler inspector
            if (_lastHealth != entity.healthPercentage)
            {
                _lastHealth = entity.healthPercentage;
                healthRatio = _lastHealth / 100f;
                SelectionOverlay.UpdateTexture(plane, selectedBox, healthRatio);
            }
            plane.transform.position = this.gameObject.transform.position + SelectionOverlay.getOffset(_collider);
        }

    }

    protected virtual void OnGUI(){}


    /// <summary>
    /// Individual operation for the current selectable object. Selects only the object
    /// </summary>
    public virtual void SelectOnlyMe()
    {
        player.selection.Select(entity);
    }

    /// <summary>
    /// Individual operation for the current selectable object. Deselects the object from the selection
    /// </summary>
	public virtual void DeselectMe()
    {
        player.selection.Deselect(entity);
    }


    /// <summary>
    /// Sets the selectable entity to selected and triggers selected event
    /// </summary>
    public virtual void SelectEntity()
    {
        this.currentlySelected = true;
        if (_unitSelection) _unitSelection.Enable();
        fire(Actions.SELECTED);
    }

    /// <summary>
    /// Sets the selectable entity to not selected and triggers deselected event
    /// </summary>
    public virtual void DeselectEntity()
    {
        this.currentlySelected = false;
        if (_unitSelection) _unitSelection.Disable();
        fire(Actions.DESELECTED);
    }

    /// <summary>
    /// Set as selected to show health bar
    /// ! Use only for rival units
    /// </summary>
    public virtual void AttackedEntity()
    {
    	this.currentlySelected = true;
    }

    /// <summary>
    ///  Hides health bar
    /// ! Use only for rival units
    /// </summary>
    public virtual void NotAttackedEntity()
    {
    	this.currentlySelected = false;
    }

    public void onUnitDied(System.Object obj)
    {

        if (race == player.race)
        {
            Debug.Log("Unit died, deselecting and other stuff");
            DeselectMe();

        }
        else
        {
            Debug.Log("Enemy died");
            this.currentlySelected = false;
            fire(Actions.DESELECTED);
        }
    }

    /// <summary>
    /// Retrieves the new selection mechanism
    /// </summary>
    private void RetrieveLightSelection()
    {
        
        GameObject selection = transform.FindChild("EntitySelection").gameObject;

        if (!selection) throw new System.Exception("FIX: " + entity.info.race + " - "  + entity.info.name + " prefab needs the EntitySelection prefab which is in Resources/prefab/selection");
        _unitSelection = selection.GetComponent<EntitySelection>();
        _unitSelection.SetColorRace(race);
        
    }
}
