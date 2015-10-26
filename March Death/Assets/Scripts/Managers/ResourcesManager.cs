using WorldResources;
using System.Collections.Generic;


namespace Managers
{

    public class ResourcesManager : IResourcesManager
    {


        private Dictionary<Type, Deposit> _deposits;

        public ResourcesManager()
        {
            _deposits = new Dictionary<Type, Deposit>();

            InitDeposit(new WorldResources.Resource(Type.FOOD, 2000));
            InitDeposit(new WorldResources.Resource(Type.METAL, 2000));
            InitDeposit(new WorldResources.Resource(Type.WOOD, 2000));
        }


        public void InitDeposit(WorldResources.Resource resource, int capacity = 0)
        {
            WorldResources.Type type = resource.GetResourceType();
            if (!_deposits.ContainsKey(type))
            {
                Deposit diposit = new Deposit(resource, capacity);
                _deposits.Add(type, diposit);
            }
            else
            {
                _deposits[type].AddAmount(capacity);
            }
        }

        public void EmptyDeposits()
        {
            foreach (var deposit in _deposits.Values)
            {
                deposit.Empty();
            }
        }


        public bool IsEnough(WorldResources.Resource other)
        {
            // HACK: It's a hack
            return true;

            if (_deposits.ContainsKey(other.GetResourceType()))
            {
                return _deposits[other.GetResourceType()].GetCapacity() >= other.GetAmount();

            }
            else
                throw new System.Exception("Type not in deposits");

        }


        public bool IsEnough(Type type, float amount)
        {
            return IsEnough(new WorldResources.Resource(type, amount));
        }


        public float SubstractAmount(WorldResources.Resource other)
        {
            if (_deposits.ContainsKey(other.GetResourceType()))
            {
                return _deposits[other.GetResourceType()].Substract(other);
            }
            else
                throw new System.Exception("Type not in deposits");
            
        }


        public float SubstractAmount(Type type, float amount)
        {
            return SubstractAmount(new WorldResources.Resource(type, amount));

        }


        public void AddAmount(WorldResources.Resource other)
        {
            if (_deposits.ContainsKey(other.GetResourceType()))
            {
                 _deposits[other.GetResourceType()].AddAmount(other.GetAmount());
            }
            else
                throw new System.Exception("Type not in deposits");
        }


        public void AddAmount(Type type, float amount)
        {
            AddAmount(new WorldResources.Resource(type, amount));
        }

        // -- Ferran --
        /// <summary>
        /// When resource building ability creates civilian unit we must 
        /// substract cost of this unit.
        /// </summary>
        /// <param name="race"></param>
        /// <param name="type"></param>
        public void payCivilUnit(Storage.Races race, Storage.UnitTypes type)
        {
            // TODO: compute civilian cost. Unit is created at Entities.Resource.cs 
            
            Unit unit = new Unit();
            unit.type = Storage.UnitTypes.CIVIL;
             
            var food = unit.info.resources.food;
            var metal = unit.info.resources.metal;
            var wood = unit.info.resources.wood;
            //var gold = unit.info.resources.gold;

            SubstractAmount(Type.FOOD, food);
            SubstractAmount(Type.METAL, metal);
            SubstractAmount(Type.WOOD, wood);
            //SubstractAmount(Type.GOLD, gold);
             
        }

    }

}