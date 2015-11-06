using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pathfinding
{
    public static class Recast
    {
        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr DefaultConfig(string logPath);

        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool handleBuild(ref Config cfg, float[] verts, int nverts, int[] tris, int ntris);

        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getPolyMesh();

        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getPolyMeshDetail();
    }
}
