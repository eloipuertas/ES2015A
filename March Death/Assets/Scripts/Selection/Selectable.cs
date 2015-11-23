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
    private EntitySelection _entitySelection;

    public bool currentlySelected { get; set; }
    private float healthRatio = 1f;
    private float _lastHealth = 0f;
    private bool entityMoving = true;
    private bool _changedVisible = false;

    public Storage.Races race {
        get { return player.race; }
    }
    
    public Selectable() { }

    //Pendiente
    //IGameEntity gameEntity;

    public override void Awake()
    {
        base.Awake();
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
        entity = GetComponent<IGameEntity>();
        bool ownUnit = entity.info.race == player.race;
        selectedBox = SelectionOverlay.CreateTexture(ownUnit);
        plane = SelectionOverlay.getPlane(gameObject);

        RetrieveLightSelection();
    }

    public override void Update() { }

    protected virtual void LateUpdate()
    {
        if (!(currentlySelected ^ _changedVisible))
        {
            if (currentlySelected) plane = SelectionOverlay.getPlane(gameObject);
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
        player.selection.SelectUnique(this);

    }

    /// <summary>
    /// Individual operation for the current selectable object. Deselects the object from the selection
    /// </summary>
	public virtual void DeselectMe()
    {
        player.selection.Deselect(this);
    }


    /// <summary>
    /// Sets the selectable entity to selected and triggers selected event
    /// </summary>
    public virtual void SelectEntity()
    {
        this.currentlySelected = true;
        if (_entitySelection) _entitySelection.Enable();
        fire(Actions.SELECTED);
    }

    /// <summary>
    /// Sets the selectable entity to not selected and triggers deselected event
    /// </summary>
    public virtual void DeselectEntity()
    {
        this.currentlySelected = false;
        if (_entitySelection) _entitySelection.Disable();
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

    private void RetrieveLightSelection()
    {
        
        GameObject selection = transform.FindChild("EntitySelection").gameObject;
        if (selection)
        {
            _entitySelection = selection.GetComponent<EntitySelection>();
            _entitySelection.SetColorRace(race);
        }
        else { _entitySelection = null; }
    }
}
