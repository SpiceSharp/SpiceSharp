using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using SpiceSharp.Parser;
using SpiceSharp.Parameters;
using System.Collections.Generic;
using SpiceSharp.Components;
using System.Reflection;
using System.Reflection.Emit;

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

            // NetlistReader nr = new NetlistReader();
            // nr.Parse("test.net");

            Console.ReadKey();
        }

        private static void Path_OnSubcircuitPathChanged(object sender, SpiceSharp.Parser.Subcircuits.SubcircuitPathChangedEventArgs e)
        {
            expr.Parameters = e.Parameters;
        }
    }
}