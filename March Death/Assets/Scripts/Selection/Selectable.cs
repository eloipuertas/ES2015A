using UnityEngine;
using System.Collections;

public class Selectable : MonoBehaviour
{

    private Rect selectedRect = new Rect();
    private Texture2D selectedBox;
    bool currentlySelected { get; set; }
    private float healthRatio = .5f;

    IGameEntity gameEntity;

    protected virtual void Start()
    {
        selectedBox = SelectionOverlay.CreateTexture();
        currentlySelected = false;
    }

    protected virtual void Update() { }

    protected virtual void LateUpdate()
    {
        float healthRatio = 1f;
        healthRatio = this.GetComponent<IGameEntity>().FakeHealthRatio;
        //calculate the box and update the texture after Update()
        selectedRect = SelectionOverlay.CalculateBox(GetComponent<Collider>());
        SelectionOverlay.UpdateTexture(selectedBox, healthRatio);
    }

    protected virtual void OnGUI()
    {
        if (currentlySelected) DrawSelection();
    }

    public virtual void Select(Player player)
    {
        //only handle input if currently selected

        Selectable oldObject = player.SelectedObject;

        if (this.Equals(oldObject))
        {
            Debug.Log("Selected the same object");
        }
        else
        {
            // old object selection is now false (if exists)
            if (oldObject) oldObject.currentlySelected = false;
            // this is now currently selected
            this.currentlySelected = true;
            // player selected object is now this current selectable object
            player.SelectedObject = this;

            //Debug pursposes
            IGameEntity thisEntity = this.GetComponent<IGameEntity>();
            Debug.Log(thisEntity.FakeType);
        }
    }
    public virtual void Deselect()
    {
        currentlySelected = false;
    }

    private void DrawSelection()
    {
        GUI.DrawTexture(selectedRect, selectedBox);
    }
}
