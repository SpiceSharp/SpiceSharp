namespace SpiceSharp.Components.Mosfets.Level3
{
    /// <summary>
    /// Model properties for a <see cref="Mosfet3Model"/>.
    /// </summary>
    public class ModelProperties : Mosfets.ModelProperties
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public double Alpha { get; set; }
        public double CoeffDepLayWidth { get; set; }
        public double NarrowFactor { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
