using System;
using System.Linq;
using System.Collections.Generic;

namespace Utils
{
    public sealed class SelectorStore<T> : Singleton<SelectorStore<T>> where T : struct, IConvertible
    {
        public static ActorSelector defaultSelector = new ActorSelector()
        {
            registerCondition = gameObject => true,
            fireCondition = gameObject => true
        };

        public Dictionary<T, Dictionary<Action<Object>, ActorSelector>> _selectors = new Dictionary<T, Dictionary<Action<Object>, ActorSelector>>();

        public ActorSelector Selector(T action, Action<Object> func)
        {
            if (!_selectors[action].ContainsKey(func))
            {
                return defaultSelector;
            }

            return _selectors[action][func];
        }

        private SelectorStore()
        {
    #if UNITY_EDITOR
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }
    #endif

            foreach (T action in Enum.GetValues(typeof(T)))
            {
                _selectors.Add(action, new Dictionary<Action<Object>, ActorSelector>());
            }
        }
    }
}
