using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using SpiceSharp.Parser;
using SpiceSharp.Parameters;
using System.Collections.Generic;

namespace SpiceSharpTest
{
    class Program
    {
        private static SpiceExpression expr = new SpiceExpression();

        /// <summary>
        /// Main method
        /// </summary>
        /// <param name="args">Input arguments</param>
        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();

            sw.Start();
            NetlistReader nr = new NetlistReader();
            sw.Stop();
            Console.WriteLine("Initialization: " + sw.ElapsedMilliseconds);

            SpiceMember.OnSpiceMemberConvert += expr.OnSpiceConvert;
            nr.Netlist.Path.OnSubcircuitPathChanged += Path_OnSubcircuitPathChanged;

            sw.Restart();
            nr.Parse("test.net");
            sw.Stop();
            Console.WriteLine("Parsing: " + sw.ElapsedMilliseconds);

            Console.WriteLine();

            Console.ReadKey();
        }

        private static void Path_OnSubcircuitPathChanged(object sender, SpiceSharp.Parser.Subcircuits.SubcircuitPathChangedEventArgs e)
        {
            expr.Parameters = e.Parameters;
        }
    }
}