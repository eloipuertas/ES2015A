namespace WorldResources
{
    /// <summary>
    /// This class represents a diposit of a particular resource
    /// capacity = 0 represent limitless capacity of the current resource
    /// it will evolve to have some other stuff wi
    /// </summary>
    class Deposit
    {
        private Resource _resource;
        private int _capacity;

        public Deposit(Resource resource, int capacity = 0)
        {
            _capacity = 0;
            _resource = resource;
        }


        public void AddAmount(int amount)
        {
            _resource.Add(amount);
        }

        public int GetAmount()
        {
            return _resource.GetAmount();
        }

        public Type GetDepositType()
        {
            return _resource.GetResourceType();
        }

        public Resource Empty()
        {
            return new Resource(_resource.GetResourceType(), _resource.Empty());
            
        }

        public int GetCapacity()
        {
            return _capacity;
        }

        public bool IsLimited()
        {
            return _capacity == 0 ? true : false;
        }
    }
}
