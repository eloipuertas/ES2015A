using UnityEngine;
using Storage;
using Utils;

public class EntityMarker : SubscribableActor<EntityMarker.Actions, EntityMarker>
{
    public enum Actions { BEING_ATTACKED, NORMAL};

    private GameObject plane;
    private Camera minimap_cam, mainCam;
    private float height;
    private Rect marker_rect;
    private Texture2D box_text;

    // Use this for initialization
    public override void Start () {

        base.Start();
    }

    // Update is called once per frame
    protected virtual void Update() {

        if (mainCam == null) { // not initialized yet in start I suppose
            mainCam = GameObject.FindGameObjectWithTag("minimap_cam").GetComponent<Camera>();
        }
        marker_rect = MinimapOverlays.CalculateBoxFromCntr(this.transform.position, mainCam, 1); 
        box_text = MinimapOverlays.CreateTextureUnit(getRaceColor(this.gameObject.GetComponent<IGameEntity>().info.race));
    }

    protected virtual void OnGUI()
    {
        if(gameObject.GetComponent<FOWEntity>().IsRevealed)
            GUI.DrawTexture(marker_rect, box_text);
    }

    private Color getRaceColor(Races r)
    {
        Color c;

        switch (r) {
            case Races.ELVES:
                c = Color.green;
                break;
            case Races.MEN:
                c = Color.red;
                break;
            default:
                c = Color.white;
                break;
        }

        return c;
    }


}
