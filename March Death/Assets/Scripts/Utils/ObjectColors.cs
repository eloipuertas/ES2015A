using UnityEngine;
using System.Collections;

namespace Utils
{
    /// <summary>
    /// Class to alter the view of the object, the idea is to provide a feedback to the user
    /// </summary>
    public class ObjectColors : MonoBehaviour
    {
        private Renderer oldRenderer;
        private Renderer[] mRenderers;
        public enum colors{ RED, GREEN, BLUE};

        void Start(){}

        /// <summary>
        /// Alters the view of the object
        /// NOT WORKING
        /// </summary>
        public void alterColor( colors newColor, float alpha = 250f)
        {
            if(!oldRenderer)
            {
                oldRenderer = GetComponent<Renderer>();
                mRenderers = GetComponentsInChildren<Renderer>();
            }
             

            Color color;

            if (newColor == colors.RED) color = new Color(250f, 0f, 0f, alpha);
            else if (newColor == colors.GREEN) color = new Color(0f, 250f, 0f, alpha);
            else color = new Color(0f, 0f, 250f, alpha);


            
            for (int i = 0; i < mRenderers.Length; i++)
            {
                for (int j = 0; j < mRenderers[i].materials.Length; j++)
                {
                    mRenderers[i].materials[j].color = color;
                    
                }
            }
        }
    }


}