using System;

namespace Utils
{
    /// <summary>
    /// Class to
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="K"></typeparam>
    sealed class Tuple<T, K> : IEquatable<Tuple<T, K>>
    {
        readonly public T Key0;
        readonly public K Key1;

        public Tuple(T key0, K key1)
        {
            Key0 = key0;
            Key1 = key1;
        }

        public override int GetHashCode()
        {
            return Key0.GetHashCode() ^ Key1.GetHashCode();
        }

        public bool Equals(Tuple<T, K> obj)
        {
            return Key0.Equals(obj.Key0) && Key1.Equals(obj.Key1);
        }
    }
}
