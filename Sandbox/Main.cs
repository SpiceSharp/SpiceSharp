using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharp.NewSparse;

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

            var output = chMain.Series.Add("Output");
            output.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            var reference = chMain.Series.Add("Reference");
            reference.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastPoint;

            /*
             * Pulsed voltage source towards a resistive voltage divider between 0V and 5V
             * Output voltage is expected to behavior like the reference
             */
            // Build circuit
            Circuit ckt = new Circuit();
            ckt.Objects.Add(
                new VoltageSource("V1", "in", "0", new Pulse(0, 5, 1e-6, 10e-9, 10e-9, 1e-6, 2e-6)),
                new VoltageSource("Vsupply", "vdd", "0", 5.0),
                new Resistor("R1", "vdd", "out", 10.0e3),
                new Resistor("R2", "out", "0", 10.0e3),
                CreateDiode("D1", "in", "out", "1N914", "Is = 2.52e-9 Rs = 0.568 N = 1.752 Cjo = 4e-12 M = 0.4 tt = 20e-9"));

            // Create simulation
            Transient tran = new Transient("tran", 1e-9, 10e-6);

            // Create exports
            Export<double>[] exports = { new RealVoltageExport(tran, "out") };

            // Create references
            double[][] references =
            {
                new double[] { 2.499987387600927e+00, 2.499987387600927e+00, 2.499987387600927e+00, 2.499987387600931e+00, 2.499987387600930e+00, 2.499987387600930e+00, 2.499987387600929e+00, 2.499987387600930e+00, 2.499987387600933e+00, 2.499987387600932e+00, 2.499987387600934e+00, 2.499987387600934e+00, 2.499987387600928e+00, 2.499987387600928e+00, 2.499987387600927e+00, 2.499987387600927e+00, 2.499987387600926e+00, 2.499987387600928e+00, 2.499987387600927e+00, 2.499987387600927e+00, 2.499987387600927e+00, 2.961897877874934e+00, 3.816739529558831e+00, 5.191526587205253e+00, 6.033962809915079e+00, 5.858560892037079e+00, 5.542221840643947e+00, 5.047948278928924e+00, 4.566221446174492e+00, 4.496222403679634e+00, 4.470638440255675e+00, 4.461747124949804e+00, 4.458828418109562e+00, 4.458043432344522e+00, 4.458104168570753e+00, 4.458101940017489e+00, 4.458103007267981e+00, 4.458102313327546e+00, 4.458102774372732e+00, 4.458102484881135e+00, 3.958696587137276e+00, 2.960963748330885e+00, 9.726613059613628e-01, -5.081214667849954e-01, -5.008294438061146e-01, -4.823457266305946e-01, -4.093596340082378e-01, 2.927533877100476e-01, 1.580990201373001e+00, 2.297731671229221e+00, 2.465955512323740e+00, 2.496576390164448e+00, 2.500278606332533e+00, 2.499872222320282e+00, 2.500061542030393e+00, 2.499929263366373e+00, 2.500032946810125e+00, 2.499951677124709e+00, 2.500009659840817e+00, 2.961918369398067e+00, 3.816756624765787e+00, 5.191537959402029e+00, 6.033971024560395e+00, 5.858568428923525e+00, 5.542228175717445e+00, 5.047952819852995e+00, 4.566222065264992e+00, 4.496222553506734e+00, 4.470638537994252e+00, 4.461747150642569e+00, 4.458828425715773e+00, 4.458043431871857e+00, 4.458104168387089e+00, 4.458101940091086e+00, 4.458103007220202e+00, 4.458102313359292e+00, 4.458102774351643e+00, 4.458102484892605e+00, 3.958696587148540e+00, 2.960963748341582e+00, 9.726613059724690e-01, -5.081214667705706e-01, -5.008294437900119e-01, -4.823457266089376e-01, -4.093596339411046e-01, 2.927533879025108e-01, 1.580990201486391e+00, 2.297731671249843e+00, 2.465955512329101e+00, 2.496576390165203e+00, 2.500278606332566e+00, 2.499872222320265e+00, 2.500061542030405e+00, 2.499929263366364e+00, 2.500032946810133e+00, 2.499951677124703e+00, 2.500009659840779e+00, 2.961918369398032e+00, 3.816756624765757e+00, 5.191537959401995e+00, 6.033971024560496e+00, 5.858568428923665e+00, 5.542228175717561e+00, 5.047952819853082e+00, 4.566222065265002e+00, 4.496222553506737e+00, 4.470638537994254e+00, 4.461747150642569e+00, 4.458828425715771e+00, 4.458043431871858e+00, 4.458104168387090e+00, 4.458101940091087e+00, 4.458103007220201e+00, 4.458102313359291e+00, 4.458102774351643e+00, 4.458102484892708e+00, 3.958696587148749e+00, 2.960963748341791e+00, 9.726613059726750e-01, -5.081214667704262e-01, -5.008294437900225e-01, -4.823457266089518e-01, -4.093596339411494e-01, 2.927533879023823e-01, 1.580990201486315e+00, 2.297731671249816e+00, 2.465955512329098e+00, 2.496576390165200e+00, 2.500278606332570e+00, 2.499872222320263e+00, 2.500061542030407e+00, 2.499929263366362e+00, 2.500032946810134e+00, 2.499951677124702e+00, 2.500009659840780e+00, 2.961918369398029e+00, 3.816756624765742e+00, 5.191537959401949e+00, 6.033971024560461e+00, 5.858568428923622e+00, 5.542228175717498e+00, 5.047952819852998e+00, 4.566222065264983e+00, 4.496222553506735e+00, 4.470638537994251e+00, 4.461747150642568e+00, 4.458828425715771e+00, 4.458043431871856e+00, 4.458104168387091e+00, 4.458101940091087e+00, 4.458103007220201e+00, 4.458102313359291e+00, 4.458102774351643e+00, 4.458102484892708e+00, 3.958696587148324e+00, 2.960963748341793e+00, 9.726613059726769e-01, -5.081214667704226e-01, -5.008294437900186e-01, -4.823457266089463e-01, -4.093596339411322e-01, 2.927533879024313e-01, 1.580990201486346e+00, 2.297731671249819e+00, 2.465955512329098e+00, 2.496576390165201e+00, 2.500278606332567e+00, 2.499872222320263e+00, 2.500061542030407e+00, 2.499929263366362e+00, 2.500032946810135e+00, 2.499951677124702e+00, 2.500009659840780e+00, 2.961918369398421e+00, 3.816756624765698e+00, 5.191537959401959e+00, 6.033971024560470e+00, 5.858568428923640e+00, 5.542228175717541e+00, 5.047952819853064e+00, 4.566222065265000e+00, 4.496222553506738e+00, 4.470638537994254e+00, 4.461747150642569e+00, 4.458828425715772e+00, 4.458043431871857e+00, 4.458104168387089e+00, 4.458101940091087e+00, 4.458103007220202e+00, 4.458102313359291e+00, 4.458102774351643e+00, 4.458102489285038e+00 }
            };
            
            int index = 0;
            tran.OnExportSimulationData += (object sender, ExportDataEventArgs args) =>
            {
                double actual = exports[0].Value;
                double expected = references[0][index++];
                output.Points.AddXY(args.Time, actual);
                reference.Points.AddXY(args.Time, expected);
            };
            tran.Run(ckt);
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
        static Diode CreateDiode(Identifier name, Identifier anode, Identifier cathode, Identifier model, string modelparams)
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
        static void ApplyParameters(Entity entity, string definition)
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
                entity.ParameterSets.SetProperty(name, value);
            }
        }
    }
}