using System;
using System.Security.Cryptography;
using UnityEngine;

namespace Utils
{
    class D6 : Singleton<D6>
    {
        private readonly RNGCryptoServiceProvider _generator = new RNGCryptoServiceProvider();

        private D6() { }

        public double roll()
        {
            byte[] val = new byte[1];
            _generator.GetBytes(val);
            return Convert.ToDouble(val[0]) / 255.0;
        }

        public int rollN(int n)
        {
            double res = roll();

            for (int i = 1; i < n; ++i)
            {
                if (res <= i / (double)n)
                {
                    return i - 1;
                }
            }

            return n - 1;
        }

        // Less expensive version of "rollN(6) + 1"
        public int rollOnce()
        {
            double res = roll();

            if (res <= 1.0 / 6.0) return 1;
            if (res <= 2.0 / 6.0) return 2;
            if (res <= 3.0 / 6.0) return 3;
            if (res <= 4.0 / 6.0) return 4;
            if (res <= 5.0 / 6.0) return 5;

            return 6;
        }

        public int rollSpecial()
        {
            int value = rollOnce();

            if (value == 6)
            {
                switch (rollOnce())
                {
                    case 6: return 9;
                    case 5: return 8;
                    case 4: return 7;
                }
            }

            return value;
        }

        public int rollAccumulate()
        {
            int value = rollOnce();
            int total = value;

            while (value == 6)
            {
                value = rollOnce();
                total += value;
            }

            return total;
        }
    }
}
