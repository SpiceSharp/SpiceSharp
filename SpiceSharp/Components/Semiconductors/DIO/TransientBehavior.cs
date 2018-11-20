using System;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// Transient behavior for a <see cref="Diode"/>
    /// </summary>
    public class TransientBehavior : BiasingBehavior, ITimeBehavior
    {
        /// <summary>
        /// Diode capacitance
        /// </summary>
        [ParameterName("cd"), ParameterInfo("Diode capacitance")]
        public double Capacitance { get; protected set; }
        [ParameterName("id"), ParameterName("c"), ParameterInfo("Diode current")]
        public double Current { get; protected set; }

        /// <summary>
        /// The charge on the junction capacitance
        /// </summary>
        public StateDerivative CapCharge { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public TransientBehavior(string name) : base(name) { }
        
        /// <summary>
        /// Create states
        /// </summary>
        /// <param name="method"></param>
        public void CreateStates(IntegrationMethod method)
        {
			if (method == null)
				throw new ArgumentNullException(nameof(method));

            CapCharge = method.CreateDerivative();
        }

        /// <summary>
        /// Calculate the state values
        /// </summary>
        /// <param name="simulation">Simulation</param>
        public void GetDcState(TimeSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.RealState;
            double capd;
            var vd = state.Solution[PosPrimeNode] - state.Solution[NegNode];

            // charge storage elements
            var czero = TempJunctionCap * BaseParameters.Area;
            if (vd < TempDepletionCap)
            {
                var arg = 1 - vd / ModelParameters.JunctionPotential;
                var sarg = Math.Exp(-ModelParameters.GradingCoefficient * Math.Log(arg));
                CapCharge.Current = ModelParameters.TransitTime * base.Current + ModelParameters.JunctionPotential * czero * (1 - arg * sarg) / (1 -
                        ModelParameters.GradingCoefficient);
                capd = ModelParameters.TransitTime * Conduct + czero * sarg;
            }
            else
            {
                var czof2 = czero / ModelTemperature.F2;
                CapCharge.Current = ModelParameters.TransitTime * base.Current + czero * TempFactor1 + czof2 * (ModelTemperature.F3 * (vd -
                    TempDepletionCap) + ModelParameters.GradingCoefficient / (ModelParameters.JunctionPotential + ModelParameters.JunctionPotential) * (vd * vd - TempDepletionCap * TempDepletionCap));
                capd = ModelParameters.TransitTime * Conduct + czof2 * (ModelTemperature.F3 + ModelParameters.GradingCoefficient * vd / ModelParameters.JunctionPotential);
            }
            Capacitance = capd;
        }

        /// <summary>
        /// Allocate elements in the Y-matrix and Rhs-vector to populate during loading. Additional
        /// equations can also be allocated here.
        /// </summary>
        /// <param name="solver">The solver.</param>
        public void GetEquationPointers(Solver<double> solver)
        {
            // No extra pointers needed
        }

        /// <summary>
        /// Transient behavior
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public void Transient(TimeSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.RealState;
            var vd = state.Solution[PosPrimeNode] - state.Solution[NegNode];

            // This is the same calculation
            GetDcState(simulation);

            // Integrate
            CapCharge.Integrate();
            var geq = CapCharge.Jacobian(Capacitance);
            var ceq = CapCharge.RhsCurrent(geq, vd);

            // Store the current
            Current = base.Current + CapCharge.Derivative;

            // Load Rhs vector
            NegPtr.Value += ceq;
            PosPrimePtr.Value -= ceq;

            // Load Y-matrix
            NegNegPtr.Value += geq;
            PosPrimePosPrimePtr.Value += geq;
            NegPosPrimePtr.Value -= geq;
            PosPrimeNegPtr.Value -= geq;
        }

        /// <summary>
        /// Use local truncation error to cut timestep
        /// </summary>
        /// <returns>The timestep that satisfies the LTE</returns>
        // public override double Truncate() => CapCharge.LocalTruncationError();
    }
}
