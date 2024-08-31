using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System;
using System.Numerics;

namespace SpiceSharpTest.Models
{
    /// <summary>
    /// Model part of the FDC604P (ONSemi)
    /// M1 2 1 4x 4x DMOS L = 1u W = 1u
    /// .MODEL DMOS PMOS(VTO= -0.7 KP= 3.8E+1 THETA = .25 VMAX= 3.5E5 LEVEL= 3)
    /// </summary>
    [TestFixture]
    public class MOS3Tests : Framework
    {
        private Mosfet3 CreateMOS3(string name, string d, string g, string s, string b, string model)
        {
            // Create transistor
            var mos = new Mosfet3(name) { Model = model };
            mos.Connect(d, g, s, b);
            return mos;
        }

        private Mosfet3Model CreateMOS3Model(string name, bool nmos, string parameters)
        {
            var model = new Mosfet3Model(name, nmos);
            ApplyParameters(model, parameters);
            return model;
        }

        [Test]
        public void When_SimpleDC_Expect_Spice3f5Reference()
        {
            /*
             * MOS3 driven by voltage sources
             * The current should match the references. References are simulation results by Spice 3f5.
             */
            // Create circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "0", "g", 0),
                new VoltageSource("V2", "0", "d", 0),
                CreateMOS3("M1", "d", "g", "0", "0", "DMOS")
                    .SetParameter("w", 1e-6)
                    .SetParameter("l", 1e-6),
                CreateMOS3Model("DMOS", false, "VTO=-0.7 KP=3.8E+1 THETA=0.25 VMAX=3.5E5")
            );

            // Create simulation
            var dc = new DC("dc", new[] {
                new ParameterSweep("V2", new LinearSweep(0, 1.8, 0.3)),
                new ParameterSweep("V1", new LinearSweep(0, 1.8, 0.3))
            });

            // Create exports
            IExport<double>[] exports = [new RealPropertyExport(dc, "V2", "i")];

            // Create references
            double[][] references =
            [
                [
                    -1.262177448353619e-29, 0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00,
                    0.000000000000000e+00, 0.000000000000000e+00, 0.000000000000000e+00, -4.159905034473416e-13,
                    -4.159905034473416e-13, -4.159905034473416e-13, -7.010973787728751e-01, -3.391621129326464e+00,
                    -5.921232876712748e+00, -8.164781906300902e+00, -8.319810068946831e-13, -8.319810068946831e-13,
                    -8.319810068946831e-13, -7.010973787732908e-01, -3.928205688972319e+00, -8.750000000000831e+00,
                    -1.323794712286243e+01, -1.247971510342025e-12, -1.247971510342025e-12, -1.247971510342025e-12,
                    -7.010973787737074e-01, -3.928205688972735e+00, -9.117778872755419e+00, -1.555322338830710e+01,
                    -1.663962013789366e-12, -1.663962013789366e-12, -1.663962013789366e-12, -7.010973787741221e-01,
                    -3.928205688973152e+00, -9.117778872755839e+00, -1.577264391107695e+01, -2.079952517236708e-12,
                    -2.079952517236708e-12, -2.079952517236708e-12, -7.010973787745387e-01, -3.928205688973566e+00,
                    -9.117778872756254e+00, -1.577264391107736e+01, -2.495943020684050e-12, -2.495943020684050e-12,
                    -2.495943020684050e-12, -7.010973787749553e-01, -3.928205688973984e+00, -9.117778872756670e+00,
                    -1.577264391107778e+01
                ],
            ];

            // Run test
            AnalyzeDC(dc, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_CommonSourceAmplifierSmallSignal_Expect_Spice3f5Reference()
        {
            /*
             * Common-source amplifier biased as a diode-connected transistor
             * Output voltage is expected to match the reference. Reference is simulated by Spice 3f5.
             */
            // Create circuit
            var ckt = new Circuit(
                new VoltageSource("Vsupply", "vdd", "0", 1.8),
                new VoltageSource("Vin", "in", "0", 0.0)
                    .SetParameter("acmag", 1.0),
                new Resistor("R1", "out", "0", 100e3),
                new Resistor("R2", "g", "out", 10e3),
                new Capacitor("C1", "in", "g", 1e-6),
                CreateMOS3("M1", "out", "g", "vdd", "vdd", "DMOS")
                    .SetParameter("w", 1e-6)
                    .SetParameter("l", 1e-6),
                CreateMOS3Model("DMOS", false, "VTO = -0.7 KP = 3.8E+1 THETA = .25 VMAX = 3.5E5")
                );

            // Create simulation
            var ac = new AC("ac", new DecadeSweep(10, 10e9, 5));

            // Create exports
            IExport<Complex>[] exports = [new ComplexVoltageExport(ac, "out")];

            // Create references
            double[] riref =
            [
                -1.448857719884684e-03, -6.260007108126745e-01, -3.639336573643311e-03, -9.921362299670315e-01,
                -9.141414195176988e-03, -1.572397969565666e+00, -2.296102101289896e-02, -2.491955503154010e+00,
                -5.766807563860867e-02, -3.948976475444068e+00, -1.448089774772143e-01, -6.256689086075871e+00,
                -3.634495125691141e-01, -9.908163806423953e+00, -9.110929278146982e-01, -1.567154314632532e+01,
                -2.276965858631552e+00, -2.471186973228724e+01, -5.647598782046568e+00, -3.867345058089153e+01,
                -1.375199410745549e+01, -5.941755334708798e+01, -3.207763017324329e+01, -8.744829839823841e+01,
                -6.832438160737344e+01, -1.175235216537343e+02, -1.241920646766530e+02, -1.347854256329389e+02,
                -1.841315808238452e+02, -1.260890489248231e+02, -2.279253114275959e+02, -9.847854969308602e+01,
                -2.517636926413673e+02, -6.863445460602944e+01, -2.627019332545188e+02, -4.518688001997734e+01,
                -2.673256909888647e+02, -2.901280942253893e+01, -2.692120585123498e+02, -1.843501927536817e+01,
                -2.699704646985724e+02, -1.166447888431620e+01, -2.702735821604205e+02, -7.368052045702465e+00,
                -2.703944449090613e+02, -4.651005490826149e+00, -2.704425913243499e+02, -2.935108605838326e+00,
                -2.704617635295234e+02, -1.852059618528605e+00, -2.704693968784006e+02, -1.168603599754638e+00,
                -2.704724358892379e+02, -7.373473088379656e-01, -2.704736457602495e+02, -4.652367810210625e-01,
                -2.704741274215870e+02, -2.935450866537507e-01, -2.704743191748967e+02, -1.852145596684913e-01,
                -2.704743955133399e+02, -1.168625197106691e-01, -2.704744259042335e+02, -7.373527339090884e-02,
                -2.704744380030681e+02, -4.652381437434735e-02, -2.704744428197012e+02, -2.935454289547597e-02,
                -2.704744447372374e+02, -1.852146456506789e-02, -2.704744455006224e+02, -1.168625413084242e-02,
                -2.704744458045314e+02, -7.373527881602024e-03, -2.704744459255197e+02, -4.652381573707377e-03,
                -2.704744459736860e+02, -2.935454323777736e-03, -2.704744459928614e+02, -1.852146465105010e-03,
                -2.704744460004952e+02, -1.168625415244017e-03, -2.704744460035343e+02, -7.373527887027131e-04,
                -2.704744460047443e+02, -4.652381575070101e-04, -2.704744460052259e+02, -2.935454324120037e-04,
                -2.704744460054176e+02, -1.852146465190992e-04, -2.704744460054940e+02, -1.168625415265614e-04
            ];
            var references = new Complex[1][];
            references[0] = new Complex[riref.Length / 2];
            for (int i = 0; i < riref.Length; i += 2)
                references[0][i / 2] = new Complex(riref[i], riref[i + 1]);

            // Run test
            AnalyzeAC(ac, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_SwitchTransient_Expect_Spice3f5Reference()
        {
            // Create circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Pulse(0, 1.8, 1e-6, 1e-9, 0.5e-6, 2e-6, 6e-6)),
                new VoltageSource("Vsupply", "vdd", "0", 1.8),
                new Resistor("R1", "out", "0", 100e3),
                CreateMOS3("M1", "out", "in", "vdd", "vdd", "DMOS")
                    .SetParameter("w", 1e-6)
                    .SetParameter("l", 1e-6),
                CreateMOS3Model("DMOS", false, "VTO=-0.7 KP=3.8E+1 THETA=0.25 VMAX=3.5E5")
                );

            // Create simulation
            var tran = new Transient("tran", 1e-9, 10e-6);

            // Create exports
            IExport<double>[] exports = [new GenericExport<double>(tran, () => tran.GetState<IIntegrationMethod>().Time), new RealVoltageExport(tran, "out")];

            // Create references
            double[][] references =
            [
                [
                    0.000000000000000e+00, 1.000000000000000e-11, 2.000000000000000e-11, 4.000000000000000e-11,
                    8.000000000000001e-11, 1.600000000000000e-10, 3.200000000000000e-10, 6.400000000000001e-10,
                    1.280000000000000e-09, 2.560000000000000e-09, 5.120000000000001e-09, 1.024000000000000e-08,
                    2.048000000000000e-08, 4.096000000000000e-08, 8.192000000000001e-08, 1.638400000000000e-07,
                    3.276800000000000e-07, 5.276800000000000e-07, 7.276800000000000e-07, 9.276800000000000e-07,
                    1.000000000000000e-06, 1.000100000000000e-06, 1.000300000000000e-06, 1.000700000000000e-06,
                    1.001000000000000e-06, 1.001080000000000e-06, 1.001240000000000e-06, 1.001560000000000e-06,
                    1.002200000000000e-06, 1.003480000000000e-06, 1.006040000000000e-06, 1.011160000000001e-06,
                    1.021400000000002e-06, 1.041880000000003e-06, 1.082840000000006e-06, 1.164760000000012e-06,
                    1.328600000000025e-06, 1.528600000000025e-06, 1.728600000000025e-06, 1.928600000000025e-06,
                    2.128600000000025e-06, 2.328600000000025e-06, 2.528600000000024e-06, 2.728600000000024e-06,
                    2.928600000000024e-06, 3.001000000000000e-06, 3.021000000000000e-06, 3.061000000000000e-06,
                    3.141000000000000e-06, 3.301000000000000e-06, 3.501000000000000e-06, 3.520999999999999e-06,
                    3.560999999999999e-06, 3.641000000000000e-06, 3.801000000000000e-06, 4.000999999999999e-06,
                    4.200999999999999e-06, 4.400999999999999e-06, 4.600999999999999e-06, 4.800999999999999e-06,
                    5.000999999999998e-06, 5.200999999999998e-06, 5.400999999999998e-06, 5.600999999999998e-06,
                    5.800999999999997e-06, 6.000999999999997e-06, 6.200999999999997e-06, 6.400999999999997e-06,
                    6.600999999999997e-06, 6.800999999999996e-06, 7.000000000000000e-06, 7.000100000000000e-06,
                    7.000300000000000e-06, 7.000700000000000e-06, 7.001000000000000e-06, 7.001079999999999e-06,
                    7.001239999999999e-06, 7.001559999999999e-06, 7.002199999999999e-06, 7.003479999999999e-06,
                    7.006039999999998e-06, 7.011159999999996e-06, 7.021399999999993e-06, 7.041879999999985e-06,
                    7.082839999999971e-06, 7.164759999999943e-06, 7.328599999999885e-06, 7.528599999999885e-06,
                    7.728599999999886e-06, 7.928599999999885e-06, 8.128599999999885e-06, 8.328599999999885e-06,
                    8.528599999999885e-06, 8.728599999999885e-06, 8.928599999999884e-06, 9.000999999999999e-06,
                    9.020999999999999e-06, 9.060999999999998e-06, 9.140999999999998e-06, 9.300999999999998e-06,
                    9.500999999999998e-06, 9.520999999999997e-06, 9.560999999999997e-06, 9.640999999999996e-06,
                    9.800999999999996e-06, 9.999999999999999e-06
                ],
                [
                    1.799999451299451e+00, 1.799999453635131e+00, 1.799999450956929e+00, 1.799999450956926e+00,
                    1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00,
                    1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00,
                    1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00,
                    1.799999450956929e+00, 1.799999450956926e+00, 1.799999450956929e+00, 1.799999450956926e+00,
                    1.799999450956929e+00, 1.799999389971110e+00, 1.799999183568271e+00, 7.448095922434256e-02,
                    -7.448046003580762e-02, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07,
                    2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07,
                    2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07,
                    2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07,
                    2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07,
                    2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07,
                    2.495942674587899e-07, 1.799998636447238e+00, 1.800000994575628e+00, 1.799999453635142e+00,
                    1.799999453635142e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00,
                    1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00,
                    1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00,
                    1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999450956928e+00,
                    1.799999450956927e+00, 1.799999450956928e+00, 1.799999450956927e+00, 1.799999389971109e+00,
                    1.799999183568270e+00, 7.448095922456173e-02, -7.448046003602679e-02, 2.495942674587899e-07,
                    2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07,
                    2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07,
                    2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07,
                    2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07,
                    2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07,
                    2.495942674587899e-07, 2.495942674587899e-07, 2.495942674587899e-07, 1.799998636447239e+00,
                    1.800000994575627e+00, 1.799999453635142e+00, 1.799999453635142e+00, 1.799999450956928e+00,
                    1.799999450956927e+00, 1.799999450956928e+00
                ],
            ];

            // Run test
            AnalyzeTransient(tran, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_CommonSourceAmplifierNoise_Expect_Spice3f5Reference()
        {
            // Create circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0),
                new VoltageSource("Vsupply", "vdd", "0", 1.8),
                new Resistor("R1", "out", "0", 100e3),
                new Resistor("R2", "g", "out", 10e3),
                new Capacitor("Cin", "in", "g", 1e-6),
                CreateMOS3("M1", "out", "g", "vdd", "vdd", "DMOS")
                    .SetParameter("w", 1e-6)
                    .SetParameter("l", 1e-6),
                CreateMOS3Model("DMOS", false, "VTO = -0.7 KP = 3.8E+1 THETA = .25 VMAX = 3.5E5 KF=1e-24")
                );

            // Make simulation, exports and references
            var noise = new Noise("Noise", "V1", "out", new DecadeSweep(10.0, 10.0e9, 10));
            IExport<double>[] exports = [new InputNoiseDensityExport(noise), new OutputNoiseDensityExport(noise)];
            double[][] references =
            [
                [
                    3.927412574628089e-08, 2.294232620901609e-08, 1.408682739799012e-08, 9.116198215479690e-09,
                    6.202108799686057e-09, 4.405704006405740e-09, 3.238553702183993e-09, 2.441654059120419e-09,
                    1.873909150363625e-09, 1.455638301315577e-09, 1.139785265766578e-09, 8.971093968098429e-10,
                    7.084624131264145e-10, 5.606783312917926e-10, 4.443234842274822e-10, 3.524178827232121e-10,
                    2.796744532995601e-10, 2.220224942384141e-10, 1.762931703614522e-10, 1.400017822223799e-10,
                    1.111908992615631e-10, 8.831381657729583e-11, 7.014602118627445e-11, 5.571689188964359e-11,
                    4.425646130953727e-11, 3.515363605700460e-11, 2.792326472830154e-11, 2.218010683542241e-11,
                    1.761821961976641e-11, 1.399461650580587e-11, 1.111630263232533e-11, 8.829984869376394e-12,
                    7.013902234050593e-12, 5.571338583778488e-12, 4.425470580212278e-12, 3.515275790057611e-12,
                    2.792282628927604e-12, 2.217988877732828e-12, 1.761811201389682e-12, 1.399456425725386e-12,
                    1.111627812820003e-12, 8.829974270432511e-13, 7.013898604224380e-13, 5.571338446796600e-13,
                    4.425472193806644e-13, 3.515278281023035e-13, 2.792285559623095e-13, 2.217992028817241e-13,
                    1.761814462931361e-13, 1.399459742627563e-13, 1.111631157468614e-13, 8.830007855983101e-14,
                    7.013932259474140e-14, 5.571372136979844e-14, 4.425505901498836e-14, 3.515311997490945e-14,
                    2.792319280489571e-14, 2.218025751888405e-14, 1.761848187107604e-14, 1.399493467357729e-14,
                    1.111664882476445e-14, 8.830345107453322e-15, 7.014269511642148e-15, 5.571709389497696e-15,
                    4.425843154192093e-15, 3.515649250272159e-15, 2.792656533314900e-15, 2.218363004735860e-15,
                    1.762185439966160e-15, 1.399830720221858e-15, 1.112002135343370e-15, 8.833717636136622e-16,
                    7.017642040332504e-16, 5.575081918191603e-16, 4.429215682887784e-16, 3.519021778968749e-16,
                    2.796029062011947e-16, 2.221735533433137e-16, 1.765557968663553e-16, 1.403203248919309e-16,
                    1.115374664040852e-16, 8.867442923111590e-17, 7.051367327307560e-17, 5.608807205166701e-17,
                    4.462940969862903e-17, 3.552747065943883e-17, 2.829754348987084e-17, 2.255460820408278e-17,
                    1.799283255638696e-17, 1.436928535894452e-17, 1.149099951015997e-17
                ],
                [
                    1.539070469628613e-08, 1.424912082112840e-08, 1.386633394610182e-08, 1.422193873823944e-08,
                    1.533483147608060e-08, 1.726419600797934e-08, 2.011261493596601e-08, 2.403145158285339e-08,
                    2.922874922990576e-08, 3.597998833239351e-08, 4.464211535095020e-08, 5.567127357196522e-08,
                    6.964455130875527e-08, 8.728566067835499e-08, 1.094934609426847e-07, 1.373700456771697e-07,
                    1.722406106300757e-07, 2.156485649717393e-07, 2.692933095829849e-07, 3.348511758003875e-07,
                    4.135815534607812e-07, 5.055844509363105e-07, 6.085994535059658e-07, 7.164545414209565e-07,
                    8.178593724242077e-07, 8.970674268205870e-07, 9.379642393461172e-07, 9.308337151660332e-07,
                    8.774382064221595e-07, 7.900465402275638e-07, 6.852975171897693e-07, 5.779008045497663e-07,
                    4.776161986390565e-07, 3.893231039734450e-07, 3.144480940692175e-07, 2.524522306130316e-07,
                    2.018953481588385e-07, 1.610631385731940e-07, 1.282862515933703e-07, 1.020772164249132e-07,
                    8.117118725793013e-08, 6.452093845828355e-08, 5.127306989532170e-08, 4.073881762732897e-08,
                    3.236559693209635e-08, 2.571171965507164e-08, 2.042495737618750e-08, 1.622483115105937e-08,
                    1.288820057540780e-08, 1.023764440277342e-08, 8.132144433612058e-09, 6.459642071506968e-09,
                    5.131104025320453e-09, 4.075797619138642e-09, 3.237532368562907e-09, 2.571671817273747e-09,
                    2.042758580387155e-09, 1.622627160639260e-09, 1.288904559819636e-09, 1.023819099095338e-09,
                    8.132541446537533e-10, 6.459964118056436e-10, 5.131388499299024e-10, 4.076063262097185e-10,
                    3.237788573610047e-10, 2.571923292143342e-10, 2.043007684545092e-10, 1.622875076623454e-10,
                    1.289151880304577e-10, 1.024066121122670e-10, 8.135010170973548e-11, 6.462432092794307e-11,
                    5.133856098295525e-11, 4.078530672775516e-11, 3.240255889904837e-11, 2.574390561133755e-11,
                    2.045474929826809e-11, 1.625342310022455e-11, 1.291619107747975e-11, 1.026533345581107e-11,
                    8.159682400597137e-12, 6.487104314919391e-12, 5.158528316662240e-12, 4.103202889258444e-12,
                    3.264928105443549e-12, 2.599062776199182e-12, 2.070147144654997e-12, 1.650014524731721e-12,
                    1.316291322397623e-12, 1.051205560200868e-12, 8.406404546644898e-13
                ]
            ];
            AnalyzeNoise(noise, ckt, exports, references);
            DestroyExports(exports);
        }

        [Test]
        public void When_ParallelMultiplier_Expect_Reference()
        {
            // Create circuit
            var ckt_ref = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0),
                new VoltageSource("Vsupply", "vdd", "0", 5),
                new Resistor("R1", "out", "0", 1.0e3),
                CreateMOS3("M1", "out", "in", "vdd", "vdd", "DMOS")
                    .SetParameter("w", 1e-6)
                    .SetParameter("l", 1e-6),
                CreateMOS3("M2", "out", "in", "vdd", "vdd", "DMOS")
                    .SetParameter("w", 1e-6)
                    .SetParameter("l", 1e-6),
                CreateMOS3Model("DMOS", false, "VTO = -0.7 KP = 3.8E+1 THETA = .25 VMAX = 3.5E5 KF=1e-24"));
            var ckt_act = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0),
                new VoltageSource("Vsupply", "vdd", "0", 5),
                new Resistor("R1", "out", "0", 1.0e3),
                CreateMOS3("M1", "out", "in", "vdd", "vdd", "DMOS")
                    .SetParameter("w", 1e-6)
                    .SetParameter("l", 1e-6)
                    .SetParameter("m", 2.0),
                CreateMOS3Model("DMOS", false, "VTO = -0.7 KP = 3.8E+1 THETA = .25 VMAX = 3.5E5 KF=1e-24"));

            // Create simulation
            var dc = new DC("op", "V1", 0.0, 5.0, 0.1);
            dc.BiasingParameters.Gmin = 0.0; // May interfere with comparison
            var exports = new[] { new RealVoltageExport(dc, "out") };
            Compare(dc, ckt_ref, ckt_act, exports);
            DestroyExports(exports);
        }

        [Test]
        public void When_ParallelMultiplierTransient_Expect_Reference()
        {
            // Create circuit
            // WARNING: We simulate both possibilities together, because the
            // timestep varies if we split them due to different timestep truncation.
            var ckt = new Circuit(
                new VoltageSource("V1r", "inr", "0", new Pulse(1, 5, 1e-6, 1e-9, 0.5e-6, 2e-6, 6e-6)),
                new VoltageSource("Vsupplyr", "vddr", "0", 5),
                new Resistor("R1r", "outr", "0", 1.0e3),
                CreateMOS3("M1r", "outr", "inr", "vddr", "vddr", "DMOS")
                    .SetParameter("w", 1e-6)
                    .SetParameter("l", 1e-6),
                CreateMOS3("M2r", "outr", "inr", "vddr", "vddr", "DMOS")
                    .SetParameter("w", 1e-6)
                    .SetParameter("l", 1e-6),

                new VoltageSource("V1a", "ina", "0", new Pulse(1, 5, 1e-6, 1e-9, 0.5e-6, 2e-6, 6e-6)),
                new VoltageSource("Vsupplya", "vdda", "0", 5),
                new Resistor("R1a", "outa", "0", 1.0e3),
                CreateMOS3("M1a", "outa", "ina", "vdda", "vdda", "DMOS")
                    .SetParameter("w", 1e-6)
                    .SetParameter("l", 1e-6)
                    .SetParameter("m", 2.0),
                CreateMOS3Model("DMOS", false, "VTO = -0.7 KP = 3.8E+1 THETA = .25 VMAX = 3.5E5 KF=1e-24"));

            // Create simulation
            var tran = new Transient("tran", 1e-9, 10e-6);
            tran.BiasingParameters.Gmin = 0.0; // May interfere with comparison
            var v_ref = new RealVoltageExport(tran, "outr");
            var v_act = new RealVoltageExport(tran, "outa");

            foreach (int _ in tran.Run(ckt))
            {
                double tol = Math.Max(Math.Abs(v_ref.Value), Math.Abs(v_act.Value)) * CompareRelTol + CompareAbsTol;
                Assert.That(v_act.Value, Is.EqualTo(v_ref.Value).Within(tol));
            }
        }

        [Test]
        public void When_ParallelMultiplierNoise_Expect_Reference()
        {
            // Create circuit
            var ckt_ref = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0),
                new VoltageSource("V2", "vdd", "0", 5.0),
                new Resistor("R1", "0", "out", 10e3),
                new Resistor("R2", "out", "g", 10e3),
                new Capacitor("Cin", "in", "g", 1e-6),
                CreateMOS3("M1", "out", "in", "vdd", "vdd", "DMOS")
                    .SetParameter("w", 1e-6)
                    .SetParameter("l", 1e-6),
                CreateMOS3("M2", "out", "in", "vdd", "vdd", "DMOS")
                    .SetParameter("w", 1e-6)
                    .SetParameter("l", 1e-6),
                CreateMOS3Model("DMOS", false, "VTO = -0.7 KP = 3.8E+1 THETA = .25 VMAX = 3.5E5 KF=1e-24"));
            var ckt_act = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0),
                new VoltageSource("V2", "vdd", "0", 5.0),
                new Resistor("R1", "0", "out", 10e3),
                new Resistor("R2", "out", "g", 10e3),
                new Capacitor("Cin", "in", "g", 1e-6),
                CreateMOS3("M1", "out", "in", "vdd", "vdd", "DMOS")
                    .SetParameter("w", 1e-6)
                    .SetParameter("l", 1e-6)
                    .SetParameter("m", 2.0),
                CreateMOS3Model("DMOS", false, "VTO = -0.7 KP = 3.8E+1 THETA = .25 VMAX = 3.5E5 KF=1e-24"));

            // Create simulation, exports and references
            var noise = new Noise("noise", "V1", "out", new DecadeSweep(10, 10e9, 10));
            noise.BiasingParameters.Gmin = 0.0; // May interfere with comparison
            var exports = new IExport<double>[] { new InputNoiseDensityExport(noise), new OutputNoiseDensityExport(noise) };
            Compare(noise, ckt_ref, ckt_act, exports);
            DestroyExports(exports);
        }

        [Test]
        public void When_ParallelMultiplierAC_Expect_Reference()
        {
            // Build circuit
            var ckt_ref = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0)
                    .SetParameter("acmag", 1.0),
                new VoltageSource("V2", "vdd", "0", 5.0),
                new Resistor("R1", "0", "out", 10.0e3),
                new Resistor("R2", "out", "g", 10.0e3),
                new Capacitor("Cin", "in", "g", 1e-6),
                CreateMOS3("M1", "out", "in", "vdd", "vdd", "DMOS")
                    .SetParameter("w", 1e-6)
                    .SetParameter("l", 1e-6),
                CreateMOS3("M2", "out", "in", "vdd", "vdd", "DMOS")
                    .SetParameter("w", 1e-6)
                    .SetParameter("l", 1e-6),
                CreateMOS3Model("DMOS", false, "VTO = -0.7 KP = 3.8E+1 THETA = .25 VMAX = 3.5E5 KF=1e-24"));
            var ckt_act = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0)
                    .SetParameter("acmag", 1.0),
                new VoltageSource("V2", "vdd", "0", 5.0),
                new Resistor("R1", "0", "out", 10.0e3),
                new Resistor("R2", "out", "g", 10.0e3),
                new Capacitor("Cin", "in", "g", 1e-6),
                CreateMOS3("M1", "out", "in", "vdd", "vdd", "DMOS")
                    .SetParameter("w", 1e-6)
                    .SetParameter("l", 1e-6)
                    .SetParameter("m", 2.0),
                CreateMOS3Model("DMOS", false, "VTO = -0.7 KP = 3.8E+1 THETA = .25 VMAX = 3.5E5 KF=1e-24"));

            // Create simulation
            var ac = new AC("ac", new DecadeSweep(10, 10e9, 5));
            ac.BiasingParameters.Gmin = 0.0; // May interfere with comparison
            var exports = new[] { new ComplexVoltageExport(ac, "out") };
            Compare(ac, ckt_ref, ckt_act, exports);
            DestroyExports(exports);
        }
    }
}
