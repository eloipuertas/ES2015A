#ifndef UNITY3D_H
#define UNITY3D_H

#include <stdlib.h>
#include <string.h>
#include <math.h>
#include <fstream>
#include "UnityClasses.h"
#include "DetourCrowd.h"

#if defined(WIN32)
	#define DLL_EXPORT extern "C" __declspec(dllexport)
#elif defined(__GNUC__) && !defined(__clang__)
	#define DLL_EXPORT extern "C"
#else
	#define DLL_EXPORT extern "C"  __attribute__((cdecl))
#endif

// Externs
extern Context* ctx;
extern bool monotonePartitioning;


// Config
DLL_EXPORT int pointerSize();
DLL_EXPORT void setMonotonePartitioning(bool enabled);
DLL_EXPORT rcConfig* DefaultConfig(char* logpath);
DLL_EXPORT void freeTileCache(dtNavMesh* navMesh, dtTileCache* tileCache);

// Build
DLL_EXPORT rcPolyMesh* getPolyMesh();
DLL_EXPORT rcPolyMeshDetail* getPolyMeshDetail();

DLL_EXPORT bool handleBuild(rcConfig* cfg, float* verts, int nverts, int* tris, int ntris);
DLL_EXPORT bool createNavmesh(rcConfig* cfg, rcPolyMesh* pmesh, rcPolyMeshDetail* dmesh, unsigned char*& navData, int& dataSize);


// TileCache
DLL_EXPORT bool handleTileCacheBuild(rcConfig* cfg, ExtendedConfig* ecfg, InputGeometry* geom, dtTileCache*& tileCache, dtNavMesh*& navMesh, dtNavMeshQuery*& navQuery);
DLL_EXPORT void addConvexVolume(float* verts, int nverts, float hmax, float hmin, int area);
DLL_EXPORT void addFlag(unsigned short area, unsigned short cost);
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
DLL_EXPORT void setFilter(dtCrowd* crowd, int filter, unsigned short include, unsigned short exclude);
DLL_EXPORT int addAgent(dtCrowd* crowd, float* p, dtCrowdAgentParams* ap);
DLL_EXPORT dtCrowdAgent* getAgent(dtCrowd* crowd, int idx);
DLL_EXPORT void updateAgent(dtCrowd* crowd, int idx, dtCrowdAgentParams* ap);
DLL_EXPORT void removeAgent(dtCrowd* crowd, int idx);
DLL_EXPORT void setMoveTarget(dtNavMeshQuery* navquery, dtCrowd* crowd, int idx, float* p, bool adjust, int filterIndex);
DLL_EXPORT void resetPath(dtCrowd* crowd, int idx);
DLL_EXPORT void updateTick(dtTileCache* tileCache, dtNavMesh* nav, dtCrowd* crowd, float dt, float* positions, float* velocity, unsigned char* state, unsigned char* targetState, bool* partial, int& nagents);
DLL_EXPORT bool isPointValid(dtCrowd* crowd, float* targetPoint);
DLL_EXPORT bool randomPoint(dtCrowd* crowd, float* targetPoint);
DLL_EXPORT bool randomPointInCircle(dtCrowd* crowd, float* initialPoint, float maxRadius, float* targetPoint);
DLL_EXPORT unsigned int addAreaFlags(dtTileCache* tileCache, dtCrowd* crowd, float* center, float* verts, int nverts, float height, unsigned short int flags);
DLL_EXPORT void removeAreaFlags(dtTileCache* tileCache, dtObstacleRef ref);

#endif
