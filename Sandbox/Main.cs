using System;
using System.Numerics;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharp.Circuits;

namespace Sandbox
{
    public partial class Main : Form
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Main()
        {
            InitializeComponent();
            var plotInput = chMain.Series.Add("Input");
            plotInput.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastPoint;
            var plotOutput = chMain.Series.Add("Output");
            plotOutput.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;

            // Create circuit
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new Voltagesource("V1", "in", "0", new Pulse(1, 5, 1e-6, 1e-9, 0.5e-6, 2e-6, 6e-6)),
                new Voltagesource("Vsupply", "vdd", "0", 5),
                new Resistor("R1", "out", "vdd", 1.0e3),
                CreateMOS1("M1", "out", "in", "0", "0",
                    "MM", "IS=1e-32 VTO=3.03646 LAMBDA=0 KP=5.28747 CGSO=6.5761e-06 CGDO=1e-11")
                );
            ckt.Objects["M1"].Parameters.Set("w", 100e-6);
            ckt.Objects["M1"].Parameters.Set("l", 100e-6);

            // Create simulation
            Transient tran = new Transient("tran", 1e-9, 10e-6);

            // Create exports
            Func<State, double>[] exports = new Func<State, double>[1];
            tran.InitializeSimulationExport += (object sender, InitializationDataEventArgs args) =>
            {
                exports[0] = tran.CreateVoltageExport("out");
            };

            // Create references
            double[] reft = { 0.000000000000000e+00, 1.000000000000000e-11, 2.000000000000000e-11, 4.000000000000000e-11, 8.000000000000001e-11, 1.600000000000000e-10, 3.200000000000000e-10, 6.400000000000001e-10, 1.280000000000000e-09, 2.560000000000000e-09, 5.120000000000001e-09, 1.024000000000000e-08, 2.048000000000000e-08, 4.096000000000000e-08, 8.192000000000001e-08, 1.638400000000000e-07, 3.276800000000000e-07, 5.276800000000000e-07, 7.276800000000000e-07, 9.276800000000000e-07, 1.000000000000000e-06, 1.000100000000000e-06, 1.000300000000000e-06, 1.000505416909167e-06, 1.000916250727500e-06, 1.001000000000000e-06, 1.001040727433433e-06, 1.001122182300300e-06, 1.001285092034034e-06, 1.001610911501501e-06, 1.002262550436436e-06, 1.003565828306307e-06, 1.006172384046046e-06, 1.011385495525526e-06, 1.021811718484486e-06, 1.042664164402405e-06, 1.084369056238244e-06, 1.167778839909921e-06, 1.334598407253275e-06, 1.534598407253275e-06, 1.734598407253275e-06, 1.934598407253275e-06, 2.134598407253274e-06, 2.334598407253274e-06, 2.534598407253274e-06, 2.734598407253274e-06, 2.934598407253274e-06, 3.001000000000000e-06, 3.021000000000000e-06, 3.061000000000000e-06, 3.141000000000000e-06, 3.234987431524397e-06, 3.325439370636820e-06, 3.420856173579678e-06, 3.501000000000000e-06, 3.511070126950724e-06, 3.531210380852173e-06, 3.571490888655072e-06, 3.652051904260869e-06, 3.813173935472463e-06, 4.013173935472463e-06, 4.213173935472463e-06, 4.413173935472463e-06, 4.613173935472462e-06, 4.813173935472462e-06, 5.013173935472462e-06, 5.213173935472462e-06, 5.413173935472462e-06, 5.613173935472461e-06, 5.813173935472461e-06, 6.013173935472461e-06, 6.213173935472461e-06, 6.413173935472460e-06, 6.613173935472460e-06, 6.813173935472460e-06, 7.000000000000000e-06, 7.000100000000000e-06, 7.000300000000000e-06, 7.000505416909549e-06, 7.000916250728647e-06, 7.001000000000000e-06, 7.001040727433111e-06, 7.001122182299335e-06, 7.001285092031782e-06, 7.001610911496677e-06, 7.002262550426466e-06, 7.003565828286043e-06, 7.006172384005199e-06, 7.011385495443511e-06, 7.021811718320133e-06, 7.042664164073380e-06, 7.084369055579872e-06, 7.167778838592857e-06, 7.334598404618826e-06, 7.534598404618826e-06, 7.734598404618826e-06, 7.934598404618826e-06, 8.134598404618826e-06, 8.334598404618826e-06, 8.534598404618825e-06, 8.734598404618825e-06, 8.934598404618825e-06, 9.000999999999999e-06, 9.020999999999999e-06, 9.060999999999998e-06, 9.140999999999998e-06, 9.234987431524396e-06, 9.325439370636819e-06, 9.420856173579678e-06, 9.500999999999999e-06, 9.511070126950724e-06, 9.531210380852173e-06, 9.571490888655071e-06, 9.652051904260870e-06, 9.813173935472464e-06, 9.999999999999999e-06 };
            double[] refv = { 4.999999995000000e+00, 4.999999995000000e+00, 4.999999995000000e+00, 4.999999995000000e+00, 4.999999995000000e+00, 4.999999995000000e+00, 4.999999995000000e+00, 4.999999995000000e+00, 4.999999995000000e+00, 4.999999995000000e+00, 4.999999995000000e+00, 4.999999995000000e+00, 4.999999995000000e+00, 4.999999995000000e+00, 4.999999995000000e+00, 4.999999995000000e+00, 4.999999995000000e+00, 4.999999995000000e+00, 4.999999995000000e+00, 4.999999995000000e+00, 4.999999995000000e+00, 5.003960391035683e+00, 5.004038814719480e+00, 5.003961923904320e+00, 5.822926169346066e-04, 4.810560374122478e-04, 4.816080793679862e-04, 4.816080824751261e-04, 4.816080793679973e-04, 4.816080824751207e-04, 4.816080793680000e-04, 4.816080824751193e-04, 4.816080793680006e-04, 4.816080824751191e-04, 4.816080793680011e-04, 4.816080824751189e-04, 4.816080793680010e-04, 4.816080824751191e-04, 4.816080793680009e-04, 4.816080824751189e-04, 4.816080793680010e-04, 4.816080824751189e-04, 4.816080793680010e-04, 4.816080824751189e-04, 4.816080793680010e-04, 4.816080824751189e-04, 4.816080793680010e-04, 4.816080824751188e-04, 5.243402581575843e-04, 6.374704766839068e-04, 1.121520168458353e-03, -1.720170200560100e+01, 4.999968188570249e+00, 5.000015800431791e+00, 4.999968190756332e+00, 4.999999991842038e+00, 4.999999998157336e+00, 4.999999991842977e+00, 4.999999998156865e+00, 4.999999991843213e+00, 4.999999998156724e+00, 4.999999991843340e+00, 4.999999998156598e+00, 4.999999991843466e+00, 4.999999998156472e+00, 4.999999991843592e+00, 4.999999998156345e+00, 4.999999991843718e+00, 4.999999998156220e+00, 4.999999991843845e+00, 4.999999998156093e+00, 4.999999991843971e+00, 4.999999998155968e+00, 4.999999991844097e+00, 4.999999998155841e+00, 4.999999991844228e+00, 5.003960391004448e+00, 5.004038814750084e+00, 5.003961923874295e+00, 5.822926152947390e-04, 4.810560374114067e-04, 4.816080793679895e-04, 4.816080824751229e-04, 4.816080793680006e-04, 4.816080824751175e-04, 4.816080793680033e-04, 4.816080824751161e-04, 4.816080793680040e-04, 4.816080824751158e-04, 4.816080793680041e-04, 4.816080824751156e-04, 4.816080793680043e-04, 4.816080824751156e-04, 4.816080793680043e-04, 4.816080824751157e-04, 4.816080793680042e-04, 4.816080824751155e-04, 4.816080793680042e-04, 4.816080824751155e-04, 4.816080793680042e-04, 4.816080824751155e-04, 4.816080793680042e-04, 4.816080824751157e-04, 5.243402581575805e-04, 6.374704766839011e-04, 1.121520168458326e-03, -1.720170200561121e+01, 4.999968188570249e+00, 5.000015800431791e+00, 4.999968190756332e+00, 4.999999991842038e+00, 4.999999998157336e+00, 4.999999991842977e+00, 4.999999998156865e+00, 4.999999991843213e+00, 4.999999998156719e+00 };

            // Run simulation
            tran.OnExportSimulationData += (object sender, SimulationDataEventArgs args) =>
            {
                double actual = exports[0](args.Circuit.State);
                plotInput.Points.AddXY(args.GetTime(), actual);
            };
            tran.Run(ckt);

            // Reference
            for (int i = 0; i < reft.Length; i++)
            {
                plotOutput.Points.AddXY(reft[i], refv[i]);
            }
        }

        /// <summary>
        /// Create a MOSFET
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="d">Drain</param>
        /// <param name="g">Gate</param>
        /// <param name="s">Source</param>
        /// <param name="b">Bulk</param>
        /// <param name="modelname">Model name</param>
        /// <param name="modelparams">Model parameters</param>
        /// <returns></returns>
        static MOS1 CreateMOS1(Identifier name, Identifier d, Identifier g, Identifier s, Identifier b,
            Identifier modelname, string modelparams)
        {
            // Create model
            MOS1Model model = new MOS1Model(modelname);
            ApplyParameters(model, modelparams);

            // Create mosfet
            MOS1 mos = new MOS1(name);
            mos.SetModel(model);
            mos.Connect(d, g, s, b);
            return mos;
        }

        /// <summary>
        /// Apply a parameter definition to an entity
        /// Parameters are a series of assignments [name]=[value] delimited by spaces.
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="definition">Definition string</param>
        protected static void ApplyParameters(Entity entity, string definition)
        {
            // Get all assignments
            definition = Regex.Replace(definition, @"\s*\=\s*", "=");
            string[] assignments = definition.Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var assignment in assignments)
            {
                // Get the name and value
                string[] parts = assignment.Split('=');
                if (parts.Length != 2)
                    throw new Exception("Invalid assignment");
                string name = parts[0].ToLower();
                double value = double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);

                // Set the entity parameter
                entity.Parameters.Set(name, value);
            }
        }
    }
}