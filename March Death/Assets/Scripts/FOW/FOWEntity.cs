using UnityEngine;
/// <summary>
/// Class which makes an entity capable of interacting with the FOW.
/// </summary>
public class FOWEntity : MonoBehaviour
{
    /// <summary>
    /// If true this entity will reveal the area around it.
    /// </summary>
    public bool IsRevealer;
    /// <summary>
    /// the range around this unit which will be revealed (only used if IsRevealer=True).
    /// </summary>
    public float Range;
    /// <summary>
    /// Size of the object.
    /// !ALERTS!
    ///     This won't be recalculated unitl OnEnable is called
    ///     size.y is actually the component z. We don't care about the real y component, so no point in storing it.
    /// </summary>
    private Vector2 size;
    /// <summary>
    /// This stores half the size vector, and it's only stored to avoid having to calculate it every time
    /// </summary>
    private Vector2 size2;
    /// <summary>
    /// Warning: this marks if an unit is being revealed to the opposite player.
    /// That means even if the player is seeing this unit this may not be true because the AI isn't seeing it
    /// </summary>
    private bool isRevealed;
    public bool IsOwnedByPlayer { get; set; }

    public Rect Bounds
    {
        get
        {
            return new Rect(
            transform.position.x - size2.x,
            transform.position.z - size2.y,
            size.x, size.y);
        }
    }

    /// <summary>
    /// Changes the visibility state of this unit.
    /// </summary>
    /// <param name="isVisible">The new visibilty state</param>
    public void changeVisible(bool isVisible)
    {
        if (isRevealed != isVisible)
        {
            /* Commented until these function exist
            if (isVisible) // Hidden->Visible
                OnEntityDiscovered(this, !IsOwnedByPlayer);
            else  //Visible->Hidden
                OnEntityHidden(this, !IsOwnedByPlayer);*/
            if (!IsOwnedByPlayer)
                changeRenders(isVisible);
            isRevealed = isVisible;
        }
    }
    /// <summary>
    /// Enables/Disables the renderers of this object.
    /// </summary>
    /// <param name="visible">if true we enable the renderer, if false we disable it</param>
    public void changeRenders(bool visible)
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.enabled = visible;
        }
    }
    public void OnEnable()
    {
        FOWManager.addEntity(this);
        if (!calcSize())
        {
            //If we didn't find any collider we are going to say the object is 1x1.
            size = Vector2.one;
        }
        size2 = size / 2;
        IsOwnedByPlayer = true; //TODO: SET when we have a good way too.
        isRevealed = true;
    }
    public void OnDisable()
    {
        FOWManager.removeEntity(this);
    }
    private bool calcSize()
    {
        //Let's find all the relevant colliders
        foreach (Component c in (Component[])GetComponents(typeof(Component)))
        {
            if (c.GetType() == typeof(BoxCollider))
            {
                Vector3 s = ((BoxCollider)c).bounds.size;
                size = new Vector2(s.x, s.z);
                return true;
            }
            else if (c.GetType() == typeof(SphereCollider))
            {
                Vector3 s = ((SphereCollider)c).bounds.size;
                size = new Vector2(s.x, s.z);
                return true;
            }
        }
        return false;
    }
}
