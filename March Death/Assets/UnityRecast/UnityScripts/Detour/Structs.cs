using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pathfinding
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MeshTile
    {
        public uint salt;

        public uint linksFreeList;
        public IntPtr header;
        public IntPtr polys;
        public IntPtr verts;
        public IntPtr links;
        public IntPtr detailMeshes;

        public IntPtr detailVerts;
        public IntPtr detailTris;

        public IntPtr bvTree;
        public IntPtr offMeshCons;

        public IntPtr data;
        public int dataSize;
        public int flags;
        public IntPtr next;
    }

    public struct TileCacheLayerHeader
    {
        public int magic;                              ///< Data magic
        public int version;                            ///< Data version
        public int tx, ty, tlayer;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] bmin;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] bmax;
        public ushort hmin, hmax;              ///< Height min/max range
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte width, height;            ///< Dimension of the layer.
        public byte minx, maxx, miny, maxy;   ///< Usable sub-region.
    };

    struct CompressedTile
    {
        public uint salt;                      ///< Counter describing modifications to the tile.
        public IntPtr header;
        public IntPtr compressed;
        public int compressedSize;
        public IntPtr data;
        public int dataSize;
        public uint flags;
        public IntPtr next;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct MeshHeader
    {
        public int magic;              ///< Tile magic number. (Used to identify the data format.)
        public int version;            ///< Tile data format version number.
        public int x;                  ///< The x-position of the tile within the dtNavMesh tile grid. (x, y, layer)
        public int y;                  ///< The y-position of the tile within the dtNavMesh tile grid. (x, y, layer)
        public int layer;              ///< The layer of the tile within the dtNavMesh tile grid. (x, y, layer)
        public uint userId;            ///< The user defined id of the tile.
        public int polyCount;          ///< The number of polygons in the tile.
        public int vertCount;          ///< The number of vertices in the tile.
        public int maxLinkCount;       ///< The number of allocated links.
        public int detailMeshCount;    ///< The number of sub-meshes in the detail mesh.

        /// The number of unique vertices in the detail mesh. (In addition to the polygon vertices.)
        public int detailVertCount;

        public int detailTriCount;         ///< The number of triangles in the detail mesh.
        public int bvNodeCount;            ///< The number of bounding volume nodes. (Zero if bounding volumes are disabled.)
        public int offMeshConCount;        ///< The number of off-mesh connections.
        public int offMeshBase;            ///< The index of the first polygon which is an off-mesh connection.
        public float walkableHeight;       ///< The height of the agents using the tile.
        public float walkableRadius;       ///< The radius of the agents using the tile.
        public float walkableClimb;        ///< The maximum climb height of the agents using the tile.
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] bmin;              ///< The minimum bounds of the tile's AABB. [(x, y, z)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] bmax;              ///< The maximum bounds of the tile's AABB. [(x, y, z)]

        /// The bounding volume quantization factor. 
        public float bvQuantFactor;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Poly
    {
        /// Index to first link in linked list. (Or #DT_NULL_LINK if there is no link.)
        public uint firstLink;

        /// The indices of the polygon's vertices.
        /// The actual vertices are located in dtMeshTile::verts.
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public ushort[] verts;

        /// Packed data representing neighbor polygons references and flags for each edge.
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public ushort[] neis;

        /// The user defined polygon flags.
        public ushort flags;

        /// The number of vertices in the polygon.
        public byte vertCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PolyDetail
    {
        public uint vertBase;          ///< The offset of the vertices in the dtMeshTile::detailVerts array.
        public uint triBase;           ///< The offset of the triangles in the dtMeshTile::detailTris array.
        public byte vertCount;        ///< The number of vertices in the sub-mesh.
        public byte triCount;         ///< The number of triangles in the sub-mesh.
    };
}
