using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharp.Parameters;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Designer;
using SpiceSharp.Diagnostics;

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

            // Initialize + Add BSIM transistor models!
            NetlistReader nr = new NetlistReader();
            BSIMParser.AddMosfetGenerators(nr.Netlist.Readers[StatementType.Component].Find<MosfetReader>().Mosfets);
            BSIMParser.AddMosfetModelGenerators(nr.Netlist.Readers[StatementType.Model].Find<MosfetModelReader>().Levels);
            nr.Parse(@"D:\Visual Studio\Info\nmos.mod");

            // Parse a circuit netlist: Diode-connected transistor
            string netlist = string.Join(Environment.NewLine,
                "Vsupply VDD GND 3.3",
                "Ibias NBIAS VDD 20u",
                "X1 NBIAS NBIAS GND GND nmos w=2.1u l=1u",
                ".dc Ibias 1u 50u 0.5u");
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(netlist));
            nr.Parse(ms);
            IParameterized mos = (IParameterized)nr.Netlist.Circuit.Objects["x1", "m1"];

            // Build a design step to find the width where vgs-vth = 0.2V (strong inversion)
            Newton step = new Newton();
            step.Minimum = 0.4e-6;
            step.Maximum = 1e-3;
            step.Target = 0.2;
            step.Apply = (DesignStep s, Circuit ckt, double value) => mos.Set("w", value);
            step.Measurement = new OPMeasurement((SimulationData data) => mos.Ask("vgs", data.Circuit) - mos.Ask("vth"));
            step.Execute(nr.Netlist.Circuit);

            MessageBox.Show($"Design step result: {(step.Result * 1e6).ToString("G3")} um / 1um in {step.Iterations} iterations");
        }
    }
}
