namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Temperature-dependent behavior for circuit objects
    /// </summary>
    public abstract class TemperatureBehavior : Behavior
    {
        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public abstract void Temperature(Circuit ckt);
    }
}
