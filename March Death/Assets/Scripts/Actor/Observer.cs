using System;
using System.Linq;
using System.Collections.Generic;

namespace Utils
{
    public abstract class Observer : UnityEngine.MonoBehaviour, IObserver
    {
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
    }
}
