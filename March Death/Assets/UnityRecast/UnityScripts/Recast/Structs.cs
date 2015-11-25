using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pathfinding
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Config
    {
        public int width;
        public int height;
        public int tileSize;
        public int borderSize;
        public float cs;
        public float ch;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] bmin;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] bmax;
        public float walkableSlopeAngle;
        public int walkableHeight;
        public int walkableClimb;
        public int walkableRadius;
        public int maxEdgeLen;
        public float maxSimplificationError;
        public int minRegionArea;
        public int mergeRegionArea;
        public int maxVertsPerPoly;
        public float detailSampleDist;
        public float detailSampleMaxError;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct PolyMesh
    {
        public IntPtr verts;  ///< The mesh vertices. [Form: (x, y, z) * #nverts]
        public IntPtr polys;  ///< Polygon and neighbor data. [Length: #maxpolys * 2 * #nvp]
        public IntPtr regs;   ///< The region id assigned to each polygon. [Length: #maxpolys]
        public IntPtr flags;  ///< The user defined flags for each polygon. [Length: #maxpolys]
        public IntPtr areas;   ///< The area id assigned to each polygon. [Length: #maxpolys]
        public int nverts;             ///< The number of vertices.
        public int npolys;             ///< The number of polygons.
        public int maxpolys;           ///< The number of allocated polygons.
        public int nvp;                ///< The maximum number of vertices per polygon.
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = 3)]
        public float[] bmin;          ///< The minimum bounds in world space. [(x, y, z)]
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R4, SizeConst = 3)]
        public float[] bmax;          ///< The maximum bounds in world space. [(x, y, z)]
        public float cs;               ///< The size of each cell. (On the xz-plane.)
        public float ch;               ///< The height of each cell. (The minimum increment along the y-axis.)
        public int borderSize;          ///< The AABB border size used to generate the source data from which the mesh was derived.
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PolyMeshDetail
    {
        public IntPtr meshes;
        public IntPtr verts;
        public IntPtr tris;
        public int nmeshes;
        public int nverts;
        public int ntris;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ExtendedConfig
    {
        public float AgentHeight;
        public float AgentRadius;
        public float AgentMaxClimb;
        public int MaxObstacles;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct InputGeometry
    {
        public IntPtr verts;
        public int nverts;
        public IntPtr tris;
        public int ntris;
    };


    [StructLayout(LayoutKind.Sequential)]
    public struct TileCacheHolder
    {
        public Config cfg;
        public ExtendedConfig ecfg;

        public IntPtr allocator;
        public IntPtr compressor;
        public IntPtr processor;

        public InputGeometry geom;
        public IntPtr chunkyMesh;

        public IntPtr navMesh;
        public IntPtr navQuery;
    }

}
