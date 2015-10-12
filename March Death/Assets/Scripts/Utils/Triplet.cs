using System;

namespace Utils
{
    /// <summary>
    /// Class to store three related items in a single object
    /// Might be used for three key dictionaries
    /// </summary>
    /// <typeparam name="T0">Type of the first key</typeparam>
    /// <typeparam name="T1">Type of the second key</typeparam>
    /// <typeparam name="T1">Type of the third key</typeparam>
    public class Triplet<T0, T1, T2> : IEquatable<Triplet<T0, T1, T2>>
    {
        readonly public T0 Key0;
        readonly public T1 Key1;
        readonly public T2 Key2;

        public Triplet(T0 key0, T1 key1, T2 key2)
        {
            Key0 = key0;
            Key1 = key1;
            Key2 = key2;
        }

        public override int GetHashCode()
        {
            return Key0.GetHashCode() ^ Key1.GetHashCode() ^ Key2.GetHashCode();
        }

        public bool Equals(Triplet<T0, T1, T2> obj)
        {
            return Key0.Equals(obj.Key0) && Key1.Equals(obj.Key1) && Key2.Equals(obj.Key2);
        }
    }
}
