using SpiceSharp.IntegrationMethods;

namespace SpiceSharp.Simulations.IntegrationMethods
{
    /// <summary>
    /// Class capable of managing timesteps and time
    /// </summary>
    public class Timesteps
    {
        /// <summary>
        /// Gets the current time
        /// </summary>
        public double Time { get; protected set; }

        /// <summary>
        /// Gets the current timestep
        /// </summary>
        public double Delta { get; protected set; }

        /// <summary>
        /// Gets the history of timesteps
        /// </summary>
        public ReadOnlyHistory<double> DeltaHistory { get; }

        /// <summary>
        /// Get a timestep in history
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns></returns>
        public double this[int index] => OldDeltas[index];

        /// <summary>
        /// Gets the history of timesteps
        /// </summary>
        protected ArrayHistory<double> OldDeltas { get; }

        /// <summary>
        /// Gets the last accepted time point
        /// </summary>
        public double SaveTime { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="history">Number of points needed</param>
        public Timesteps(int history)
        {
            // We will allocate the required number of points + the current one
            OldDeltas = new ArrayHistory<double>(history + 1);
            DeltaHistory = new ReadOnlyHistory<double>(OldDeltas);
        }

        /// <summary>
        /// Initialize the controller
        /// The history is cleared with the initial timestep
        /// </summary>
        /// <param name="delta">Initial timestep</param>
        public virtual void Initialize(double delta)
        {
            OldDeltas.Clear(i => delta);
            Delta = delta;
            SaveTime = 0.0;
            Time = 0.0;
        }

        /// <summary>
        /// Clear the timesteps
        /// </summary>
        public virtual void Clear()
        {
            OldDeltas.Clear(0.0);
            Delta = 0.0;
            SaveTime = 0.0;
            Time = 0.0;
        }

        /// <summary>
        /// Probe a new time point
        /// </summary>
        /// <param name="delta">Timestep</param>
        public virtual void Probe(double delta)
        {
            Delta = delta;
            Time = SaveTime + delta;
            OldDeltas.Current = delta;
        }

        /// <summary>
        /// Accept the new time point
        /// The previous timestep will be used as the new timestep
        /// </summary>
        public virtual void Accept()
        {
            // Flag the current timepoint as accepted
            SaveTime = Time;
            OldDeltas.Store(Delta);
        }
    }
}
