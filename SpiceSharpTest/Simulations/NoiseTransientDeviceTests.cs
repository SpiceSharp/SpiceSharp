using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using System;

namespace SpiceSharpTest.Simulations;

/// <summary>
/// Tests for the device coverage of the <see cref="NoiseTransient"/> analysis: the thermal and shot
/// noise sources of a <see cref="Diode"/>, a <see cref="BipolarJunctionTransistor"/> and the three
/// mosfet levels.
/// </summary>
/// <remarks>
/// <para>
/// Every device is biased by an ideal current source, which is noiseless, and loaded by a capacitor.
/// The node then has a single pole, so the injected power has a closed-form output variance and the
/// captured fraction of section 3 of the design applies unchanged.
/// </para>
/// <para>
/// The variances are Monte-Carlo estimates over a single sample path, so their relative standard
/// error is a few percent no matter how right the implementation is. The windows below are wide
/// enough for that and still far narrower than the mistakes they guard against - a missing band
/// limit, a two-sided density, a thermal law where a shot law belongs, or a missing factor 2/3.
/// </para>
/// </remarks>
[TestFixture]
public class NoiseTransientDeviceTests
{
    private const double Capacitance = 1e-9;
    private const double BiasCurrent = 1e-3;
    private const int Seed = 0x5EED;

    /// <summary>
    /// The output variance of a node of conductance <paramref name="conductance"/> and capacitance
    /// <see cref="Capacitance"/>, driven by a total one-sided noise density
    /// <paramref name="density"/> that is band-limited to <paramref name="maximumFrequency"/>.
    /// </summary>
    private static double ExpectedVariance(double density, double conductance, double maximumFrequency, int order)
    {
        double unbanded = density / (4.0 * conductance * Capacitance);
        double pole = PoleFrequency(conductance);
        double a = maximumFrequency, b = pole;
        double captured = order == 1 ?
            a / (a + b) :
            a * (b + (2.0 * a)) / (2.0 * (a + b) * (a + b));
        return unbanded * captured;
    }

    /// <summary>
    /// The pole of the node, in Hz.
    /// </summary>
    private static double PoleFrequency(double conductance)
        => conductance / (2.0 * Math.PI * Capacitance);

    /// <summary>
    /// Builds time parameters that resolve the pole of the node 20 times per time constant, and that
    /// run for <paramref name="timeConstants"/> of them.
    /// </summary>
    private static Trapezoidal CreateTimeParameters(double conductance, int timeConstants)
    {
        double tau = 1.0 / (2.0 * Math.PI * PoleFrequency(conductance));
        return new Trapezoidal
        {
            InitialStep = tau / 20.0,
            MaxStep = tau / 20.0,
            StopTime = tau * timeConstants,

            // The injected process is rough, so the truncation error estimator would otherwise keep
            // cutting the timestep. These tests are about the injected power, not about step control.
            LteRelTol = 1e-1,
            LteAbsTol = 1e-1,
            ChargeTolerance = 1e-9
        };
    }

    /// <summary>
    /// Reads a property of an entity at the noiseless operating point.
    /// </summary>
    private static double OperatingPoint(IEntityCollection ckt, string entity, string property)
    {
        var op = new OP("op");
        op.RunToEnd(ckt);
        return op.EntityBehaviors[entity].GetProperty<double>(property);
    }

    /// <summary>
    /// Runs a simulation and returns the timestep-weighted mean and variance of the output node,
    /// ignoring the first tenth of the run.
    /// </summary>
    /// <remarks>
    /// The nodes of these circuits sit at a bias of a few volts while the noise on them is of the
    /// order of a microvolt, so <c>E[v^2] - E[v]^2</c> cancels away every significant digit of the
    /// answer. Everything is therefore accumulated relative to the first sample, which brings the
    /// summands down to the scale of the quantity being measured.
    /// </remarks>
    private static (double Mean, double Variance) Measure(NoiseTransient tran, IEntityCollection ckt, string node)
    {
        double burnIn = tran.TimeParameters.StopTime / 10.0;
        double weight = 0.0, sum = 0.0, squares = 0.0, previousTime = 0.0, reference = double.NaN;
        foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
        {
            double time = tran.Time;
            double dt = time - previousTime;
            previousTime = time;
            if (time < burnIn)
                continue;

            double v = tran.GetVoltage(node);
            if (double.IsNaN(reference))
                reference = v;
            v -= reference;

            weight += dt;
            sum += dt * v;
            squares += dt * v * v;
        }
        Assert.That(weight, Is.GreaterThan(0.0));
        double mean = sum / weight;
        return (reference + mean, (squares / weight) - (mean * mean));
    }

    private static NoiseTransient CreateSimulation(double conductance, int order, int timeConstants = 5000)
    {
        var tran = new NoiseTransient("tran", CreateTimeParameters(conductance, timeConstants));
        tran.NoiseParameters.MaximumNoiseFrequency = 2.0 * PoleFrequency(conductance);
        tran.NoiseParameters.BandLimitOrder = order;
        tran.NoiseParameters.Seed = Seed;
        return tran;
    }

    private static Circuit CreateDiodeCircuit(double seriesResistance = 0.0)
    {
        var model = new DiodeModel("DM");
        if (seriesResistance > 0.0)
            model.SetParameter("rs", seriesResistance);
        return new Circuit(
            new CurrentSource("I1", "0", "out", BiasCurrent),
            new Diode("D1", "out", "0", "DM"),
            new Capacitor("C1", "out", "0", Capacitance),
            model);
    }

    /// <summary>
    /// A bipolar transistor with its base tied to its collector, so that the whole bias current runs
    /// into a single node.
    /// </summary>
    private static Circuit CreateDiodeConnectedBipolarCircuit()
    {
        var q = new BipolarJunctionTransistor("Q1") { Model = "QM" };
        q.Connect("out", "out", "0", "0");
        return new Circuit(
            new CurrentSource("I1", "0", "out", BiasCurrent),
            q,
            new Capacitor("C1", "out", "0", Capacitance),
            new BipolarJunctionTransistorModel("QM"));
    }

    /// <summary>
    /// A mosfet with its gate tied to its drain, so that the node it drives has a conductance of
    /// <c>gm + gds</c> and the channel noise of <c>4kT*(2/3)*gm</c> charges it.
    /// </summary>
    private static Circuit CreateDiodeConnectedMosfetCircuit(int level)
    {
        IComponent mosfet = level switch
        {
            1 => new Mosfet1("M1") { Model = "MM" },
            2 => new Mosfet2("M1") { Model = "MM" },
            _ => new Mosfet3("M1") { Model = "MM" }
        };
        mosfet.Connect("out", "out", "0", "0");

        IEntity model = level switch
        {
            1 => new Mosfet1Model("MM"),
            2 => new Mosfet2Model("MM"),
            _ => new Mosfet3Model("MM")
        };
        model.SetParameter("kp", 1e-3);
        model.SetParameter("vto", 1.0);

        return new Circuit(
            new CurrentSource("I1", "0", "out", BiasCurrent),
            mosfet,
            new Capacitor("C1", "out", "0", Capacitance),
            model);
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    public void When_Diode_Expect_ShotNoiseVariance(int order)
    {
        // A current-biased diode loaded by a capacitor. Its shot noise is 2*q*I and the node it
        // charges has a conductance gd, so the output variance is q*Vte/(2*C) - half of the kT/C
        // that the thermal noise of a resistor of the same conductance would give.
        var ckt = CreateDiodeCircuit();
        double gd = OperatingPoint(ckt, "D1", "gd");
        double current = OperatingPoint(ckt, "D1", "id");
        double density = 2.0 * Constants.Charge * Math.Abs(current);

        var tran = CreateSimulation(gd, order);
        double expected = ExpectedVariance(density, gd, tran.NoiseParameters.MaximumNoiseFrequency, order);

        var (mean, variance) = Measure(tran, ckt, "out");
        Assert.Multiple(() =>
        {
            // The bias point moves with the noise, so the mean is not the noiseless solution. It has
            // to stay within a fraction of a standard deviation of it all the same.
            Assert.That(mean - OperatingPoint(ckt, "D1", "v"),
                Is.EqualTo(0.0).Within(0.15 * Math.Sqrt(expected)), "mean");
            Assert.That(variance, Is.EqualTo(expected).Within(12.0).Percent, "variance");
        });
    }

    [Test]
    public void When_DiodeConnectedBipolar_Expect_ShotNoiseVariance()
    {
        // The two shot noise sources of a bipolar transistor inject 2*q*(Ic + Ib) into a node whose
        // conductance is gpi + gm + go, which for an ideal transistor is exactly (Ic + Ib)/Vt again.
        var ckt = CreateDiodeConnectedBipolarCircuit();
        double conductance = OperatingPoint(ckt, "Q1", "gpi") +
            OperatingPoint(ckt, "Q1", "gm") +
            OperatingPoint(ckt, "Q1", "go");
        double density = 2.0 * Constants.Charge *
            (Math.Abs(OperatingPoint(ckt, "Q1", "ic")) + Math.Abs(OperatingPoint(ckt, "Q1", "ib")));

        var tran = CreateSimulation(conductance, 2);
        double expected = ExpectedVariance(density, conductance, tran.NoiseParameters.MaximumNoiseFrequency, 2);

        var (_, variance) = Measure(tran, ckt, "out");
        Assert.That(variance, Is.EqualTo(expected).Within(12.0).Percent);
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void When_DiodeConnectedMosfet_Expect_ChannelNoiseVariance(int level)
    {
        // The channel noise of a mosfet is the thermal noise of 2/3 of its transconductance, so a
        // diode-connected mosfet without channel-length modulation charges its load to (2/3)*kT/C.
        var ckt = CreateDiodeConnectedMosfetCircuit(level);
        double gm = OperatingPoint(ckt, "M1", "gm");
        double conductance = gm + OperatingPoint(ckt, "M1", "gds");
        double density = 4.0 * Constants.Boltzmann * Constants.ReferenceTemperature * (2.0 / 3.0) * gm;

        var tran = CreateSimulation(conductance, 2);
        double expected = ExpectedVariance(density, conductance, tran.NoiseParameters.MaximumNoiseFrequency, 2);

        var (_, variance) = Measure(tran, ckt, "out");
        Assert.That(variance, Is.EqualTo(expected).Within(12.0).Percent);
    }

    [Test]
    public void When_Diode_Expect_NoiseDensities()
    {
        // The densities that the device hands to its sources have to be the ones of its own
        // operating point: 4kTG for the series resistance and 2q|I| for the junction current.
        var ckt = CreateDiodeCircuit(10.0);
        double gd = OperatingPoint(ckt, "D1", "gd");
        var tran = CreateSimulation(gd, 2, timeConstants: 20);
        var behavior = RunAndGetBehavior(tran, ckt, "D1");

        double expectedThermal = 4.0 * Constants.Boltzmann * Constants.ReferenceTemperature * 0.1;
        double expectedShot = 2.0 * Constants.Charge *
            Math.Abs(tran.EntityBehaviors["D1"].GetProperty<double>("id"));

        Assert.Multiple(() =>
        {
            Assert.That(behavior.GetProperty<ITimeNoiseSource>("rs").NoiseDensity,
                Is.EqualTo(expectedThermal).Within(1e-6).Percent, "rs");
            Assert.That(behavior.GetProperty<ITimeNoiseSource>("id").NoiseDensity,
                Is.EqualTo(expectedShot).Within(0.1).Percent, "id");
            Assert.That(behavior.NoiseDensity, Is.GreaterThan(0.0), "total");
        });
    }

    [Test]
    public void When_Bipolar_Expect_NoiseDensities()
    {
        var ckt = CreateDiodeConnectedBipolarCircuit();
        ckt["QM"].SetParameter("rb", 10.0);
        ckt["QM"].SetParameter("rc", 20.0);
        ckt["QM"].SetParameter("re", 5.0);
        double conductance = OperatingPoint(ckt, "Q1", "gpi") + OperatingPoint(ckt, "Q1", "gm");
        var tran = CreateSimulation(conductance, 2, timeConstants: 20);
        var behavior = RunAndGetBehavior(tran, ckt, "Q1");

        double kt = Constants.Boltzmann * Constants.ReferenceTemperature;
        Assert.Multiple(() =>
        {
            Assert.That(behavior.GetProperty<ITimeNoiseSource>("rc").NoiseDensity,
                Is.EqualTo(4.0 * kt / 20.0).Within(1e-6).Percent, "rc");
            Assert.That(behavior.GetProperty<ITimeNoiseSource>("re").NoiseDensity,
                Is.EqualTo(4.0 * kt / 5.0).Within(1e-6).Percent, "re");
            Assert.That(behavior.GetProperty<ITimeNoiseSource>("rb").NoiseDensity,
                Is.GreaterThan(0.0), "rb");
            Assert.That(behavior.GetProperty<ITimeNoiseSource>("ic").NoiseDensity,
                Is.EqualTo(2.0 * Constants.Charge *
                    Math.Abs(tran.EntityBehaviors["Q1"].GetProperty<double>("ic"))).Within(0.1).Percent, "ic");
            Assert.That(behavior.GetProperty<ITimeNoiseSource>("ib").NoiseDensity,
                Is.EqualTo(2.0 * Constants.Charge *
                    Math.Abs(tran.EntityBehaviors["Q1"].GetProperty<double>("ib"))).Within(0.1).Percent, "ib");
        });
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void When_Mosfet_Expect_NoiseDensities(int level)
    {
        var ckt = CreateDiodeConnectedMosfetCircuit(level);
        ckt["MM"].SetParameter("rd", 20.0);
        ckt["MM"].SetParameter("rs", 5.0);
        double conductance = OperatingPoint(ckt, "M1", "gm") + OperatingPoint(ckt, "M1", "gds");
        var tran = CreateSimulation(conductance, 2, timeConstants: 20);
        var behavior = RunAndGetBehavior(tran, ckt, "M1");

        double kt = Constants.Boltzmann * Constants.ReferenceTemperature;
        double expectedChannel = 4.0 * kt * (2.0 / 3.0) *
            Math.Abs(tran.EntityBehaviors["M1"].GetProperty<double>("gm"));
        Assert.Multiple(() =>
        {
            Assert.That(behavior.GetProperty<ITimeNoiseSource>("rd").NoiseDensity,
                Is.EqualTo(4.0 * kt / 20.0).Within(1e-6).Percent, "rd");
            Assert.That(behavior.GetProperty<ITimeNoiseSource>("rs").NoiseDensity,
                Is.EqualTo(4.0 * kt / 5.0).Within(1e-6).Percent, "rs");
            Assert.That(behavior.GetProperty<ITimeNoiseSource>("id").NoiseDensity,
                Is.EqualTo(expectedChannel).Within(0.5).Percent, "id");
        });
    }

    [Test]
    public void When_CustomNoiseSource_Expect_SpecifiedDensity()
    {
        // A density that a device outside the framework supplies through its own subclass of
        // TimeNoiseCurrentSource, injected on top of the thermal noise of the resistor that sets the
        // pole. Both are white and independent, so their powers add and the output is 3 times kT/C.
        const double resistance = 1e3;
        double conductance = 1.0 / resistance;
        double thermal = 4.0 * Constants.Boltzmann * Constants.ReferenceTemperature * conductance;

        var ckt = new Circuit(
            new Resistor("R1", "out", "0", resistance),
            new Capacitor("C1", "out", "0", Capacitance),
            new NoiseInjector("N1", "out", "0", 2.0 * thermal));

        var tran = CreateSimulation(conductance, 2);
        double expected = ExpectedVariance(3.0 * thermal, conductance,
            tran.NoiseParameters.MaximumNoiseFrequency, 2);

        var (_, variance) = Measure(tran, ckt, "out");
        Assert.That(variance, Is.EqualTo(expected).Within(12.0).Percent);
    }

    /// <summary>
    /// Runs a simulation to the end and returns the transient noise behavior of an entity, so that
    /// the densities of the last probed timepoint can be compared against the operating point that
    /// they were computed from.
    /// </summary>
    private static ITimeNoiseBehavior RunAndGetBehavior(NoiseTransient tran, IEntityCollection ckt, string name)
    {
        tran.RunToEnd(ckt);
        return tran.EntityBehaviors[name].GetValue<ITimeNoiseBehavior>();
    }
}
