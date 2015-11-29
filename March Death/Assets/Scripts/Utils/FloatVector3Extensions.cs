using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Utils
{
    public static class FloatVector3Extensions
    {
        public static float[] ToFloat(this Vector3 p)
        {
            return new float[] { p.x, p.y, p.z };
        }

        public static Vector3 ToVector3(this float[] p, int off = 0)
        {
            return new Vector3(p[off + 0], p[off + 1], p[off + 2]);
        }
    }
}
