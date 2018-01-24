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
                CreateMOS3("M1", "out", "in", "vdd", "vdd",
                    "DMOS", false, "VTO = -0.7 KP = 3.8E+1 THETA = .25 VMAX = 3.5E5"),
                new Resistor("R1", "out", "0", 100e3),
                new Voltagesource("Vsupply", "vdd", "0", 1.8),
                new Voltagesource("V1", "in", "0", new Pulse(0, 1.8, 1e-6, 1e-9, 0.5e-6, 2e-6, 6e-6))
                );
            ckt.Objects["M1"].Parameters.Set("w", 1e-6);
            ckt.Objects["M1"].Parameters.Set("l", 1e-6);

            // Create simulation
            Transient tran = new Transient("tran", 1e-9, 10e-6);

            // Create exports
            Func<State, double>[] exports = new Func<State, double>[2];
            tran.InitializeSimulationExport += (object sender, InitializationDataEventArgs args) =>
            {
                exports[0] = (State state) => tran.Method.Time;
                exports[1] = tran.CreateVoltageExport("out");
            };

            // Create references
            double[][] references = new double[2][];
            references[0] = new double[] { 0.000000000000000e+00, 1.000000000000000e-11, 2.000000000000000e-11, 4.000000000000000e-11, 8.000000000000001e-11, 1.600000000000000e-10, 3.200000000000000e-10, 6.400000000000001e-10, 1.280000000000000e-09, 2.560000000000000e-09, 5.120000000000001e-09, 1.024000000000000e-08, 2.048000000000000e-08, 4.096000000000000e-08, 8.192000000000001e-08, 1.638400000000000e-07, 3.276800000000000e-07, 5.276800000000000e-07, 7.276800000000000e-07, 9.276800000000000e-07, 1.000000000000000e-06, 1.000100000000000e-06, 1.000300000000000e-06, 1.000700000000000e-06, 1.001000000000000e-06, 1.001080000000000e-06, 1.001240000000000e-06, 1.001560000000000e-06, 1.002200000000000e-06, 1.003480000000000e-06, 1.006040000000000e-06, 1.011160000000001e-06, 1.021400000000002e-06, 1.041880000000003e-06, 1.082840000000006e-06, 1.164760000000012e-06, 1.328600000000025e-06, 1.528600000000025e-06, 1.728600000000025e-06, 1.928600000000025e-06, 2.128600000000025e-06, 2.328600000000025e-06, 2.528600000000024e-06, 2.728600000000024e-06, 2.928600000000024e-06, 3.001000000000000e-06, 3.021000000000000e-06, 3.061000000000000e-06, 3.141000000000000e-06, 3.301000000000000e-06, 3.501000000000000e-06, 3.520999999999999e-06, 3.560999999999999e-06, 3.641000000000000e-06, 3.801000000000000e-06, 4.000999999999999e-06, 4.200999999999999e-06, 4.400999999999999e-06, 4.600999999999999e-06, 4.800999999999999e-06, 5.000999999999998e-06, 5.200999999999998e-06, 5.400999999999998e-06, 5.600999999999998e-06, 5.800999999999997e-06, 6.000999999999997e-06, 6.200999999999997e-06, 6.400999999999997e-06, 6.600999999999997e-06, 6.800999999999996e-06, 7.000000000000000e-06, 7.000100000000000e-06, 7.000300000000000e-06, 7.000700000000000e-06, 7.001000000000000e-06, 7.001079999999999e-06, 7.001239999999999e-06, 7.001559999999999e-06, 7.002199999999999e-06, 7.003479999999999e-06, 7.006039999999998e-06, 7.011159999999996e-06, 7.021399999999993e-06, 7.041879999999985e-06, 7.082839999999971e-06, 7.164759999999943e-06, 7.328599999999885e-06, 7.528599999999885e-06, 7.728599999999886e-06, 7.928599999999885e-06, 8.128599999999885e-06, 8.328599999999885e-06, 8.528599999999885e-06, 8.728599999999885e-06, 8.928599999999884e-06, 9.000999999999999e-06, 9.020999999999999e-06, 9.060999999999998e-06, 9.140999999999998e-06, 9.300999999999998e-06, 9.500999999999998e-06, 9.520999999999997e-06, 9.560999999999997e-06, 9.640999999999996e-06, 9.800999999999996e-06, 9.999999999999999e-06 };
            references[1] = new double[] { 1.799999451299451e+00, 1.799999453635131e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999389971110e+00, 1.799999183568271e+00, 7.448095922434256e-02, -7.448046003580762e-02, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 1.799998636447238e+00, 1.800000994575628e+00, 1.799999453635142e+00, 1.799999453635142e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999389971109e+00, 1.799999183568270e+00, 7.448095922456173e-02, -7.448046003602679e-02, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 1.799998636447239e+00, 1.800000994575627e+00, 1.799999453635142e+00, 1.799999453635142e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00 };

            // Map nodes
            ckt.Nodes.Map("out");
            ckt.Nodes.Map("in");
            ckt.Nodes.Map("vdd");

            // Execute simulation
            int index = 0;
            tran.OnExportSimulationData += (object sender, ExportDataEventArgs args) =>
            {
                double t = tran.Method.Time;
                double actual = exports[1](tran.State);
                double expected = references[1][index++];
                plotInput.Points.AddXY(t, actual);
                plotOutput.Points.AddXY(t, expected);
            };
            tran.Run(ckt);
        }

        /// <summary>
        /// Create a MOS3 transistor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="d">Drain</param>
        /// <param name="g">Gate</param>
        /// <param name="s">Source</param>
        /// <param name="b">Bulk</param>
        /// <param name="modelname">Model name</param>
        /// <param name="nmos">True for NMOS, false for PMOS</param>
        /// <param name="modelparams">Model parameters</param>
        /// <returns></returns>
        static MOS3 CreateMOS3(Identifier name, Identifier d, Identifier g, Identifier s, Identifier b,
            Identifier modelname, bool nmos, string modelparams)
        {
            // Create model
            MOS3Model model = new MOS3Model(modelname, nmos);
            ApplyParameters(model, modelparams);

            // Create transistor
            MOS3 mos = new MOS3(name);
            mos.Connect(d, g, s, b);
            mos.SetModel(model);
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