using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Utils;

namespace Assets.Scripts.AI.Agents
{
    public class ExplorerAgent : BaseAgent
    {
		//Those are the confidence posibilities of our Explorer Agent
		const int CONFIDENCE_EXPLORER_BY_DEFAULT = 50;
		const int CONFIDENCE_EXPLORER_ALL_CIVILS = 50;
		const int CONFIDENCE_EXPLORER_DISABLED = -1;
		const int CONFIDENCE_HERO_ALREADY_FOUND = 0;

        bool heroVisible;
		bool allCivils;

        FOWManager fowManager;
        /// <summary>
        /// Last position when we saw the enemy hero
        /// </summary>
        Vector3 heroLastPos;
        /// <summary>
        /// Helper array for fast explroe.
        /// </summary>
        int[,] dirHelper = new int[8,2]{ { 0, -1 }, { -1, -1 }, { -1, 0 }, { -1, 1 }, { 0, 1 }, { 1, 1 }, { 1, 0 }, { 1, -1 } };
		int confidence = 0;
        
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

        public override void controlUnits(SquadAI squad)
        {
            if (fowManager.Enabled)
            {
                //PLACEHOLDER, until sprint2 when we split things
                if (squad.units.Count < 2)
                {
                    int num = 2 - squad.units.Count();
                    if (ai.Macro.canTakeArms() >= num)
                        ai.Macro.takeArms(num);
                }
                foreach (Unit u in squad.units)
                {
                    u.moveTo(findPlaceToExplore(u, fowManager.aiVision, fowManager.getGridSize()));
                    if (AIController.AI_DEBUG_ENABLED)
                    {
                        ai.aiDebug.registerDebugInfoAboutUnit(u, this.agentName);
                    }
                }
                //We have a previous last seen location for the hero, better send someone to scout it
                if (heroLastPos != Vector3.zero && !heroVisible && squad.units.Count > 0)
                {
                    float bVal = float.MaxValue;
                    Unit bUnit = squad.units[0];
                    //better find which unit is closer to the last hero position
                    foreach (Unit u in squad.units)
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

		/// <summary>
		/// Gets the confidence of this agent.
		/// </summary>
		/// <returns>The confidence.</returns>
		/// <param name="units">Units.</param>
        public override int getConfidence(SquadAI squad)
        {
			//Explorer agent has some confidence by default
			confidence = CONFIDENCE_EXPLORER_BY_DEFAULT;
            
			//If fow manager is not enabled this agent will never act
			if (!fowManager.Enabled)
			{
                return CONFIDENCE_EXPLORER_DISABLED;
			}

			//If we have found enemy's hero we don't need to explore.
            if (heroVisible)
			{
                return CONFIDENCE_HERO_ALREADY_FOUND;
			}

			//If all units of the squad adds some more confidence to this behaivour
			foreach(Unit unit in squad.units)
			{
				if(unit.type != Storage.UnitTypes.CIVIL)
				{
					allCivils = false;
				}
			}

			if(allCivils)
			{
				confidence += CONFIDENCE_EXPLORER_ALL_CIVILS;
			}

			return confidence;
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
