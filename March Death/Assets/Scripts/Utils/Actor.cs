using System;
using System.Collections.Generic;
using UnityEngine;

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
    public abstract class SubscribableActor<T, S> : MonoBehaviour where T : struct, IConvertible where S : class
    {
        private Dictionary<T, List<Action<GameObject>>> callbacks = new Dictionary<T, List<Action<GameObject>>>();

        public SubscribableActor()
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            foreach (T action in Enum.GetValues(typeof(T)))
            {
                callbacks.Add(action, new List<Action<GameObject>>());
            }
        }

        public virtual void Start()
        {
            Subscriber<T, S>.get.onActorStart(this);
        }

        public void register(T action, Action<GameObject> func)
        {
            callbacks[action].Add(func);
        }

        public void unregister(T action, Action<GameObject> func)
        {
            callbacks[action].Remove(func);
        }

        protected void fire(T action)
        {
            foreach (Action<GameObject> func in callbacks[action])
            {
                func.Invoke(gameObject);
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
                callbacks.Add(action, new List<Action<GameObject>>());
            }
        }

        private Dictionary<T, List<Action<GameObject>>> callbacks = new Dictionary<T, List<Action<GameObject>>>();

        public void registerForAll(T action, Action<GameObject> func)
        {
            callbacks[action].Add(func);
        }

        public void onActorStart(SubscribableActor<T, S> actor)
        {
            Debug.Log(actor);
            foreach (KeyValuePair<T, List < Action < GameObject >>> entry in callbacks)
            {
                foreach (Action<GameObject> action in entry.Value)
                {
                    actor.register(entry.Key, action);
                }
            }
        }
    }
}
