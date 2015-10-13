public interface IPlayerResource
{
	
	WorldResources.Type getType();

	/// <summary>
	/// compares the specified amount.
	/// </summary>
	/// <param name="amount">Amount.</param>
	bool enough(int amount);

	/// <summary>
	/// Restarts the player resource to its initial value.
	/// </summary>
	void restartResource();
	

	/// <summary>
	/// Checks the amount and substracts.
	/// </summary>
	/// <returns><c>true</c>, if is enough amount, <c>false</c> otherwise.</returns>
	bool checkAmountAndSubstract( int amount );

    /// <summary>
    /// Substract the amount specified and returns the remaining amount
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    int substractAndGet(int amount);

    /// <summary>
    /// Returns all the amount and substracts it
    /// </summary>
    /// <returns>The amount substracted</returns>
    int getAndSubstractAll();

    /// <summary>
    /// Merge the resources with the other. This means, adds the amount of the other resource to the current
    /// resource and returns the new resource instance
    /// </summary>
    /// <param name="other">Other.</param>
    void addResources( IPlayerResource other);
}



