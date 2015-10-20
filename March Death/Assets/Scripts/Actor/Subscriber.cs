using System;
using System.Linq;
using System.Collections.Generic;

namespace Utils
{
	public sealed class SelectorStore<T> : Singleton<SelectorStore<T>> where T : struct, IConvertible
	{
		public static ActorSelector defaultSelector = new ActorSelector()
		{
			registerCondition = gameObject => true,
			fireCondition = gameObject => true
		};
		
		public Dictionary<T, Dictionary<Action<Object>, ActorSelector>> _selectors = new Dictionary<T, Dictionary<Action<Object>, ActorSelector>>();
		
		public ActorSelector Selector(T action, Action<Object> func)
		{
			if (!_selectors[action].ContainsKey(func))
			{
				return defaultSelector;
			}
			
			return _selectors[action][func];
		}
		
		private SelectorStore()
		{
			if (!typeof(T).IsEnum)
			{
				throw new ArgumentException("T must be an enumerated type");
			}
			
			foreach (T action in Enum.GetValues(typeof(T)))
			{
				_selectors.Add(action, new Dictionary<Action<Object>, ActorSelector>());
			}
		}
	}
	
	public sealed class Subscriber<T, S> : Singleton<Subscriber<T, S>> where T : struct, IConvertible where S : class
	{        
		private Dictionary<T, List<Action<Object>>> callbacks = new Dictionary<T, List<Action<Object>>>();
		
		private Subscriber()
		{
			if (!typeof(T).IsEnum)
			{
				throw new ArgumentException("T must be an enumerated type");
			}
			
			foreach (T action in Enum.GetValues(typeof(T)))
			{
				callbacks.Add(action, new List<Action<Object>>());
			}
		}
		
		public void registerForAll(T action, Action<Object> func)
		{
			registerForAll(action, func, SelectorStore<T>.defaultSelector);
		}
		
		public void registerForAll(T action, Action<Object> func, ActorSelector selector)
		{
			callbacks[action].Add(func);
			SelectorStore<T>.get._selectors[action].Add(func, selector);
			
			// Find all already existing gameobjects of this type
			UnityEngine.Object[] alreadyExistingActors = UnityEngine.Object.FindObjectsOfType(typeof(SubscribableActor<T,S>));
			foreach (UnityEngine.Object obj in alreadyExistingActors)
			{
				var actor = (SubscribableActor<T, S>)obj;
				
				if (selector.registerCondition(actor.gameObject))
				{
					actor.register(action, func);
				}
			}
		}
		
		public void unregisterFromAll(T action, Action<Object> func)
		{
			callbacks[action].Remove(func);
			SelectorStore<T>.get._selectors[action].Remove(func);
			
			// Find all already existing gameobjects of this type
			UnityEngine.Object[] alreadyExistingActors = UnityEngine.Object.FindObjectsOfType(typeof(SubscribableActor<T,S>));
			foreach (UnityEngine.Object obj in alreadyExistingActors)
			{
				((SubscribableActor<T, S>)obj).unregister(action, func);
			}
		}
		
		public void onActorStart(SubscribableActor<T, S> actor)
		{
			foreach (KeyValuePair<T, List < Action < Object >>> entry in callbacks)
			{
				foreach (Action<Object> action in entry.Value.ToList())
				{
					if (SelectorStore<T>.get._selectors[entry.Key][action].registerCondition(actor.gameObject))
					{
						actor.register(entry.Key, action);
					}
				}
			}
		}
	}
}