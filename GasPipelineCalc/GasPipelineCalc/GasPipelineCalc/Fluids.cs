using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace GasPipelineCalc
{
    public class Components
    {
        public List<double> MoleFractions { get; set; }
        public Components(string path)
        {
            using (StreamReader file = File.OpenText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                Components components = (Components)serializer.Deserialize(file, typeof(Components));
                MoleFractions = components.MoleFractions;
            };
        }
    }
    class Mixture
    {
        public const double R = 0.0083675;
        public List<double> ACritical { get; set; }
        public List<double> B { get; set; }
        public List<double> M { get;set }
        public List<double> Alpha { get; set; }
        public List<double> A { get; set; }


        public Mixture()
        {

        }


        public void SetCoefficients(List<double> list, List<double> moleFractions, int param)
        {

        }


        public static double Formula(Components components, int param)
        {
            switch (param)
            {
                case 1:
                    return 1;
                    break;
            }
        }


        public static double ACriticalFormula(double temp, double pres)
        {
            return 1;
        }
    }
}
