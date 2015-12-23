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
    private Quaternion _LifeBarDefaultRotation;

    public bool currentlySelected { get; set; }
    private bool _attackedEntity = false;
    private bool _selectionVisible = false;
    private float healthRatio = 1f;
    private float _lastHealth = 0f;
    private bool entityMoving = true;
    private float _TimeToShowAttackedEntity = 10f;
    

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
            plane = SelectionOverlay.getPlane(gameObject, selectedBox, true);
        }
        else
        {
        	bool ownUnit = entity.info.race == player.race;
        	selectedBox = SelectionOverlay.CreateTexture(ownUnit);
            plane = SelectionOverlay.getPlane(gameObject, selectedBox, false);
        }

        //ponemos el plane dentro del gameobject
        _LifeBarDefaultRotation = plane.transform.rotation;
        plane.transform.parent = transform;
        entity.doIfUnit(unit =>
        {
            unit.register(Unit.Actions.DIED, onUnitDied);
        });

        // only apply for units of the player
        if (entity.info.isUnit && player.race == race) RetrieveLightSelection();
        else _unitSelection = null;


        currentlySelected = false;
        _selectionVisible = true;
        _attackedEntity = false;
        AdjustTexture();

    }

    public void OnDisable()
    {
        if (currentlySelected)
        {
            // As this is an special case, let's first remove us form the squad
            Unit unit = GetComponent<Unit>();
            if (unit != null)
            {
                // Remove fromt he squad
                unit.Squad.RemoveUnit(unit);

                // If it has troop, remove from troop
                if (unit.Troop != null && unit.Squad != unit.Troop)
                {
                    unit.Troop.RemoveUnit(unit);
                }

                // No more units... deselect if selected
                if (player.selection.SelectedSquad == unit.Squad && 
                    player.selection.SelectedSquad.Units.Count == 0)
                {
                    player.selection.Deselect(entity);
                }

                // Now remove us from the squad, so that it doesn't get completely unselected
                unit.Squad = null;

                // Deselect without changing squad!
                DeselectEntity();
            }
            else if (entity.info.isBuilding)
            {
                // Deselect now
                player.selection.Deselect(entity);
            }
        }
    }


    /// <summary>
    /// Adjust the texture initally, needed for the buildings
    /// </summary>
    public void AdjustTexture() {

        selectedRect = SelectionOverlay.CalculateBox(_collider);

        Vector3 position = this.gameObject.transform.position;
        position.y = _collider.bounds.max.y + (_collider.bounds.size.y * .5f);
        // move the plane a little bit from the center
        plane.transform.position = position;
        // rotate the plain to its original position
        plane.transform.rotation = _LifeBarDefaultRotation;

    }

    protected virtual void LateUpdate()
    {


            // only updates the texture if there's been a change in the healthy -> improves quite a lot the results in the profiler inspector
            if (_lastHealth != entity.healthPercentage)
            {
                _lastHealth = entity.healthPercentage;
                healthRatio = _lastHealth / 100f;
                SelectionOverlay.UpdateTexture(plane, selectedBox, healthRatio);

            
        }



        if (_attackedEntity || (currentlySelected && entity.info.isUnit))
        {

            selectedRect = SelectionOverlay.CalculateBox(_collider);

            Vector3 position = this.gameObject.transform.position;
            position.y = _collider.bounds.max.y + (_collider.bounds.size.y * .5f);
            // move the plane a little bit from the center
            plane.transform.position = position;
            // rotate the plain to its original position
            plane.transform.rotation = _LifeBarDefaultRotation;


            if (!_selectionVisible)
            {
                plane.SetActive(true);
                _selectionVisible = true;
            }
        }
        else
        {
            if(_selectionVisible)
            {
                plane.SetActive(false);
                _selectionVisible = false;
            }
        }


    }

    /// <summary>
    /// Individual operation for the current selectable object. Selects only the object
    /// </summary>
    public virtual void SelectOnlyMe()
    {
        player.selection.Select(entity);
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
        if (!this._attackedEntity)
        {
            this._attackedEntity = true;
            StartCoroutine(DisableAttackedEntity());
        }
    }

    IEnumerator DisableAttackedEntity()
    {
        yield return new WaitForSeconds(_TimeToShowAttackedEntity);
        NotAttackedEntity();
    }

    /// <summary>
    ///  Hides health bar
    /// ! Use only for rival units
    /// </summary>
    public virtual void NotAttackedEntity()
    {
    	this._attackedEntity = false;
    }

    public void onUnitDied(System.Object obj)
    {

        if (race == player.race)
        {
            Debug.Log("Unit died, deselecting and other stuff");
            OnDisable();
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
