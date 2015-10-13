
namespace WorldResources
{
	/// <summary>
	/// Describes the general behaviour of a world resource.
	/// </summary>
	public abstract class WorldResource
	{
		/// <summary>
		/// The current amount.
		/// </summary>
		private int _amount;

		/// <summary>
		/// The initial amount.
		/// </summary>
		private int _initAmount;

		/// <summary>
		/// The _type.
		/// </summary>
		protected Type _type;

		/// <summary>
		/// Initializes a new instance of the <see cref="WorldResources.WorldResource"/> class.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <param name="initAmount">Init amount.</param>
		protected WorldResource (Type type, int initAmount = 0)
		{
			_type = type;
			_amount = _initAmount = initAmount;
		}

		/// <summary>
		/// Reset this instance to its initAmount.
		/// </summary>
		protected void reset()
		{
			_amount = _initAmount;
		}

		/// <summary>
		/// Add the specified amount.
		/// </summary>
		/// <param name="amount">Amount.</param>
		public void add(int amount) 
		{
			_amount += amount;
		}

		/// <summary>
		/// Substract the specified amount. If the amount is below 0, the result will be 0
		/// </summary>
		/// <param name="amount">Amount.</param>
		public void substract(int amount)
		{
			_amount = amount > 0 ? amount : 0;
		}


		/// <summary>
		/// Gets the current amount.
		/// </summary>
		/// <returns>The amount.</returns>
		public int getAmount()
		{
			return _amount;
		}

		/// <summary>
		/// Gets the type.
		/// </summary>
		/// <returns>The type.</returns>
		public Type getType()
		{
			return _type;
		}


	}
}
