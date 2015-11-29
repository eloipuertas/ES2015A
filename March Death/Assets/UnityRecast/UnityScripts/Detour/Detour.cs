using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pathfinding
{
    public static class Detour
    {
        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern int pointerSize();

        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool createNavmesh(ref Config cfg, ref PolyMesh pmesh, ref PolyMeshDetail dmesh, ref IntPtr navData, ref int dataSize);

        [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getTile(IntPtr navmesh, int i);

        public static class Query
        {

        }

        public static class Crowd
        {
            
        }
    }
}
