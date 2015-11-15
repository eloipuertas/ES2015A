#include "Unity3d.h"

// Vars
Context* ctx;
bool monotonePartitioning = false;


void setMonotonePartitioning(bool enabled)
{
	monotonePartitioning = enabled;
}

rcConfig* DefaultConfig(char* logpath)
{
	rcConfig* cfg = new rcConfig();

	float cellSize = 0.3f;
	float cellHeight = 0.2f;
	float agentHeight = 2.0f;
	float agentRadius = 0.6f;
	float agentMaxClimb = 0.8f;
	float agentMaxSlope = 20.0f;
	float regionMinSize = 8;
	float regionMergeSize = 20;
	float edgeMaxLen = 12.0f;
	float edgeMaxError = 1.3f;
	float vertsPerPoly = 6.0f;
	float detailSampleDist = 6.0f;
	float detailSampleMaxError = 1.0f;
	monotonePartitioning = false;

	// Init build configuration from GUI
	memset(cfg, 0, sizeof(rcConfig));
	cfg->cs = cellSize;
	cfg->ch = cellHeight;
	cfg->tileSize = 48;
	cfg->walkableSlopeAngle = agentMaxSlope;
	cfg->walkableHeight = (int)ceilf(agentHeight / cfg->ch);
	cfg->walkableClimb = (int)floorf(agentMaxClimb / cfg->ch);
	cfg->walkableRadius = (int)ceilf(agentRadius / cfg->cs);
	cfg->maxEdgeLen = (int)(edgeMaxLen / cellSize);
	cfg->maxSimplificationError = edgeMaxError;
	cfg->minRegionArea = (int)rcSqr(regionMinSize);		// Note: area = size*size
	cfg->mergeRegionArea = (int)rcSqr(regionMergeSize);	// Note: area = size*size
	cfg->maxVertsPerPoly = (int)vertsPerPoly;
	cfg->detailSampleDist = detailSampleDist < 0.9f ? 0 : cellSize * detailSampleDist;
	cfg->detailSampleMaxError = cellHeight * detailSampleMaxError;
	
	ctx = new Context(logpath);
	ctx->enableLog(true);

	return cfg;
}

