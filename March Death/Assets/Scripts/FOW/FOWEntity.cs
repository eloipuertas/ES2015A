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

    public Rect Bounds
    {
        get
        {
            return new Rect(
            transform.position.x - transform.localScale.x / 2,
            transform.position.z - transform.localScale.z / 2,
            transform.localScale.x, transform.localScale.z);
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
    }
    public void OndDisable()
    {
        FOWManager.removeEntity(this);
    }
}
