using SpiceSharp.Components.Common;

namespace SpiceSharp.Components.Mosfets
{
    /// <summary>
    /// Contributions for loading a mosfet.
    /// </summary>
    /// <remarks>
    /// Please be careful using this struct, as it is mutable. It was created to be
    /// used to group the contribution variables for a mosfet with 4 terminals.
    /// </remarks>
    /// <typeparam name="T">The base value type.</typeparam>
    public class Contributions<T> where T : struct
    {
        /// <summary>
        /// The gate-drain contribution.
        /// </summary>
        public Contribution<T> Gd;

        /// <summary>
        /// The gate-source contribution.
        /// </summary>
        public Contribution<T> Gs;

        /// <summary>
        /// The gate-bulk contribution.
        /// </summary>
        public Contribution<T> Gb;

        /// <summary>
        /// The bulk-drain contribution.
        /// </summary>
        public Contribution<T> Bd;

        /// <summary>
        /// The bulk-source contribution.
        /// </summary>
        public Contribution<T> Bs;

        /// <summary>
        /// The drain-source contribution.
        /// </summary>
        public Contribution<T> Ds;

        /// <summary>
        /// Reset all the contributions.
        /// </summary>
        public void Reset()
        {
            Gd.G = default;
            Gd.C = default;
            Gs.G = default;
            Gs.C = default;
            Gb.G = default;
            Gb.C = default;
            Bd.G = default;
            Bd.C = default;
            Bs.G = default;
            Bs.C = default;
            Ds.G = default;
            Ds.C = default;
        }
    }
}
