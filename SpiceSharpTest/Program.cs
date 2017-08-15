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
        /// <summary>
        /// Main method
        /// </summary>
        /// <param name="args">Input arguments</param>
        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();

            NetlistReader nr = new NetlistReader();
            sw.Start();
            nr.Parse("test.net");
            sw.Stop();
            Console.WriteLine("Time taken to parse: " + sw.ElapsedMilliseconds + " ms");

            nr = new NetlistReader();
            sw.Restart();
            nr.Parse("test.net");
            sw.Stop();
            Console.WriteLine("Time taken to parse (2): " + sw.ElapsedMilliseconds + " ms");

            Console.WriteLine("Parsing finished");
            Console.ReadKey();
        }
    }
}