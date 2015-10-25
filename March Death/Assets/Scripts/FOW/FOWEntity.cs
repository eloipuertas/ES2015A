using UnityEngine;
using Utils;
/// <summary>
/// Class which makes an entity capable of interacting with the FOW.
/// !WARNING! Has to be manually activated by calling one of the Activate!
/// </summary>
public class FOWEntity : SubscribableActor<FOWEntity.Actions, FOWEntity>
{
    public enum Actions { DISCOVERED,HIDDEN }
    /// <summary>
    /// If true this entity will reveal the area around it.
    /// </summary>
    public bool IsRevealer { get; set; }
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
    public bool isRevealed { get; set; }
    public bool IsOwnedByPlayer { get; set; }
    public bool IsRevealed { get { return isRevealed; } }
    private bool activated;
    private bool notFullyOpaque;
    public override void Start()
    {
        base.Start();
        activated = false;
        IsRevealer = true;
    }
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
            fire((isVisible) ? Actions.DISCOVERED : Actions.HIDDEN);
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
        if (!notFullyOpaque)
        {
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                r.enabled = visible;
            }
        }
    }
    /// <summary>
    /// Enables the FOWEntity to start working as so.
    /// If you have the owner of this entity you should call it with Activate(owner)
    /// if you only have the race you can call it with Activate(race)
    /// </summary>
    public void Activate()
    {
        GameObject gameInformationObject = GameObject.Find("GameInformationObject");
        Activate(this.gameObject.GetComponent<IGameEntity>().getRace() == gameInformationObject.GetComponent<GameInformation>().GetPlayerRace());
    }
    /// <summary>
    /// Enables the FOWEntity to start working as so.
    /// If you have the owner of this entity you should call it with Activate(owner)
    /// </summary>
    public void Activate(Storage.Races race)
    {
        GameObject gameInformationObject = GameObject.Find("GameInformationObject");
        Activate(race == gameInformationObject.GetComponent<GameInformation>().GetPlayerRace());
    }
    /// <summary>
    /// Enables the FOWEntity to start working as so.
    /// </summary>
    public void Activate(bool owner)
    {
        if (!calcSize())
        {
            //If we didn't find any collider we are going to say the object is 1x1.
            size = Vector2.one;
        }
        size2 = size / 2;
        IsOwnedByPlayer = owner;
        onEnableSoft();
        activated = true;
    }
    //method to avoid having to call OnEnable Directly,
    private void onEnableSoft()
    {
        FOWManager fow = FOWManager.Instance;
        if (fow)
        {
            fow.addEntity(this);
            if(fow.Enabled)
                isRevealed = fow.isThereinRect(Bounds, FOWManager.visible.visible, !IsOwnedByPlayer);
            notFullyOpaque = fow.NotFullyOpaque;
            if (!IsOwnedByPlayer)
                changeRenders(isRevealed);
            //If the entity gets created in a visible zone it's still "discovered".
            //If it gets created deep in the FOW it doesn't count as "hidden"-
            if (isRevealed)
            {
                fire(Actions.DISCOVERED);
            }
        }
    }
    public void OnEnable()
    {
        if (activated)
            onEnableSoft();
    }
    public void OnDisable()
    {
        if (activated)
        {
            FOWManager fow = FOWManager.Instance;
            if (fow)
            {
                fow.removeEntity(this);
            }
        }
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
