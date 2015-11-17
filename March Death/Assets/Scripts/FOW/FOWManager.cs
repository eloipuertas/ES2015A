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
    float fadeRate;
    /// <summary>
    /// Activates 1 frame of X.
    /// if frames = 1 FOWManager will activate every frame
    /// if frames = 3 FOWManager will activate once every 3 frames
    /// </summary>
    int frames;
    int cFrame;
    public bool Enabled;

    List<FOWEntity> entities;
    Texture2D fowTex;
    Color32[] pixels;
    /// <summary>
    /// Dictionary Range,Table
    /// Contains precalculated tables for optimitzation purposes
    /// </summary>
    Dictionary<int, Color32[]> rangeTables;
    /// <summary>
    /// This will contain the grid for the vision of the AI.
    /// This can be only an enum because it doesn't need to be passed to the shader nor does it need to have all the fancy gradient fading the normal FOW has.
    /// </summary>
    public visible[] aiVision { get; set; }

    void Awake()
    {
        frames = 0;
        cFrame = 0;
        fadeRate=200;
        if (Application.isPlaying)
            InitializeTexture();
        entities= new List<FOWEntity>();
        rangeTables = new Dictionary<int, Color32[]>();
    }
    /// <summary>
    /// Creates a new texture the size of terrain
    /// This may fail with an error message if the created texture is too large, or terrain isn't assigned.
    /// </summary>
    void InitializeTexture()
    {
        if(Terrain)
        { 
            int width = Mathf.RoundToInt(Terrain.terrainData.size.x* Quality)+50;
            int height = Mathf.RoundToInt(Terrain.terrainData.size.z * Quality)+50;

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
            Color cc = NotFullyOpaque? new Color(0, 255, 0): Color.black;
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
        if (cFrame == frames)
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
                int fade = Mathf.RoundToInt(Time.deltaTime * fadeRate);

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
                    if (e.IsActor)
                        reveal(e);
                }
                //Hide or show the other entities
                foreach (FOWEntity e in entities)
                    if (e.IsActor)
                    {
                        e.changeVisible(isThereinRect(e.Bounds, visible.visible, !e.IsOwnedByPlayer));
                    }
                    else
                    {
                        e.changeVisible(isThereinRect(e.Bounds, visible.explored));
                    }
                    

                fowTex.SetPixels32(pixels);
                fowTex.Apply();
            }
            cFrame = 0;
        }
        else
        {
            cFrame++;
        }
    }
    /// <summary>
    /// Reveals an area around the entity passed as paramater
    /// </summary>
    /// <param name="entity">Entity which reveals an area</param>
    private void reveal(FOWEntity entity)
    {
        Color32[] table;
        int range = Mathf.RoundToInt(entity.Range*Quality);

        if (!rangeTables.TryGetValue(range,out table))
        {
            table = makeTable(entity.Range);
            rangeTables.Add(range, table);
        }
        Vector2 center = entity.Bounds.center*Quality;
        float xOff = center.x - range;
        float yOff = center.y - range;
        int xCen = Mathf.RoundToInt(xOff);
        int yCen = Mathf.RoundToInt(yOff);
        int texWidth = fowTex.width;
        int dRange = range * 2;
        if (entity.IsOwnedByPlayer)
        {
            Vector2 offset = new Vector2(xOff - xCen, yOff - yCen);
            for (int x = 0; x <= dRange; x++)
            {
                for (int y = 0; y <= dRange; y++)
                {
                    int n = x + y * dRange;
                    int n2 = (xCen+x) + (yCen+y) * texWidth;
                    if (n2 < pixels.Length)
                    {

                        if (table[n].b > 0)
                        {
                            if (table[n].b == 255)
                            {
                                pixels[n2].b = (byte)Mathf.Max(pixels[n2].b, table[n].b);
                                pixels[n2].g = (byte)Mathf.Max(pixels[n2].g, table[n].g);
                            }
                            else
                            {
                                int valOff = Mathf.RoundToInt(offset.x * (x - range) + offset.y * (y - range));
                                pixels[n2].b = (byte)Mathf.Max(pixels[n2].b, Mathf.Min(table[n].b + valOff, 255));
                                pixels[n2].g = (byte)Mathf.Max(pixels[n2].g, Mathf.Min(table[n].g + valOff, 255));
                            }
                        }
                    }
                }
            }
        }
        else
        {
            for (int x = 0; x <= dRange; x++)
            {
                for (int y = 0; y <= dRange; y++)
                {
                    int n = x + y * dRange;
                    int n2 = (xCen + x) + (yCen + y) * texWidth;
                    if (n2 < pixels.Length && table[n].b > 1)
                    {
                        aiVision[n2] = (visible.explored | visible.visible);
                    }
                }
            }
        }
    }
    private Color32[] makeTable(float range)
    {
        //Initialize variables to optimize
        float halfRange = (range / 2);
        float sqrRange = range * range;
        int dRange = Mathf.RoundToInt(2*range*Quality);
        Color32[] table = new Color32[dRange*(dRange+1)+1];
        Vector2 pos;
        Vector2 intlPos = new Vector2(range, range);
        for (int y = 0; y <= dRange; y++)
        {
            for (int x = 0; x <= dRange; x++)
            {
                pos = new Vector2(x, y) / Quality;

                float dist = (intlPos - pos).sqrMagnitude;
                //Check if it's out of range
                if (dist > sqrRange)
                    continue;
                int n = x + y * dRange;
                if (n < table.Length)
                {
                    float fade = 1;
                    if (dist > range)
                        fade = Mathf.Clamp01((range - Mathf.Sqrt(dist)) / halfRange);
                    table[n].g = (byte)Mathf.Max(pixels[n].g, 255 * fade);
                    table[n].b = (byte)Mathf.Max(pixels[n].b, 255 * fade);
                }
            }
        }
        return table;
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
                           (vis == visible.visible && pixels[p].b > 200) ||
                           (vis == visible.unexplored && pixels[p].g == 0))
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
    public void addEntity(FOWEntity e)
    {
        if (!entities.Contains(e))
            entities.Add(e);
    }
    public void removeEntity(FOWEntity e)
    {
        if (entities.Contains(e))
            entities.Remove(e);
    }
    public Vector2 getGridSize()
    {
        return new Vector2(fowTex.width,fowTex.height);
    }
    /// <summary>
    /// Can't return a z coord because I don't really know it.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public Vector2 CoordtoWorld(int x, int y)
    {
        return new Vector2(x/Quality,y/Quality);
    }
    public Vector2 CoordtoGrid(Vector3 coord)
    {
        int x = Mathf.RoundToInt(coord.x * Quality);
        int y = Mathf.RoundToInt(coord.z * Quality - 1);
        if (x < 0) x = 0;
        else if (x >= fowTex.width) x = fowTex.width - 1;
        if (y < 0) y = 0;
        else if (y >= fowTex.height) y = fowTex.height - 1;
        return new Vector2(x, y);
    }
    static FOWManager _instance;
    public static FOWManager Instance
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
