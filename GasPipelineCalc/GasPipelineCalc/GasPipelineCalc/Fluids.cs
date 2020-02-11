using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.RootFinding;

namespace GasPipelineCalc
{
    public class Components
    {
        public List<double> MoleFractions { get; set; }
        public double Temperature { get; set; }
        public double Pressure { get; set; }

    }


    class Mixture
    {
        public const double R = 0.0083675;
        public List<double> B = new List<double> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };      // 1
        public List<double> ACritical = new List<double> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };   // 2
        public List<double> M = new List<double> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };          // 3
        public List<double> Alpha = new List<double> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };      // 4
        public List<double> A = new List<double> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };         // 5

        public Components MComponents;

        public double Temperature;
        public double Pressure;
        public double Volume;

        public double a;
        public double b;


        public Mixture(Components components)
        {
            MComponents = components;

            this.Temperature = components.Temperature;
            this.Pressure = components.Pressure;

            SetCoefficients(1);
            SetCoefficients(2);
            SetCoefficients(3);
            SetCoefficients(4);
            SetCoefficients(5);

            a = SetA();
            b = SetB();
        }


        public void SetCoefficients(int param)
        {
            for (int index = 0; index < MComponents.MoleFractions.Count; index++)
            {
                var t = CriticalParameters.Temperature[index];
                var p = CriticalParameters.Pressure[index];

                switch (param)
                {
                    case 1:
                        B[index] = BFormula(t, p);
                        break;
                    case 2:
                        ACritical[index] = ACriticalFormula(t, p);
                        break;
                    case 3:
                        M[index] = MFormula(Coefficients.AcentricFactor[index]);
                        break;
                    case 4:
                        Alpha[index] = AlphaFormula(t, Temperature, M[index]);
                        break;
                    case 5:
                        A[index] = AFormula(Alpha[index], ACritical[index]);
                        break;
                    default:
                        break;
                }
            }
        }


        public double SetA()
        {
#if DEBUG
            Console.WriteLine("A");
            foreach(var item in A)
            {
                Console.Write($"{item}, ");
            }
            Console.WriteLine();
            Console.WriteLine("Alpha");
            foreach (var item in Alpha)
            {
                Console.Write($"{item}, ");
            }
            Console.WriteLine();
            Console.WriteLine("M");
            foreach (var item in M)
            {
                Console.Write($"{item}, ");
            }
            Console.WriteLine();
            Console.WriteLine("ACrit");
            foreach (var item in ACritical)
            {
                Console.Write($"{item}, ");
            }
            Console.WriteLine();
            Console.WriteLine("B");
            foreach (var item in B)
            {
                Console.Write($"{item}, ");
            }
            Console.WriteLine();
#endif
            var rv = 0.0;

            for (int i = 0; i < MComponents.MoleFractions.Count; i++)
            {
                for (int j = 0; j < MComponents.MoleFractions.Count; j++)
                {
#if DEBUG
                    Console.WriteLine($"i = {i}; j = {j} \n" +
                        $"mu[i] = {MComponents.MoleFractions[i]} \n" +
                        $"mu[j] = {MComponents.MoleFractions[j]} \n" +
                        $"C[i,j] = {Coefficients.Interaction[i, j]} \n" +
                        $"A[i] = {A[i]} \n" +
                        $"A[j] = {A[j]}");
#endif
                    var tmpRv = 0.0;
                    tmpRv = rv;

                    rv += MComponents.MoleFractions[i] *
                            MComponents.MoleFractions[j] *
                            (1 - Coefficients.Interaction[i, j]) *
                            Math.Sqrt(A[i] * A[j]);
#if DEBUG
                    Console.WriteLine($"rv = {rv-tmpRv}");
                    Console.WriteLine();
#endif
                }
            }

            return rv;
        }


        public double SetB()
        {
            var rv = 0.0;

            for (int i = 0; i < MComponents.MoleFractions.Count; i++)
            {
                rv += MComponents.MoleFractions[i] * B[i];
            }

            return rv;
        }


        public double ACriticalFormula(double temp, double pres)
        {
            if (temp == 0 || pres == 0) { return 0; }
            return 0.457235 * Math.Pow(R, 2) * Math.Pow(temp, 2) / pres;
        }


        public double BFormula(double temp, double pres)
        {
            if (temp == 0 || pres == 0) { return 0; }
            return 0.077796 * R * temp / pres;
        }


        public double MFormula(double omega)
        {
            if (omega == 0) { return 0; }
            return 0.37464 + 1.54226 * omega - 0.26992 * Math.Pow(omega, 2);
        }


        public double AlphaFormula(double tempCritical, double temp, double m)
        {
#if DEBUG
            Console.WriteLine($"ALphaFormula: Tc = {tempCritical} // temp = {temp} // m = {m}");
#endif
            if (tempCritical == 0 || m == 0) { return 0; }
            return Math.Pow(1 + m * (1 - Math.Sqrt(temp / tempCritical)), 2);
        }


        public double AFormula(double alpha, double aCritical)
        {
            if (alpha == 0 || aCritical == 0) { return 0; }
            return aCritical * alpha;
        }


        public class Z
        {
            public double Value;

            Mixture Mixture;


            public Z(Mixture mix)
            {
                Mixture = mix;

                Value = Calculate();
            }


            public double Calculate()
            {
                var r = R;
                var p = Mixture.Pressure;
                var t = Mixture.Temperature;

                var A = Mixture.a * p / (Math.Pow(r, 2) * Math.Pow(t, 2));
                var B = Mixture.b * p / (r * t);

                var a0 = A * B - Math.Pow(B, 2) - Math.Pow(B, 3);
                var a1 = A - 3 * Math.Pow(B, 2) - 2 * B;
                var a2 = 1 - B;

                var _roots = Cubic.RealRoots(-a0, a1, -a2);
                List<double> roots = new List<double> { _roots.Item1, _roots.Item2, _roots.Item3 };

                for (int i = 0; i < 3; i++)
                {
                    if (roots[i] < 0)
                    {
                        roots[i] = 0;
                    }
                }
#if DEBUG
                Console.WriteLine(roots.Max());
#endif
                return Math.Round(roots.Max(), 3);
            }
        }


        public class Cp
        {
            Mixture Mixture;
            Mixture.Z Z;

            List<double> Cp0_Fractions = new List<double> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            public double Cp0 { get; set; }


            public List<double> dAlphadt_list = new List<double> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            public List<double> d2Alphadt2_list = new List<double> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            public List<double> dadt_list = new List<double> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            public List<double> d2adt2_list = new List<double> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };


            public double DADT { get; set; }
            public double D2ADT2 { get; set; }
            public double DPDTV { get; set; }
            public double DPDTT { get; set; }



            public Cp(Mixture mix, Mixture.Z Z)
            {
                Mixture = mix;
                Cp0 = SetCp0();
                CalcDAlphaDT(1);
                CalcDAlphaDT(2);

                for (int i = 0; i < Mixture.MComponents.MoleFractions.Count; i++)
                {
                    dadt_list[i] = Mixture.ACritical[i] * dAlphadt_list[i];
                    d2adt2_list[i] = Mixture.ACritical[i] * d2Alphadt2_list[i];
                }

                this.Mixture.Volume = Z.Value * R * Mixture.Temperature / Mixture.Pressure;

            }


            public double SetCp0()
            {
                for (int index = 0; index < Mixture.MComponents.MoleFractions.Count; index++)
                {
                    if (Mixture.MComponents.MoleFractions[index] == 0) { Cp0_Fractions[index] = 0; continue; }
                    Cp0_Fractions[index] = 4.1868 *
                                (Coefficients.Bc[index] + 2 * Coefficients.Cc[index] * 0.001 * 1.8 * Mixture.Temperature +
                                    3 * Coefficients.Dc[index] * 1e-6 * Math.Pow(1.8 * Mixture.Temperature, 2) +
                                    4 * Coefficients.Ec[index] * 1e-10 * Math.Pow(1.8 * Mixture.Temperature, 3) +
                                    5 * Coefficients.Fc[index] * 1e-14 * Math.Pow(1.8 * Mixture.Temperature, 4));
                }

                var rv = 0.0;

                for (int index = 0; index < Mixture.MComponents.MoleFractions.Count; index++)
                {
                    rv += Mixture.MComponents.MoleFractions[index] * Cp0_Fractions[index];
                }

                return rv;
            }


            public double Setd2Alphadt2(double m, double tCrit)
            {
                return 0.5 * m * (1 + m) * (1 / Math.Sqrt(tCrit)) * Math.Pow(Mixture.Temperature, -1.5);
            }


            public double SetdAlphadt(double m, double tCrit)
            {
                return -1 * (1 / Math.Sqrt(Mixture.Temperature * tCrit)) * (1 + m * (1 - Math.Sqrt(Mixture.Temperature / tCrit)));
            }


            public void CalcDAlphaDT(int degree)
            {
                for (int i = 0; i < Mixture.MComponents.MoleFractions.Count; i++)
                {
                    var m = Mixture.M[i];
                    var tc = CriticalParameters.Temperature[i];
                    var t = Mixture.Temperature;

                    switch (degree)
                    {
                        case 1:
                            dAlphadt_list[i] = -m * (1 / Math.Sqrt(t * tc)) * (1 + m * (1 - Math.Sqrt(t / tc)));
                            break;
                        case 2:
                            d2Alphadt2_list[i] = 0.5 * m * (1 + m) * (1 / Math.Sqrt(tc)) * Math.Pow(t, -1.5);
                            break;
                        default:
                            break;
                    }
                }
            }


            public double CalcDADT()
            {
                var rv = 0.0;

                for (int i = 0; i < Mixture.MComponents.MoleFractions.Count; i++)
                {
                    for (int j = 0; j < Mixture.MComponents.MoleFractions.Count; j++)
                    {
                        var c = Coefficients.Interaction[i, j];
                        var nui = Mixture.MComponents.MoleFractions[i];
                        var nuj = Mixture.MComponents.MoleFractions[j];
                        var ai = Mixture.A[i];
                        var aj = Mixture.A[j];
                        var dai = dadt_list[i];
                        var daj = dadt_list[j];

                        rv += 0.5 * (1 - c) * nui * nuj * (Math.Sqrt(aj / ai) * dai + Math.Sqrt(ai / aj) * daj);
                    }
                }

                return rv;
            }


            public double CalcD2ADT2()
            {
                var rv = 0.0;

                for (int i = 0; i < Mixture.MComponents.MoleFractions.Count; i++)
                {
                    for (int j = 0; j < Mixture.MComponents.MoleFractions.Count; j++)
                    {
                        var c = Coefficients.Interaction[i, j];
                        var nui = Mixture.MComponents.MoleFractions[i];
                        var nuj = Mixture.MComponents.MoleFractions[j];
                        var ai = Mixture.A[i];
                        var aj = Mixture.A[j];
                        var dai = dadt_list[i];
                        var daj = dadt_list[j];
                        var d2ai = d2adt2_list[i];
                        var d2aj = d2adt2_list[j];

                        rv += (1 - c) * nui * nuj * (
                            Math.Sqrt(aj / ai) * d2ai + Math.Sqrt(ai / aj) * d2aj +
                            0.5 * Math.Sqrt(ai / aj) * dai * ((ai * daj - aj * dai) / Math.Pow(ai, 2)) +
                            0.5 * Math.Sqrt(aj / ai) * daj * ((aj * dai - ai * daj) / Math.Pow(aj, 2)));
                        rv *= 0.5;
                    }
                }

                return rv;
            }


            public double CalcDeltaCv()
            {
                var rv = Mixture.Temperature * D2ADT2 * (1 / (2 * Math.Sqrt(2) * Mixture.b)) *
                    Math.Log((Mixture.Volume + (Math.Sqrt(2) + 1) * Mixture.b) / (Mixture.Volume - (Math.Sqrt(2) - 1) * Mixture.b));
                return rv;
            }


            public double CalcDPDTV()
            {
                var V = this.Mixture.Volume;
                var b = Mixture.b;
                var rv = Mixture.R / (V - b) - DADT / (V * (V + b) + Mixture.b * (V - b));
                return rv;
            }


            //public double Calc
        }
    }
}
