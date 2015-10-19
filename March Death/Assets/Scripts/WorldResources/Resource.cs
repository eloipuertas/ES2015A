namespace WorldResources
{
    /// <summary>
    /// Describes the general behaviour of a world resource.
    /// </summary>
    public class Resource
    {
        /// <summary>
        /// The current amount.
        /// </summary>
        private float _amount;



        /// <summary>
        /// The _type.
        /// </summary>
        private Type _type;

        /// <summary>
        /// Initializes the resource with the type and the amoung
        /// </summary>
        /// <param name="type"></param>
        /// <param name="amount"></param>
        public Resource(Type type, float amount = 0f)
        {
            _type = type;
            _amount = amount;
        }

        /// <summary>
        /// Reset this instance and returns what it had.
        /// </summary>
        public float Empty()
        {
            float temp = _amount;
            _amount = 0f;

            return temp;
        }

        /// <summary>
        /// Add the specified amount.
        /// </summary>
        /// <param name="amount">Amount.</param>
        public void Add(float amount)
        {
            _amount += amount;
        }

        /// <summary>
        /// Substracts the quantity recieved and returns the remaining amount
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public float Substract(float amount)
        {
            _amount = amount > 0f ? _amount - amount : 0f;
            return _amount;
        }

        /// <summary>
        /// Compares the current amount with the parameter and returns if its higher or the same value
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public bool IsEnough(float amount)
        {
            return _amount >= amount;
        }

        /// <summary>
        /// Gets the current amount.
        /// </summary>
        /// <returns>The amount.</returns>
        public float GetAmount()
        {
            return _amount;
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <returns>The type.</returns>
        public Type GetResourceType()
        {
            return _type;
        }

        /// <summary>
        /// Returns a copy of the current instance
        /// </summary>
        /// <returns></returns>
        public Resource Clone()
        {

            return new Resource(_type, _amount);

        }

    }
}