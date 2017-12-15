namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Temperature-dependent behaviour for circuit objects
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
