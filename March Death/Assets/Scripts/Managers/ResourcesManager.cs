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

        public bool IsEnough(Resource other)
        {
            //TODO
            return true;
        }

        public bool IsEnough(Type type, float amount)
        {
            //TODO
            return true;
        }



        public float SubstractAmount(Resource other)
        {
            //TODO
            return 1f;
        }

        public float SubstractAmount(Type type, float amount)
        {
            //TODO
            return 1f;
        }

        public void AddAmount(Resource other)
        {
            //TODO
        }
        public void AddAmount(Type type, float amount)
        {
            //TODO
        }

    }

}