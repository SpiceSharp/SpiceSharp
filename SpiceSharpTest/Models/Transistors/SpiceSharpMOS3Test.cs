using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Models.Transistors
{
    [TestClass]
    public class SpiceSharpMOS3Test
    {
        /**
         * Note to self:
         * SmartSpice uses extended models, or propriety models for mosfet parasitic capacitances for LEVEL=1,2,3. If they are not specified by the model,
         * use CAPMOD=1 to use the legacy parasitic capacitance calculations!
         **/

        /// <summary>
        /// The testing model
        /// </summary>
        public MOS3Model TestModel
        {
            get
            {
                // Model part of the FDC604P (ONSemi)
                // M1 2 1 4x 4x DMOS L = 1u W = 1u
                // .MODEL DMOS PMOS(VTO= -0.7 KP= 3.8E+1 THETA = .25 VMAX= 3.5E5 LEVEL= 3)
                MOS3Model model = new MOS3Model("DMOS");
                model.SetPMOS(true);
                model.Set("vto", -0.7);
                model.Set("kp", 3.8e1);
                model.Set("theta", 0.25);
                model.Set("vmax", 3.5e5);
                return model;
            }
        }

        [TestMethod]
        public void TestMOS3_DC()
        {
            // Simulated by LTSpiceXVII
            // GMIN = 0
            // vds,vgs: 0->5V in 0.5V steps
            // I first tried this with SmartSpice, but it seems to be (very significantly) different from our model. So I tried again in LTSpice, and this matches our results.
            // I believe that SmartSpice either has another flag I haven't set, or they just made a mistake in their model (they focus on IC design, so they might have missed it).
            double[] reference = new double[]
            {
                0.000000e+000, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, 
                0.000000e+000, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, 
                0.000000e+000, -1.518862e+000, -1.518862e+000, -1.518862e+000, -1.518862e+000, -1.518862e+000, -1.518862e+000, -1.518862e+000, -1.518862e+000, -1.518862e+000, -1.518862e+000, 
                0.000000e+000, -8.127778e+000, -9.117779e+000, -9.117779e+000, -9.117779e+000, -9.117779e+000, -9.117779e+000, -9.117779e+000, -9.117779e+000, -9.117779e+000, -9.117779e+000, 
                0.000000e+000, -1.414177e+001, -2.031503e+001, -2.085084e+001, -2.085084e+001, -2.085084e+001, -2.085084e+001, -2.085084e+001, -2.085084e+001, -2.085084e+001, -2.085084e+001, 
                0.000000e+000, -1.917674e+001, -3.046696e+001, -3.505858e+001, -3.526929e+001, -3.526929e+001, -3.526929e+001, -3.526929e+001, -3.526929e+001, -3.526929e+001, -3.526929e+001, 
                0.000000e+000, -2.345376e+001, -3.916565e+001, -4.822222e+001, -5.151583e+001, -5.153519e+001, -5.153519e+001, -5.153519e+001, -5.153519e+001, -5.153519e+001, -5.153519e+001, 
                0.000000e+000, -2.713200e+001, -4.670229e+001, -5.970438e+001, -6.696503e+001, -6.912774e+001, -6.912774e+001, -6.912774e+001, -6.912774e+001, -6.912774e+001, -6.912774e+001, 
                0.000000e+000, -3.032897e+001, -5.329517e+001, -6.980789e+001, -8.063262e+001, -8.641838e+001, -8.770544e+001, -8.770544e+001, -8.770544e+001, -8.770544e+001, -8.770544e+001, 
                0.000000e+000, -3.313334e+001, -5.911111e+001, -7.876699e+001, -9.280997e+001, -1.018468e+002, -1.064000e+002, -1.070348e+002, -1.070348e+002, -1.070348e+002, -1.070348e+002, 
                0.000000e+000, -3.561322e+001, -6.427981e+001, -8.676569e+001, -1.037282e+002, -1.157347e+002, -1.232772e+002, -1.267850e+002, -1.269506e+002, -1.269506e+002, -1.269506e+002
            };

            // Create the circuit
            Circuit ckt = new Circuit();
            MOS3 m = new MOS3("M1");
            m.Set("w", 1e-6); m.Set("l", 1e-6);
            m.SetModel(TestModel);
            m.Connect("D", "G", "0", "0");
            ckt.Objects.Add(
                new Voltagesource("V1", "0", "G", 0.0),
                new Voltagesource("V2", "0", "D", 0.0),
                m);

            // Create the simulation
            DC dc = new DC("TestMOS3_DC");
            dc.Config.Gmin = 0;
            dc.Sweeps.Add(new DC.Sweep("V1", 0.0, 5.0, 0.5));
            dc.Sweeps.Add(new DC.Sweep("V2", 0.0, 5.0, 0.5));
            int index = 0;
            dc.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                double vds = dc.Sweeps[1].CurrentValue;
                double expected = reference[index];
                double actual = data.Ask("V2", "i");
                double tol = Math.Max(Math.Abs(expected), Math.Abs(actual)) * 1e-6 + 1e-10; // Absolute tolerance weaker because of different BS/BD diode model
                Assert.AreEqual(expected, actual, tol);

                index++;
            };
            ckt.Simulate(dc);
        }

        [TestMethod]
        public void TestMOS3_AC()
        {
            // Simulated by LTSpiceXVII
            // GMIN = 0
            // Actually a bit stupid, since the model doesn't contain any parasitic capacitance calculations...
            double[] reference_db = new double[]
            {
                5.23569204830391e+001, 5.23569204827803e+001, 5.23569204821301e+001, 5.23569204804970e+001, 5.23569204763947e+001,
                5.23569204660902e+001, 5.23569204402065e+001, 5.23569203751896e+001, 5.23569202118745e+001, 5.23569198016457e+001,
                5.23569187711977e+001, 5.23569161828303e+001, 5.23569096811521e+001, 5.23568933497179e+001, 5.23568523272807e+001,
                5.23567492852865e+001, 5.23564904662823e+001, 5.23558404103620e+001, 5.23542079727555e+001, 5.23501101792863e+001,
                5.23398340092309e+001, 5.23141281781199e+001, 5.22502212813778e+001, 5.20937235951098e+001, 5.17238269113227e+001,
                5.09136523893448e+001, 4.93679541595498e+001, 4.69320660989670e+001, 4.37548962052641e+001, 4.01314189208328e+001,
                3.62908976851091e+001, 3.23560537627044e+001, 2.83822673724706e+001, 2.43927473913558e+001, 2.03969266102217e+001,
                1.63985915070871e+001, 1.23992544921319e+001, 8.39951845940133e+000, 4.39962355112929e+000, 3.99665389607493e-001,
                -3.60031795408214e+000
            };
            double[] reference_ph = new double[]
            {
                -3.59736084578309e-004, -5.70143271519450e-004, -9.03616189713592e-004, -1.43213514749521e-003, -2.26978124523478e-003,
                -3.59736084110336e-003, -5.70143269656417e-003, -9.03616182296722e-003, -1.43213511796812e-002, -2.26978112768532e-002,
                -3.59736037313060e-002, -5.70143083353170e-002, -9.03615440610809e-002, -1.43213216527004e-001, -2.26976937284984e-001,
                -3.59731357696052e-001, -5.70124454135118e-001, -9.03541283085018e-001, -1.43183700608802e+000, -2.26859499480473e+000,
                -3.59264499611456e+000, -5.68272521408002e+000, -8.96234257461569e+000, -1.40338025945571e+001, -2.16110719597444e+001,
                -3.21229821137224e+001, -4.48589276118930e+001, -5.76223570288012e+001, -6.81950128513375e+001, -7.58328660296330e+001,
                -8.09504143515078e+001, -8.42614040993042e+001, -8.63718916390832e+001, -8.77089756012569e+001, -8.85539975047450e+001,
                -8.90875175039256e+001, -8.94242331689840e+001, -8.96367083288925e+001, -8.97707766029906e+001, -8.98553693496892e+001,
                -8.99087441122862e+001
            };

            // Create the circuit
            Circuit ckt = new Circuit();
            MOS3 m = new MOS3("M1");
            m.Connect("D", "G", "0", "0");
            m.Set("w", 1e-6); m.Set("l", 1e-6);
            m.SetModel(TestModel);
            Voltagesource vsrc;
            ckt.Objects.Add(
                vsrc = new Voltagesource("V1", "0", "G", 0.711),
                new Voltagesource("V2", "0", "VDD", 5.0),
                new Resistor("R1", "VDD", "D", 1e3),
                new Capacitor("C1", "D", "0", 1e-12),
                m);
            vsrc.Set("acmag", 1.0);

            // Create the simulation
            AC ac = new AC("TestMOS3_AC");
            ac.StepType = AC.StepTypes.Decade;
            ac.StartFreq = 1e3;
            ac.StopFreq = 100e9;
            ac.NumberSteps = 5;
            int index = 0;
            ac.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                double frequency = data.GetFrequency();
                double expected = reference_db[index];
                double actual = data.GetDb("D");
                double tol = Math.Max(Math.Abs(expected), Math.Abs(actual)) * 1e-6 + 1e-12;
                Assert.AreEqual(expected, actual, tol);

                expected = reference_ph[index];
                actual = data.GetPhase("D");
                tol = Math.Max(Math.Abs(expected), Math.Abs(actual)) * 1e-6 + 1e-12;
                Assert.AreEqual(expected, actual, tol);

                index++;
            };
            ckt.Simulate(ac);
        }
    }
}
