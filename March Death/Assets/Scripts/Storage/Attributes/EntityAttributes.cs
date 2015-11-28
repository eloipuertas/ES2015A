using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Storage
{
    public class EntityAttributes
    {
        public int capacity;
        public int resistance;
        public int wounds;
        public int sightRange;
		public int creationTime = 10;
		public float autoRecoveryRate = 0.2f;
    }
}
