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
        private float _capacity;

        public Deposit(Resource resource, float capacity = 0f)
        {
            _capacity = capacity;
            _resource = resource;
        }


        public void AddAmount(float amount)
        {
            _resource.Add(amount);
        }

        public float GetAmount()
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

        public float Substract(Resource other)
        {
            return _resource.Substract(other.GetAmount());
        }

        public float GetCapacity()
        {
            return _capacity;
        }

        public bool IsLimited()
        {
            return _capacity == 0f ? true : false;
        }
    }
}
