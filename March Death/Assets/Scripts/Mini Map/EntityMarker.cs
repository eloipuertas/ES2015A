using UnityEngine;
using Storage;
using Utils;

public class EntityMarker : SubscribableActor<EntityMarker.Actions, EntityMarker>
{
    public enum Actions { BEING_ATTACKED, NORMAL};

    private const float REFRESH_TIME = 1.0f;
    private float tot_timer = 1f;

    private float res;

    private const float ANIMATION_TIME = 0.6f;
    private float creation_timer = 0f;
    private float underAttack_timer = 0f;
    private bool creation_ON = false;
    private bool underAttack_ON = false;
    private Rect creation_rect;
    private Rect underAttack_rect;
    private int creation_ind = 0;
    private int underAttack_ind = 0;
    private Texture2D creation_tex;
    private Texture2D underAttack_tex;

    private FOWEntity fe;

    private GameObject plane;
    private Camera minimap_cam, mainCam;

    private float height;

    private  Rect marker_rect;
    private Texture2D box_text;

	private bool isOnSight;
	private int onSightIdx;
	private float onSightTimer;
	private Rect onSightRect;
	private Texture2D onSightTexture;


    // Use this for initialization
    public override void Start () {
        base.Start();

        if (GameObject.FindGameObjectWithTag("minimap_cam"))
            mainCam = GameObject.FindGameObjectWithTag("minimap_cam").GetComponent<Camera>();
        else Debug.LogWarning("There is no minimap camera!");
        fe = gameObject.GetComponent<FOWEntity>();
        box_text = null;

        creation_tex = new Texture2D(32, 32);
        creation_tex = (Texture2D)Resources.Load("creation");
        underAttack_tex = new Texture2D(32, 32);
        underAttack_tex = (Texture2D)Resources.Load("underAttack");

        creation_ON = true;

		isOnSight = false;
		onSightIdx = 0;
		onSightTimer = 0f;
		onSightTexture = new Texture2D(32, 32);
		onSightTexture = (Texture2D) Resources.Load("redcross");

        res = Screen.currentResolution.width;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (tot_timer >= REFRESH_TIME)
        {
            if (box_text == null)
                box_text = MinimapOverlays.CreateTextureUnit(getRaceColor(gameObject.GetComponent<IGameEntity>().info.race));
            marker_rect = MinimapOverlays.CalculateBoxFromCntr(this.transform.position, mainCam, 1);
            
            underAttack_rect = MinimapOverlays.CalculateBoxFromCntr(this.transform.position, mainCam, 10);
            tot_timer = 0f;
        }
        else tot_timer += Time.deltaTime;

        if (creation_ON)
        {
            if (!hasAnimationEnd(ref creation_timer, ANIMATION_TIME, ref creation_ON))
            {
                creation_ind = getIndex(creation_timer, ANIMATION_TIME);
            }
        }

        if (underAttack_ON)
        {
            if (!hasAnimationEnd(ref underAttack_timer, ANIMATION_TIME, ref underAttack_ON))
            {
                underAttack_ind = getIndex(underAttack_timer, ANIMATION_TIME);
            }
        }
		if (isOnSight)
		{
			if (!hasAnimationEnd(ref onSightTimer, ANIMATION_TIME, ref isOnSight))
			{
				onSightIdx = getIndex(onSightTimer, ANIMATION_TIME);
			}
		}
    }

    protected virtual void OnGUI()
    {
        if (Event.current.type.Equals(EventType.repaint))
        {
            if (fe)
            {
                if (fe.IsRevealed || fe.IsOwnedByPlayer)
                {
                    GUI.depth = 0;
                    GUI.DrawTexture(marker_rect, box_text);
                    if (creation_ON) showCreate(creation_ind);
                    if (underAttack_ON) showAttack(underAttack_ind);
					if (isOnSight) showOnSight(onSightIdx);
                }

            }
            else {
                Debug.Log(gameObject.name + " has no FowEntity or perhaps has more than one."); // RAUL_DEB
            }
        }
    }

    private int getIndex(float actual_time, float total_time)
    {
        return (int)((actual_time * 15 )/ total_time);
    }

    private void showCreate(int index)
    {
        Debug.Log(gameObject.name + ": " + "CREATING"); // RAUL_DEB
        creation_rect = MinimapOverlays.CalculateBoxFromCntr(this.transform.position, mainCam, (int)((15f/(15f+11f))*(index+1))); // 11f : hack to make less vig the texture raul_hack
        GUI.DrawTexture(creation_rect, creation_tex) ;
    }

    private void showAttack(int index)
    {
        underAttack_rect = MinimapOverlays.CalculateBoxFromCntr(this.transform.position, mainCam, (int)((13f * 3f)/(index + 1))); // 3f : hack to make less vig the texture raul_hack
        GUI.DrawTexture(underAttack_rect, underAttack_tex);
    }

	private void showOnSight(int index)
	{
		onSightRect = MinimapOverlays.CalculateBoxFromCntr(transform.position, mainCam, (int)((5f * 3f)/(index + 1)));
		GUI.DrawTexture(onSightRect, onSightTexture);
	}

    private bool hasAnimationEnd(ref float actual_time, float tot_time, ref bool ON)
    {
        actual_time += Time.deltaTime;
        if (actual_time > tot_time) { ON = false;  actual_time = 0f; }
        return ON ? false: true;
    }


    public void entityUnderAttack()
    {
        underAttack_ON = true;
    }

	public void entityOnSight()
	{
		isOnSight = true;
	}


    // info methods
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
    
    /*
    private void registerMinimapActions()
    {
        EntityMarker em = gameObject.GetComponent<EntityMarker>();
        register(Actions.DAMAGED, em.onEntityUnderAttack);
    }*/
   /* private Info getEntityInfo()
    {
        gameObject.
    }*/

}
