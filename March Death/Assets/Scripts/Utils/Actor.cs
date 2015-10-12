using System;
using System.Linq;
using System.Collections.Generic;

namespace Utils
{
    public sealed class DummyClass
    {
        private DummyClass() { }
    }

    public abstract class Actor<T> : SubscribableActor<T, DummyClass> where T : struct, IConvertible
    {
        public Actor() { }
        public new virtual void Start() { }
    }

    // T, enum actions
    // S, base class
    public abstract class SubscribableActor<T, S> : UnityEngine.MonoBehaviour where T : struct, IConvertible where S : class
    {
        private Dictionary<T, List<Action<Object>>> callbacks = new Dictionary<T, List<Action<Object>>>();

        public SubscribableActor()
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

        public virtual void Start()
        {
            Subscriber<T, S>.get.onActorStart(this);
        }

        public void register(T action, Action<Object> func)
        {
            callbacks[action].Add(func);
        }

        public void unregister(T action, Action<Object> func)
        {
            callbacks[action].Remove(func);
        }

        protected void fire(T action)
        {
            foreach (Action<Object> func in callbacks[action].ToList())
            {
                func.Invoke(gameObject);
            }
        }

        protected void fire(T action, Object obj)
        {
            foreach (Action<Object> func in callbacks[action].ToList())
            {
                func.Invoke(obj);
            }
        }
    }

    public sealed class Subscriber<T, S> : Singleton<Subscriber<T, S>> where T : struct, IConvertible where S : class
    {

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

        private Dictionary<T, List<Action<Object>>> callbacks = new Dictionary<T, List<Action<Object>>>();

        public void registerForAll(T action, Action<Object> func)
        {
            callbacks[action].Add(func);

            // Find all already existing gameobjects of this type
            UnityEngine.Object[] alreadyExistingActors = UnityEngine.Object.FindObjectsOfType(typeof(SubscribableActor<T,S>));
            foreach (UnityEngine.Object obj in alreadyExistingActors)
            {
                ((SubscribableActor<T, S>)obj).register(action, func);
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
                    actor.register(entry.Key, action);
                }
            }
        }
    }
}
