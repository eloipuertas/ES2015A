#include <malloc.h>
#include <string.h>
#include <math.h>
#include <fstream>
#include "UnityClasses.h"

#ifdef WIN32
	#define DLL_EXPORT extern "C" __declspec(dllexport)
#else
	#define DLL_EXPORT extern "C"
#endif

// Externs
extern Context* ctx;
extern bool monotonePartitioning;


// Config
DLL_EXPORT void setMonotonePartitioning(bool enabled);
DLL_EXPORT rcConfig* DefaultConfig(char* logpath);

// Build
DLL_EXPORT rcPolyMesh* getPolyMesh();
DLL_EXPORT rcPolyMeshDetail* getPolyMeshDetail();

DLL_EXPORT bool handleBuild(rcConfig* cfg, float* verts, int nverts, int* tris, int ntris);
DLL_EXPORT bool createNavmesh(rcConfig* cfg, rcPolyMesh* pmesh, rcPolyMeshDetail* dmesh, unsigned char*& navData, int& dataSize);


// TileCache
DLL_EXPORT bool handleTileCacheBuild(rcConfig* cfg, ExtendedConfig* ecfg, InputGeometry* geom, dtTileCache*& tileCache, dtNavMesh*& navMesh, dtNavMeshQuery*& navQuery);
DLL_EXPORT void getTileCacheHeaders(TileCacheSetHeader& header, TileCacheTileHeader*& tilesHeader, dtTileCache* tileCache, dtNavMesh* navMesh);
DLL_EXPORT bool loadFromTileCacheHeaders(TileCacheSetHeader header, TileCacheTileHeader* tilesHeader, unsigned char* data, dtTileCache*& tileCache, dtNavMesh*& navMesh);

// Class related
DLL_EXPORT dtCompressedTile* getTileCacheTile(dtTileCache* tileCache, int i);
DLL_EXPORT dtMeshTile* getTile(dtNavMesh* navmesh, int i);
