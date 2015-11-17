#include "Unity3d.h"

static const int EXPECTED_LAYERS_PER_TILE = 4;
static const int TILECACHESET_MAGIC = 'T' << 24 | 'S' << 16 | 'E' << 8 | 'T'; //'TSET';
static const int TILECACHESET_VERSION = 1;

LinearAllocator* allocator = new LinearAllocator(5*1024*1024);
FastLZCompressor* compressor = new FastLZCompressor();
MeshProcess* processor = new MeshProcess();


static int rasterizeTileLayers(Context* ctx, TileCacheHolder* holder,
	const int tx, const int ty,
	const rcConfig* cfg,
	TileCacheData* tiles,
	const int maxTiles)
{
	FastLZCompressor comp;
	RasterizationContext rc;

	const float* verts = holder->geom->verts;
	const int nverts = holder->geom->nverts;
	const rcChunkyTriMesh* chunkyMesh = holder->chunkyMesh;

	// Tile bounds.
	const float tcs = cfg->tileSize * cfg->cs;

	rcConfig tcfg;
	memcpy(&tcfg, cfg, sizeof(tcfg));

	tcfg.bmin[0] = cfg->bmin[0] + tx*tcs;
	tcfg.bmin[1] = cfg->bmin[1];
	tcfg.bmin[2] = cfg->bmin[2] + ty*tcs;
	tcfg.bmax[0] = cfg->bmin[0] + (tx + 1)*tcs;
	tcfg.bmax[1] = cfg->bmax[1];
	tcfg.bmax[2] = cfg->bmin[2] + (ty + 1)*tcs;
	tcfg.bmin[0] -= tcfg.borderSize*tcfg.cs;
	tcfg.bmin[2] -= tcfg.borderSize*tcfg.cs;
	tcfg.bmax[0] += tcfg.borderSize*tcfg.cs;
	tcfg.bmax[2] += tcfg.borderSize*tcfg.cs;

	// Allocate voxel heightfield where we rasterize our input data to.
	rc.solid = rcAllocHeightfield();
	if (!rc.solid)
	{
		ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'solid'.");
		return 0;
	}
	if (!rcCreateHeightfield(ctx, *rc.solid, tcfg.width, tcfg.height, tcfg.bmin, tcfg.bmax, tcfg.cs, tcfg.ch))
	{
		ctx->log(RC_LOG_ERROR, "buildNavigation: Could not create solid heightfield.");
		return 0;
	}

	// Allocate array that can hold triangle flags.
	// If you have multiple meshes you need to process, allocate
	// and array which can hold the max number of triangles you need to process.
	rc.triareas = new unsigned char[chunkyMesh->maxTrisPerChunk];
	if (!rc.triareas)
	{
		ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'm_triareas' (%d).", chunkyMesh->maxTrisPerChunk);
		return 0;
	}

	float tbmin[2], tbmax[2];
	tbmin[0] = tcfg.bmin[0];
	tbmin[1] = tcfg.bmin[2];
	tbmax[0] = tcfg.bmax[0];
	tbmax[1] = tcfg.bmax[2];
	int cid[512];// TODO: Make grow when returning too many items.
	const int ncid = rcGetChunksOverlappingRect(chunkyMesh, tbmin, tbmax, cid, 512);
	if (!ncid)
	{
		return 0; // empty
	}

	for (int i = 0; i < ncid; ++i)
	{
		const rcChunkyTriMeshNode& node = chunkyMesh->nodes[cid[i]];
		const int* tris = &chunkyMesh->tris[node.i * 3];
		const int ntris = node.n;

		memset(rc.triareas, 0, ntris*sizeof(unsigned char));
		rcMarkWalkableTriangles(ctx, tcfg.walkableSlopeAngle,
			verts, nverts, tris, ntris, rc.triareas);
		
		rcRasterizeTriangles(ctx, verts, nverts, tris, rc.triareas, ntris, *rc.solid, tcfg.walkableClimb);
	}

	// Once all geometry is rasterized, we do initial pass of filtering to
	// remove unwanted overhangs caused by the conservative rasterization
	// as well as filter spans where the character cannot possibly stand.
	rcFilterLowHangingWalkableObstacles(ctx, tcfg.walkableClimb, *rc.solid);
	rcFilterLedgeSpans(ctx, tcfg.walkableHeight, tcfg.walkableClimb, *rc.solid);
	rcFilterWalkableLowHeightSpans(ctx, tcfg.walkableHeight, *rc.solid);


	rc.chf = rcAllocCompactHeightfield();
	if (!rc.chf)
	{
		ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'chf'.");
		return 0;
	}
	if (!rcBuildCompactHeightfield(ctx, tcfg.walkableHeight, tcfg.walkableClimb, *rc.solid, *rc.chf))
	{
		ctx->log(RC_LOG_ERROR, "buildNavigation: Could not build compact data.");
		return 0;
	}

	// Erode the walkable area by agent radius.
	if (!rcErodeWalkableArea(ctx, tcfg.walkableRadius, *rc.chf))
	{
		ctx->log(RC_LOG_ERROR, "buildNavigation: Could not erode.");
		return 0;
	}

	// (Optional) Mark areas.
	/*
	const ConvexVolume* vols = geom->getConvexVolumes();
	for (int i = 0; i < geom->getConvexVolumeCount(); ++i)
	{
		rcMarkConvexPolyArea(ctx, vols[i].verts, vols[i].nverts,
			vols[i].hmin, vols[i].hmax,
			(unsigned char)vols[i].area, *rc.chf);
	}
	*/

	rc.lset = rcAllocHeightfieldLayerSet();
	if (!rc.lset)
	{
		ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'lset'.");
		return 0;
	}
	if (!rcBuildHeightfieldLayers(ctx, *rc.chf, tcfg.borderSize, tcfg.walkableHeight, *rc.lset))
	{
		ctx->log(RC_LOG_ERROR, "buildNavigation: Could not build heighfield layers.");
		return 0;
	}

	rc.ntiles = 0;
	for (int i = 0; i < rcMin(rc.lset->nlayers, MAX_LAYERS); ++i)
	{
		TileCacheData* tile = &rc.tiles[rc.ntiles++];
		const rcHeightfieldLayer* layer = &rc.lset->layers[i];

		// Store header
		dtTileCacheLayerHeader header;
		header.magic = DT_TILECACHE_MAGIC;
		header.version = DT_TILECACHE_VERSION;

		// Tile layer location in the navmesh.
		header.tx = tx;
		header.ty = ty;
		header.tlayer = i;
		dtVcopy(header.bmin, layer->bmin);
		dtVcopy(header.bmax, layer->bmax);

		// Tile info.
		header.width = (unsigned char)layer->width;
		header.height = (unsigned char)layer->height;
		header.minx = (unsigned char)layer->minx;
		header.maxx = (unsigned char)layer->maxx;
		header.miny = (unsigned char)layer->miny;
		header.maxy = (unsigned char)layer->maxy;
		header.hmin = (unsigned short)layer->hmin;
		header.hmax = (unsigned short)layer->hmax;

		dtStatus status = dtBuildTileCacheLayer(&comp, &header, layer->heights, layer->areas, layer->cons,
			&tile->data, &tile->dataSize);
		if (dtStatusFailed(status))
		{
			return 0;
		}
	}

	// Transfer ownsership of tile data from build context to the caller.
	int n = 0;
	for (int i = 0; i < rcMin(rc.ntiles, maxTiles); ++i)
	{
		tiles[n++] = rc.tiles[i];
		rc.tiles[i].data = 0;
		rc.tiles[i].dataSize = 0;
	}

	return n;
}

bool handleTileCacheBuild(rcConfig* cfg, ExtendedConfig* ecfg, InputGeometry* geom, 
	dtTileCache*& tileCache, dtNavMesh*& navMesh, dtNavMeshQuery*& navQuery)
{
	TileCacheHolder* holder = new TileCacheHolder();

	holder->cfg = cfg;
	holder->ecfg = ecfg;
	holder->geom = geom;

	navQuery = dtAllocNavMeshQuery();

	float bmin[3];
	float bmax[3];
	rcCalcBounds(geom->verts, geom->nverts / 3, bmin, bmax);

	holder->chunkyMesh = new rcChunkyTriMesh;
	if (!holder->chunkyMesh)
	{
		ctx->log(RC_LOG_ERROR, "buildTiledNavigation: Out of memory 'm_chunkyMesh'.");
		return false;
	}
	if (!rcCreateChunkyTriMesh(geom->verts, geom->tris, geom->ntris, 256, holder->chunkyMesh))
	{
		ctx->log(RC_LOG_ERROR, "buildTiledNavigation: Failed to build chunky mesh.");
		return false;
	}

	//m_tmproc->init(m_geom);

	// Init cache
	int gw = 0, gh = 0;
	rcCalcGridSize(bmin, bmax, cfg->cs, &gw, &gh);
	const int ts = cfg->tileSize;
	const int tw = (gw + ts - 1) / ts;
	const int th = (gh + ts - 1) / ts;

	int tileBits = rcMin((int)dtIlog2(dtNextPow2(tw*th*EXPECTED_LAYERS_PER_TILE)), 14);
	if (tileBits > 14) tileBits = 14;
	int polyBits = 22 - tileBits;
	int maxTiles = 1 << tileBits;
	int maxPolysPerTile = 1 << polyBits;

	// Generation params.
	cfg->borderSize = cfg->walkableRadius + 3; // Reserve enough padding.
	cfg->width = cfg->tileSize + cfg->borderSize * 2;
	cfg->height = cfg->tileSize + cfg->borderSize * 2;

	rcVcopy(cfg->bmin, bmin);
	rcVcopy(cfg->bmax, bmax);

	// Tile cache params.
	dtTileCacheParams tcparams;
	memset(&tcparams, 0, sizeof(tcparams));
	rcVcopy(tcparams.orig, bmin);
	tcparams.cs = cfg->cs;
	tcparams.ch = cfg->ch;
	tcparams.width = cfg->tileSize;
	tcparams.height = cfg->tileSize;
	tcparams.walkableHeight = ecfg->AgentHeight;
	tcparams.walkableRadius = ecfg->AgentRadius;
	tcparams.walkableClimb = ecfg->AgentMaxClimb;
	tcparams.maxSimplificationError = cfg->maxSimplificationError;
	tcparams.maxTiles = tw * th * EXPECTED_LAYERS_PER_TILE;
	tcparams.maxObstacles = 128;

	tileCache = dtAllocTileCache();
	if (!tileCache)
	{
		ctx->log(RC_LOG_ERROR, "buildTiledNavigation: Could not allocate tile cache.");
		return false;
	}

	dtStatus status = tileCache->init(&tcparams, allocator, compressor, processor);
	if (dtStatusFailed(status))
	{
		ctx->log(RC_LOG_ERROR, "buildTiledNavigation: Could not init tile cache.");
		return false;
	}

	navMesh = dtAllocNavMesh();
	if (!navMesh)
	{
		ctx->log(RC_LOG_ERROR, "buildTiledNavigation: Could not allocate navmesh.");
		return false;
	}

	dtNavMeshParams params;
	memset(&params, 0, sizeof(params));
	rcVcopy(params.orig, bmin);
	params.tileWidth = cfg->tileSize * cfg->cs;
	params.tileHeight = cfg->tileSize * cfg->cs;
	params.maxTiles = maxTiles;
	params.maxPolys = maxPolysPerTile;

	status = navMesh->init(&params);
	if (dtStatusFailed(status))
	{
		ctx->log(RC_LOG_ERROR, "buildTiledNavigation: Could not init navmesh.");
		return false;
	}

	status = navQuery->init(navMesh, 2048);
	if (dtStatusFailed(status))
	{
		ctx->log(RC_LOG_ERROR, "buildTiledNavigation: Could not init Detour navmesh query");
		return false;
	}

	// Preprocess tiles.

	ctx->resetTimers();

	int cacheLayerCount = 0;
	int cacheCompressedSize = 0;
	int cacheRawSize = 0;

	for (int y = 0; y < th; ++y)
	{
		for (int x = 0; x < tw; ++x)
		{
			TileCacheData tiles[MAX_LAYERS];
			memset(tiles, 0, sizeof(tiles));
			int ntiles = rasterizeTileLayers(ctx, holder, x, y, cfg, tiles, MAX_LAYERS);

			for (int i = 0; i < ntiles; ++i)
			{
				TileCacheData* tile = &tiles[i];
				status = tileCache->addTile(tile->data, tile->dataSize, DT_COMPRESSEDTILE_FREE_DATA, 0);
				if (dtStatusFailed(status))
				{
					dtFree(tile->data);
					tile->data = 0;
					continue;
				}

				cacheLayerCount++;
				cacheCompressedSize += tile->dataSize;
				//cacheRawSize += calcLayerBufferSize(tcparams.width, tcparams.height);
			}
		}
	}

	// Build initial meshes
	ctx->startTimer(RC_TIMER_TOTAL);
	for (int y = 0; y < th; ++y)
		for (int x = 0; x < tw; ++x)
			tileCache->buildNavMeshTilesAt(x, y, navMesh);
	ctx->stopTimer(RC_TIMER_TOTAL);

	return true;
}

void getTileCacheHeaders(TileCacheSetHeader& header, TileCacheTileHeader*& tilesHeader, dtTileCache* tileCache, dtNavMesh* navMesh)
{
	// Store header.
	header.magic = TILECACHESET_MAGIC;
	header.version = TILECACHESET_VERSION;
	header.numTiles = 0;
	for (int i = 0; i < tileCache->getTileCount(); ++i)
	{
		const dtCompressedTile* tile = tileCache->getTile(i);
		if (!tile || !tile->header || !tile->dataSize) continue;
		header.numTiles++;
	}
	memcpy(&header.cacheParams, tileCache->getParams(), sizeof(dtTileCacheParams));
	memcpy(&header.meshParams, navMesh->getParams(), sizeof(dtNavMeshParams));
	
	// Allocate memory
	int n = 0;
	tilesHeader = new TileCacheTileHeader[header.numTiles];

	// Store tiles.
	for (int i = 0; i < tileCache->getTileCount(); ++i)
	{
		const dtCompressedTile* tile = tileCache->getTile(i);
		if (!tile || !tile->header || !tile->dataSize) continue;

		tilesHeader[n].tileRef = tileCache->getTileRef(tile);
		tilesHeader[n].dataSize = tile->dataSize;
		++n;
	}
}

bool loadFromTileCacheHeaders(TileCacheSetHeader* header, TileCacheTileHeader* tilesHeader, unsigned char* data, dtTileCache*& tileCache, dtNavMesh*& navMesh, dtNavMeshQuery*& navQuery)
{
	if (header->magic != TILECACHESET_MAGIC)
	{
		ctx->log(RC_LOG_ERROR, "FAILED MAGIC");
		return false;
	}
	if (header->version != TILECACHESET_VERSION)
	{
		ctx->log(RC_LOG_ERROR, "FAILED VERSION");
		return false;
	}

	navMesh = dtAllocNavMesh();
	if (!navMesh)
	{
		ctx->log(RC_LOG_ERROR, "FAILED dtAllocNavMesh");
		return false;
	}

	dtStatus status = navMesh->init(&header->meshParams);
	if (dtStatusFailed(status))
	{
		ctx->log(RC_LOG_ERROR, "FAILED navMesh->init");
		return false;
	}

	tileCache = dtAllocTileCache();
	if (!tileCache)
	{
		ctx->log(RC_LOG_ERROR, "FAILED dtAllocTileCache");
		return false;
	}
	status = tileCache->init(&header->cacheParams, allocator, compressor, processor);
	if (dtStatusFailed(status))
	{
		ctx->log(RC_LOG_ERROR, "FAILED tileCache->init");
		return false;
	}

	ctx->log(RC_LOG_ERROR, "We have %d tiles", header->numTiles);

	// Read tiles.
	int n = 0;
	int start = 0;
	for (int i = 0; i < header->numTiles; ++i)
	{
		TileCacheTileHeader& tileHeader = tilesHeader[n++];
		if (!tileHeader.tileRef || !tileHeader.dataSize)
			break;

		dtCompressedTileRef tile = 0;
		tileCache->addTile(data + start, tileHeader.dataSize, DT_COMPRESSEDTILE_FREE_DATA, &tile);
		start += tileHeader.dataSize;

		dtStatus status = DT_FAILURE;
		if (tile)
			status = tileCache->buildNavMeshTile(tile, navMesh);

		if (dtStatusFailed(status))
		{
			ctx->log(RC_LOG_ERROR, "FAILED BUILDING TILE %d [%x] [[%lld]]", i, status, status);
		}
	}

	navQuery = dtAllocNavMeshQuery();
	status = navQuery->init(navMesh, 2048);
	if (dtStatusFailed(status))
	{
		ctx->log(RC_LOG_ERROR, "buildTiledNavigation: Could not init Detour navmesh query");
		return false;
	}

	return true;
}

dtCompressedTile* getTileCacheTile(dtTileCache* tileCache, int i)
{
	return (dtCompressedTile*)tileCache->getTile(i);
}

dtMeshTile* getTile(dtNavMesh* navmesh, int i)
{
	return (dtMeshTile*)navmesh->getTileIdx(i);
}

dtObstacleRef addObstacle(dtTileCache* tileCache, float* pos, float* verts, int nverts, int height)
{
	for (int i = 0; i < nverts; ++i)
	{
		ctx->log(RC_LOG_ERROR, "Vert: (%.3f, %.3f, %.3f)", verts[i * 3 + 0], verts[i * 3 + 1], verts[i * 3 + 2]);
	}

	dtObstacleRef ref;
	tileCache->addObstacle(pos, verts, nverts, height, &ref);
	return ref;
}

void removeObstacle(dtTileCache* tileCache, dtObstacleRef ref)
{
	tileCache->removeObstacle(ref);
}

float* getObstacles(dtTileCache* tc, int& nobstacles)
{
	nobstacles = tc->getObstacleCount();

	if (nobstacles > 0)
	{
		float* vertices = new float[nobstacles * 6];

		// Draw obstacles
		for (int i = 0; i < nobstacles; ++i)
		{
			const dtTileCacheObstacle* ob = tc->getObstacle(i);
			if (ob->state == DT_OBSTACLE_EMPTY) continue;
			tc->getObstacleBounds(ob, &vertices[i * 6], &vertices[i * 6 + 3]);

			float bmin[3], bmax[3];
			tc->getObstacleBounds(ob, bmin, bmax);
			ctx->log(RC_LOG_ERROR, "BMIN: (%.3f, %.3f, %.3f)", bmin[0], bmin[1], bmin[2]);
			ctx->log(RC_LOG_ERROR, "BMAX: (%.3f, %.3f, %.3f)", bmax[0], bmax[1], bmax[2]);
		}

		return vertices;
	}

	return NULL;
}

dtCrowd* createCrowd(int maxAgents, float maxRadius, dtNavMesh* navmesh)
{
	dtCrowd* crowd = dtAllocCrowd();
	crowd->init(maxAgents, maxRadius, navmesh);
	crowd->getEditableFilter(0)->setIncludeFlags(SAMPLE_POLYFLAGS_WALK);

	dtObstacleAvoidanceParams params;
	// Use mostly default settings, copy from dtCrowd.
	memcpy(&params, crowd->getObstacleAvoidanceParams(0), sizeof(dtObstacleAvoidanceParams));

	// Low (11)
	params.velBias = 0.5f;
	params.adaptiveDivs = 5;
	params.adaptiveRings = 2;
	params.adaptiveDepth = 1;
	crowd->setObstacleAvoidanceParams(0, &params);

	// Medium (22)
	params.velBias = 0.5f;
	params.adaptiveDivs = 5;
	params.adaptiveRings = 2;
	params.adaptiveDepth = 2;
	crowd->setObstacleAvoidanceParams(1, &params);

	// Good (45)
	params.velBias = 0.5f;
	params.adaptiveDivs = 7;
	params.adaptiveRings = 2;
	params.adaptiveDepth = 3;
	crowd->setObstacleAvoidanceParams(2, &params);

	// High (66)
	params.velBias = 0.5f;
	params.adaptiveDivs = 7;
	params.adaptiveRings = 3;
	params.adaptiveDepth = 3;

	crowd->setObstacleAvoidanceParams(3, &params);
	return crowd;
}

int addAgent(dtCrowd* crowd, float* p, float radius, float height)
{
	dtCrowdAgentParams ap;
	memset(&ap, 0, sizeof(ap));
	ap.radius = radius;
	ap.height = height;
	ap.maxAcceleration = 8.0f;
	ap.maxSpeed = 3.5f;
	ap.collisionQueryRange = ap.radius * 12.0f;
	ap.pathOptimizationRange = ap.radius * 30.0f;
	ap.updateFlags = 0;

	//if (m_toolParams.m_anticipateTurns)
		ap.updateFlags |= DT_CROWD_ANTICIPATE_TURNS;
	//if (m_toolParams.m_optimizeVis)
		ap.updateFlags |= DT_CROWD_OPTIMIZE_VIS;
	//if (m_toolParams.m_optimizeTopo)
		ap.updateFlags |= DT_CROWD_OPTIMIZE_TOPO;
	//if (m_toolParams.m_obstacleAvoidance)
		ap.updateFlags |= DT_CROWD_OBSTACLE_AVOIDANCE;
	//if (m_toolParams.m_separation)
		ap.updateFlags |= DT_CROWD_SEPARATION;

	ap.obstacleAvoidanceType = (unsigned char)2;
	ap.separationWeight = 1;
	int r =  crowd->addAgent(p, &ap);
	return r;
}

dtCrowdAgent* getAgent(dtCrowd* crowd, int idx)
{
	return (dtCrowdAgent*)crowd->getAgent(idx);
}

void updateAgent(dtCrowd* crowd, int idx, float maxAcceleration, float maxSpeed)
{
	dtCrowdAgentParams ap = crowd->getAgent(idx)->params;
	ap.maxAcceleration = maxAcceleration;
	ap.maxSpeed = maxSpeed;
	crowd->updateAgentParameters(idx, &ap);
}

void removeAgent(dtCrowd* crowd, int idx)
{
	crowd->removeAgent(idx);
}

static void calcVel(float* vel, const float* pos, const float* tgt, const float speed)
{
	dtVsub(vel, tgt, pos);
	vel[1] = 0.0;
	dtVnormalize(vel);
	dtVscale(vel, vel, speed);
}

void setMoveTarget(dtNavMeshQuery* navquery, dtCrowd* crowd, int idx, float* p, bool adjust)
{
	// Find nearest point on navmesh and set move request to that location.
	const dtQueryFilter* filter = crowd->getFilter(0);
	const float* ext = crowd->getQueryExtents();

	if (adjust)
	{
		float vel[3];
		// Request velocity
		if (idx != -1)
		{
			const dtCrowdAgent* ag = crowd->getAgent(idx);
			if (ag && ag->active)
			{
				calcVel(vel, ag->npos, p, ag->params.maxSpeed);
				crowd->requestMoveVelocity(idx, vel);
			}
		}
		else
		{
			for (int i = 0; i < crowd->getAgentCount(); ++i)
			{
				const dtCrowdAgent* ag = crowd->getAgent(i);
				if (!ag->active) continue;
				calcVel(vel, ag->npos, p, ag->params.maxSpeed);
				crowd->requestMoveVelocity(i, vel);
			}
		}
	}
	else
	{
		dtPolyRef targetRef;
		float targetPos[3] = { 0, 0, 0 };
		dtStatus status = navquery->findNearestPoly(p, ext, filter, &targetRef, targetPos);

		ctx->log(RC_LOG_ERROR, "DebugMove: %d -> (%d,%d,%d) [%d]", targetRef, targetPos[0], targetPos[1], targetPos[2], status);

		if (idx != -1)
		{
			const dtCrowdAgent* ag = crowd->getAgent(idx);
			if (ag && ag->active)
			{
				ctx->log(RC_LOG_ERROR, "DebugMove: Requesting move for %x (%d)", ag, idx);
				crowd->requestMoveTarget(idx, targetRef, targetPos);
			}
		}
		else
		{
			for (int i = 0; i < crowd->getAgentCount(); ++i)
			{
				const dtCrowdAgent* ag = crowd->getAgent(i);
				if (!ag->active) continue;
				crowd->requestMoveTarget(i, targetRef, targetPos);
			}
		}
	}
}

void resetPath(dtCrowd* crowd, int idx)
{
	crowd->resetMoveTarget(idx);
}

void updateTick(dtTileCache* tileCache, dtNavMesh* nav, dtCrowd* crowd, float dt, float* positions, float* velocity, unsigned char* state, unsigned char* targetState, int& nagents)
{
	if (!nav || !crowd) return;

	tileCache->update(dt, nav);
	crowd->update(dt, NULL);

	// Update agent trails
	nagents = crowd->getAgentCount();
	for (int i = 0; i < nagents; ++i)
	{
		const dtCrowdAgent* ag = crowd->getAgent(i);

		positions[i * 3 + 0] = ag->npos[0];
		positions[i * 3 + 1] = ag->npos[1];
		positions[i * 3 + 2] = ag->npos[2];

		velocity[i * 3 + 0] = ag->vel[0];
		velocity[i * 3 + 1] = ag->vel[1];
		velocity[i * 3 + 2] = ag->vel[2];

		state[i] = ag->state;
		targetState[i] = ag->targetState;
	}
}