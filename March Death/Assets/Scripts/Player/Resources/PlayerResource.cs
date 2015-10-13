
/// <summary>
/// A typical Player resource
/// </summary>
using System;


public class PlayerResource : WorldResources.WorldResource, IPlayerResource
{


	public PlayerResource (WorldResources.Type type, int amount = 0) : base(type, amount) {}



	/// <summary>
	/// compares the specified amount.
	/// </summary>
	/// <param name="amount">Amount.</param>
	public bool enough(int amount)
	{
		return getAmount () >= amount;
	}

	/// <summary>
	/// Restarts the player resource to its initial value.
	/// </summary>
	public void restartResource(){
		reset ();
	}

	/// <summary>
	/// Checks the amount and substract.
	/// </summary>
	/// <returns><c>true</c>, if is enough amount, <c>false</c> otherwise.</returns>
	/// <param name="amount">Amount.</param>
	public bool checkAmountAndSubstract( int amount )
	{
		if (enough (amount)) 
		{
			substract (amount);
			return true;
		} 
		else
			return false;
	}

	/// <summary>
	/// Returns all the amount and substracts it
	/// </summary>
	/// <returns>The amount substracted</returns>
	public int getAndSubstractAll()
	{
		int temp = getAmount();
		substract(temp);

		return temp;
	}

    /// <summary>
    /// Substract the amount specified and returns the remaining amount
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public int substractAndGet(int amount)
    {
        substract(amount);
        return getAmount();
    }

	/// <summary>
	/// Merge the resources with the other. This means, adds the amount of the other resource to the current
	/// resource and returns the new resource instance
	/// </summary>
	/// <param name="other">Other.</param>
	public void addResources( IPlayerResource other)
	{
		if (!_type.Equals(other.getType()))
		{
			throw new Exception ("Merging different type of resources");
		}

		add (other.getAndSubstractAll());

	}
}


