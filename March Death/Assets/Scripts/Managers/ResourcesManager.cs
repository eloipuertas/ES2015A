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

    }

}