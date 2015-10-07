using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Utils
{
    using System;

    /// <summary>
    /// Base class for a Singleton class
    /// </summary>
    /// <typeparam name="T">Base class</typeparam>
    public abstract class Singleton<T> where T : class
    {
        private static volatile T instance;
        private static object syncRoot = new Object();

        protected Singleton() { }
        
        /// <summary>
        /// Returns an instance of the class. If none has yet been created
        /// locks and creates it.
        /// Otherwise, the previously created object is returned
        /// </summary>
        public static T get
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            // HACK: Using reflection to get the default constructor might no be the best idea
                            var constructor = typeof(T).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
                            if (constructor == null)
                                throw new ArgumentException("No default argument found for " + typeof(T));

                            instance = (T)constructor.Invoke(null);
                        }
                    }
                }

                return instance;
            }
        }
    }

}
