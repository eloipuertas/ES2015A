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
    /// Enables/Disables the renderers of this object.
    /// </summary>
    /// <param name="isVisible">if true we enable the renderer, if false we disable it</param>
    public void changeVisible(bool isVisible)
    {
        foreach(Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.enabled = isVisible;
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
