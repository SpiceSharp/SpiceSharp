using System;
using SpiceSharp.Algebra;
using SpiceSharp.Simulations;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// Provides methods for differential equation integration in time-domain analysis. This is an abstract class.
    /// </summary>
    public abstract class IntegrationMethod
    {
        /// <summary>
        /// Gets the maximum order for the integration method
        /// </summary>
        public int MaxOrder { get; }

        /// <summary>
        /// Gets the current order
        /// </summary>
        public int Order { get; protected set; }

        /// <summary>
        /// Gets the previously accepted integration states
        /// </summary>
        protected History<IntegrationState> IntegrationStates { get; }

        /// <summary>
        /// Class for managing state indices
        /// </summary>
        protected StateManager StateManager { get; } = new StateManager();

        /// <summary>
        /// Gets the time of the last accepted point
        /// </summary>
        public double BaseTime { get; private set; }

        /// <summary>
        /// Gets the time of the currently probed point
        /// </summary>
        public double Time { get; private set; }

        /// <summary>
        /// The first order derivative of any variable that is
        /// dependent on the timestep
        /// </summary>
        public double Slope { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxOrder">Maximum integration order</param>
        protected IntegrationMethod(int maxOrder)
        {
            if (maxOrder < 1)
                throw new CircuitException("Invalid order {0}".FormatString(maxOrder));
            MaxOrder = maxOrder;

            // Allocate history of timesteps and solutions
            IntegrationStates = new ArrayHistory<IntegrationState>(maxOrder + 1);
        }

        /// <summary>
        /// Setup the integration method
        /// </summary>
        /// <param name="simulation">The simulation</param>
        public virtual void Setup(TimeSimulation simulation)
        {
            var solver = simulation?.RealState?.Solver;
            if (solver == null)
                throw new CircuitException("Could not extract solver");
            IntegrationStates.Clear(i => new IntegrationState(1.0, 
                new DenseVector<double>(solver.Order), 
                StateManager.Build()));
        }

        /// <summary>
        /// Create a history
        /// </summary>
        public virtual StateHistory CreateHistory() => new StateHistoryDefault(IntegrationStates, StateManager);

        /// <summary>
        /// Create a state that can be derived
        /// </summary>
        /// <returns></returns>
        public abstract StateDerivative CreateDerivative();
        
        /// <summary>
        /// Initialize/reset the integration method
        /// </summary>
        public virtual void Initialize()
        {
            // Initialize variables
            Slope = 0.0;
            Order = 1;

            // Copy the first state to all other states (assume DC situation)
            for (var i = 1; i < IntegrationStates.Length; i++)
            {
                IntegrationStates[i].Delta = IntegrationStates[0].Delta;
                IntegrationStates[0].Solution.CopyTo(IntegrationStates[i].Solution);
                IntegrationStates[0].State.CopyTo(IntegrationStates[i].State);
            }
        }

        /// <summary>
        /// Probe a new time point
        /// </summary>
        /// <param name="simulation">Time simulation</param>
        /// <param name="delta">The timestep to be probed</param>
        public virtual void Probe(TimeSimulation simulation, double delta)
        {
            // Advance the probing time
            Time = BaseTime + delta;
            IntegrationStates[0].Delta = delta;
        }

        /// <summary>
        /// Evaluate (usually the states of) the time simulation if the current solution can be accepted.
        /// Returning false indicates that the solution is not acceptable, and that the time simulation
        /// should try again probing a new time step
        /// </summary>
        /// <param name="simulation">Time simulation</param>
        /// <param name="newDelta">The requested timestep</param>
        public virtual bool Evaluate(TimeSimulation simulation, out double newDelta)
        {
            // Store the current solution
            simulation.RealState?.Solution.CopyTo(IntegrationStates[0].Solution);

            // Use the same delta as before
            newDelta = IntegrationStates[0].Delta * 2.0;
            return true;
        }

        /// <summary>
        /// Accept the last evaluated time point as valid
        /// </summary>
        public virtual void Accept()
        {
            // Shift the solutions and overwrite index 0 with the current solution
            IntegrationStates.Cycle();
            BaseTime = Time;
        }

        /// <summary>
        /// Unsetup the integration method
        /// </summary>
        public virtual void Unsetup()
        {
            // Clear the timesteps and solutions
            IntegrationStates.Clear((IntegrationState) null);

            Order = 0;
            Slope = 0.0;
        }

        /// <summary>
        /// Get a timestep
        /// </summary>
        /// <param name="index">Points to go back in time</param>
        /// <returns></returns>
        public double GetTimestep(int index) => IntegrationStates[index].Delta;
    }
}
