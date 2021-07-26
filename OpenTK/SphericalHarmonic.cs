using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace OpenTK
{
    class SphericalHarmonic
    {
        private const string DllFilePath = @"C:\Users\admin\source\repos\SphericalHarmonic\Debug\SphericalHarmonic.DLL";

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        private extern static double spherical_harmonic(int degree, int order, double theta, double phi);

        public static double Spherical_harmonic(int degree, int order, double theta, double phi)
        {
            return spherical_harmonic(degree, order, theta, phi);
        }
        public static void car2sph(double x, double y, double z, out double theta, out double phi)
        {
            phi = Math.Atan2(y, x);
            theta = Math.Atan2(z, Math.Sqrt(x * x + y * y));
            if (phi < 0)
                phi += 2 * Math.PI;
            theta = Math.PI / 2 - theta;
        }
        public static void Sph2Car(double theta, double phi, out double x, out double y, out double z)
        {
            x = Math.Sin(theta) * Math.Sin(phi);
            y = Math.Sin(theta) * Math.Cos(phi);
            z = Math.Cos(theta);
        }
        public static void Reconstruct(double theta, double phi, List<double[]> coefs, int maxDegree, out double x, out double y, out double z)
        {
            x = 0; y = 0; z = 0;
            int coefIndex = 0;
            for (int d = 0; d <= maxDegree; d++)
            {
                for (int order = -d; order <= d; order++)
                {
                    
                    //double SHValue = Spherical_harmonic(d, order, phi, theta);
                    double SHValue = Spherical_harmonic(d, order, phi, theta);
                    x += SHValue * coefs[coefIndex][0];
                    y += SHValue * coefs[coefIndex][1];
                    z += SHValue * coefs[coefIndex][2];
                    //Console.WriteLine(d + "," + order + "," + SHValue + "," + x + "," + y + "," + z);

                    coefIndex++;
                }
            }
        }
    }
}
