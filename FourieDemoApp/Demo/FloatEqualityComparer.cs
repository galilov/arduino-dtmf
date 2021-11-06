using System;
using System.Collections.Generic;

namespace Demo
{
    internal struct FloatEqualityComparer : IEqualityComparer<float>
    {
        private const float Epsilon = 0.001f;
        public bool Equals(float x, float y)
        {
            return Math.Abs(x - y) < Epsilon;
        }

        public int GetHashCode(float obj)
        {
            var a = obj / Epsilon;
            var ia = (int) a;
            var b = a - ia;
            if (Math.Abs(b) < 0.5f)
            {
                return ia;
            }

            return ia + Math.Sign(b);
        }
    }
}