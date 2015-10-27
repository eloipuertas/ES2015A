using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Storage
{
    /// <summary>
    /// TODO future use, meeting point for new civilians
    /// </summary>
    class Spot
    {
        /// <summary>
        /// X, Y, Z coordinates of the spot at map
        /// </summary>
        /// 
        public Vector3 position { get;set; }
        /// <summary>
        /// Rotation of the spot at map
        /// </summary>
        private Quaternion rotation { get;set; }

    public Spot() { }
    }
}
