using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;


namespace GasPipelineCalc
{

    class Program
    {

        public const double R = 0.0083675;

        private static void Main(string[] args)
        {

            var path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var directory = Path.GetDirectoryName(path);
            path = string.Concat(directory, "\\CalcSettings.json");

            Components components = null;

            using (var file = File.OpenText(path))
            {
                var serializer = new JsonSerializer();
                var _components = (Components)serializer.Deserialize(file, typeof(Components));
                components = _components;
            };


            var mix = new Mixture(components);

            Console.WriteLine("----- Исходные данные ----- \n");
            Console.WriteLine($"Температура = \t {mix.Temperature} K");
            Console.WriteLine($"Давление = \t {mix.Pressure} МПа");
            Console.WriteLine();
            for (var index = 0; index < mix.MComponents.MoleFractions.Count; index++)
            {
                Console.WriteLine($"{Coefficients.InteractionCoeffsNames[index]}: {mix.MComponents.MoleFractions[index]}");
            }
            Console.WriteLine("--------------------------- \n \n");

            Console.WriteLine($"Коэффициент a для смеси = {mix.a}");
            Console.WriteLine($"Коэффициент b для смеси = {mix.b}");

            var Z = new Mixture.Z(mix);

            Console.WriteLine($"\nКоэффициент сжимаемости Z = {Z.Value}");



            Console.ReadKey();
        }
    }
}
