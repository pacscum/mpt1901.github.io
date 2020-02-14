using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;
using OfficeOpenXml;


namespace GasPipelineCalc
{
    public static class Excel
    {
        public static OfficeOpenXml.ExcelWorksheet ws;
        static Excel()
        {
            var excel_path = new FileInfo(@"C:\_site\mpt1901.github.io\GasPipelineCalc\GasPipelineCalc\GasPipelineCalc\bin\Release\Calcs.xlsx");
            var excel_file = new ExcelPackage(excel_path);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ws = excel_file.Workbook.Worksheets[0];
        }
    }
    public static class Row
    {
        public static int row;
    }
    class Program
    {
        
        public const double R = 0.0083675;

        private static void Main(string[] args)
        {
            // FileStream filestream = new FileStream("out.txt", FileMode.Create, FileAccess.Write);
            // var streamwriter = new StreamWriter(filestream);
            // Console.SetOut(streamwriter);
            
            var path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var directory = Path.GetDirectoryName(path);
            path = string.Concat(directory, "\\CalcSettings.json");

            

            for (Row.row = 3; Row.row < 11; Row.row++)
            {
                Console.WriteLine($"Вариант № {Row.row - 2}");
                Console.WriteLine($"-----------------------------------");

                Components components = null;

                using (var file = File.OpenText(path))
                {
                    var serializer = new JsonSerializer();
                    var _components = (Components) serializer.Deserialize(file, typeof(Components));
                    components = _components;
                }

                var mix = new Mixture(components);
                
                Console.WriteLine("_____________________");

                Console.WriteLine($"Коэффициент a для смеси = {mix.a}");
                Console.WriteLine($"Коэффициент b для смеси = {mix.b}");

                var Z = new Mixture.Z(mix);
                var Cp = new Mixture.Cp(mix, Z);
                var D = new Mixture.D(mix, Cp, Z);

                Console.WriteLine($"Коэффициент сжимаемости Z = {Z.Value}");
                Console.WriteLine($"Теплоемкость Cp = {Cp.Value}");
                Console.WriteLine(D.Value);
                Console.WriteLine(D.Value2);
                Console.WriteLine(D.Value3);
                Console.WriteLine("\n \n");
            }

        }
    }
}