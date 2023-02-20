using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBBPresentationParserUpdater
{
    public static class MathExtension
    {
        public static double Lerp(double a, double b, double t) => a + t * (b - a);

        public static double InverseLerp(double a, double b, double v) => (v - a) / (b - a);

        public static double Clamp(double value) => value > 1 ? 1 : value < 0 ? 0 : value;
    }
}
