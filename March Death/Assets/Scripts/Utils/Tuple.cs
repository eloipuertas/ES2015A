using System;

namespace Utils
{
    /// <summary>
    /// Class to store two related items in a single object
    /// Might be used for two key dictionaries
    /// </summary>
    /// <typeparam name="T0">Type of the first key</typeparam>
    /// <typeparam name="T1">Type of the second key</typeparam>
    public class Tuple<T0, T1> : IEquatable<Tuple<T0, T1>>
    {
        readonly public T0 Key0;
        readonly public T1 Key1;

        public Tuple(T0 key0, T1 key1)
        {
            Key0 = key0;
            Key1 = key1;
        }

        public override int GetHashCode()
        {
            return Key0.GetHashCode() ^ Key1.GetHashCode();
        }

        public bool Equals(Tuple<T0, T1> obj)
        {
            return Key0.Equals(obj.Key0) && Key1.Equals(obj.Key1);
        }
    }
}
