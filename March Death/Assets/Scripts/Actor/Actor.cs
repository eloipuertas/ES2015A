using System;
using System.Linq;
using System.Collections.Generic;

namespace Utils
{
    public abstract class Actor<T> : UnityEngine.MonoBehaviour, IActor<T> where T : struct, IConvertible
    {
        private Dictionary<T, List<Action<Object>>> callbacks = new Dictionary<T, List<Action<Object>>>();

        public Actor()
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

        public virtual void Start() { }
        public virtual void Update() { }

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
}
