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
            plotOutput.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastPoint;
            var plotRef = chMain.Series.Add("Spice 3f5");
            plotRef.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;

            double[] reft = { 0.000000000000000e+00, 1.000000000000000e-11, 2.000000000000000e-11, 4.000000000000000e-11, 8.000000000000001e-11, 1.600000000000000e-10, 3.200000000000000e-10, 6.400000000000001e-10, 1.280000000000000e-09, 2.560000000000000e-09, 5.120000000000001e-09, 1.024000000000000e-08, 2.048000000000000e-08, 4.096000000000000e-08, 8.192000000000001e-08, 1.638400000000000e-07, 3.276800000000000e-07, 6.553600000000001e-07, 1.310720000000000e-06, 2.621440000000000e-06, 5.242880000000001e-06, 9.999999999999999e-06, 1.007393971207461e-05, 1.022181913622384e-05, 1.051757798452229e-05, 1.091897888633419e-05, 1.172178068995800e-05, 1.314180861296600e-05, 1.532877008098056e-05, 1.879934982908853e-05, 2.459263936906224e-05, 3.617921844900963e-05, 5.617921844900963e-05, 7.617921844900963e-05, 9.617921844900963e-05, 1.161792184490096e-04, 1.361792184490096e-04, 1.561792184490096e-04, 1.761792184490096e-04, 1.961792184490096e-04, 2.161792184490096e-04, 2.361792184490096e-04, 2.561792184490096e-04, 2.761792184490096e-04, 2.961792184490096e-04, 3.161792184490096e-04, 3.361792184490096e-04, 3.561792184490096e-04, 3.761792184490096e-04, 3.961792184490096e-04, 4.161792184490096e-04, 4.361792184490096e-04, 4.561792184490096e-04, 4.761792184490096e-04, 4.961792184490097e-04, 5.161792184490097e-04, 5.361792184490098e-04, 5.561792184490098e-04, 5.761792184490099e-04, 5.961792184490099e-04, 6.161792184490100e-04, 6.361792184490100e-04, 6.561792184490101e-04, 6.761792184490101e-04, 6.961792184490102e-04, 7.161792184490102e-04, 7.361792184490103e-04, 7.561792184490103e-04, 7.761792184490104e-04, 7.961792184490104e-04, 8.161792184490105e-04, 8.361792184490106e-04, 8.561792184490106e-04, 8.761792184490107e-04, 8.961792184490107e-04, 9.161792184490108e-04, 9.361792184490108e-04, 9.561792184490109e-04, 9.761792184490109e-04, 9.961792184490109e-04, 1.000000000000000e-03 };
            double[] refv = { 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, 1.062884564151630e-02, 3.112725266832868e-02, 6.787655011041410e-02, 1.098046833004332e-01, 1.723878794355580e-01, 2.366546464022079e-01, 2.750203140900375e-01, 2.829582375053465e-01, 2.690664614523476e-01, 2.397632261553791e-01, 1.971256999778316e-01, 1.617852815525827e-01, 1.329690526768618e-01, 1.091607123593418e-01, 8.969769526949078e-02, 7.365035892214453e-02, 6.050999999955608e-02, 4.969024767309361e-02, 4.082092563792295e-02, 3.352428625644566e-02, 2.753879376007540e-02, 2.261740576906793e-02, 1.857851971490070e-02, 1.525888198702412e-02, 1.253371969603800e-02, 1.029438683787516e-02, 8.455719792876633e-03, 6.945073673196496e-03, 5.704562097326969e-03, 4.685460974988563e-03, 3.848529057783312e-03, 3.161019727495129e-03, 2.596376683174123e-03, 2.132562200753686e-03, 1.751624200255417e-03, 1.438718942335832e-03, 1.181719417168424e-03, 9.706218336282765e-04, 7.972378974102767e-04, 6.548231844159739e-04, 5.378505065018704e-04, 4.417717814795211e-04, 3.628568138490980e-04, 2.980381452713743e-04, 2.447986501501069e-04, 2.010692714961289e-04, 1.651515968835799e-04, 1.356499189147252e-04, 1.114183058612167e-04, 9.151522687941098e-05, 7.516753932782038e-05, 6.174007820947462e-05, 5.071122743502418e-05, 4.165249154186664e-05, 3.421195684590642e-05, 2.810054812098083e-05, 2.308084535637467e-05, 1.895782873283595e-05, 1.826048844380424e-05 };

            // Create circuit
            double r1 = 100.0;
            double r2 = 500.0;
            double l1 = 10e-3;
            double l2 = 2e-3;
            double mut = 0.693;
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new Voltagesource("V1", "IN", "0", new Pulse(0, 1, 1e-6, 1e-12, 1e-12, 1, 2)),
                new Resistor("R1", "IN", "1", r1),
                new Inductor("L1", "1", "0", l1),
                new Inductor("L2", "OUT", "0", l2),
                new Resistor("R2", "OUT", "0", r2),
                new MutualInductance("M1", "L1", "L2", mut)
                );
            ckt.Nodes.IC["1"] = 0;
            ckt.Objects["L1"].Parameters.Set("ic", 0);

            // Create simulation
            Transient tran = new Transient("tran", 1e-9, 1e-3);

            // Create exports
            Func<State, double>[] exports = new Func<State, double>[1];
            tran.InitializeSimulationExport += (object sender, InitializationDataEventArgs args) =>
            {
                exports[0] = tran.CreateVoltageExport("OUT");
            };

            // Create references
            Func<double, double>[] references = { (double t) => 0.0 };

            // Run simulation
            tran.OnExportSimulationData += (object sender, SimulationDataEventArgs args) =>
            {
                double actual = exports[0](args.Circuit.State);
                double expected = references[0](args.GetTime());
                plotInput.Points.AddXY(args.GetTime(), actual);
                plotOutput.Points.AddXY(args.GetTime(), expected);
            };
            tran.Run(ckt);

            for (int i = 0; i < reft.Length; i++)
                plotRef.Points.AddXY(reft[i], refv[i]);
        }

        /// <summary>
        /// Create a diode with a model
        /// </summary>
        /// <param name="name">Diode name</param>
        /// <param name="anode">Anode</param>
        /// <param name="cathode">Cathode</param>
        /// <param name="model">Model</param>
        /// <param name="modelparams">Model parameters</param>
        /// <returns></returns>
        protected static Diode CreateDiode(Identifier name, Identifier anode, Identifier cathode, Identifier model, string modelparams)
        {
            Diode d = new Diode(name);
            DiodeModel dm = new DiodeModel(model);
            ApplyParameters(dm, modelparams);
            d.SetModel(dm);
            d.Connect(anode, cathode);
            return d;
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