using System;
using System.Linq;
using System.Collections.Generic;

namespace Utils
{
    public sealed class Subscriber<T, S> : Singleton<Subscriber<T, S>> where T : struct, IConvertible where S : class
    {

        private Dictionary<T, List<Action<Object>>> callbacks = new Dictionary<T, List<Action<Object>>>();
        private Dictionary<T, Dictionary<Action<Object>, Func<UnityEngine.GameObject, bool>>> conditions = new Dictionary<T, Dictionary<Action<Object>, Func<UnityEngine.GameObject, bool>>>();

        private Subscriber()
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            foreach (T action in Enum.GetValues(typeof(T)))
            {
                callbacks.Add(action, new List<Action<Object>>());
                conditions.Add(action, new Dictionary<Action<Object>, Func<UnityEngine.GameObject, bool>>());
            }
        }

        public void registerForAll(T action, Action<Object> func)
        {
            registerForAll(action, func, x => true);
        }

        public void registerForAll(T action, Action<Object> func, Func<UnityEngine.GameObject, bool> condition)
        {
            callbacks[action].Add(func);
            conditions[action].Add(func, condition);

            // Find all already existing gameobjects of this type
            UnityEngine.Object[] alreadyExistingActors = UnityEngine.Object.FindObjectsOfType(typeof(SubscribableActor<T,S>));
            foreach (UnityEngine.Object obj in alreadyExistingActors)
            {
                var actor = (SubscribableActor<T, S>)obj;

                if (condition.Invoke(actor.gameObject))
                {
                    actor.register(action, func);
                }
            }
        }

        public void unregisterFromAll(T action, Action<Object> func)
        {
            callbacks[action].Remove(func);

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
                    if (conditions[entry.Key][action].Invoke(actor.gameObject))
                    {
                        actor.register(entry.Key, action);
                    }
                }
            }
        }
    }
}
