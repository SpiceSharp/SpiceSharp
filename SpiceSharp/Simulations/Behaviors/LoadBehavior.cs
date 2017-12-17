namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// General behavior for a circuit object
    /// </summary>
    public abstract class LoadBehavior : Behavior
    {
        /// <summary>
        /// Load the Y-matrix and Rhs-vector
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public abstract void Load(Circuit ckt);

        /// <summary>
        /// Test convergence
        /// </summary>
        /// <returns></returns>
        public virtual bool IsConvergent(Circuit ckt)
        {
            return true;
        }
    }
}
