using System;
using System.Linq;
using System.Collections.Generic;

namespace Utils
{
    public abstract class Observer<T> : UnityEngine.MonoBehaviour, IObserver<T> where T : struct, IConvertible
    {
        private List<AutoUnregister<T>> autoUnregisters = new List<AutoUnregister<T>>();

        public virtual void OnDestroy()
        {
            foreach (AutoUnregister<T> auto in autoUnregisters.ToList())
            {
                auto.unregisterAll();
            }

            // This should always be true, as AutoUnregister.unregisterAll
            // automatically unregisters itself from the actor
            UnityEngine.Debug.Assert(autoUnregisters.Count == 0);
        }

        public void register(AutoUnregister<T> auto)
        {
            autoUnregisters.Add(auto);
        }

        public void unregister(AutoUnregister<T> auto)
        {
            autoUnregisters.Remove(auto);
        }
    }
}
