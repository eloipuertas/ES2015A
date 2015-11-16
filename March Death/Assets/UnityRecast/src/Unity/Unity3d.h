#ifndef UNITY3D_H
#define UNITY3D_H

#include <stdlib.h>
#include <string.h>
#include <math.h>
#include <fstream>
#include "UnityClasses.h"
#include "DetourCrowd.h"

#ifdef WIN32
	#define DLL_EXPORT extern "C" __declspec(dllexport)
#else
	#define DLL_EXPORT extern "C"  __attribute__((cdecl))
#endif

// Externs
extern Context* ctx;
extern bool monotonePartitioning;


// Config
DLL_EXPORT unsigned char pointerSize();
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
DLL_EXPORT bool loadFromTileCacheHeaders(TileCacheSetHeader* header, TileCacheTileHeader* tilesHeader, unsigned char* data, dtTileCache*& tileCache, dtNavMesh*& navMesh, dtNavMeshQuery*& navQuery);

// Class related
DLL_EXPORT dtMeshTile* getTile(dtNavMesh* navmesh, int i);
DLL_EXPORT dtCompressedTile* getTileCacheTile(dtTileCache* tileCache, int i);
DLL_EXPORT dtObstacleRef addObstacle(dtTileCache* tileCache, float* pos, float* verts, int nverts, int height);
DLL_EXPORT void removeObstacle(dtTileCache* tileCache, dtObstacleRef ref);
DLL_EXPORT float* getObstacles(dtTileCache* tc, int& nobstacles);

// Crowd
DLL_EXPORT dtCrowd* createCrowd(int maxAgents, float maxRadius, dtNavMesh* navmesh);
DLL_EXPORT int addAgent(dtCrowd* crowd, const float* p, float radius, float height);
DLL_EXPORT dtCrowdAgent* getAgent(dtCrowd* crowd, int idx);
DLL_EXPORT void updateAgent(dtCrowd* crowd, int idx, float maxAcceleration, float maxSpeed);
DLL_EXPORT void removeAgent(dtCrowd* crowd, int idx);
DLL_EXPORT void setMoveTarget(dtNavMeshQuery* navquery, dtCrowd* crowd, int idx, float* p, bool adjust);
DLL_EXPORT void resetPath(dtCrowd* crowd, int idx);
DLL_EXPORT void updateTick(dtTileCache* tileCache, dtNavMesh* nav, dtCrowd* crowd, float dt, float* positions, float* velocity, unsigned char* state, unsigned char* targetState, int& nagents);

#endif
