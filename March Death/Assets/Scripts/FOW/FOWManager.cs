using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton tasked with keeping, updating and passing to the shader a texture with differents color for uncovered/covered areas.
/// Also manages FOWEntities.
/// </summary>
[ExecuteInEditMode]
public class FOWManager : MonoBehaviour
{
    /// <summary>
    /// Terrain from which to create the texture.
    /// </summary>
    public Terrain Terrain;

    public float Quality=1;
    /// <summary>
    /// If enabled zones that should be completly black will just be a little darker. Used mainly for debugging purposes.
    /// Also you will be able to see fowEntities in semifog
    /// </summary>
    public bool NotFullyOpaque = false;
    /// <summary>
    /// Rate at which the uncovered areas darken up after not being lit anymore.
    /// </summary>
    [Range(0,400)]
    public float FadeRate=200;

    public bool Enabled;

    List<FOWEntity> entities = new List<FOWEntity>();
    Texture2D fowTex;
    Color32[] pixels;

    /// <summary>
    /// This will contain the grid for the vision of the AI.
    /// This can be only an enum because it doesn't need to be passed to the shader nor does it need to have all the fancy gradient fading the normal FOW has.
    /// </summary>
    visible[] aiVision;

    void Start()
    {
        if (Application.isPlaying)
            InitializeTexture();
    }
    /// <summary>
    /// Creates a new texture the size of terrain
    /// This may fail with an error message if the created texture is too large, or terrain isn't assigned.
    /// </summary>
    void InitializeTexture()
    {
        if(Terrain)
        { 
            int width = Mathf.RoundToInt(Terrain.terrainData.size.x* Quality);
            int height = Mathf.RoundToInt(Terrain.terrainData.size.z * Quality);

            //Let's make sure we aren't going to generate a huge texture
            if (width*height>4000000) 
            {
                Debug.LogError("FOW: Generated texture may be too large, consider lowering the Quality or using a smaller Terrain");
                return;
            }

            if (fowTex)
                DestroyImmediate(fowTex);
            fowTex = new Texture2D(width, height, TextureFormat.RGB24, false);
            pixels = fowTex.GetPixels32();
            aiVision = new visible[width * height];
            
            //Paint it all black
            Color cc = NotFullyOpaque? new Color(0, 1, 0, 255): Color.black;
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = cc;
                aiVision[i] = visible.unexplored;
            }

            fowTex.SetPixels32(pixels);

            Shader.SetGlobalTexture("_FOWTex", fowTex);
            Shader.SetGlobalVector("_FOWTex_ST",
                new Vector4(
                    Quality / width,
                    Quality / height,
                    (0.5f - Quality * 0.5f) / width,
                    (0.5f - Quality * 0.5f) / height));
        }
        else
        {
            Debug.LogError("FOW: FOWManager doesn't have a terrain assigned");
        }
    }
    
    void Update()
    {
#if UNITY_EDITOR
        //Don't show fog on the editor or if not enabled.
        if (!Application.isPlaying || !Enabled)
        {
            Shader.SetGlobalTexture("_FOWTex", UnityEditor.EditorGUIUtility.whiteTexture);
            if (fowTex != null)
                DestroyImmediate(fowTex);
            fowTex = null;
        }
#endif
        if (fowTex)
        {
            int fade = Mathf.RoundToInt(Time.deltaTime * FadeRate);

            //Fade all the map
            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].b > 0)
                    pixels[i].b = (byte)Mathf.Max(pixels[i].b - fade, 0);
                aiVision[i] &= ~visible.visible; //Remove the visible flag
            }
            //Reveal the area around the revealer entities
            foreach (FOWEntity e in entities)
            {
                if (e.IsRevealer)
                    reveal(e);
            }
            //Hide or show the other entities
            foreach (FOWEntity e in entities)
                if (!e.IsOwnedByPlayer)
                    e.changeVisible(isThereinRect(e.Bounds,visible.visible));
            
            fowTex.SetPixels32(pixels);
            fowTex.Apply();
        }
    }
    /// <summary>
    /// Reveals an area around the entity passed as paramater
    /// </summary>
    /// <param name="entity">Entity which reveals an area</param>
    private void reveal(FOWEntity entity)
    {
        Rect rect = entity.Bounds;
        int xMin, xMax, yMin, yMax;
        getBounds(rect, Mathf.RoundToInt(entity.Range * Quality), out xMin, out xMax, out yMin, out yMax);
        for (int y = yMin; y <= yMax; y++)
        {
            float yIntl = Mathf.Clamp(y, rect.yMin, rect.yMax - 1);
            for (int x = xMin; x <= xMax; x++)
            {
                Vector2 pos = new Vector2(x, y) / Quality;
                Vector2 intlPos = new Vector2(Mathf.Clamp(pos.x, rect.xMin, rect.xMax - 1), yIntl);

                float dist = (intlPos - pos).sqrMagnitude;
                //Check if it's out of range
                if (dist > entity.Range * entity.Range)
                    continue;
                int n = x + y * fowTex.width;
                float fade = 1;
                if (dist > entity.Range)
                    fade = Mathf.Clamp01((entity.Range - Mathf.Sqrt(dist)) / (entity.Range / 2));
                if (entity.IsOwnedByPlayer)
                {
                    pixels[n].g = (byte)Mathf.Max(pixels[n].g, 255 * fade);
                    pixels[n].b = (byte)Mathf.Max(pixels[n].b, 255 * fade);
                }
                else
                    aiVision[n] = (visible.explored | visible.visible);
            }
        }
    }
    /// <summary>
    /// Checks if there is some point of the rectange with visiblity = vis
    /// (Might be a little wonky if the quality is too low)
    /// </summary>
    /// <param name="rect">Rectange in world coords to check</param>
    /// <param name="vis">visible.unexplored: means that a point has been never revealed
    ///                   visible.explored: means that a point has been explored OR is being explored
    ///                   visible.visible: means that a point is currently being revealed </param>
    /// <param name="checkForPlayer">Defaults to true. if true we will check the player visibility map
    ///                                                if false we will check the AI visibility map.</param>
    /// <returns>true if atleast a pixel of the rectangle is in vis state, false otherwise</returns>
    public bool isThereinRect(Rect rect,visible vis, bool askForPlayer=true)
    {
        int xMin, xMax, yMin, yMax;
        getBounds(rect, 0, out xMin, out xMax, out yMin, out yMax);
        for (int x = xMin; x <= xMax; x++)
            for (int y = yMin; y <= yMax; ++y)
            {
                int p = x + y * fowTex.width;
                if (p <= pixels.Length)
                    if (askForPlayer)
                    {
                        if ((vis == visible.explored && pixels[p].g > 0) ||
                           (vis == visible.visible && pixels[p].b > 0 || (NotFullyOpaque && pixels[p].g > 0)) ||
                           (vis == visible.unexplored && pixels[p].b == 0 && pixels[p].g == 0))
                            return true;
                    }else if ((vis & aiVision[p]) == vis)
                        return true;
            }
        return false;
    }
    /// <summary>
    /// Translate a rectangle in word position to texture coords.
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="range">Range to extend the base rectangle</param>
    /// <param name="xMin"> Will be modified to the new value</param>
    /// <param name="xMax"> Will be modified to the new value</param>
    /// <param name="yMin"> Will be modified to the new value</param>
    /// <param name="yMax"> Will be modified to the new value</param>
    private void getBounds(Rect rect, int range, out int xMin, out int xMax, out int yMin, out int yMax)
    {
        xMin = Mathf.RoundToInt(rect.xMin * Quality) - range;
        xMax = Mathf.RoundToInt(rect.xMax * Quality) + range;
        yMin = Mathf.RoundToInt(rect.yMin * Quality - 1) - range;
        yMax = Mathf.RoundToInt(rect.yMax * Quality - 1) + range;
        if (xMin < 0) xMin = 0;
        else if (xMax >= fowTex.width) xMax = fowTex.width - 1;
        if (yMin < 0) yMin = 0;
        else if (yMax >= fowTex.height) yMax = fowTex.height - 1;
        if (xMax < xMin) xMax = xMin;
        if (yMax < yMin) yMax = yMin;
    }
    public static void addEntity(FOWEntity e)
    {
        if (Instance && !Instance.entities.Contains(e))
            Instance.entities.Add(e);
    }

    public static void removeEntity(FOWEntity e)
    {
        if (Instance && Instance.entities.Contains(e))
            Instance.entities.Remove(e);
    }

    static FOWManager _instance;
    static FOWManager Instance
    {
        get
        {
            if (!_instance)
                _instance = GameObject.FindObjectOfType<FOWManager>();
            return _instance;
        }
    }
    [Flags]
    public enum visible{
        unexplored=1, //No one has been near this area
        explored=2,   //Someone revelaed this area and then left
        visible=4    //Someone is currently revealing this area
    }
}
