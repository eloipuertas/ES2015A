using System;
using System.Linq;
using System.Collections.Generic;

namespace Utils
{
    public abstract class Actor<T> : UnityEngine.MonoBehaviour, IObserver, IActor<T>, IBaseActor where T : struct, IConvertible
    {
        private Dictionary<T, List<Action<Object>>> callbacks = new Dictionary<T, List<Action<Object>>>();
        private List<AutoUnregister> autoUnregisters = new List<AutoUnregister>();

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

        public virtual void OnDestroy()
        {
            foreach (AutoUnregister auto in autoUnregisters.ToList())
            {
                auto.unregisterAll();
            }

            // This should always be true, as AutoUnregister.unregisterAll
            // automatically unregisters itself from the actor
            UnityEngine.Debug.Assert(autoUnregisters.Count == 0);
        }

        public void register(AutoUnregister auto)
        {
            autoUnregisters.Add(auto);
        }

        public RegisterResult<T> register(T action, Action<Object> func)
        {
            callbacks[action].Add(func);
            return new RegisterResult<T>(this, action, func);
        }

        public void unregister(AutoUnregister auto)
        {
            autoUnregisters.Remove(auto);
        }

        public IKeyGetter unregister<A>(A action, Action<Object> func)
        {
            T realAction = (T)Convert.ChangeType(action, typeof(T));
            callbacks[realAction].Remove(func);

            return new RegisterResult<T>(this, realAction, func);
        }

        public void unregisterAll(Action<Object> func)
        {
            foreach (var pair in callbacks)
            {
                foreach (var action in pair.Value.ToList())
                {
                    if (action == func)
                    {
                        callbacks[pair.Key].Remove(action);
                    }
                }
            }
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
