
using System.Collections.Generic;

public class ResourcesManager : IResourcesManager
{


	private Dictionary<WorldResources.Type, PlayerResource> _resources; 

	public ResourcesManager ()
	{
		_resources = new Dictionary<WorldResources.Type, PlayerResource> ();

	}

	/// <summary>
	/// Inits the specified resource and ads it to the collection
	/// </summary>
	/// <param name="type">Type.</param>
	/// <param name="initAmount">Init amount.</param>
	public void initResource(WorldResources.Type type, int initAmount = 0 )
	{
		if( !_resources.ContainsKey(type) )
		   {
			PlayerResource newResource = new PlayerResource(type, initAmount);
			_resources.Add (type, newResource);
		}
		else
		{
			_resources[type] = new PlayerResource(type, initAmount);
		}


	}

	public void restartResources(){

		foreach(var item in _resources.Values)
		{
			item.restartResource();
		}

	}

	public IPlayerResource getResource(WorldResources.Type type)
	{
		return _resources.ContainsKey(type) ? (IPlayerResource) _resources[type] : null;
	}


}

