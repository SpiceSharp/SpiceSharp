using System;

namespace Spice2SpiceSharp
{
    public class Program
    {
        /// <summary>
        /// Main program
        /// </summary>
        /// <param name="args">Arguments</param>
        public static void Main(string[] args)
        {
            SpiceDevice dev = new SpiceDevice();
            // dev.Folder = @"D:\Visual Studio\Info\SpiceSharp\spice3f5\src\lib\dev\mos3"; // MOS2
            dev.Folder = @"D:\Visual Studio\Info\SpiceSharp\ftpv321\ftpv321\src";
            dev.ITF = @"bsim3itf.h";
            dev.Def = @"bsim3def.h";
            dev.Defined.AddRange(new string[] {
                "AN_pz",
                "AN_noise",
                "NOBYPASS",
                "NEWTRUNC",
                "NEWCONV",
                "PREDICTOR",
                "DEV_bsim3"
            });

            // Generate
            SpiceClassGenerator scg = new SpiceClassGenerator(dev);
            scg.ExportModel("model.cs");
            scg.ExportDevice("device.cs");

            foreach (var msg in ConverterWarnings.Warnings)
                Console.WriteLine("Warning: " + msg);
            Console.WriteLine("Conversion finished");
            Console.ReadKey();
        }
    }
}
