﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Utils;

namespace Assets.Scripts.AI.Agents
{
    public class ExplorerAgent : BaseAgent
    {
        bool heroVisible;
        FOWManager fowManager;
        /// <summary>
        /// Last position when we saw the enemy hero
        /// </summary>
        Vector3 heroLastPos;
        /// <summary>
        /// Helper array for fast explroe.
        /// </summary>
        int[,] dirHelper = new int[8,2]{ { 0, -1 }, { -1, -1 }, { -1, 0 }, { -1, 1 }, { 0, 1 }, { 1, 1 }, { 1, 0 }, { 1, -1 } };

        public ExplorerAgent(AIController ai, String name) : base(ai, name)
        {
            ActorSelector selector = new ActorSelector()
            {
                registerCondition = gameObject => gameObject.GetComponent<FOWEntity>().IsOwnedByPlayer,
                fireCondition = gameObject => true
            };
            Subscriber<FOWEntity.Actions, FOWEntity>.get.registerForAll(FOWEntity.Actions.DISCOVERED, OnEntityFound, selector);
            Subscriber<FOWEntity.Actions, FOWEntity>.get.registerForAll(FOWEntity.Actions.HIDDEN, OnEntityLost, selector);
            heroVisible = false;
            fowManager = FOWManager.Instance;
            heroLastPos = Vector3.zero;
        }
        public override void controlUnits(List<Unit> units)
        {
            //PLACEHOLDER, until sprint2 when we split things
            if (units.Count < 2)
            {
                int num = 2 - units.Count();
                if (ai.Macro.canTakeArms() >= num)
                    ai.Macro.takeArms(num);
            }
            foreach (Unit u in units)
            {
                u.moveTo(findPlaceToExplore(u, fowManager.aiVision, fowManager.getGridSize()));
                if (AIController.AI_DEBUG_ENABLED)
                {
                    ai.aiDebug.registerDebugInfoAboutUnit(u, this.agentName);
                }
            }
            //We have a previous last seen location for the hero, better send someone to scout it
            if (heroLastPos!=Vector3.zero && !heroVisible && units.Count>0 )
            {
                float bVal = float.MaxValue;
                Unit bUnit = units[0];
                //better find which unit is closer to the last hero position
                foreach (Unit u in units) 
                {
                    float dist = Vector3.Distance(u.transform.position, heroLastPos);
                    //We arrived where the hero was before, scout around here
                    if (dist < 3f)
                        heroLastPos = Vector3.zero; 
                    else if (bVal > dist)
                    {
                        bVal = dist;
                        bUnit = u;
                    }
                }
                bUnit.moveTo(heroLastPos);
            }
        }
        Vector3 findPlaceToExplore(Unit u,FOWManager.visible[] grid, Vector2 size)
        {
            //Basic try of an algorithm, just check the grid in a star way until something unexplored
            //Directions = D,DL,L,UL,U,UR,R,DR,D  D=Down, L=Left, R=Right, U=Up
            bool[] directions = new bool[]{ true, true, true, true, true, true, true, true};
            Vector2 pos = fowManager.CoordtoGrid(u.transform.position);
            //The way to get the range is horrible, we shoul really:
            //TODO: put a range variable in unit to avoid having to get the component
            int range = 1;
            FOWEntity fe = u.GetComponent<FOWEntity>();
            if (fe)
                range = Mathf.RoundToInt(fe.Range*fowManager.Quality);
            for(int i=range;i<50;i++) //do no continue forever
            {
                for(int dir = 0; dir < 8; dir++)
                {
                    if (directions[dir])
                    {
                        int x = (int)pos.x+ i * dirHelper[dir, 0];
                        int y = ((int)pos.y+ i * dirHelper[dir, 1]);
                        if (x < 0 || x >= size.x || y < 0 || y >= size.y)
                            directions[dir] = false;
                        else if ((FOWManager.visible.unexplored & grid[x + y*(int)size.y]) == FOWManager.visible.unexplored)
                        {
                            Vector2 realPos = fowManager.CoordtoWorld(x, y);
                            return new Vector3(realPos.x, u.transform.position.y, realPos.y);
                        }
                    }
                }
            }
            return new Vector3(u.transform.position.x+1, u.transform.position.y, u.transform.position.z);
        }
        public override int getConfidence(List<Unit> units)
        {
            if (heroVisible)
                return 0;
            return 20;
        }
        void OnEntityFound(System.Object obj)
        {
            IGameEntity g = ((GameObject)obj).GetComponent<IGameEntity>();
            if (g.info.isArmy)
                if (((Unit)g).type == Storage.UnitTypes.HERO)
                    heroVisible = true;
        }
        void OnEntityLost(System.Object obj)
        {
            IGameEntity g = ((GameObject)obj).GetComponent<IGameEntity>();
            if (g.info.isArmy)
            {
                Unit u = (Unit)g;
                if (u.type == Storage.UnitTypes.HERO)
                {
                    heroVisible = false;
                    heroLastPos = u.transform.position;
                }
            }
        }
    }
}
