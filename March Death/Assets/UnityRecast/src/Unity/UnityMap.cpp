#include "Unity3d.h"


rcPolyMesh* pmesh;
rcPolyMeshDetail* dmesh;

bool handleBuild(rcConfig* cfg, float* verts, int nverts, int* tris, int ntris)
{
	// Calc boundaries
	float bmin[3];
	float bmax[3];
	rcCalcBounds(verts, nverts / 3, bmin, bmax);

	ctx->log(RC_LOG_PROGRESS, "OK 1");
	ctx->log(RC_LOG_PROGRESS, "OK 2");
	// Set the area where the navigation will be build.
	// Here the bounds of the input mesh are used, but the
	// area could be specified by an user defined box, etc.
	rcVcopy(cfg->bmin, bmin);
	rcVcopy(cfg->bmax, bmax);
	rcCalcGridSize(cfg->bmin, cfg->bmax, cfg->cs, &cfg->width, &cfg->height);

	// Reset build times gathering.
	ctx->resetTimers();

	// Start the build process.	
	ctx->startTimer(RC_TIMER_TOTAL);

	ctx->log(RC_LOG_PROGRESS, "Building navigation:");
	ctx->log(RC_LOG_PROGRESS, " - %d x %d cells", cfg->width, cfg->height);
	ctx->log(RC_LOG_PROGRESS, " - %.1fK verts, %.1fK tris", nverts / 1000.0f, ntris / 1000.0f);

	//
	// Step 2. Rasterize input polygon soup.
	//

	// Allocate voxel heightfield where we rasterize our input data to.
	rcHeightfield* solid = rcAllocHeightfield();
	if (!solid)
	{
		return false;
	}
	if (!rcCreateHeightfield(ctx, *solid, cfg->width, cfg->height, cfg->bmin, cfg->bmax, cfg->cs, cfg->ch))
	{
		ctx->log(RC_LOG_ERROR, "buildNavigation: Could not create solid heightfield.");
		return false;
	}
	ctx->log(RC_LOG_PROGRESS, "OK 3");

	// Allocate array that can hold triangle area types.
	// If you have multiple meshes you need to process, allocate
	// and array which can hold the max number of triangles you need to process.
	unsigned char* triareas = new unsigned char[ntris];
	if (!triareas)
	{
		ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'm_triareas' (%d).", ntris);
		return false;
	}
	ctx->log(RC_LOG_PROGRESS, "OK 4");

	// Find triangles which are walkable based on their slope and rasterize them.
	// If your input data is multiple meshes, you can transform them here, calculate
	// the are type for each of the meshes and rasterize them.
	memset(triareas, 0, ntris*sizeof(unsigned char));
	rcMarkWalkableTriangles(ctx, cfg->walkableSlopeAngle, verts, nverts, tris, ntris, triareas);
	rcRasterizeTriangles(ctx, verts, nverts, tris, triareas, ntris, *solid, cfg->walkableClimb);

	//if (!m_keepInterResults)
	{
		delete[] triareas;
		triareas = 0;
	}
	ctx->log(RC_LOG_PROGRESS, "OK 5");

	//
	// Step 3. Filter walkables surfaces.
	//

	// Once all geoemtry is rasterized, we do initial pass of filtering to
	// remove unwanted overhangs caused by the conservative rasterization
	// as well as filter spans where the character cannot possibly stand.
	rcFilterLowHangingWalkableObstacles(ctx, cfg->walkableClimb, *solid);
	rcFilterLedgeSpans(ctx, cfg->walkableHeight, cfg->walkableClimb, *solid);
	rcFilterWalkableLowHeightSpans(ctx, cfg->walkableHeight, *solid);

	ctx->log(RC_LOG_PROGRESS, "OK 6");

	//
	// Step 4. Partition walkable surface to simple regions.
	//

	// Compact the heightfield so that it is faster to handle from now on.
	// This will result more cache coherent data as well as the neighbours
	// between walkable cells will be calculated.
	rcCompactHeightfield* chf = rcAllocCompactHeightfield();
	if (!chf)
	{
		ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'chf'.");
		return false;
	}
	if (!rcBuildCompactHeightfield(ctx, cfg->walkableHeight, cfg->walkableClimb, *solid, *chf))
	{
		ctx->log(RC_LOG_ERROR, "buildNavigation: Could not build compact data.");
		return false;
	}

	ctx->log(RC_LOG_PROGRESS, "OK 7");
	//if (!m_keepInterResults)
	{
		rcFreeHeightField(solid);
		solid = 0;
	}

	// Erode the walkable area by agent radius.
	if (!rcErodeWalkableArea(ctx, cfg->walkableRadius, *chf))
	{
		ctx->log(RC_LOG_ERROR, "buildNavigation: Could not erode.");
		return false;
	}

	// (Optional) Mark areas.
	//const ConvexVolume* vols = m_geom->getConvexVolumes();
	//for (int i  = 0; i < m_geom->getConvexVolumeCount(); ++i)
	//rcMarkConvexPolyArea(&ctx, vols[i].verts, vols[i].nverts, vols[i].hmin, vols[i].hmax, (unsigned char)vols[i].area, *chf);

	ctx->log(RC_LOG_PROGRESS, "OK 8");
	if (monotonePartitioning)
	{
		// Partition the walkable surface into simple regions without holes.
		// Monotone partitioning does not need distancefield.
		if (!rcBuildRegionsMonotone(ctx, *chf, 0, cfg->minRegionArea, cfg->mergeRegionArea))
		{
			ctx->log(RC_LOG_ERROR, "buildNavigation: Could not build regions.");
			return false;
		}
	}
	else
	{
		// Prepare for region partitioning, by calculating distance field along the walkable surface.
		if (!rcBuildDistanceField(ctx, *chf))
		{
			ctx->log(RC_LOG_ERROR, "buildNavigation: Could not build distance field.");
			return false;
		}

		// Partition the walkable surface into simple regions without holes.
		if (!rcBuildRegions(ctx, *chf, 0, cfg->minRegionArea, cfg->mergeRegionArea))
		{
			ctx->log(RC_LOG_ERROR, "buildNavigation: Could not build regions.");
			return false;
		}
	}

	//
	// Step 5. Trace and simplify region contours.
	//
	ctx->log(RC_LOG_PROGRESS, "OK 9");

	// Create contours.
	rcContourSet* cset = rcAllocContourSet();
	if (!cset)
	{
		ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'cset'.");
		return false;
	}
	if (!rcBuildContours(ctx, *chf, cfg->maxSimplificationError, cfg->maxEdgeLen, *cset))
	{
		ctx->log(RC_LOG_ERROR, "buildNavigation: Could not create contours.");
		return false;
	}

	//
	// Step 6. Build polygons mesh from contours.
	//

	ctx->log(RC_LOG_PROGRESS, "OK 10");

	// Build polygon navmesh from the contours.
	pmesh = rcAllocPolyMesh();
	if (!pmesh)
	{
		ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'pmesh'.");
		return false;
	}
	if (!rcBuildPolyMesh(ctx, *cset, cfg->maxVertsPerPoly, *pmesh))
	{
		ctx->log(RC_LOG_ERROR, "buildNavigation: Could not triangulate contours.");
		return false;
	}

	//
	// Step 7. Create detail mesh which allows to access approximate height on each polygon.
	//

	dmesh = rcAllocPolyMeshDetail();
	if (!dmesh)
	{
		ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'pmdtl'.");
		return false;
	}

	if (!rcBuildPolyMeshDetail(ctx, *pmesh, *chf, cfg->detailSampleDist, cfg->detailSampleMaxError, *dmesh))
	{
		ctx->log(RC_LOG_ERROR, "buildNavigation: Could not build detail mesh.");
		return false;
	}

	//if (!m_keepInterResults)
	{
		rcFreeCompactHeightfield(chf);
		chf = 0;
		rcFreeContourSet(cset);
		cset = 0;
	}

	// At this point the navigation mesh data is ready, you can access it from m_pmesh.
	// See duDebugDrawPolyMesh or dtCreateNavMeshData as examples how to access the data.

	//
	// (Optional) Step 8. Create Detour data from Recast poly mesh.
	//
	// ...

	ctx->stopTimer(RC_TIMER_TOTAL);

	// Show performance stats.
	//duLogBuildTimes(*&ctx, ctx.getAccumulatedTime(RC_TIMER_TOTAL));
	ctx->log(RC_LOG_PROGRESS, ">> Polymesh: %d vertices  %d polygons", pmesh->nverts, pmesh->npolys);

	//m_totalBuildTimeMs = ctx.getAccumulatedTime(RC_TIMER_TOTAL) / 1000.0f;
	return true;
}

rcPolyMesh* getPolyMesh()
{
	return pmesh;
}

rcPolyMeshDetail* getPolyMeshDetail()
{
	return dmesh;
}

bool createNavmesh(rcConfig* cfg, rcPolyMesh* pmesh, rcPolyMeshDetail* dmesh, unsigned char*& navData, int& dataSize)
{
	dtNavMeshCreateParams* createParams = new dtNavMeshCreateParams();
	memset(createParams, 0, sizeof(dtNavMeshCreateParams));

	createParams->verts = pmesh->verts;
	createParams->vertCount = pmesh->nverts;
	createParams->polys = pmesh->polys;
	createParams->polyFlags = pmesh->flags;
	createParams->polyAreas = pmesh->areas;
	createParams->polyCount = pmesh->npolys;
	createParams->nvp = pmesh->nvp;

	createParams->detailMeshes = dmesh->meshes;
	createParams->detailVerts = dmesh->verts;
	createParams->detailVertsCount = dmesh->nverts;
	createParams->detailTris = dmesh->tris;
	createParams->detailTriCount = dmesh->ntris;

	/*
	createParams->offMeshConVerts = NULL;
	createParams->offMeshConRad = NULL;
	createParams->offMeshConFlags = NULL;
	createParams->offMeshConAreas = NULL;
	createParams->offMeshConDir = NULL;
	createParams->offMeshConUserID = NULL;
	createParams->offMeshConCount = 0;
	*/

	createParams->walkableHeight = cfg->walkableHeight;
	createParams->walkableRadius = cfg->walkableRadius;
	createParams->walkableClimb = cfg->walkableClimb;
	createParams->cs = cfg->cs;
	createParams->ch = cfg->ch;

	createParams->buildBvTree = true;

	if (dtCreateNavMeshData(createParams, &navData, &dataSize))
	{
		dtNavMesh* navmesh = dtAllocNavMesh();
		navmesh->init(navData, dataSize, DT_TILE_FREE_DATA);

		return true;
	}

	return false;
}
