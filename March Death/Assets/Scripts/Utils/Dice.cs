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
            double step = (1.0 / n) + Double.Epsilon;

            return (int)(res / step);
        }

        // Less expensive version of "rollN(6) + 1"
        public int rollOnce()
        {
            return rollN(6) + 1;
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
