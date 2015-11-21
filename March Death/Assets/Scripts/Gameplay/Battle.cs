using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
public class Battle
{
    public struct ResourceAmount
    {
        public uint Wood { get; set; }
        public uint Food { get; set; }
        public uint Metal { get; set; }
        public uint Gold { get; set; }
    }

    /// <summary>
    /// Definition for the object that holds the position of playable game
    /// entities, such as units and buildings.
    /// </summary>
    public struct PlayableEntity
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct EntityTypeUnion
        {
            [FieldOffset(0)]
            public Storage.BuildingTypes building;
            [FieldOffset(0)]
            public Storage.UnitTypes unit;
        }

        public struct EntityPosition
        {
            public float X { get; set; }
            public float Y { get; set; }
        }

        /// <summary>
        /// The type of building or unit this entity represents.
        /// </summary>
        public EntityTypeUnion type;
        public EntityPosition position;
        public Storage.EntityType entityType;
    }

    public enum MissionType
    {
        DESTROY, NEW, KEEP, CONQUER
    }

    public struct MissionDefinition
    {
        [StructLayout(LayoutKind.Explicit)] public struct TargetType
        {
            [FieldOffset(0)] public Storage.BuildingTypes building;
            [FieldOffset(0)] public Storage.UnitTypes unit;
            [FieldOffset(0)] public WorldResources.Type resources;
        }

        /// <summary>
        /// The kind of action that should be taken for this mission
        /// </summary>
        public MissionType purpose;
        public uint amount;
        public Storage.EntityType target;
        public TargetType targetType;
        public uint priority;
        public bool main;
        public string information;
    }

    /// <summary>
    /// Utility class to hold the information necessary to instantiate all
    /// players and their game entities.
    /// </summary>
    public class PlayerInformation
    {
        private ResourceAmount resources;
        private List<PlayableEntity> buildings;
        private List<PlayableEntity> units;
        private Storage.Races _race;
        public Storage.Races Race { get {return _race;} }

        /// <summary>
        /// Adds a building with the given type and position to this player's information.
        /// </summary>
        /// <param name="type">Type of building.</param>
        /// <param name="x">The x coordinate in the grid.</param>
        /// <param name="y">The y coordinate in the grid.</param>
        public void AddBuilding(Storage.BuildingTypes type, float x, float y)
        {
            PlayableEntity e = new PlayableEntity();
            e.type.building = type;
            e.entityType = Storage.EntityType.BUILDING;
            e.position.X = x;
            e.position.Y = y;
            buildings.Add(e);
        }

        /// <summary>
        /// Adds a unit with the given type and position to this player's information.
        /// </summary>
        /// <param name="type">Type of building.</param>
        /// <param name="x">The x coordinate in the grid.</param>
        /// <param name="y">The y coordinate in the grid.</param>
        public void AddUnit(Storage.UnitTypes type, float x, float y)
        {
            PlayableEntity e = new PlayableEntity();
            e.type.unit = type;
            e.entityType = Storage.EntityType.UNIT;
            e.position.X = x;
            e.position.Y = y;
            units.Add(e);
        }

        public void SetInitialResources(uint wood, uint food, uint metal, uint gold)
        {
            resources.Food = food;
            resources.Metal = metal;
            resources.Wood = wood;
            resources.Gold = gold;
        }

        public List<PlayableEntity> GetBuildings()
        {
            return buildings;
        }

        public List<PlayableEntity> GetUnits()
        {
            return units;
        }

        public ResourceAmount GetResources()
        {
            return resources;
        }

        public PlayerInformation(Storage.Races race)
        {
            _race = race;
            buildings = new List<PlayableEntity>();
            units = new List<PlayableEntity>();
            resources = new ResourceAmount();
        }
    }

    private List<PlayerInformation> players;
    private ResourceAmount worldResources;
    private List<MissionDefinition> missions;

    public void AddPlayerInformation(PlayerInformation player)
    {
        players.Add(player);
    }

    public List<PlayerInformation> GetPlayerInformationList()
    {
        return players;
    }

    public void SetWorldResources(uint wood, uint food, uint metal)
    {
        worldResources.Food = food;
        worldResources.Metal = metal;
        worldResources.Wood = wood;
        worldResources.Gold = 0;
    }

    public ResourceAmount GetWorldResources()
    {
        return worldResources;
    }

    public void AddMission(MissionDefinition definition)
    {
        missions.Add(definition);
    }

    /// <summary>
    /// Adds a mission.
    /// </summary>
    /// <param name="action">Action to be performed to complete the mission</param>
    /// <param name="quantity">Amount of targets the action must be applied to.</param>
    /// <param name="what">Target of the mission.</param>
    /// <param name="type">Type of target.</param>
    /// <param name="priority">Priority of the mission.</param>
    /// <param name="isMain">If set to <c>false</c> it is a secondary mission.</param>
    /// <param name="information">Additional information for this mission (such as a description).</param>
    public void AddMission(MissionType action, uint quantity,
                           Storage.EntityType what,
                           MissionDefinition.TargetType type, uint priority,
                           bool isMain, string information)
    {
        MissionDefinition definition = new MissionDefinition();
        definition.amount = quantity;
        definition.information = information;
        definition.main = isMain;
        definition.priority = priority;
        definition.purpose = action;
        definition.target = what;
        definition.targetType = type;
        missions.Add(definition);
    }

    public List<MissionDefinition> GetMissions()
    {
        return missions;
    }

    public Battle ()
    {
        players = new List<PlayerInformation>();
        worldResources = new ResourceAmount();
        missions = new List<MissionDefinition>();
    }
}

