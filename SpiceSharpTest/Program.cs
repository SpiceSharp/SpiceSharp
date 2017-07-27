using System;
using System.Collections.Generic;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System.IO;
using Spice2SpiceSharp;

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
            /* SpiceDevice dev = new SpiceDevice();
            dev.Defined.AddRange(new string[] { "DEV_bsim4", "AN_pz", "AN_noise", "NOBYPASS" });
            dev.Folder = @"D:\Visual Studio\Info\SpiceSharp\BSIM480\BSIM480_Code";
            dev.ITF = @"bsim4itf.h";
            dev.Def = @"bsim4def.h";
            SpiceClassGenerator scg = new SpiceClassGenerator(dev);
            scg.ExportModel("model.cs");
            scg.ExportDevice("device.cs");

            // Show all the warnings
            Console.WriteLine("Warnings:");
            foreach (string msg in ConverterWarnings.Warnings)
                Console.WriteLine(msg); */

            Console.ReadKey();
        }
    }
}