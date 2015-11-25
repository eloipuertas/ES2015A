using System;
using System.Collections.Generic;
using UnityEngine;
namespace Utils
{
    class Layer : Singleton<Layer>
    {
        public enum Layers
        {
            WATER = 4, 
            UI = 5,
            MINIMAP = 8,
            TERRAIN = 9,
            UNIT = 10,
        }

        private Dictionary<Layers, LayerMask> _layersMasks;
        private LayerMask _terrainLayer;
        private LayerMask _unitLayer;
        private RaycastHit _hit;

        private Layer()
        {
            Config();
        }


        /// <summary>
        /// Load layers dynamically
        /// </summary>
        private void Config()
        {
            _hit = new RaycastHit();
            _layersMasks = new Dictionary<Layers, LayerMask>();

            for (int i = 0; i < 32; i++)
            {
                if (Enum.IsDefined(typeof(Layers), i))
                {
                    _layersMasks.Add((Layers)i, 1 << i);
                }
            }
        }

        /// <summary>
        /// Checks if there is terrain (almost always true)
        /// </summary>
        /// <returns></returns>
        public bool IsTerrain()
        {
            if (IsLayer(_layersMasks[Layers.TERRAIN]))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if there is unit
        /// </summary>
        /// <returns></returns>
        public bool IsUnit()
        {
            if (IsLayer(_layersMasks[Layers.UNIT]))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Checks if there is minimap
        /// </summary>
        /// <returns></returns>
        public bool IsMiniMap()
        {
            if (IsLayer(_layersMasks[Layers.MINIMAP]))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Checks if there is water
        /// </summary>
        /// <returns></returns>
        public bool IsWater()
        {
            if (IsLayer(_layersMasks[Layers.UNIT]))
            {
                Debug.Log("Is water");
                return true;
            }
            else
            {
                Debug.Log("Isn't water");
                return false;
            }

        }

        /// <summary>
        /// Returns the point in the specified layer, Vector3(0,0,0) if there isn't the specified layer there
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public Vector3 PointIn(Layers layer)
        {
            Ray _ray = Camera.main.ScreenPointToRay(MousePosition());
            if (Physics.Raycast(_ray, out _hit, Mathf.Infinity, _layersMasks[layer]))
                return _hit.point;
            else
                return new Vector3(0,0,0);
        }

        //Private stuff
        
        private Vector3 MousePosition() {
            return Input.mousePosition;
        }

        
        private bool IsLayer(LayerMask layer)
        {
            Ray _ray = Camera.main.ScreenPointToRay(MousePosition());
            return IsLayer(layer, _ray);
            
        }

        private bool IsLayer(LayerMask layer, Ray ray)
        {
            if (Physics.Raycast(ray, out _hit, Mathf.Infinity, layer))
                return true;
            else
                return false;
        }
    }
}
