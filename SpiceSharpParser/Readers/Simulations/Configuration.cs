using System;
using System.Collections.Generic;
using SpiceSharp.Simulations;

namespace SpiceSharp.Parser.Readers.Simulations
{
    public static class Configuration
    {
        /// <summary>
        /// The default configurations (doubles)
        /// </summary>
        public static Dictionary<string, double> Defaults { get; } = new Dictionary<string, double>()
        {
            { "abstol", 1e-12 }, // Absolute tolerance
            { "reltol", 1e-3 }, // Relative tolerance
            { "vntol", 1e-6 }, // Voltage tolerance
            { "gmin", 1e-12 }, // Minimum conductance for PN junctions
            { "itl1", 50 }, // DC OP iterations
            { "itl2", 50 }, // TRCV sweep iterations
            { "itl4", 10 }, // Transient max iterations
            { "temp", 27.0 }, // Circuit temperature
            { "tnom", 27.0 } // Nominal measurement temperature
        };

        /// <summary>
        /// The default configuration (strings)
        /// </summary>
        public static Dictionary<string, string> StringDefaults { get; } = new Dictionary<string, string>()
        {
            { "method", "trap" }
        };

        /// <summary>
        /// Default flags
        /// </summary>
        public static HashSet<string> FlagDefaults { get; } = new HashSet<string>();

        /// <summary>
        /// Set the defaults to the current setup
        /// </summary>
        /// <param name="config">The configuration</param>
        /// <param name="ckt">The circuit</param>
        public static void Initialize(SimulationConfiguration config)
        {
            foreach (var item in Defaults)
            {
                switch (item.Key)
                {
                    case "abstol": config.AbsTol = item.Value; break;
                    case "reltol": config.RelTol = item.Value; break;
                    case "vntol": config.VoltTol = item.Value; break;
                    case "gmin": config.Gmin = item.Value; break;

                    case "itl1":
                        if (config is AC.Configuration)
                            (config as AC.Configuration).DcMaxIterations = (int)(item.Value + 0.25);
                        if (config is Transient.Configuration)
                            (config as Transient.Configuration).DcMaxIterations = (int)(item.Value + 0.25);
                        break;

                    case "itl2":
                        if (config is DC.Configuration)
                            (config as DC.Configuration).MaxIterations = (int)(item.Value + 0.25);
                        break;

                    case "itl4":
                        if (config is Transient.Configuration)
                            (config as Transient.Configuration).TranMaxIterations = (int)(item.Value + 0.25);
                        break;
                }
            }

            foreach (var item in StringDefaults)
            {
                switch (item.Key)
                {
                    case "method":
                        switch (item.Value)
                        {
                            case "trap":
                            case "trapezoidal":
                                if (config is Transient.Configuration)
                                    (config as Transient.Configuration).Method = new IntegrationMethods.Trapezoidal();
                                break;
                        }
                        break;
                }
            }

            foreach (var item in FlagDefaults)
            {
                switch (item)
                {
                    case "keepopinfo":
                        if (config is AC.Configuration)
                            (config as AC.Configuration).KeepOpInfo = true;
                        break;
                }
            }
        }

        /// <summary>
        /// Initialize a circuit
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public static void Initialize(Circuit ckt)
        {
            if (Defaults.ContainsKey("temp"))
                ckt.State.Temperature = Defaults["temp"] + Circuit.CONSTCtoK;
            if (Defaults.ContainsKey("tnom"))
                ckt.State.NominalTemperature = Defaults["tnom"] + Circuit.CONSTCtoK;
        }
    }
}
