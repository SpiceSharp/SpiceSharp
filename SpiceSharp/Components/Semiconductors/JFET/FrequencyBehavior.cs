using System;
using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.JFETBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="JFET" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.Behaviors.BaseFrequencyBehavior" />
    /// <seealso cref="SpiceSharp.Components.IConnectedBehavior" />
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior
    {
        /// <summary>
        /// Gets the G-S capacitance.
        /// </summary>
        /// <value>
        /// The capacitance.
        /// </value>
        [ParameterName("capgs"), ParameterInfo("Capacitance G-S")]
        public double CapGs { get; private set; }

        /// <summary>
        /// Gets the G-D capacitance.
        /// </summary>
        /// <value>
        /// The capacitance.
        /// </value>
        [ParameterName("capgd"), ParameterInfo("Capacitance G-D")]
        public double CapGd { get; private set; }

        /// <summary>
        /// Nodes
        /// </summary>
        protected MatrixElement<Complex> CDrainDrainPtr { get; private set; }
        protected MatrixElement<Complex> CGateGatePtr { get; private set; }
        protected MatrixElement<Complex> CSourceSourcePtr { get; private set; }
        protected MatrixElement<Complex> CDrainPrimeDrainPrimePtr { get; private set; }
        protected MatrixElement<Complex> CSourcePrimeSourcePrimePtr { get; private set; }
        protected MatrixElement<Complex> CDrainDrainPrimePtr { get; private set; }
        protected MatrixElement<Complex> CGateDrainPrimePtr { get; private set; }
        protected MatrixElement<Complex> CGateSourcePrimePtr { get; private set; }
        protected MatrixElement<Complex> CSourceSourcePrimePtr { get; private set; }
        protected MatrixElement<Complex> CDrainPrimeDrainPtr { get; private set; }
        protected MatrixElement<Complex> CDrainPrimeGatePtr { get; private set; }
        protected MatrixElement<Complex> CDrainPrimeSourcePrimePtr { get; private set; }
        protected MatrixElement<Complex> CSourcePrimeGatePtr { get; private set; }
        protected MatrixElement<Complex> CSourcePrimeSourcePtr { get; private set; }
        protected MatrixElement<Complex> CSourcePrimeDrainPrimePtr { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public FrequencyBehavior(string name) : base(name)
        {
        }

        /// <summary>
        /// Allocate elements in the Y-matrix and Rhs-vector to populate during loading.
        /// </summary>
        /// <param name="solver">The solver.</param>
        public void GetEquationPointers(Solver<Complex> solver)
        {
            solver.ThrowIfNull(nameof(solver));
            CDrainDrainPtr = solver.GetMatrixElement(DrainNode, DrainNode);
            CGateGatePtr = solver.GetMatrixElement(GateNode, GateNode);
            CSourceSourcePtr = solver.GetMatrixElement(SourceNode, SourceNode);
            CDrainPrimeDrainPrimePtr = solver.GetMatrixElement(DrainPrimeNode, DrainPrimeNode);
            CSourcePrimeSourcePrimePtr = solver.GetMatrixElement(SourcePrimeNode, SourcePrimeNode);
            CDrainDrainPrimePtr = solver.GetMatrixElement(DrainNode, DrainPrimeNode);
            CGateDrainPrimePtr = solver.GetMatrixElement(GateNode, DrainPrimeNode);
            CGateSourcePrimePtr = solver.GetMatrixElement(GateNode, SourcePrimeNode);
            CSourceSourcePrimePtr = solver.GetMatrixElement(SourceNode, SourcePrimeNode);
            CDrainPrimeDrainPtr = solver.GetMatrixElement(DrainPrimeNode, DrainNode);
            CDrainPrimeGatePtr = solver.GetMatrixElement(DrainPrimeNode, GateNode);
            CDrainPrimeSourcePrimePtr = solver.GetMatrixElement(DrainPrimeNode, SourcePrimeNode);
            CSourcePrimeGatePtr = solver.GetMatrixElement(SourcePrimeNode, GateNode);
            CSourcePrimeSourcePtr = solver.GetMatrixElement(SourcePrimeNode, SourceNode);
            CSourcePrimeDrainPrimePtr = solver.GetMatrixElement(SourcePrimeNode, DrainPrimeNode);
        }

        /// <summary>
        /// Initializes the parameters.
        /// </summary>
        /// <param name="simulation">The frequency simulation.</param>
        public void InitializeParameters(FrequencySimulation simulation)
        {
            simulation.ThrowIfNull(nameof(simulation));
            var vgs = Vgs;
            var vgd = Vgd;

            // Calculate charge storage elements
            var czgs = TempCapGs * BaseParameters.Area;
            var czgd = TempCapGd * BaseParameters.Area;
            var twop = TempGatePotential + TempGatePotential;
            var czgsf2 = czgs / ModelTemperature.F2;
            var czgdf2 = czgd / ModelTemperature.F2;
            if (vgs < CorDepCap)
            {
                var sarg = Math.Sqrt(1 - vgs / TempGatePotential);
                CapGs = czgs / sarg;
            }
            else
                CapGs = czgsf2 * (ModelTemperature.F3 + vgs / twop);

            if (vgd < CorDepCap)
            {
                var sarg = Math.Sqrt(1 - vgd / TempGatePotential);
                CapGd = czgd / sarg;
            }
            else
                CapGd = czgdf2 * (ModelTemperature.F3 + vgd / twop);
        }

        /// <summary>
        /// Load the Y-matrix and right-hand side vector for frequency domain analysis.
        /// </summary>
        /// <param name="simulation">The frequency simulation.</param>
        public void Load(FrequencySimulation simulation)
        {
            simulation.ThrowIfNull(nameof(simulation));
            var omega = simulation.ComplexState.Laplace.Imaginary;

            var gdpr = ModelParameters.DrainConductance * BaseParameters.Area;
            var gspr = ModelParameters.SourceConductance * BaseParameters.Area;
            var gm = Gm;
            var gds = Gds;
            var ggs = Ggs;
            var xgs = CapGs * omega;
            var ggd = Ggd;
            var xgd = CapGd * omega;

            CDrainDrainPtr.Value += gdpr;
            CGateGatePtr.Value += new Complex(ggd + ggs, xgd + xgs);
            CSourceSourcePtr.Value += gspr;
            CDrainPrimeDrainPrimePtr.Value += new Complex(gdpr + gds + ggd, xgd);
            CSourcePrimeSourcePrimePtr.Value += new Complex(gspr + gds + gm + ggs, xgs);
            CDrainDrainPrimePtr.Value -= gdpr;
            CGateDrainPrimePtr.Value -= new Complex(ggd, xgd);
            CGateSourcePrimePtr.Value -= new Complex(ggs, xgs);
            CSourceSourcePrimePtr.Value -= gspr;
            CDrainPrimeDrainPtr.Value -= gdpr;
            CDrainPrimeGatePtr.Value += new Complex(-ggd + gm, -xgd);
            CDrainPrimeSourcePrimePtr.Value += (-gds - gm);
            CSourcePrimeGatePtr.Value -= new Complex(ggs + gm, xgs);
            CSourcePrimeSourcePtr.Value -= gspr;
            CSourcePrimeDrainPrimePtr.Value -= gds;
        }
    }
}
