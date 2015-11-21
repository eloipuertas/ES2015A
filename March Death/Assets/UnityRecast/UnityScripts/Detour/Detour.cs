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
            [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr createCrowd(int maxAgents, float maxRadius, IntPtr navmesh);

            [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
            public static extern int addAgent(IntPtr crowd, float[] p, ref CrowdAgentParams ap);

            [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr getAgent(IntPtr crowd, int idx);

            [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
            public static extern void updateAgent(IntPtr crowd, int idx, ref CrowdAgentParams ap);

            [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
            public static extern void removeAgent(IntPtr crowd, int idx);

            [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
            public static extern void setMoveTarget(IntPtr navquery, IntPtr crowd, int idx, float[] p, bool adjust);

            [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
            public static extern void resetPath(IntPtr crowd, int idx);

            [DllImport("Recast", CallingConvention = CallingConvention.Cdecl)]
            public static extern void updateTick(IntPtr tileCache, IntPtr nav, IntPtr crowd, float dt, float[] positions, float[] velocities, byte[] states, byte[] targetStates, ref int nagents);
        }
    }
}
