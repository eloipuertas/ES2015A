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
    public bool currentlySelected { get; set; }
    private float healthRatio = 1f;
    private bool updateHealthRatio = true;
    private bool entityMoving = true;

	public Selectable() { }

    //Pendiente
    //IGameEntity gameEntity;

    public override void Awake()
    {
        base.Awake();

		GameObject gameObject = GameObject.Find("GameController");
        player = gameObject.GetComponent<Player>();

        //Pendiente
        //gameEntity = this.GetComponent<IGameEntity>();
        selectedBox = SelectionOverlay.CreateTexture();
        currentlySelected = false;
    }

    public override void Start()
    {
        base.Start();
        fire(Actions.CREATED, this.gameObject);
    }

    public override void Update() { }

    protected virtual void LateUpdate()
    {
        bool updateSomething = false;

        // the GameEntity is moving
        if(entityMoving)
        {
            // calculates the box
            selectedRect = SelectionOverlay.CalculateBox(GetComponent<Collider>());
            updateSomething = true;
        }

        if (updateHealthRatio)
        {
            //Pendiente
			IGameEntity entity = gameObject.GetComponent<IGameEntity>();
			healthRatio = entity.healthPercentage / 100f;
            updateSomething = true;
            // doesn't update until gets the callback
            updateHealthRatio = false;
        }

        if(updateSomething) SelectionOverlay.UpdateTexture(selectedBox, healthRatio);
    }

    protected virtual void OnGUI()
    {
        if (currentlySelected)
        {
            DrawSelection();
        }
    }

    public virtual void SelectUnique()
    {
		//Deselect other selected objects
        //TODO : We could move this operation inside the player object
		ArrayList selectedUnits = player.getSelectedObjects ();
		foreach (Selectable selectedObject in selectedUnits) {
			if (selectedObject == this) continue;
			selectedObject.Deselect();
		}

		if (!player.SelectedObjects.Contains(this)) {
			player.SelectedObjects.Add (this);
		}
        this.currentlySelected = true;
		fire (Actions.SELECTED);

    }

	public virtual void Select()
	{
		if (!player.SelectedObjects.Contains(this)) {
			player.SelectedObjects.Add (this);
		}

		this.currentlySelected = true;
		fire (Actions.SELECTED);
	}

	public virtual void Deselect()
    {

		if (player.SelectedObjects.Contains(this)) {
			player.SelectedObjects.Remove(this);
		}

        this.currentlySelected = false;
		fire (Actions.DESELECTED);
    }

    private void DrawSelection()
    {
        GUI.DrawTexture(selectedRect, selectedBox);
    }
}
