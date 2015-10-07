using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Storage;
using Utils;

public class Selectable : SubscribableActor<Selectable.Actions, Selectable>
{
	
	public enum Actions { SELECTED, DESELECTED };

    private Rect selectedRect = new Rect();
    private Texture2D selectedBox;
    bool currentlySelected { get; set; }
    private float healthRatio = 1f;
    private bool updateHealthRatio = true;
    private bool entityMoving = true;

	public Selectable() { }

    //Pendiente
    //IGameEntity gameEntity;

    public override void Start()
    {
        base.Start();

        //Pendiente
        //gameEntity = this.GetComponent<IGameEntity>();
        selectedBox = SelectionOverlay.CreateTexture();
        currentlySelected = false;
    }

    protected virtual void Update() { }

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
            //healthRatio = gameEntity.healthPercentage() / 100f;
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

    public virtual void Select(Player player)
    {
        //only handle input if currently selected
        Selectable oldObject = player.SelectedObject;

        if ( !this.Equals(oldObject))
        {
            // old object selection is now false (if exists)
            if (oldObject) oldObject.currentlySelected = false;
            // player selected object is now this current selectable object
            player.SelectedObject = this;
            this.currentlySelected = true;
            //Debug pursposes
            //Pendiente
            //Debug.Log(gameEntity.info.name);
            registerEntityCallbacks();

			fire (Actions.SELECTED);

        }
    }

    private void registerEntityCallbacks()
    {
        //TODO
    }
    private void unregisterEntityCallbacks()
    {
        //TODO
    }
    public virtual void Deselect()
    {
        currentlySelected = false;
		fire (Actions.DESELECTED);
    }

    private void DrawSelection()
    {
        GUI.DrawTexture(selectedRect, selectedBox);
    }
}
