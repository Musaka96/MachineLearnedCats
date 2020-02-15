using System;

namespace MLAgents.InferenceBrain.Utils
{
    /// <summary>
    /// RandomNormal - A random number generator that produces normally distributed random
    /// numbers using the Marsaglia polar method:
    /// https://en.wikipedia.org/wiki/Marsaglia_polar_method
    /// TODO: worth overriding System.Random instead of aggregating?
    /// </summary>
    public class RandomNormal
    {
        readonly double m_Mean;
        readonly double m_Stddev;
        readonly Random m_Random;

        public RandomNormal(int seed, float mean = 0.0f, float stddev = 1.0f)
        {
            this.m_Mean = mean;
            this.m_Stddev = stddev;
            this.m_Random = new Random(seed);
        }

        // Each iteration produces two numbers. Hold one here for next call
        bool m_HasSpare;
        double m_SpareUnscaled;

        /// <summary>
        /// Return the next random double number
        /// </summary>
        /// <returns>Next random double number</returns>
        public double NextDouble()
        {
            if (this.m_HasSpare)
            {
                this.m_HasSpare = false;
                return this.m_SpareUnscaled * this.m_Stddev + this.m_Mean;
            }

            double u, v, s;
            do
            {
                u = this.m_Random.NextDouble() * 2.0 - 1.0;
                v = this.m_Random.NextDouble() * 2.0 - 1.0;
                s = u * u + v * v;
            }
            while (s >= 1.0 || Math.Abs(s) < double.Epsilon);

            s = Math.Sqrt(-2.0 * Math.Log(s) / s);
            this.m_SpareUnscaled = u * s;
            this.m_HasSpare = true;

            return v * s * this.m_Stddev + this.m_Mean;
        }
    }
}
