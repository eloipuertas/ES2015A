using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Utils;
using Pathfinding;

namespace Assets.Scripts.AI.Agents
{
    public class ExplorerAgent : BaseAgent
    {
        // Target rescheduling typ
        enum RescheduleType
        {
            NONE,
            RANDOM_IN_CURRENT_POSITION,
            RANDOM_IN_CURRENT_TARGET,
            RANDOM_IN_DIRECTION,
            RANDOM_AROUND_TARGET
        }

        // Configurable parameters
        float RescheduleSightRange { get; set; }
        int ReschuduleDiceFaces { get; set; }
        int RescheduleRandomPointValue { get; set; }
        int RescheduleRandomDirectionValue { get; set; }
        int RescheduleRandomAroundTargetValue { get; set; }

        //Those are the confidence posibilities of our Explorer Agent
        const int CONFIDENCE_EXPLORER_BY_DEFAULT = 50;
		const int CONFIDENCE_EXPLORER_ALL_CIVILS = 50;
		const int CONFIDENCE_EXPLORER_DISABLED = -1;
		const int CONFIDENCE_HERO_ALREADY_FOUND = 0;

        bool heroVisible;
		bool allCivils;

        AssistAgent assistAgent;

        FOWManager fowManager;

        /// <summary>
        /// Last position when we saw the enemy hero
        /// </summary>
        Vector3 heroLastPos;
        Vector3 targetPos;

        // Known enemy building positions
        List<Vector3> knownPositions = new List<Vector3>();

        /// <summary>
        /// Helper array for fast explroe.
        /// </summary>
        int[,] dirHelper = new int[8,2]{ { 0, -1 }, { -1, -1 }, { -1, 0 }, { -1, 1 }, { 0, 1 }, { 1, 1 }, { 1, 0 }, { 1, -1 } };
		int confidence = 0;
        
		public ExplorerAgent(AIController ai, AssistAgent assist, String name) : base(ai, name)
        {
            ActorSelector selector = new ActorSelector()
            {
                registerCondition = gameObject => gameObject.GetComponent<FOWEntity>().IsOwnedByPlayer,
                fireCondition = gameObject => true
            };
            Subscriber<FOWEntity.Actions, FOWEntity>.get.registerForAll(FOWEntity.Actions.DISCOVERED, OnEntityFound, selector);
            Subscriber<FOWEntity.Actions, FOWEntity>.get.registerForAll(FOWEntity.Actions.HIDDEN, OnEntityLost, selector);

            RescheduleSightRange = 30;
            ReschuduleDiceFaces = 1000;
            RescheduleRandomPointValue = 1000;
            RescheduleRandomAroundTargetValue = 990;
            RescheduleRandomDirectionValue = 970;

            heroVisible = false;
            fowManager = FOWManager.Instance;
            heroLastPos = Vector3.zero;
            assistAgent = assist;
        }

        public override void controlUnits(SquadAI squad)
        {
            if (squad.units.Count == 0)
            {
                return;
            }

            if (fowManager.Enabled)
            {
                //PLACEHOLDER, until sprint2 when we split things
                if (squad.units.Count < 2)
                {
                    int num = 2 - squad.units.Count();
                    if (ai.Macro.canTakeArms() >= num)
                        ai.Macro.takeArms(num);
                }

                bool lostHero = (heroLastPos != Vector3.zero && !heroVisible && squad.units.Count > 0);

                // Static values
                FOWManager.visible[] grid = fowManager.aiVision;
                Vector2 gridSize = fowManager.getGridSize();

                // Get a random unit as a reference point
                Unit reference = squad.units[D6.get.rollN(squad.units.Count)];
                DetourAgent agent = reference.GetComponent<DetourAgent>();

                // Check if target is already explored
                Vector3 direction = (agent.TargetPoint - reference.transform.position).normalized;
                // Set the further point as the targetPoint + sighRange - offset
                Vector3 targetFOWPos = agent.TargetPoint + direction * (reference.info.attributes.sightRange - RescheduleSightRange);
                Vector2 targetGrid = fowManager.CoordtoGrid(targetFOWPos);
                // Check grid status
                bool targetExplored = ((FOWManager.visible.explored & grid[(int)(targetGrid.x + targetGrid.y * gridSize.y)]) == FOWManager.visible.explored);

                // If we are moving to a not seen target, don't reschedule at first, otherwise go to a random position in a circle around current position
                RescheduleType reschedule = RescheduleType.RANDOM_IN_CURRENT_POSITION;
                if (targetExplored && agent.IsMoving)
                {
                    reschedule = RescheduleType.RANDOM_IN_DIRECTION;
                }
                else if (agent.IsMoving)
                {
                    reschedule = RescheduleType.NONE;
                }

                // Throw a dice
                int diceValue = D6.get.rollN(ReschuduleDiceFaces);
                if (diceValue >= RescheduleRandomPointValue)
                {
                    // Change target point taking into account current direction (but not distance!)
                    reschedule = RescheduleType.RANDOM_IN_DIRECTION;
                }
                else if (diceValue >= RescheduleRandomAroundTargetValue && knownPositions.Count > 0)
                {
                    reschedule = RescheduleType.RANDOM_AROUND_TARGET;
                }
                else if (diceValue >= RescheduleRandomDirectionValue)
                {
                    // Change target point taking into account distance (but not direction!)
                    // Do note that direction is implicitelly taken into account because FOWManager reports
                    // some areas as explored
                    reschedule = RescheduleType.RANDOM_IN_CURRENT_TARGET;
                }

                // If we are moving and we don't have to reschedule, skip this squad
                if (agent.IsMoving && !targetExplored && reschedule == RescheduleType.NONE)
                {
                    return;
                }

                Debug.Log(reference + " " + reschedule);

                // If we've lost the hero, seek it with the closer unit, regardless of what FOWManager tells us
                if (lostHero)
                {
                    float dist = (reference.transform.position - heroLastPos).sqrMagnitude;
                    if (dist < reference.info.attributes.sightRange) // Nota, aixo implica "distancia_actual < sqrt(sightRange)"
                    {
                        heroLastPos = Vector3.zero;
                        lostHero = false;
                    }
                }

                // Switch to current reschedule type and find a target point
                bool result = false;
                switch (reschedule)
                {
                    case RescheduleType.NONE:
                        break;

                    case RescheduleType.RANDOM_IN_DIRECTION:
                        result = findPlaceToExplore(grid, gridSize, out targetPos, true, reference.transform.position, agent.TargetPoint);
                        break;

                    case RescheduleType.RANDOM_IN_CURRENT_TARGET:
                        result = findPlaceToExplore(grid, gridSize, out targetPos, agent.TargetPoint, 25f);
                        break;

                    case RescheduleType.RANDOM_IN_CURRENT_POSITION:
                        result = findPlaceToExplore(grid, gridSize, out targetPos, reference.transform.position, 75f);
                        break;

                    case RescheduleType.RANDOM_AROUND_TARGET:
                        result = findPlaceToExplore(grid, gridSize, out targetPos, knownPositions[D6.get.rollN(knownPositions.Count)], 5f);
                        break;

                }

                // If we failed to find a valid target and we are not moving (thus we are IDLE), find a random point along all the map
                if (!result && !agent.IsMoving)
                {
                    result = findPlaceToExplore(fowManager.aiVision, fowManager.getGridSize(), out targetPos);
                }

                // If we have a point, move there
                foreach (Unit u in squad.units)
                {
                    if (lostHero)
                    {
                        u.moveTo(heroLastPos);
                    }
                    else if (result)
                    {
                        u.moveTo(targetPos);

                        if (AIController.AI_DEBUG_ENABLED)
                        {
                            ai.aiDebug.registerDebugInfoAboutUnit(u, agentName);
                        }
                    }
                    else
                    {
                        if (AIController.AI_DEBUG_ENABLED)
                        {
                            ai.aiDebug.registerDebugInfoAboutUnit(u, agentName + " -> No Target");
                        }
                    }
                }
            }
        }

        static float angleDirection(Vector3 from, Vector3 initialTo, Vector3 newTo)
        {
            Vector3 referenceForward = (initialTo - from).normalized;
            Vector3 referenceRight = Vector3.Cross(Vector3.up, referenceForward);
            Vector3 newDirection = (newTo - from).normalized;

            float angle = Vector3.Angle(newDirection, referenceForward);
            float sign = Mathf.Sign(Vector3.Dot(newDirection, referenceRight));
            return sign * angle;
        }

        bool findPlaceToExplore(FOWManager.visible[] grid, Vector2 size, out Vector3 targetPoint, int maxTries = 5)
        {
            return findPlaceToExplore(grid, size, out targetPoint, false, Vector3.zero, Vector3.zero, maxTries);
        }

        bool findPlaceToExplore(FOWManager.visible[] grid, Vector2 size, out Vector3 targetPoint, bool enforceDirection, Vector3 currentPos, Vector3 currentPoint, int maxTries = 5)
        {
            bool found = false;
            targetPoint = Vector3.zero;

            Vector3 direction = Vector3.zero;

            if (enforceDirection)
            {
                direction = (currentPoint - currentPos).normalized;
            }

            for (int i = 0; i < maxTries && !found; ++i)
            {
                if (!Pathfinding.DetourCrowd.Instance.RandomValidPoint(ref targetPoint))
                {
                    return false;
                }

                Vector2 gridPos = fowManager.CoordtoGrid(targetPoint);
                if ((FOWManager.visible.unexplored & grid[(int)(gridPos.x + gridPos.y * size.y)]) == FOWManager.visible.unexplored)
                {
                    if (enforceDirection)
                    {
                        float angle = angleDirection(currentPos, currentPoint, targetPoint);
                        if (angle < -10f || angle > 10f)
                        {
                            continue;
                        }
                    }

                    found = true;
                }
            }

            return found;
        }

        bool findPlaceToExplore(FOWManager.visible[] grid, Vector2 size, out Vector3 targetPoint, Vector3 center, float maxRadius, int maxTries = 5)
        {
            bool found = false;
            targetPoint = Vector3.zero;

            for (int i = 0; i < maxTries && !found; ++i)
            {
                if (!Pathfinding.DetourCrowd.Instance.RandomValidPointInCircle(center, maxRadius, ref targetPoint))
                {
                    return false;
                }

                Vector2 gridPos = fowManager.CoordtoGrid(targetPoint);
                if ((FOWManager.visible.unexplored & grid[(int)(gridPos.x + gridPos.y * size.y)]) == FOWManager.visible.unexplored)
                {
                    found = true;
                }
            }

            return found;
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
            {
                if (((Unit)g).type == Storage.UnitTypes.HERO)
                {
                    heroVisible = true;
                }
            }
            else if (g.info.isBuilding)
            {
                knownPositions.Add(g.getTransform().position);
            }
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
