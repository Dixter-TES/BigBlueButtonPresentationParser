using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigBlueButtonPresentationParser
{
    public static class MathExtension
    {
        public static double Lerp(double a, double b, double t) => a + (b - a) * t;

        public static double InverseLerp(double a, double b, double value) => Clamp01((value - a) / (b - a));

        public static double Clamp01(double value)
        {
            if(value < 0)
                return 0;

            if(value > 1)
                return 1;

            return value;
        }
    }
}
