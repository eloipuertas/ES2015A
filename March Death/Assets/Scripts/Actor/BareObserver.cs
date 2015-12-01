using System;
using System.Linq;
using System.Collections.Generic;

namespace Utils
{
    public abstract class BareObserver<T> : IObserver, IActor<T> where T : struct, IConvertible
    {
        private Dictionary<T, List<Action<Object>>> callbacks = new Dictionary<T, List<Action<Object>>>();
        private List<AutoUnregister> autoUnregisters = new List<AutoUnregister>();

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

        public void unregister(AutoUnregister auto)
        {
            autoUnregisters.Remove(auto);
        }

        public BareObserver()
        {
#if UNITY_EDITOR
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }
#endif

            foreach (T action in Enum.GetValues(typeof(T)))
            {
                callbacks.Add(action, new List<Action<Object>>());
            }
        }

        public RegisterResult<T> register(T action, Action<Object> func)
        {
            callbacks[action].Add(func);
            return new RegisterResult<T>(this, action, func);
        }

        public IKeyGetter unregister<A>(A action, Action<Object> func)
        {
            T realAction = (T)Convert.ChangeType(action, typeof(T));
            callbacks[realAction].Remove(func);

            return new RegisterResult<T>(this, realAction, func);
        }

        protected void fire(T action)
        {
            foreach (Action<Object> func in callbacks[action].ToList())
            {
                func.Invoke(this);
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
