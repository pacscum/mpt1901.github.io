using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.RootFinding;
using OfficeOpenXml;

namespace GasPipelineCalc
{
    public class Components
    {
        public List<double> MoleFractions { get; set; }
        public double Temperature { get; set; }
        public double Pressure { get; set; }
    }


    internal class Mixture
    {
        private const double R = 0.0083675;
        private List<double> B = new List<double> {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}; // 1
        private List<double> ACritical = new List<double> {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}; // 2
        private List<double> M = new List<double> {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}; // 3
        private List<double> Alpha = new List<double> {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}; // 4
        private List<double> A = new List<double> {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}; // 5

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

            MComponents.MoleFractions[3] = (double) Excel.ws.Cells[Row.row, 3].Value;
            MComponents.MoleFractions[4] = (double) Excel.ws.Cells[Row.row, 4].Value;
            MComponents.MoleFractions[5] = (double) Excel.ws.Cells[Row.row, 5].Value;
            MComponents.MoleFractions[6] = (double) Excel.ws.Cells[Row.row, 6].Value;

            Console.WriteLine($"Температура = \t {Temperature} K");
            Console.WriteLine($"Давление = \t {Pressure} МПа");
            Console.WriteLine();

            for (var index = 0; index < MComponents.MoleFractions.Count; index++)
            {
                Console.WriteLine(
                    $"{Coefficients.InteractionCoeffsNames[index]}: {MComponents.MoleFractions[index]}");
            }

            SetCoefficients(1);
            SetCoefficients(2);
            SetCoefficients(3);
            SetCoefficients(4);
            SetCoefficients(5);

            a = SetA();
            b = SetB();

            for (var index = 0; index < MComponents.MoleFractions.Count; index++)
            {
                if (MComponents.MoleFractions[index] != 0)
                {
                    Console.WriteLine($"Component: {Coefficients.InteractionCoeffsNames[index]} \n" +
                                      $"\t a_critical = {ACritical[index]} +\n" +
                                      $"\t m = {M[index]} + \n" +
                                      $"\t alpha = {Alpha[index]} +\n" +
                                      $"\t a_i = {A[index]} + \n" +
                                      $"\t b_i = {B[index]}");
                }
            }
        }


        public void SetCoefficients(int param)
        {
            for (var index = 0; index < MComponents.MoleFractions.Count; index++)
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
                        Alpha[index] = AlphaFormula(t, Temperature, M[index], MComponents.MoleFractions[index]);
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
            var rv = 0.0;

            for (var i = 0; i < MComponents.MoleFractions.Count; i++)
            {
                for (var j = 0; j < MComponents.MoleFractions.Count; j++)
                {
                    var tmpRv = 0.0;
                    tmpRv = rv;

                    rv += MComponents.MoleFractions[i] *
                          MComponents.MoleFractions[j] *
                          (1 - Coefficients.Interaction[i, j]) *
                          Math.Sqrt(A[i] * A[j]);
                }
            }

            return rv;
        }


        public double SetB()
        {
            var rv = 0.0;

            for (var i = 0; i < MComponents.MoleFractions.Count; i++)
            {
                rv += MComponents.MoleFractions[i] * B[i];
            }

            return rv;
        }


        public double ACriticalFormula(double temp, double pres)
        {
            if (temp == 0 || pres == 0)
            {
                return 0;
            }

            return 0.457235 * Math.Pow(R, 2) * Math.Pow(temp, 2) / pres;
        }


        public double BFormula(double temp, double pres)
        {
            if (temp == 0 || pres == 0)
            {
                return 0;
            }

            return 0.077796 * R * temp / pres;
        }


        public double MFormula(double omega)
        {
            if (omega == 0)
            {
                return 0;
            }

            return 0.37464 + 1.54226 * omega - 0.26992 * Math.Pow(omega, 2);
        }


        public double AlphaFormula(double tempCritical, double temp, double m, double c)
        {
            if (tempCritical == 0 || m == 0 || c == 0)
            {
                return 0;
            }

            return Math.Pow(1 + m * (1 - Math.Sqrt(temp / tempCritical)), 2);
        }


        public double AFormula(double alpha, double aCritical)
        {
            if (alpha == 0 || aCritical == 0)
            {
                return 0;
            }

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
                // Console.WriteLine("!!!!!!!!!!!!!!!!");
                // Console.WriteLine(A);
                // Console.WriteLine(B);
                // Console.WriteLine("!!!!!!!!!!!!!!!!");

                var a0 = A * B - Math.Pow(B, 2) - Math.Pow(B, 3);
                var a1 = A - 3 * Math.Pow(B, 2) - 2 * B;
                var a2 = 1 - B;

                var _roots = Cubic.RealRoots(-a0, a1, -a2);
                var roots = new List<double> {_roots.Item1, _roots.Item2, _roots.Item3};

                for (var i = 0; i < 3; i++)
                {
                    if (roots[i] < 0)
                    {
                        roots[i] = 0;
                    }
                }

                return Math.Round(roots.Max(), 3);
            }
        }


        public class Cp
        {
            Mixture Mixture;
            Mixture.Z Z;

            List<double> Cp0_Fractions = new List<double> {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
            public double Cp0 { get; set; }


            public List<double> dAlphadt_list = new List<double> {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
            public List<double> d2Alphadt2_list = new List<double> {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
            public List<double> dadt_list = new List<double> {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
            public List<double> d2adt2_list = new List<double> {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};


            public double DADT { get; set; }
            public double D2ADT2 { get; set; }
            public double DPDTV { get; set; }
            public double DPDTT { get; set; }

            public double Value { get; set; }

            public Cp(Mixture mix, Mixture.Z Z)
            {
                Mixture = mix;
                this.Mixture.Volume = Z.Value * R * Mixture.Temperature / Mixture.Pressure;
#if DEBUG
                Console.WriteLine($"mix.a {mix.a}");
                Console.WriteLine($"mix.b {mix.b}");
                Console.WriteLine($"mix.Pressure {mix.Pressure}");
                Console.WriteLine($"mix.Volume {mix.Volume}");
                Console.WriteLine($"mix.Temperature {mix.Temperature}");
                mix.MComponents.MoleFractions.ForEach(Console.WriteLine);
#endif


                Cp0 = CalcCp0();
#if DEBUG
                Console.WriteLine($"Cp0 {Cp0}");
#endif
                CalcDAlphaDT(1);
                CalcDAlphaDT(2);
#if DEBUG
                Console.WriteLine("dAlpha/dt list");
                dAlphadt_list.ForEach(Console.WriteLine);
                Console.WriteLine("d2Alpha/dt_2 list");
                d2Alphadt2_list.ForEach(Console.WriteLine);
#endif

                for (var i = 0; i < Mixture.MComponents.MoleFractions.Count; i++)
                {
                    dadt_list[i] = Mixture.ACritical[i] * dAlphadt_list[i];
                    d2adt2_list[i] = Mixture.ACritical[i] * d2Alphadt2_list[i];
                }

                DADT = CalcDADT();
                D2ADT2 = CalcD2ADT2();
#if DEBUG
                Console.WriteLine($"DA/DT = {DADT}");
                Console.WriteLine($"D2A/DT2 = {D2ADT2}");
#endif

                DPDTV = CalcDPDTV();
                DPDTT = CalcDPDTT();
#if DEBUG
                Console.WriteLine($"DPDTV = {DPDTV}");
                Console.WriteLine($"DPDTT = {DPDTT}");
#endif
                Value = CalcCp();
            }


            public double CalcCp0()
            {
                for (var index = 0; index < Mixture.MComponents.MoleFractions.Count; index++)
                {
                    if (Mixture.MComponents.MoleFractions[index] == 0)
                    {
                        Cp0_Fractions[index] = 0;
                        continue;
                    }

                    Cp0_Fractions[index] = 4.1868 *
                                           (Coefficients.Bc[index] + 2 * Coefficients.Cc[index] * 0.001 * 1.8 *
                                            Mixture.Temperature +
                                            3 * Coefficients.Dc[index] * 1e-6 * Math.Pow(1.8 * Mixture.Temperature, 2) +
                                            4 * Coefficients.Ec[index] * 1e-10 *
                                            Math.Pow(1.8 * Mixture.Temperature, 3) +
                                            5 * Coefficients.Fc[index] * 1e-14 *
                                            Math.Pow(1.8 * Mixture.Temperature, 4));
                }

                var rv = 0.0;

                for (var index = 0; index < Mixture.MComponents.MoleFractions.Count; index++)
                {
                    rv += Mixture.MComponents.MoleFractions[index] * Cp0_Fractions[index];
                }

                return rv;
            }


            public double CalcdAlpha2dt2(double m, double tCrit)
            {
                return 0.5 * m * (1 + m) * (1 / Math.Sqrt(tCrit)) * Math.Pow(Mixture.Temperature, -1.5);
            }


            public double CalcdAlphadt(double m, double tCrit)
            {
                return -1 * (1 / Math.Sqrt(Mixture.Temperature * tCrit)) *
                       (1 + m * (1 - Math.Sqrt(Mixture.Temperature / tCrit)));
            }


            public void CalcDAlphaDT(int degree)
            {
                for (var i = 0; i < Mixture.MComponents.MoleFractions.Count; i++)
                {
                    var m = Mixture.M[i];
                    var tc = CriticalParameters.Temperature[i];
                    var t = Mixture.Temperature;

                    if (Mixture.MComponents.MoleFractions[i] == 0)
                    {
                        dAlphadt_list[i] = 0;
                        continue;
                    }

                    switch (degree)
                    {
                        case 1:
                            if (Cp0_Fractions[i] == 0)
                            {
                                dAlphadt_list[i] = 0;
                                continue;
                            }

                            dAlphadt_list[i] = -m * (1 / Math.Sqrt(t * tc)) * (1 + m * (1 - Math.Sqrt(t / tc)));
                            break;
                        case 2:
                            if (Cp0_Fractions[i] == 0)
                            {
                                d2Alphadt2_list[i] = 0;
                                continue;
                            }

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

                for (var i = 0; i < Mixture.MComponents.MoleFractions.Count; i++)
                {
                    for (var j = 0; j < Mixture.MComponents.MoleFractions.Count; j++)
                    {
                        if (Mixture.MComponents.MoleFractions[i] == 0 || Mixture.MComponents.MoleFractions[j] == 0)
                        {
                            continue;
                        }

                        var c = Coefficients.Interaction[i, j];
                        var nui = Mixture.MComponents.MoleFractions[i];
                        var nuj = Mixture.MComponents.MoleFractions[j];
                        var ai = Mixture.A[i];
                        var aj = Mixture.A[j];
                        var dai = dadt_list[i];
                        var daj = dadt_list[j];
#if DEBUG
                        Console.WriteLine($"DA/DT for i = {i} and j = {j}");
                        Console.WriteLine();
                        Console.WriteLine($"{c}, {nui}, {nuj}, {ai}, {aj}, {dai}, {daj}");
#endif

                        rv += 0.5 * (1 - c) * nui * nuj * (Math.Sqrt(aj / ai) * dai + Math.Sqrt(ai / aj) * daj);
                    }
                }

                return rv;
            }


            public double CalcD2ADT2()
            {
                var rv = 0.0;

                for (var i = 0; i < Mixture.MComponents.MoleFractions.Count; i++)
                {
                    for (var j = 0; j < Mixture.MComponents.MoleFractions.Count; j++)
                    {
                        if (Cp0_Fractions[i] == 0 || Cp0_Fractions[j] == 0)
                        {
                            continue;
                        }


                        var c = Coefficients.Interaction[i, j];
                        var nui = Mixture.MComponents.MoleFractions[i];
                        var nuj = Mixture.MComponents.MoleFractions[j];
                        var ai = Mixture.A[i];
                        var aj = Mixture.A[j];
                        var dai = dadt_list[i];
                        var daj = dadt_list[j];
                        var d2ai = d2adt2_list[i];
                        var d2aj = d2adt2_list[j];

#if DEBUG
                        Console.WriteLine($"D2A/DT2 for i = {i} and j = {j}");
                        Console.WriteLine();
                        Console.WriteLine($"{c}, {nui}, {nuj}, {ai}, {aj}, {dai}, {daj}, {d2ai}, {d2aj}");
#endif


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
                         Math.Log((Mixture.Volume + (Math.Sqrt(2) + 1) * Mixture.b) /
                                  (Mixture.Volume - (Math.Sqrt(2) - 1) * Mixture.b));
                return rv;
            }


            public double CalcDPDTV()
            {
                var V = this.Mixture.Volume;
                var b = this.Mixture.b;
                var rv = Mixture.R / (V - b) - DADT / (V * (V + b) + b * (V - b));
                return rv;
            }


            public double CalcDPDTT()
            {
                var rv = 0.0;
                rv = -1 * R * Mixture.Temperature / Math.Pow(Mixture.Volume - Mixture.b, 2) +
                     2 * Mixture.a * (Mixture.Volume + Mixture.b) /
                     Math.Pow(Mixture.Volume * (Mixture.Volume + Mixture.b) + Mixture.b * (Mixture.Volume - Mixture.b),
                         2);
                return rv;
            }


            public double CalcDeltaCp()
            {
                var dCv = CalcDeltaCv();
                var dPdTv = CalcDPDTV();
                var dPdTt = CalcDPDTT();

                var dCp = dCv - (Mixture.Temperature * Math.Pow(dPdTv, 2) / dPdTt) - R;
                return dCp;
            }


            public double CalcCp()
            {
                return Cp0 + CalcDeltaCp();
            }
        }


        public class D
        {
            private Mixture Mix;
            private Cp Cp;
            private Z Z;

            public double Value { get; set; }
            public double Value2 { get; set; }
            public double Value3 { get; set; }

            public D(Mixture _mix, Cp _cp, Z _z)
            {
                Mix = _mix;
                Cp = _cp;
                Z = _z;

                Value = -1 * (Mix.Temperature / (Cp.Value * 1000)) * (
                            (Z.Value * R / Mix.Pressure) + Cp.DPDTV / Cp.DPDTT);

                var dvdt = (Cp.DPDTV / Cp.DPDTV);
                var v = Z.Value * 8.31 * Mix.Temperature / (Mix.Pressure*1e6);
                Console.WriteLine($"v = {v}");
                Value2 = (Mix.Temperature * dvdt - v) / Cp.Value;
                var dvdt_p = 1 / ((Mix.Temperature / (v - Mix.b)) - 2 * Mix.a * (v - Mix.b) /
                                  (R * Math.Pow(v, 3)));
                Value3 = (1 / Cp.Value/1000) * (v - Mix.Temperature * dvdt_p);
#if DEBUG
                Console.WriteLine($"Mix.Temperature = {Mix.Temperature}");
                Console.WriteLine($"Cp.Value = {Cp.Value}");
                Console.WriteLine($"Z.Value = {Z.Value}");
                Console.WriteLine($"R = {R}");
                Console.WriteLine($"Mix.Pressure = {Mix.Pressure}");
                Console.WriteLine($"Cp.DPDTV = {Cp.DPDTV}");
                Console.WriteLine($"Cp.DPDTT = {Cp.DPDTT}");


#endif
            }
        }
    }
}