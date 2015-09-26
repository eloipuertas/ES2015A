using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public abstract class Actor<T> : MonoBehaviour where T : struct, IConvertible
    {
        private Dictionary<T, List<Action<GameObject>>> callbacks = new Dictionary<T, List<Action<GameObject>>>();

        public Actor()
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
}
