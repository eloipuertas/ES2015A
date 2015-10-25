using UnityEngine;
using Storage;
using Utils;

public class EntityMarker : SubscribableActor<EntityMarker.Actions, EntityMarker>
{
    public enum Actions { BEING_ATTACKED, NORMAL};

    private const float REFRESH_TIME = 1.0f;
    private float tot_timer = 1f;

    private GameObject plane;
    private Camera minimap_cam, mainCam;

    private float height;

    private  Rect marker_rect;
    private Texture2D box_text;

    // Use this for initialization
    public override void Start () {
        base.Start();

        if (GameObject.FindGameObjectWithTag("minimap_cam"))
            mainCam = GameObject.FindGameObjectWithTag("minimap_cam").GetComponent<Camera>();
        else Debug.LogWarning("There is no minimap camera!");
        box_text = null;
    }

    // Update is called once per frame
    protected virtual void Update() {
        if (tot_timer >= REFRESH_TIME)
        {
            if (box_text == null)
                box_text = MinimapOverlays.CreateTextureUnit(getRaceColor(gameObject.GetComponent<IGameEntity>().info.race));
            marker_rect = MinimapOverlays.CalculateBoxFromCntr(this.transform.position, mainCam, 1);
            tot_timer = 0f;
        }
        else tot_timer += Time.deltaTime;
    }

    protected virtual void OnGUI()
    {
        //if(gameObject.GetComponent<FOWEntity>().IsRevealed)
            GUI.depth = 1;
            GUI.DrawTexture(marker_rect, box_text);
    }

    private Color getRaceColor(Races r)
    {
        Color c;

        switch (r) {
            case Races.ELVES:
                c = Color.yellow;
                break;
            case Races.MEN:
                c = Color.blue;
                break;
            default:
                c = Color.white;
                break;
        }

        return c;
    }


}
